using static Pica.TokenType;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pica {

    public class PicaInterpreter : ExprVisitor<object>, StmtVisitor<PicaStmtResult> {

        public PicaEnv globals;
        public PicaEnv currentEnv;

        public Stack<object> restoreStack;
        private bool restoring;
        private PicaStmtResult wait;

        private float waitGoal;

        private List<Stmt> statements;

        public PicaInterpreter(List<Stmt> statements, PicaEnv ffi) {
            globals = new PicaEnv(ffi);
            globals.Set("global", globals.GetVals());
            currentEnv = globals;

            this.statements = statements;

            restoreStack = new Stack<object>();
            restoring = false;
        }

        public bool Run() {
            int i = 0;
            
            if(restoring) {
                if(wait.type == PicaStmtResultType.WAIT) {
                    if(Time.time < waitGoal) {
                        return false;
                    }
                } else {
                    if(!IsTruthy(Evaluate(wait.condition))) {
                        return false;
                    }
                }

                i = (int)PopRestore();
            }

            try {
                for(; i < statements.Count; i++) {
                    wait = Execute(statements[i]);
                    if(wait.type == PicaStmtResultType.WAIT || wait.type == PicaStmtResultType.WAIT_UNTIL) {
                        restoring = true;
                        if(wait.type == PicaStmtResultType.WAIT) {
                            waitGoal = Time.time + (float)wait.seconds;
                        }

                        if(!wait.stepUsed) {
                            restoreStack.Push(i + 1);
                            wait.stepUsed = true;
                        } else {
                            restoreStack.Push(i);
                        }
                        return false;
                    }
                }
            } catch(RuntimeError e) {
                PicaError.Error(e.message, e.op.line);
            }

            return true;
        }

        public PicaStmtResult Execute(Stmt stmt) {
            return stmt.Accept(this);
        }

        public object VisitBinary(BinaryExpr expr) {
            var left = Evaluate(expr.left);
            var right = Evaluate(expr.right);

            switch(expr.op.type) {
                case MINUS:
                    if(left is List<object>) {
                        var n = DeepCopyList((List <object>) left, new Dictionary<object, object>());
                        n.Remove(right);
                        return n;
                    }

                    CheckNumericOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case PLUS: {
                    if(left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if(left is string || right is string) {
                        return Stringify(left) + Stringify(right);
                    }

                    if(left is List<object> && right is List<object>) {
                        var le = (List<object>)left;
                        var n = DeepCopyList(le, new Dictionary<object, object>());
                        var ri = (List<object>)right;
                        n.AddRange(DeepCopyList(ri, new Dictionary<object, object>()));
                        return n;
                    }

                    if(left is List<object>) {
                        var le = (List<object>)left;
                        var n = DeepCopyList(le, new Dictionary<object, object>());
                        n.Add(DeepCopyObject(right));
                        return n;
                    }

                    if(right is List<object>) {
                        var n = new List<object>();
                        var ri = (List<object>) right;
                        n.Insert(0, DeepCopyObject(left));
                        n.AddRange(DeepCopyList(ri, new Dictionary<object, object>()));
                        return n;
                    }

                    throw new RuntimeError(expr.op, "Must have numeric or string operands");
                }
                case STAR:
                    CheckNumericOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case SLASH:
                    CheckNumericOperands(expr.op, left, right);
                    var r = (double)right;
                    if(r == 0) {
                        throw new RuntimeError(expr.op, "Cannot divide by zero");
                    }
                    return (double)left / (double)right;
                case NOT_EQUAL:
                    return !left.Equals(right);
                case EQUAL_EQUAL:
                    return left.Equals(right);
                case LESS:
                    CheckNumericOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckNumericOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case GREATER:
                    CheckNumericOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckNumericOperands(expr.op, left, right);
                    return (double)left >= (double)right;
            }

            return null;
        }

        public object VisitLogical(LogicalExpr expr) {
            var left = Evaluate(expr.left);

            if(expr.op.type == OR) {
                if(IsTruthy(left)) {
                    return true;
                }
            } else {
                if(!IsTruthy(left)) {
                    return false;
                }
            }

            return IsTruthy(Evaluate(expr.right));
        }

        public object VisitGrouping(GroupingExpr expr) {
            return Evaluate(expr.expr);
        }

        public object VisitLiteral(LiteralExpr expr) {
            return expr.val;
        }

        public object VisitUnary(UnaryExpr expr) {
            var right = Evaluate(expr.right);

            switch(expr.op.type) {
                case MINUS:
                    CheckNumericOperand(expr.op, right);
                    return -(double)right;
                case NOT:
                    return !IsTruthy(right);
            }

            return null;
        }

        public object VisitVariable(VariableExpr expr) {
            return currentEnv.Get(expr.ident);
        }

        public object VisitAssign(AssignmentExpr expr) {
            return currentEnv.Set(expr.ident, DeepCopyObject(Evaluate(expr.val)));
        }

        public PicaStmtResult VisitBlock(BlockStmt expr) {
            var r = new PicaStmtResult();
            var i = 0;

            if(restoring) {
                i = (int)PopRestore();
            }

            for(; i < expr.statements.Count; i++) {
                r = Execute(expr.statements[i]);
                if(r.type == PicaStmtResultType.WAIT || r.type == PicaStmtResultType.WAIT_UNTIL) {
                    if(!r.stepUsed) {
                        restoreStack.Push(i + 1);
                        r.stepUsed = true;
                    } else {
                        restoreStack.Push(i);
                    }
                    return r;
                }
            }

            return r;
        }

        public PicaStmtResult VisitIf(IfStmt expr) {
            var r = new PicaStmtResult();

            var decision = true;
            if(restoring) {
                decision = (bool)PopRestore();
            } else {
                decision = IsTruthy(Evaluate(expr.condition));
            }

            if(decision) {
                r = Execute(expr.thenBranch);
                if(r.type == PicaStmtResultType.WAIT || r.type == PicaStmtResultType.WAIT_UNTIL) {
                    restoreStack.Push(true);
                }

            } else if(expr.elseBranch != null) {
                r = Execute(expr.elseBranch);
                if(r.type == PicaStmtResultType.WAIT || r.type == PicaStmtResultType.WAIT_UNTIL) {
                    restoreStack.Push(false);
                }
            }

            return r;
        }

        public PicaStmtResult VisitWhile(WhileStmt expr) {
            var r = new PicaStmtResult();

            var decision = true;
            if(!restoring) {
                IsTruthy(Evaluate(expr.condition));
            }

            while(decision) {
                r = Execute(expr.loop);
                if(r.type == PicaStmtResultType.WAIT || r.type == PicaStmtResultType.WAIT_UNTIL) {
                    return r;
                }
                decision = IsTruthy(Evaluate(expr.condition));
            }

            return r;
        }

        public PicaStmtResult VisitFor(ForStmt expr) {
            var r = new PicaStmtResult();

            var lower = Evaluate(expr.lower);
            var upper = Evaluate(expr.upper);

            CheckNumericOperands(expr.ident, lower, upper);

            var lowerNum = (double)lower;
            var upperNum = (double)upper;

            var i = lowerNum;
            if(restoring) {
                i = (double)PopRestore();
            }

            currentEnv.Set(expr.ident, lowerNum);

            if(expr.inclusive) {
                for(; i <= upperNum; i++) {
                    currentEnv.Set(expr.ident, i);

                    try {
                        r = Execute(expr.loop);
                        if(r.type == PicaStmtResultType.WAIT || r.type == PicaStmtResultType.WAIT_UNTIL) {
                            restoreStack.Push(i);
                            return r;
                        }
                    } catch(Break) {
                        return new PicaStmtResult();
                    } catch(Continue) {
                    }
                }
            } else {
                for(; i < upperNum; i++) {
                    currentEnv.Set(expr.ident, i);

                    try {
                        r = Execute(expr.loop);
                        if(r.type == PicaStmtResultType.WAIT || r.type == PicaStmtResultType.WAIT_UNTIL) {
                            restoreStack.Push(i);
                            return r;
                        }
                    } catch(Break) {
                        return new PicaStmtResult();
                    } catch(Continue) {
                    }
                }
            }

            return r;
        }

        public object VisitCall(CallExpr expr) {
            object callee = Evaluate(expr.callee);

            var args = new List<object>();
            for(int i = 0; i < expr.args.Count; i++) {
                args.Add(DeepCopyObject(Evaluate(expr.args[i])));
            }

            var func = callee as Callable;

            if(func != null) {
                if(func.Arity() != args.Count) {
                    throw new RuntimeError(expr.right, "Expected " + func.Arity() + " arguments but found " + args.Count);
                }

                var r = func.Call(this, args);
                return DeepCopyObject(r);
            }

            throw new RuntimeError(expr.right, "Can only call functions");
        }

        public object VisitPostfix(PostfixExpr expr) {
            var pre = currentEnv.Get(expr.ident);

            var preQ = pre as double?;
            if(!preQ.HasValue) {
                throw new RuntimeError(expr.op, "Can only use " + expr.op + " on numeric variables. " + expr.ident + " is not numeric");
            }

            var added = preQ.Value;
            if(expr.op.type == PLUS_PLUS) {
                added += 1;
            } else {
                added -= 1;
            }

            currentEnv.Set(expr.ident, added);

            return pre;
        }

        public object VisitListLiteral(ListLiteralExpr expr) {
            var list = new List<object>(expr.elements.Length);
            
            for(int i = 0; i < expr.elements.Length; i++) {
                list.Add(Evaluate(expr.elements[i]));
            }

            return list;
        }

        public object VisitListGet(ListGetExpr expr) {
            var left = Evaluate(expr.left);

            var list = left as List<object>;
            if(left == null) {
                throw new RuntimeError(expr.token, "Can only index lists");
            }

            var index = Evaluate(expr.index);

            var indNumber = index as double?;

            if(!indNumber.HasValue) {
                throw new RuntimeError(expr.token, "Can only index lists with numbers");
            }

            var indInt = (int)indNumber;

            if(indInt >= list.Count) {
                throw new RuntimeError(expr.token, "Index " + indInt + " out of range for list");
            }

            return list[indInt];
        }

        public object VisitListSet(ListSetExpr expr) {
            var left = Evaluate(expr.left);

            var list = left as List<object>;
            if(left == null) {
                throw new RuntimeError(expr.token, "Can only index lists");
            }

            var index = Evaluate(expr.index);

            var indNumber = index as double?;

            if(!indNumber.HasValue) {
                throw new RuntimeError(expr.token, "Can only index lists with numbers");
            }

            var indInt = (int)indNumber;

            if(indInt >= list.Count) {
                throw new RuntimeError(expr.token, "Index " + indInt + " out of range for list");
            }

            var right = Evaluate(expr.right);

            list[indInt] = DeepCopyObject(right);

            return right;
        }

        public object VisitTableLiteral(TableLiteralExpr expr) {
            var table = new Dictionary<string, object>();

            for(int i = 0; i < expr.items.Count; i++) {
                table.Add(expr.ident[i].lexeme, Evaluate(expr.items[i]));
            }

            return table;
        }

        public object VisitTableGet(TableGetExpr expr) {
            var left = Evaluate(expr.left);

            var asTable = left as Dictionary<string, object>;

            if(asTable == null) {
                throw new RuntimeError(expr.ident, "Can only access tables with '.' operators");
            }

            if(asTable.TryGetValue(expr.ident.lexeme, out var res)) {
                return res;
            } else {
                throw new RuntimeError(expr.ident, "Item " + expr.ident.lexeme + " does not exist on table");
            }
        }

        public object VisitTableSet(TableSetExpr expr) {
            var left = Evaluate(expr.left);

            var asTable = left as Dictionary<string, object>;

            if(asTable == null) {
                throw new RuntimeError(expr.ident, "Can only access tables with '.' operators");
            }

            var right = Evaluate(expr.right);

            asTable[expr.ident.lexeme] = DeepCopyObject(right);

            return right;
        }

        public PicaStmtResult VisitExpression(ExpressionStmt stmt) {
            Evaluate(stmt.expr);

            return new PicaStmtResult();
        }

        public PicaStmtResult VisitFunc(FuncStmt stmt) {
            currentEnv.Set(stmt.name, new UserFunc(stmt));
            return new PicaStmtResult();
        }

        public PicaStmtResult VisitReturn(ReturnStmt stmt) {
            object val = null;
            if(stmt.val != null) {
                val = Evaluate(stmt.val);
            }

            return new PicaStmtResult();
        }

        public PicaStmtResult VisitBreak(BreakStmt stmt) {
            throw new Break();
        }

        public PicaStmtResult VisitContinue(ContinueStmt stmt) {
            throw new Continue();
        }

        public PicaStmtResult VisitWait(WaitStmt stmt) {
            var timeObj = Evaluate(stmt.time);

            var time = timeObj as double?;

            if(!time.HasValue) {
                throw new RuntimeError(stmt.word, "Wait must be used with a number");
            }

            return new PicaStmtResult(time.Value);
        }

        public PicaStmtResult VisitWaitUntil(WaitUntilStmt stmt) {
            var cond = Evaluate(stmt.condition);

            if(IsTruthy(cond)) {
                return new PicaStmtResult();
            } else {
                return new PicaStmtResult(stmt.condition);
            }
        }

        public object Evaluate(Expr expr) {
            return expr.Accept<object>(this);
        }

        private void CheckNumericOperands(Token op, object left, object right) {
            if(left is double && right is double) {
                return;
            }

            throw new RuntimeError(op, "Operands must be numbers");
        }

        private void CheckNumericOperand(Token op, object right) {
            if(right is double) {
                return;
            }

            throw new RuntimeError(op, "Operand must be numbers");
        }

        private bool IsTruthy(object val) {
            var b = val as bool?;

            return b.GetValueOrDefault();
        }

        private string Stringify(object val) {
            if(val == null) {
                return "none";
            }

            return val.ToString();
        }

        public object DeepCopyObject(object o) {
            if(o is double) {
                return (double)o;
            } else if(o is bool) {
                return (bool)o;
            } else if(o is string) {
                return (String.Copy((string)o));
            } else {
                return DeepCopyObject(o, new Dictionary<object, object>());
            }
        }

        public object DeepCopyObject(object o, Dictionary<object, object> seenObjects) {
            if(o is double) {
                return (double) o;
            } else if(o is bool) {
                return (bool) o;
            } else if(o is string) {
                return (String.Copy((string)o));
            } else if(o is List<object>) {
                return DeepCopyList((List<object>)o, seenObjects);
            } else if(o is Dictionary<string, object>) {
                return DeepCopyTable((Dictionary<string, object>)o, seenObjects);
            } else if(o is Callable) {
                return o;
            }

            return null;
        }

        public List<object> DeepCopyList(List<object> inn, Dictionary<object, object> seenObjects) {
            var o = new List<object>(inn.Count);

            for(int i = 0; i < inn.Count; i++) {
                var ob = inn[i];
                o.Add(DeepCopyObject(ob, seenObjects));
            }

            return o;
        }

        public Dictionary<string, object> DeepCopyTable(Dictionary<string, object> inn, Dictionary<object, object> seenObjects) {
            var o = new Dictionary<string, object>();

            foreach(var pair in inn) {
                o.Add(pair.Key, DeepCopyObject(pair.Value, seenObjects));
            }

            return o;
        }

        public object PopRestore() {
            var o = restoreStack.Pop();
            if(restoreStack.Count == 0) {
                restoring = false;
            }
            return o;
        }

        private class Break : Exception {}

        private class Continue : Exception {}

        private class Wait : Exception {

            public PicaStmtResultType type;

            public Wait(PicaStmtResultType type) {
                this.type = type;
            }

        }

    }

}