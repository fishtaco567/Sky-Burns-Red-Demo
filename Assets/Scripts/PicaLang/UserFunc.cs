using System.Collections.Generic;

namespace Pica {

    public class UserFunc : Callable {

        private FuncStmt func;

        public UserFunc(FuncStmt func) {
            this.func = func;
        }

        public object Call(PicaInterpreter interpreter, List<object> args) {
            PicaEnv funcEnv = new PicaEnv(interpreter.currentEnv);

            for(int i = 0; i < args.Count; i++) {
                funcEnv.Set(func.parameters[i], args[i]);
            }

            var lastEnv = interpreter.currentEnv;

            object ret = null;
            try {
                interpreter.currentEnv = funcEnv;
                interpreter.Execute(func.body);

                if(func.final != null) {
                    ret = interpreter.Evaluate(func.final);
                }
            } catch(Return r) {
                ret = r.val;
            }

            interpreter.currentEnv = lastEnv;

            return ret;
        }

        public int Arity() {
            return func.parameters.Count;
        }

    }

    public class Return : System.Exception {

        public object val;

        public Return(object val) {
            this.val = val;
        }

    }

}