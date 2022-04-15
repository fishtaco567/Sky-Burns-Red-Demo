using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pica {

    public static class GameEventInterface {

        public static void Setup(PicaEnv ffi) {
            ffi.Set("do_pop", new NativeCallable(GameEventInterface.SpawnPop, 5));
            ffi.Set("do_pop_left", new NativeCallable(GameEventInterface.SpawnPopLeft, 3));
            ffi.Set("do_pop_right", new NativeCallable(GameEventInterface.SpawnPopRight, 3));
            ffi.Set("set_stand_repair", new NativeCallable(GameEventInterface.SetStandRepair, 1));
            ffi.Set("add_stand_repair", new NativeCallable(GameEventInterface.AddStandRepair, 1));
            ffi.Set("add_stand_force", new NativeCallable(GameEventInterface.AddStandForce, 2));
            ffi.Set("set_crowd_mood", new NativeCallable(GameEventInterface.SetCrowdMood, 1));
            ffi.Set("add_crowd_mood", new NativeCallable(GameEventInterface.AddCrowdMood, 1));
        }

        public static object SpawnPop(List<object> args) {
            var evaledArgs = EvaluateArgs<string, float, float, float, float>("do_pop", args);

            if(!PopManager.Instance.HasPop(evaledArgs.Item1)) {
                LogError(evaledArgs.Item1 + " pop does not exist");
            }
            PopManager.Instance.DoPop(evaledArgs.Item1, evaledArgs.Item2, evaledArgs.Item3, evaledArgs.Item4, evaledArgs.Item5);

            return null;
        }

        public static object SpawnPopLeft(List<object> args) {
            var evaledArgs = EvaluateArgs<string, float, float>("do_pop_left", args);

            if(!PopManager.Instance.HasPop(evaledArgs.Item1)) {
                LogError(evaledArgs.Item1 + " pop does not exist");
            }
            PopManager.Instance.DoPopSide(evaledArgs.Item1, evaledArgs.Item2, evaledArgs.Item3, false);

            return null;
        }

        public static object SpawnPopRight(List<object> args) {
            var evaledArgs = EvaluateArgs<string, float, float>("do_pop_right", args);

            if(!PopManager.Instance.HasPop(evaledArgs.Item1)) {
                LogError(evaledArgs.Item1 + " pop does not exist");
            }
            PopManager.Instance.DoPopSide(evaledArgs.Item1, evaledArgs.Item2, evaledArgs.Item3, true);

            return null;
        }

        public static object SetStandRepair(List<object> args) {
            var evaledArgs = EvaluateArgs<float>("set_stand_repair", args);

            SimulatorManager.Instance.currentSim.SetRepair(evaledArgs);

            return null;
        }

        public static object AddStandRepair(List<object> args) {
            var evaledArgs = EvaluateArgs<float>("add_stand_repair", args);

            SimulatorManager.Instance.currentSim.Repair(evaledArgs);

            return null;
        }

        public static object AddStandForce(List<object> args) {
            var resolved = EvaluateArgs<float, float>("add_stand_force", args);

            SimulatorManager.Instance.currentSim.ApplySmoothedImpulse(resolved.Item1, resolved.Item2);

            return null;
        }

        public static object SetCrowdMood(List<object> args) {
            var resolved = EvaluateArgs<float>("set_crowd_mood", args);

            BeatmapController.Instance.crowd.mood = 1 - Mathf.Clamp01(resolved);

            return null;
        }

        public static object AddCrowdMood(List<object> args) {
            var resolved = EvaluateArgs<float>("add_crowd_mood", args);

            var newMood = BeatmapController.Instance.crowd.mood + (1 - resolved);
            BeatmapController.Instance.crowd.mood = Mathf.Clamp01(newMood);

            return null;
        }
        
        #region argumentresolvers
        private static T EvaluateArgs<T>(string func, List<object> args) {
            try {
                return (T)Convert.ChangeType(args[0], typeof(T));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[0].GetType() + ", requires type " + typeof(T));
                return default;
            }
        }

        private static (T1, T2) EvaluateArgs<T1, T2>(string func, List<object> args) {
            T1 final1;
            T2 final2;

            try {
                final1 = (T1)Convert.ChangeType(args[0], typeof(T1));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[0].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final2 = (T2)Convert.ChangeType(args[1], typeof(T2));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[1].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            return (final1, final2);
        }

        private static (T1, T2, T3) EvaluateArgs<T1, T2, T3>(string func, List<object> args) {
            T1 final1;
            T2 final2;
            T3 final3;

            try {
                final1 = (T1)Convert.ChangeType(args[0], typeof(T1));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[0].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final2 = (T2)Convert.ChangeType(args[1], typeof(T2));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[1].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final3 = (T3)Convert.ChangeType(args[2], typeof(T3));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[2].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            return (final1, final2, final3);
        }

        private static (T1, T2, T3, T4) EvaluateArgs<T1, T2, T3, T4>(string func, List<object> args) {
            T1 final1;
            T2 final2;
            T3 final3;
            T4 final4;

            try {
                final1 = (T1)Convert.ChangeType(args[0], typeof(T1));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[0].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final2 = (T2)Convert.ChangeType(args[1], typeof(T2));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[1].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final3 = (T3)Convert.ChangeType(args[2], typeof(T3));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[2].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final4 = (T4)Convert.ChangeType(args[3], typeof(T4));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[3].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            return (final1, final2, final3, final4);
        }

        private static (T1, T2, T3, T4, T5) EvaluateArgs<T1, T2, T3, T4, T5>(string func, List<object> args) {
            T1 final1;
            T2 final2;
            T3 final3;
            T4 final4;
            T5 final5;

            try {
                final1 = (T1)Convert.ChangeType(args[0], typeof(T1));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[0].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final2 = (T2)Convert.ChangeType(args[1], typeof(T2));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[1].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final3 = (T3)Convert.ChangeType(args[2], typeof(T3));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[2].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final4 = (T4)Convert.ChangeType(args[3], typeof(T4));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[3].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final5 = (T5)Convert.ChangeType(args[4], typeof(T5));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[4].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            return (final1, final2, final3, final4, final5);
        }

        private static (T1, T2, T3, T4, T5, T6) EvaluateArgs<T1, T2, T3, T4, T5, T6>(string func, List<object> args) {
            T1 final1;
            T2 final2;
            T3 final3;
            T4 final4;
            T5 final5;
            T6 final6;

            try {
                final1 = (T1)Convert.ChangeType(args[0], typeof(T1));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[0].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final2 = (T2)Convert.ChangeType(args[1], typeof(T2));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[1].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final3 = (T3)Convert.ChangeType(args[2], typeof(T3));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[2].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final4 = (T4)Convert.ChangeType(args[3], typeof(T4));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[3].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final5 = (T5)Convert.ChangeType(args[4], typeof(T5));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[4].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            try {
                final6 = (T6)Convert.ChangeType(args[5], typeof(T6));
            } catch(Exception) {
                LogError(func + " parameter 1 is of type " + args[5].GetType() + ", requires type " + typeof(T1));
                return default;
            }

            return (final1, final2, final3, final4, final5, final6);
        }
        #endregion
        private static void LogError(string s) {
            Debug.LogError(s);
        }

        private static string Stringify(object val) {
            if(val == null) {
                return "none";
            }

            return val.ToString();
        }

    }

}