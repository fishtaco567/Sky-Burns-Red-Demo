using UnityEngine;
using System.Collections.Generic;
using System;
using NCalc;

public class SkyEventManager : Singleton<SkyEventManager> {

    protected Dictionary<string, float> gameVariables;
    protected Dictionary<string, Func<FunctionArgs, float>> gameFunctions;
    protected Dictionary<string, Action<FunctionArgs>> gameEvents;

    protected List<SkyEventProgram> activePrograms;

    protected Pica.PicaRuntime rt;

    public void Awake() {
        gameVariables = new Dictionary<string, float>();
        gameFunctions = new Dictionary<string, Func<FunctionArgs, float>>();
        gameEvents = new Dictionary<string, Action<FunctionArgs>>();
        activePrograms = new List<SkyEventProgram>();

        gameEvents.Add("SpawnPop", SpawnPop);
        gameEvents.Add("SpawnPopLeft", SpawnPopLeft);
        gameEvents.Add("SpawnPopRight", SpawnPopRight);
        gameEvents.Add("SetStandRepair", SetStandRepair);
        gameEvents.Add("AddStandForce", AddStandForce);
        gameEvents.Add("AddStandRepair", AddStandRepair);
        gameEvents.Add("SetCrowdMood", SetCrowdMood);
        gameEvents.Add("AddCrowdMood", AddCrowdMood);

        gameFunctions.Add("Random", Random);

        var inn = System.IO.File.ReadAllText(Application.persistentDataPath + "/test.pica");
        rt = new Pica.PicaRuntime();
        rt.AddProgram("overall", inn);
    }

    public SkyEventProgram CreateProgram(string s) {
        var parser = new SkyEventLangParser();
        var program = parser.Parse(s);
        program.LoadGameData(gameVariables, gameFunctions, gameEvents);
        return program;
    }

    public void ExecuteProgram(SkyEventProgram program) {
        activePrograms.Add(program);
    }

    public void CreateAndExecuteProgram(string s) {
        var program = CreateProgram(s);
        ExecuteProgram(program);
    }

    protected void LateUpdate() {
        UpdateVariables();

        for(int i = activePrograms.Count - 1; i >= 0; i--) {
            var program = activePrograms[i];
            program.ExecuteFrame();

            if(program.IsDone()) {
                activePrograms.RemoveAt(i);
            }
        }
        rt.RunTick();
    }

    #region Variables and Functions
    public void UpdateVariables() {
        gameVariables["standRepair"] = SimulatorManager.Instance.currentSim.GetRepair();
        gameVariables["standAngle"] = SimulatorManager.Instance.currentSim.GetAngle();
        gameVariables["crowdMood"] = 1 - BeatmapController.Instance.crowd.mood;
        gameVariables["perfHits"] = BeatmapController.Instance.numPerfect;
        gameVariables["goodHits"] = BeatmapController.Instance.numGood;
        gameVariables["okHits"] = BeatmapController.Instance.numOk;
        gameVariables["missedHits"] = BeatmapController.Instance.numMissed;
        gameVariables["totalHits"] = BeatmapController.Instance.numHit;
        gameVariables["currentStreak"] = BeatmapController.Instance.currentStreak;
        gameVariables["maxStreak"] = BeatmapController.Instance.maxStreak;
        gameVariables["maxAngle"] = BeatmapController.Instance.maxAngle;
        gameVariables["songTime"] = BeatmapController.Instance.currentSongTime;
    }

    protected float Random(FunctionArgs args) {
        var resolved = EvaluateArgs<float, float>("Random", args);

        return UnityEngine.Random.Range(resolved.Item1, resolved.Item2);
    }

    protected void SpawnPop(FunctionArgs args) {
        var evaledArgs = EvaluateArgs<string, float, float, float, float>("DoPop", args);

        if(!PopManager.Instance.HasPop(evaledArgs.Item1)) {
            LogError(evaledArgs.Item1 + " pop does not exist");
        }
        PopManager.Instance.DoPop(evaledArgs.Item1, evaledArgs.Item2, evaledArgs.Item3, evaledArgs.Item4, evaledArgs.Item5);
        args.Result = true;
    }

