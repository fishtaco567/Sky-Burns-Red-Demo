namespace Pica {

    public struct PicaStmtResult {

        public PicaStmtResultType type;
        public double seconds;
        public Expr condition;
        public bool stepUsed;

        public PicaStmtResult(PicaStmtResultType type) {
            this.type = type;
            seconds = 0;
            condition = null;
            stepUsed = false;
        }

        public PicaStmtResult(double seconds) {
            type = PicaStmtResultType.WAIT;
            this.seconds = seconds;
            condition = null;
            stepUsed = false;
        }

        public PicaStmtResult(Expr condition) {
            type = PicaStmtResultType.WAIT_UNTIL;
            seconds = 0;
            this.condition = condition;
            stepUsed = false;
        }

    }

    public enum PicaStmtResultType {
        OK = 0,
        ERR,
        WAIT,
        WAIT_UNTIL,
    }
    
}