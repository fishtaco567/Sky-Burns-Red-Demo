using System.Collections.Generic;
using System;

namespace Pica {

    public class NativeCallable : Callable {

        public Func<List<object>, object> Ca;
        public int arity;

        public NativeCallable(Func<List<object>, object> Ca, int arity) {
            this.Ca = Ca;
            this.arity = arity;
        }

        public object Call(PicaInterpreter interp, List<object> args) {
            return Ca(args);
        }

        public int Arity() {
            return arity;
        }

        public override string ToString() {
            return "<native fn>";
        }

    }

}