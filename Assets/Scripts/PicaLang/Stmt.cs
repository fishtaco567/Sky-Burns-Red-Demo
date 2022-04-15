using System.Collections.Generic;

namespace Pica {

    public interface Stmt {

        T Accept<T>(StmtVisitor<T> visitor);

    }

    public interface StmtVisitor<T> {

        T VisitExpression(ExpressionStmt stmt);
        T VisitFunc(FuncStmt stmt);
        T VisitReturn(ReturnStmt stmt);
        T VisitBreak(BreakStmt stmt);
        T VisitContinue(ContinueStmt stmt);
        T VisitIf(IfStmt stmt);
        T VisitWhile(WhileStmt stmt);
        T VisitFor(ForStmt stmt);
        T VisitBlock(BlockStmt stmt);
        T VisitWait(WaitStmt stmt);
        T VisitWaitUntil(WaitUntilStmt stmt);
    }

    public struct ExpressionStmt : Stmt {

        public Expr expr;

        public ExpressionStmt(Expr expr) {
            this.expr = expr;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitExpression(this);
        }

    }

    public struct BlockStmt : Stmt {

        public int indentLevel;
        public List<Stmt> statements;

        public BlockStmt(int indentLevel, List<Stmt> statements) {
            this.indentLevel = indentLevel;
            this.statements = statements;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitBlock(this);
        }

    }

    public struct FuncStmt : Stmt {

        public Token name;
        public List<Token> parameters;
        public Stmt body;
        public Expr final;

        public FuncStmt(Token name, List<Token> parameters, Stmt body, Expr final) {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
            this.final = final;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitFunc(this);
        }

    }

    public struct ReturnStmt : Stmt {

        public Token word;
        public Expr val;

        public ReturnStmt(Token word, Expr val) {
            this.word = word;
            this.val = val;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitReturn(this);
        }

    }

    public struct BreakStmt : Stmt {

        public Token word;

        public BreakStmt(Token word) {
            this.word = word;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitBreak(this);
        }

    }

    public struct ContinueStmt : Stmt {

        public Token word;

        public ContinueStmt(Token word) {
            this.word = word;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitContinue(this);
        }

    }

    public struct IfStmt : Stmt {

        public Expr condition;
        public Stmt thenBranch;
        public Stmt elseBranch;

        public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch) {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitIf(this);
        }

    }

    public struct WhileStmt : Stmt {

        public Expr condition;
        public Stmt loop;

        public WhileStmt(Expr condition, Stmt loop) {
            this.condition = condition;
            this.loop = loop;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitWhile(this);
        }

    }

    public struct ForStmt : Stmt {

        public Token ident;
        public Expr lower;
        public Expr upper;
        public Stmt loop;

        public bool inclusive;

        public ForStmt(Token ident, Expr lower, Expr upper, Stmt loop, bool inclusive) {
            this.ident = ident;
            this.lower = lower;
            this.upper = upper;
            this.loop = loop;
            this.inclusive = inclusive;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitFor(this);
        }

    }

    public struct WaitStmt : Stmt {

        public Token word;
        public Expr time;

        public WaitStmt(Token word, Expr time) {
            this.word = word;
            this.time = time;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitWait(this);
        }

    }

    public struct WaitUntilStmt : Stmt {

        public Token word;
        public Expr condition;

        public WaitUntilStmt(Token word, Expr condition) {
            this.word = word;
            this.condition = condition;
        }

        public T Accept<T>(StmtVisitor<T> visitor) {
            return visitor.VisitWaitUntil(this);
        }

    }

}