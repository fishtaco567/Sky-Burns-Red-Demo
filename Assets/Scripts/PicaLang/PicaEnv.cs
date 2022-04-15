using System.Collections.Generic;

namespace Pica {

    public class PicaEnv {

        private Dictionary<string, object> vals;

        private PicaEnv parent;

        public PicaEnv() {
            vals = new Dictionary<string, object>();
            parent = null;
        }

        public PicaEnv(PicaEnv parent) {
            this.parent = parent;
            this.vals = new Dictionary<string, object>();
        }

        public Dictionary<string, object> GetVals() {
            return vals;
        }

        public object Get(Token ident) {
            if(vals.TryGetValue(ident.lexeme, out var val)) {
                return val;
            }

            if(parent != null) {
                return parent.Get(ident);
            }

            throw new RuntimeError(ident, "Name \'" + ident.lexeme + "\' is not defined");
        }

        public object Set(Token ident, object val) {
            return vals[ident.lexeme] = val;
        }

        public object Set(string ident, object val) {
            return vals[ident] = val;
        }

    }

}