    protected void SpawnPopLeft(FunctionArgs args) {
        var evaledArgs = EvaluateArgs<string, float, float>("DoPopLeft", args);

        if(!PopManager.Instance.HasPop(evaledArgs.Item1)) {
            LogError(evaledArgs.Item1 + " pop does not exist");
        }
        PopManager.Instance.DoPopSide(evaledArgs.Item1, evaledArgs.Item2, evaledArgs.Item3, false);
        args.Result = true;
    }

    protected void SpawnPopRight(FunctionArgs args) {
        var evaledArgs = EvaluateArgs<string, float, float>("DoPopRight", args);

        if(!PopManager.Instance.HasPop(evaledArgs.Item1)) {
            LogError(evaledArgs.Item1 + " pop does not exist");
        }
        PopManager.Instance.DoPopSide(evaledArgs.Item1, evaledArgs.Item2, evaledArgs.Item3, true);
        args.Result = true;
    }

    protected void SetStandRepair(FunctionArgs args) {
        var evaledArgs = EvaluateArgs<float>("SetStandRepair", args);

        SimulatorManager.Instance.currentSim.SetRepair(evaledArgs);
        args.Result = true;
    }

    protected void AddStandRepair(FunctionArgs args) {
        var evaledArgs = EvaluateArgs<float>("AddStandRepair", args);

        SimulatorManager.Instance.currentSim.Repair(evaledArgs);
        args.Result = true;
    }

    protected void AddStandForce(FunctionArgs args) {
        var resolved = EvaluateArgs<float, float>("AddStandForce", args);

        SimulatorManager.Instance.currentSim.ApplySmoothedImpulse(resolved.Item1, resolved.Item2);
    }

    protected void SetCrowdMood(FunctionArgs args) {
        var resolved = EvaluateArgs<float>("SetCrowdMood", args);

        BeatmapController.Instance.crowd.mood = 1 - Mathf.Clamp01(resolved);
    }

    protected void AddCrowdMood(FunctionArgs args) {
        var resolved = EvaluateArgs<float>("AddCrowdMood", args);

        var newMood = BeatmapController.Instance.crowd.mood + (1 - resolved);
        BeatmapController.Instance.crowd.mood = Mathf.Clamp01(newMood);
    }

