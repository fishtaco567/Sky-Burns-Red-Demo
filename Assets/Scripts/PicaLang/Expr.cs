using System.Collections.Generic;

namespace Pica {

    public interface Expr {

        T Accept<T>(ExprVisitor<T> visitor);

    }

    public interface ExprVisitor<T> {
        T VisitBinary(BinaryExpr expr);
        T VisitLogical(LogicalExpr expr);
        T VisitUnary(UnaryExpr expr);
        T VisitGrouping(GroupingExpr expr);
        T VisitLiteral(LiteralExpr expr);
        T VisitVariable(VariableExpr expr);
        T VisitAssign(AssignmentExpr expr);
        T VisitCall(CallExpr expr);
        T VisitPostfix(PostfixExpr expr);
        T VisitListLiteral(ListLiteralExpr expr);
        T VisitListGet(ListGetExpr expr);
        T VisitListSet(ListSetExpr expr);
        T VisitTableLiteral(TableLiteralExpr expr);
        T VisitTableGet(TableGetExpr expr);
        T VisitTableSet(TableSetExpr expr);
    }

    public struct BinaryExpr : Expr {

        public Expr left;
        public Token op;
        public Expr right;

        public BinaryExpr(Expr left, Token op, Expr right) {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitBinary(this);
        }

    }

    public struct LogicalExpr : Expr {

        public Expr left;
        public Token op;
        public Expr right;

        public LogicalExpr(Expr left, Token op, Expr right) {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitLogical(this);
        }
    }

    public struct UnaryExpr : Expr {

        public Token op;
        public Expr right;

        public UnaryExpr(Token op, Expr right) {
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitUnary(this);
        }

    }

    public struct LiteralExpr : Expr {

        public object val;

        public LiteralExpr(object val) {
            this.val = val;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitLiteral(this);
        }

    }

    public struct GroupingExpr : Expr {

        public Expr expr;

        public GroupingExpr(Expr expr) {
            this.expr = expr;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitGrouping(this);
        }

    }

    public struct VariableExpr : Expr {

        public Token ident;

        public VariableExpr(Token ident) {
            this.ident = ident;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitVariable(this);
        }

    }

    public struct AssignmentExpr : Expr {

        public Token ident;
        public Expr val;

        public AssignmentExpr(Token ident, Expr val) {
            this.ident = ident;
            this.val = val;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitAssign(this);
        }

    }

    public struct CallExpr : Expr {

        public Token right;
        public Expr callee;
        public List<Expr> args;

        public CallExpr(Token right, Expr callee, List<Expr> args) {
            this.right = right;
            this.callee = callee;
            this.args = args;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitCall(this);
        }

    }

    public struct PostfixExpr : Expr {

        public Token ident;
        public Token op;

        public PostfixExpr(Token ident, Token op) {
            this.ident = ident;
            this.op = op;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitPostfix(this);
        }

    }

    public struct ListLiteralExpr : Expr {

        public Token open;
        public Expr[] elements;

        public ListLiteralExpr(Token open, Expr[] elements) {
            this.open = open;
            this.elements = elements;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitListLiteral(this);
        }

    }

    public struct ListGetExpr : Expr {

        public Token token;
        public Expr left;
        public Expr index;

        public ListGetExpr(Token token, Expr left, Expr index) {
            this.token = token;
            this.left = left;
            this.index = index;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitListGet(this);
        }

    }

    public struct ListSetExpr : Expr {

        public Token token;
        public Expr left;
        public Expr index;
        public Expr right;

        public ListSetExpr(Token token, Expr left, Expr index, Expr right) {
            this.token = token;
            this.left = left;
            this.index = index;
            this.right = right;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitListSet(this);
        }

    }

    public struct TableLiteralExpr : Expr {

        public Token token;
        public List<Token> ident;
        public List<Expr> items;

        public TableLiteralExpr(Token token, List<Token> ident, List<Expr> items) {
            this.token = token;
            this.ident = ident;
            this.items = items;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitTableLiteral(this);
        }

    }

    public struct TableGetExpr : Expr {

        public Expr left;
        public Token ident;

        public TableGetExpr(Expr left, Token ident) {
            this.left = left;
            this.ident = ident;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitTableGet(this);
        }

    }

    public struct TableSetExpr : Expr {

        public Expr left;
        public Token ident;
        public Expr right;

        public TableSetExpr(Expr left, Token ident, Expr right) {
            this.left = left;
            this.ident = ident;
            this.right = right;
        }

        public T Accept<T>(ExprVisitor<T> visitor) {
            return visitor.VisitTableSet(this);
        }

    }

}