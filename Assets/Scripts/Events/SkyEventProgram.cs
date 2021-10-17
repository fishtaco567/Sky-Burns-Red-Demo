using System.Collections.Generic;
using System;
using NCalc;

public class SkyEventProgram {

    public List<SkyEventExpression> program;
    protected int executionHead;

    public Dictionary<string, float> variables;
    public Dictionary<string, float> gameVariables;
    protected Dictionary<string, Func<FunctionArgs, float>> gameFunctions;
    protected Dictionary<string, Action<FunctionArgs>> gameEvents;

    public List<SkyEventExpression> asyncExpressions;

    public SkyEventProgram(List<SkyEventExpression> program) {
        this.program = program;
        this.executionHead = 0;
        this.variables = new Dictionary<string, float>();
        this.asyncExpressions = new List<SkyEventExpression>();
    }

    public void LoadGameData(Dictionary<string, float> gameVariables, Dictionary<string, Func<FunctionArgs, float>> gameFunctions, Dictionary<string, Action<FunctionArgs>> gameEvents) {
        this.gameVariables = gameVariables;
        this.gameFunctions = gameFunctions;
        this.gameEvents = gameEvents;
    }

    public void ExecuteFrame() {
        while(executionHead < program.Count) {
            var result = program[executionHead].Execute(this);
            if(result == SkyEventExpressionResult.Success) {
                executionHead++;
            } else if(result == SkyEventExpressionResult.Wait) {
                break;
            } else if(result == SkyEventExpressionResult.Error) {
                //TODO handle error
                executionHead = program.Count + 1;
            }
        }

        for(int i = asyncExpressions.Count - 1; i >= 0; i--) {
            var result = asyncExpressions[i].Execute(this);
            if(result == SkyEventExpressionResult.Success) {
                asyncExpressions.RemoveAt(i);
            } else if(result == SkyEventExpressionResult.Wait) {
                continue;
            } else if(result == SkyEventExpressionResult.Error) {
                //TODO handle error
                asyncExpressions.RemoveAt(i);
            }
        }
    }

    public float? Calculate(string s) {
        var expression = new Expression(s);
        expression.EvaluateFunction += EvaluateFunction;
        expression.EvaluateParameter += EvaluateParameter;
        var hasErrors = expression.HasErrors();
        if(hasErrors) {
            //Handle Error
            return null;
        }

        var evaled = expression.Evaluate();
        try {
            return Convert.ToSingle(evaled);
        } catch(Exception e) {
            return null;
        }
    }

    public bool? CalculateBool(string s) {
        var expression = new Expression(s);
        expression.EvaluateFunction += EvaluateFunction;
        expression.EvaluateParameter += EvaluateParameter;
        var hasErrors = expression.HasErrors();
        if(hasErrors) {
            //Handle Error
            return null;
        }


        var evaled = expression.Evaluate();
        try {
            return Convert.ToBoolean(evaled);
        } catch(Exception e) {
            return null;
        }
    }

    public bool DispatchEvent(string s) {
        var expression = new Expression(s);
        expression.EvaluateParameter += EvaluateParameter;
        expression.EvaluateFunction += EvaluateFunction;
        expression.EvaluateFunction += EvaluateEvent;
        var hasErrors = expression.HasErrors();
        if(hasErrors) {
            //Handle Error
            return false;
        }

        expression.Evaluate();
        return true;
    }

    public bool DispatchEvent(string s, Dictionary<string, float> intVar, Dictionary<string, float> intGameVar) {
        var expression = new Expression(s);
        expression.EvaluateParameter += delegate(string name, ParameterArgs args) {

            if(intGameVar.TryGetValue(name, out float gv)) {
                args.Result = gv;
            } else if(intVar.TryGetValue(name, out float v)) {
                args.Result = v;
            }
        };
        expression.EvaluateFunction += EvaluateFunction;
        expression.EvaluateFunction += EvaluateEvent;
        var hasErrors = expression.HasErrors();
        if(hasErrors) {
            //Handle Error
            return false;
        }

        expression.Evaluate();
        return true;
    }

    private void EvaluateFunction(string name, FunctionArgs args) {
        if(gameFunctions.TryGetValue(name, out var function)) {
            args.Result = function?.Invoke(args);
        }
    }

    private void EvaluateParameter(string name, ParameterArgs args) {
        if(gameVariables.TryGetValue(name, out float gv)) {
            args.Result = gv;
        } else if(variables.TryGetValue(name, out float v)) {
            args.Result = v;
        }
    }

    private void EvaluateEvent(string name, FunctionArgs args) {
        if(gameEvents.TryGetValue(name, out var e)) {
            e?.Invoke(args);
        }
    } 

    public bool IsDone() {
        return executionHead >= program.Count && asyncExpressions.Count == 0;
    }

}