    #region argumentresolvers
    private T EvaluateArgs<T>(string func, FunctionArgs args) {
        if(args.Parameters.Length != 1) {
            LogError(func + " requires 1 parameter, " + args.Parameters.Length + " parameters given");
        }

        if(args.Parameters[0].HasErrors()) {
            LogError(func + " parameter 1 has error: " + args.Parameters[0].Error);
        }

        var result = args.Parameters[0].Evaluate();
        try {
            return (T)Convert.ChangeType(result, typeof(T));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T));
            return default;
        }
    }

    private (T1, T2) EvaluateArgs<T1, T2>(string func, FunctionArgs args) {
        if(args.Parameters.Length != 2) {
            LogError(func + " requires 2 parameters, " + args.Parameters.Length + " parameters given");
        }

        for(int i = 0; i < args.Parameters.Length; i++) {
            if(args.Parameters[i].HasErrors()) {
                LogError(func + " parameter " + i + " has error: " + args.Parameters[i].Error);
            }
        }

        T1 final1;
        T2 final2;

        var result = args.Parameters[0].Evaluate();
        try {
            final1 = (T1)Convert.ChangeType(result, typeof(T1));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[1].Evaluate();
        try {
            final2 = (T2)Convert.ChangeType(result, typeof(T2));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        return (final1, final2);
    }

    private (T1, T2, T3) EvaluateArgs<T1, T2, T3>(string func, FunctionArgs args) {
        if(args.Parameters.Length != 3) {
            LogError(func + " requires 3 parameters, " + args.Parameters.Length + " parameters given");
        }

        for(int i = 0; i < args.Parameters.Length; i++) {
            if(args.Parameters[i].HasErrors()) {
                LogError(func + " parameter " + i + " has error: " + args.Parameters[i].Error);
            }
        }

        T1 final1;
        T2 final2;
        T3 final3;

        var result = args.Parameters[0].Evaluate();
        try {
            final1 = (T1)Convert.ChangeType(result, typeof(T1));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[1].Evaluate();
        try {
            final2 = (T2)Convert.ChangeType(result, typeof(T2));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[2].Evaluate();
        try {
            final3 = (T3)Convert.ChangeType(result, typeof(T3));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        return (final1, final2, final3);
    }

    private (T1, T2, T3, T4) EvaluateArgs<T1, T2, T3, T4>(string func, FunctionArgs args) {
        if(args.Parameters.Length != 4) {
            LogError(func + " requires 4 parameters, " + args.Parameters.Length + " parameters given");
        }

        for(int i = 0; i < args.Parameters.Length; i++) {
            if(args.Parameters[i].HasErrors()) {
                LogError(func + " parameter " + i + " has error: " + args.Parameters[i].Error);
            }
        }

        T1 final1;
        T2 final2;
        T3 final3;
        T4 final4;

        var result = args.Parameters[0].Evaluate();
        try {
            final1 = (T1)Convert.ChangeType(result, typeof(T1));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[1].Evaluate();
        try {
            final2 = (T2)Convert.ChangeType(result, typeof(T2));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[2].Evaluate();
        try {
            final3 = (T3)Convert.ChangeType(result, typeof(T3));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[3].Evaluate();
        try {
            final4 = (T4)Convert.ChangeType(result, typeof(T4));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        return (final1, final2, final3, final4);
    }

    private (T1, T2, T3, T4, T5) EvaluateArgs<T1, T2, T3, T4, T5>(string func, FunctionArgs args) {
        if(args.Parameters.Length != 5) {
            LogError(func + " requires 5 parameters, " + args.Parameters.Length + " parameters given");
        }

        for(int i = 0; i < args.Parameters.Length; i++) {
            if(args.Parameters[i].HasErrors()) {
                LogError(func + " parameter " + i + " has error: " + args.Parameters[i].Error);
            }
        }

        T1 final1;
        T2 final2;
        T3 final3;
        T4 final4;
        T5 final5;

        var result = args.Parameters[0].Evaluate();
        try {
            final1 = (T1)Convert.ChangeType(result, typeof(T1));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[1].Evaluate();
        try {
            final2 = (T2)Convert.ChangeType(result, typeof(T2));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[2].Evaluate();
        try {
            final3 = (T3)Convert.ChangeType(result, typeof(T3));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[3].Evaluate();
        try {
            final4 = (T4)Convert.ChangeType(result, typeof(T4));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[4].Evaluate();
        try {
            final5 = (T5)Convert.ChangeType(result, typeof(T5));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        return (final1, final2, final3, final4, final5);
    }

    private (T1, T2, T3, T4, T5, T6) EvaluateArgs<T1, T2, T3, T4, T5, T6>(string func, FunctionArgs args) {
        if(args.Parameters.Length != 6) {
            LogError(func + " requires 6 parameters, " + args.Parameters.Length + " parameters given");
        }

        for(int i = 0; i < args.Parameters.Length; i++) {
            if(args.Parameters[i].HasErrors()) {
                LogError(func + " parameter " + i + " has error: " + args.Parameters[i].Error);
            }
        }

        T1 final1;
        T2 final2;
        T3 final3;
        T4 final4;
        T5 final5;
        T6 final6;
        
        var result = args.Parameters[0].Evaluate();
        try {
            final1 = (T1)Convert.ChangeType(result, typeof(T1));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[1].Evaluate();
        try {
            final2 = (T2)Convert.ChangeType(result, typeof(T2));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[2].Evaluate();
        try {
            final3 = (T3)Convert.ChangeType(result, typeof(T3));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[3].Evaluate();
        try {
            final4 = (T4)Convert.ChangeType(result, typeof(T4));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[4].Evaluate();
        try {
            final5 = (T5)Convert.ChangeType(result, typeof(T5));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        result = args.Parameters[5].Evaluate();
        try {
            final6 = (T6)Convert.ChangeType(result, typeof(T6));
        } catch(Exception) {
            LogError(func + " parameter 1 is of type " + result.GetType() + ", requires type " + typeof(T1));
            return default;
        }

        return (final1, final2, final3, final4, final5, final6);
    }
    #endregion
    #endregion

    private void LogError(string s) {
        Debug.LogError(s);
    }

}
