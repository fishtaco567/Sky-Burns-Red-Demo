using UnityEngine;
using System.Collections.Generic;
using System;
using Utils;

namespace Pica {

    public static class BuiltInFunctions {

        static SRandom rand;

        public static void Setup(PicaEnv ffi) {
            rand = new SRandom((uint)DateTime.Now.Ticks);

            ffi.Set("clock", new NativeCallable((b) => DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond, 0));
            ffi.Set("print", new NativeCallable((b) => { Debug.Log(Stringify(b[0])); return null; }, 1));
            ffi.Set("random", new NativeCallable(Random, 2));
            ffi.Set("sin", new NativeCallable(Sin, 1));
            ffi.Set("cos", new NativeCallable(Cos, 1));
            ffi.Set("tan", new NativeCallable(Tan, 1));
            ffi.Set("asin", new NativeCallable(ASin, 1));
            ffi.Set("acos", new NativeCallable(ACos, 1));
            ffi.Set("atan", new NativeCallable(ATan, 1));
            ffi.Set("ceil", new NativeCallable(Ceil, 1));
            ffi.Set("floor", new NativeCallable(Floor, 1));
            ffi.Set("frac", new NativeCallable(Fract, 1));
            ffi.Set("lerp", new NativeCallable(Lerp, 3));
            ffi.Set("clamp", new NativeCallable(Clamp, 3));
            ffi.Set("len", new NativeCallable(LenList, 1));
            ffi.Set("push", new NativeCallable(Push, 2));
            ffi.Set("pop", new NativeCallable(Pop, 1));
            ffi.Set("remove_at", new NativeCallable(RemoveAt, 2));
            ffi.Set("insert_at", new NativeCallable(Insert, 3));
        }

        public static object Random(List<object> args) {
            var (lower, upper) = EvaluateArgs<float, float>("random", args);

            return (double)rand.RandomFloatInRange(lower, upper);
        }

        public static object Sin(List<object> args) {
            var x = EvaluateArgs<double>("sin", args);
            return Math.Sin(x);
        }

        public static object Cos(List<object> args) {
            var x = EvaluateArgs<double>("cos", args);
            return Math.Cos(x);
        }

        public static object Tan(List<object> args) {
            var x = EvaluateArgs<double>("tan", args);
            return Math.Tan(x);
        }

        public static object ASin(List<object> args) {
            var x = EvaluateArgs<double>("asin", args);
            return Math.Asin(x);
        }

        public static object ACos(List<object> args) {
            var x = EvaluateArgs<double>("acos", args);
            return Math.Acos(x);
        }

        public static object ATan(List<object> args) {
            var x = EvaluateArgs<double>("atan", args);
            return Math.Atan(x);
        }

        public static object Floor(List<object> args) {
            var x = EvaluateArgs<double>("floor", args);
            return Math.Floor(x);
        }

        public static object Ceil(List<object> args) {
            var x = EvaluateArgs<double>("ceil", args);
            return Math.Ceiling(x);
        }

        public static object Fract(List<object> args) {
            var x = EvaluateArgs<double>("fract", args);
            return x - (int)x;
        }

        public static object Lerp(List<object> args) {
            var (t, x1, x2) = EvaluateArgs<double, double, double>("lerp", args);
            return x1 + (x2 - x1) * t;
        }

        public static object Clamp(List<object> args) {
            var (t, x1, x2) = EvaluateArgs<double, double, double>("lerp", args);
            return Math.Min(Math.Max(x1, t), x2);
        }

        public static object LenList(List<object> args) {
            var list = EvaluateArgs<List<object>>("len", args);
            return (double)list.Count;
        }

        public static object Push(List<object> args) {
            var (list, o) = EvaluateArgs<List<object>, object>("push", args);
            list.Add(o);
            return list;
        }

        public static object Pop(List<object> args) {
            var list = EvaluateArgs<List<object>>("pop", args);
            var thing = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return thing;
        }

        public static object RemoveAt(List<object> args) {
            var (list, i) = EvaluateArgs<List<object>, int>("remove_at", args);
            var thing = list[i];
            list.RemoveAt(i);
            return thing;
        }

        public static object Insert(List<object> args) {
            var (list, o, i) = EvaluateArgs<List<object>, object, int>("insert_at", args);
            list.Insert(i, o);
            return list;
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