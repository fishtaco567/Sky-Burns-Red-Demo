using System;
using System.Text;

namespace Pica {

    public class AstDisplay : ExprVisitor<string>, StmtVisitor<string> {
        public string VisitBinary(BinaryExpr expr) {
            return AddParens(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitLogical(LogicalExpr expr) {
            return AddParens(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitGrouping(GroupingExpr expr) {
            return AddParens("group", expr.expr);
        }

        public string VisitLiteral(LiteralExpr expr) {
            if(expr.val == null) {
                return "NULL";
            }

            return expr.val.ToString();
        }

        public string VisitUnary(UnaryExpr expr) {
            return AddParens(expr.op.lexeme, expr.right);
        }

        public string VisitVariable(VariableExpr expr) {
            return expr.ident.lexeme;
        }

        public string VisitAssign(AssignmentExpr expr) {
            return AddParens(expr.ident.lexeme, expr.val);
        }

        public string VisitBlock(BlockStmt expr) {
            var indentSB = new StringBuilder();
            for(int i = 0; i < expr.indentLevel; i++) {
                indentSB.Append('\t');
            }
            var indent = indentSB.ToString();
            var sb = new StringBuilder();

            foreach(var stmt in expr.statements) {
                sb.Append(indent);
                sb.Append(stmt.Accept(this));
                sb.Append('\n');
            }

            return AddParens(sb.ToString(0, sb.Length - 1));
        }

        public string VisitIf(IfStmt stmt) {
            var sb = new StringBuilder();

            sb.Append("(If ");
            sb.Append(stmt.condition.Accept(this));
            sb.Append(" (");
            sb.Append(stmt.thenBranch.Accept(this));
            sb.Append(")");

            if(stmt.elseBranch != null) {
                sb.Append(" Else (");
                sb.Append(stmt.elseBranch.Accept(this));
                sb.Append(")");
            }

            sb.Append(")");

            return sb.ToString();
        }

        public string VisitWhile(WhileStmt stmt) {
            var sb = new StringBuilder();

            sb.Append("(While ");
            sb.Append(stmt.condition.Accept(this));
            sb.Append(" (");
            sb.Append(stmt.loop.Accept(this));
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitFor(ForStmt stmt) {
            var sb = new StringBuilder();

            sb.Append("(For ");
            sb.Append(AddParens(stmt.inclusive ? "..=" : "..", stmt.lower, stmt.upper));
            sb.Append(" (");
            sb.Append(stmt.loop.Accept(this));
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitCall(CallExpr expr) {
            var sb = new StringBuilder();

            sb.Append("(Call ");
            sb.Append(expr.callee.Accept(this));
            sb.Append(" ");
            sb.Append(AddParens("", expr.args.ToArray()));
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitPostfix(PostfixExpr expr) {
            var sb = new StringBuilder();

            sb.Append("(");
            sb.Append(expr.ident.lexeme);
            sb.Append(" ");
            sb.Append(expr.op.lexeme);
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitExpression(ExpressionStmt stmt) {
            var sb = new StringBuilder();
            sb.Append("=> ");
            sb.Append(stmt.expr.Accept(this));

            return sb.ToString();
        }

        public string VisitFunc(FuncStmt stmt) {
            var sb = new StringBuilder();
            sb.Append("Func ");
            sb.Append(stmt.name.lexeme);
            sb.Append(" (");
            foreach(var param in stmt.parameters) {
                sb.Append(param.lexeme);
            }

            sb.Append(") ");
            sb.Append(stmt.body.Accept(this));
            if(stmt.final != null) {
                sb.Append(stmt.final.Accept(this));
            }

            return sb.ToString();
        }

        public string VisitReturn(ReturnStmt stmt) {
            return "Return " + stmt.val.Accept(this);
        }

        public string VisitBreak(BreakStmt stmt) {
            return "Break";
        }

        public string VisitContinue(ContinueStmt stmt) {
            return "Continue";
        }

        public string VisitListLiteral(ListLiteralExpr expr) {
            var sb = new StringBuilder();
            sb.Append("[");
            for(int i = 0; i < expr.elements.Length; i++) {
                sb.Append(expr.elements[i].Accept(this));
                if(i != expr.elements.Length - 1) {
                    sb.Append(",");
                }
            }

            sb.Append("]");
            return sb.ToString();
        }

        public string VisitListGet(ListGetExpr expr) {
            var sb = new StringBuilder();

            sb.Append(expr.left.Accept(this));
            sb.Append("[");
            sb.Append(expr.index.Accept(this));
            sb.Append("]");

            return sb.ToString();
        }

        public string VisitListSet(ListSetExpr expr) {
            var sb = new StringBuilder();

            sb.Append("(= ");
            sb.Append(expr.left.Accept(this));
            sb.Append("[");
            sb.Append(expr.index.Accept(this));
            sb.Append("] ");
            sb.Append(expr.right.Accept(this));
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitTableLiteral(TableLiteralExpr expr) {
            var sb = new StringBuilder();

            sb.Append("{");
            for(int i = 0; i < expr.items.Count; i++) {
                sb.Append(expr.ident[i].lexeme);
                sb.Append(": ");
                sb.Append(expr.items[i].Accept(this));

                if(i != expr.items.Count - 1) {
                    sb.Append(",");
                }
            }
            sb.Append("}");

            return sb.ToString();
        }

        public string VisitTableGet(TableGetExpr expr) {
            var sb = new StringBuilder();

            sb.Append(expr.left.Accept(this));
            sb.Append(".");
            sb.Append(expr.ident.lexeme);

            return sb.ToString();
        }

        public string VisitTableSet(TableSetExpr expr) {
            var sb = new StringBuilder();

            sb.Append("(= ");
            sb.Append(expr.left.Accept(this));
            sb.Append(".");
            sb.Append(expr.ident.lexeme);
            sb.Append(" ");
            sb.Append(expr.right.Accept(this));
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitWait(WaitStmt stmt) {
            return "wait " + stmt.time.Accept(this);
        }

        public string VisitWaitUntil(WaitUntilStmt stmt) {
            return "wait until " + stmt.condition.Accept(this);
        }

        public string AddParens(string name, params Expr[] exprs) {
            var sb = new StringBuilder();

            sb.Append("(").Append(name);

            foreach(var expr in exprs) {
                sb.Append(" ");
                sb.Append(expr.Accept(this));
            }

            sb.Append(")");

            return sb.ToString();
        }

    }

}