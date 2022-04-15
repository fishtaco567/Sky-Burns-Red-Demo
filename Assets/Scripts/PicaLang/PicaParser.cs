using System;
using System.Collections.Generic;
using static Pica.TokenType;

namespace Pica {

    public class PicaParser {

        private List<Token> tokens;
        private int current;

        private int lastIndentLevel;
        private int thisIndentLevel;

        private bool inFunction;
        private bool inLoop;

        public bool hadError;

        private HashSet<string> reservedNames;

        public PicaParser(List<Token> tokens) {
            this.tokens = tokens;
            current = 0;

            lastIndentLevel = 0;
            thisIndentLevel = 0;

            inFunction = false;
            inLoop = false;
            hadError = false;

            reservedNames = new HashSet<string>();
            reservedNames.Add("global");
            reservedNames.Add("clock");
            reservedNames.Add("print");
        }

        public List<Stmt> Parse() {
            var statements = new List<Stmt>();

            while(!IsDone()) {
                if(Match(NL)) {
                    continue;
                }

                try {
                    statements.Add(Declaration());
                } catch(ParseError) {
                    hadError = true;
                    Synchronize();
                }
            }

            return statements;
        }

        private Stmt Declaration() {
            if(Match(FN)) {
                return FunctionDeclaration();
            }

            return Statement();
        }

        private Stmt FunctionDeclaration() {
            var name = Consume(IDENTIFIER, "Expect function name");

            Consume(LEFT_PAREN, "Expect '(' following function name");

            var parameters = new List<Token>();
            if(!Check(RIGHT_PAREN)) {
                do {
                    if(parameters.Count >= 63) {
                        Error(Peek(), "Can't have more than 63 parameters");
                    }

                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name"));
                } while(Match(COMMA));
            }

            Consume(RIGHT_PAREN, "Expect ')' after parameters");
            Consume(NL, "Expect new line following function declaration");

            lastIndentLevel = thisIndentLevel;
            thisIndentLevel = Peek().indentLevel;

            inFunction = true;

            BlockStmt body = Block();

            Expr final = null;
            if(body.statements[body.statements.Count - 1] is ExpressionStmt) {
                final = ((ExpressionStmt)body.statements[body.statements.Count - 1]).expr;
                body.statements.RemoveAt(body.statements.Count - 1);
            }

            inFunction = false;

            return new FuncStmt(name, parameters, body, final);
        }

        private Stmt Statement() {
            lastIndentLevel = thisIndentLevel;
            thisIndentLevel = Peek().indentLevel;

            if(Match(IF)) {
                return If();
            }

            if(Match(WHILE)) {
                return While();
            }

            if(Match(FOR)) {
                return For();
            }

            if(Match(RETURN)) {
                return ReturnStmt();
            }

            if(Match(BREAK)) {
                if(!inLoop) {
                    Error(Prev(), "Can only break within loops");
                }

                return BreakStmt();
            }

            if(Match(CONTINUE)) {
                if(!inLoop) {
                    Error(Prev(), "Can only continue within loops");
                }

                var word = Prev();
                if(!IsDone()) {
                    Consume(NL, "Expect new line after continue statement");
                }
                return new ContinueStmt(word);
            }

            if(Match(WAIT)) {
                if(inFunction) {
                    Error(Prev(), "Cannot wait in functions");
                }

                if(Match(UNTIL)) {
                    return WaitUntilStmt();
                }

                return WaitStmt();
            }

            return ExpressionStatement();
        }

        private Stmt ReturnStmt() {
            if(!inFunction) {
                Error(Prev(), "Can only return from functions");
            }

            Token word = Prev();
            Expr val = null;
            if(!Check(NL) && !Check(EOF)) {
                val = Expression();
            }

            if(!IsDone()) {
                Consume(NL, "Expect new line after return statement");
            }

            return new ReturnStmt(word, val);
        }

        private Stmt BreakStmt() {
            Token word = Prev();

            if(!IsDone()) {
                Consume(NL, "Expect new line after break statement");
            }

            return new BreakStmt(word);
        }

        private Stmt WaitStmt() {
            Token word = Prev();

            if(Match(NL)) {
                return new WaitStmt(word, new LiteralExpr(0));
            }

            var expr = Expression();

            if(!IsDone()) {
                Consume(NL, "Expect new line after wait statement");
            }

            return new WaitStmt(word, expr);
        }

        private Stmt WaitUntilStmt() {
            Token word = Prev();

            var expr = Expression();

            if(!IsDone()) {
                Consume(NL, "Expect new line after wait statement");
            }

            return new WaitUntilStmt(word, expr);
        }

        private Stmt ExpressionStatement() {
            Expr expr = Expression();
            ConsumeOrEnd(NL, "Expect new line following expression statement");
            return new ExpressionStmt(expr);
        }

        private Expr Expression() {
            var expr = Assign();
            return expr;
        }

        private Expr Assign() {
            var left = Or();

            if(Match(EQUAL)) {
                Token eq = Prev();
                var right = Expression();

                if(left is VariableExpr) {
                    var ident = ((VariableExpr)left).ident;

                    if(reservedNames.Contains(ident.lexeme)) {
                        Error(ident, "Cannot assign to reserved name " + ident.lexeme);
                    }

                    return new AssignmentExpr(ident, right);
                }

                if(left is ListGetExpr) {
                    var lg = ((ListGetExpr) left);
                    var l = lg.left;
                    var i = lg.index;
                    return new ListSetExpr(lg.token, l, i, right);
                }

                if(left is TableGetExpr) {
                    var tg = ((TableGetExpr)left);
                    var l = tg.left;
                    var ident = tg.ident;
                    return new TableSetExpr(l, ident, right);
                }

                Error(eq, "Invalid assign target");
            }

            if(Match(STAR_EQUAL, SLASH_EQUAL, PLUS_EQUAL, MINUS_EQUAL)) {
                var op = Prev();
                var realOp = new Token();
                switch(op.type) {
                    case STAR_EQUAL:
                        realOp = new Token(STAR, op.indentLevel, op.line, op.lexeme);
                        break;
                    case SLASH_EQUAL:
                        realOp = new Token(SLASH, op.indentLevel, op.line, op.lexeme);
                        break;
                    case PLUS_EQUAL:
                        realOp = new Token(PLUS, op.indentLevel, op.line, op.lexeme);
                        break;
                    case MINUS_EQUAL:
                        realOp = new Token(MINUS, op.indentLevel, op.line, op.lexeme);
                        break;
                }

                if(left is VariableExpr) {
                    var right = Expression();
                    var ident = ((VariableExpr)left).ident;
                    return new AssignmentExpr(ident, new BinaryExpr(left, realOp, right));
                }
            }

            return left;
        }

        private Expr Or() {
            Expr left = And();

            while(Match(OR)) {
                Token op = Prev();
                Expr right = And();
                left = new LogicalExpr(left, op, right);
            }

            return left;
        }

        private Expr And() {
            Expr left = Equality();

            while(Match(AND)) {
                Token op = Prev();
                Expr right = Equality();
                left = new LogicalExpr(left, op, right);
            }

            return left;
        }

        private Expr Equality() {
            var left = Comparison();

            while(Match(EQUAL_EQUAL, NOT_EQUAL)) {
                var op = Prev();
                var right = Comparison();

                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        private Expr Comparison() {
            var left = Term();

            while(Match(LESS, LESS_EQUAL, GREATER, GREATER_EQUAL)) {
                var op = Prev();
                var right = Term();

                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        private Expr Term() {
            var left = Factor();

            while(Match(MINUS, PLUS)) {
                var op = Prev();
                var right = Factor();

                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        private Expr Factor() {
            var left = Unary();

            while(Match(STAR, SLASH)) {
                var op = Prev();
                var right = Unary();

                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        private Expr Unary() {
            if(Match(NOT, MINUS)) {
                var op = Prev();
                var right = Unary();

                return new UnaryExpr(op, right);
            }

            if(Match(PLUS_PLUS, MINUS_MINUS)) {
                var opop = Prev();
                var realOp = new Token(opop.type == PLUS_PLUS ? PLUS : MINUS, opop.indentLevel, opop.line, opop.lexeme);

                var right = Primary();
                if(right is VariableExpr) {
                    var rightIdent = ((VariableExpr)right).ident;
                    return new AssignmentExpr(rightIdent, new BinaryExpr(right, realOp, new LiteralExpr(1.0d)));
                } else {
                    throw Error(Prev(), "Can only use prefix increment and decrement operators with variables");
                }
            }

            return Call();
        }

        private Expr Call() {
            Expr left = Primary();

            while(Match(LEFT_PAREN, LEFT_BRACKET, DOT)) {
                var prev = Prev();
                if(prev.type == LEFT_PAREN) {
                    var args = new List<Expr>();

                    if(!Check(RIGHT_PAREN)) {
                        do {
                            if(args.Count >= 63) {
                                Error(Peek(), "Too many arguments for function, max of 63 arguments");
                            }

                            args.Add(Expression());
                        } while(Match(COMMA));
                    }

                    Token rightParen = Consume(RIGHT_PAREN, "Expect ')' after function arguments");

                    left = new CallExpr(rightParen, left, args);
                } else if(prev.type == DOT) {
                    var ident = Consume(IDENTIFIER, "Expect table item name following '.'");
                    left = new TableGetExpr(left, ident);
                } else {
                    var ind = Expression();
                    Token rightBracket = Consume(RIGHT_BRACKET, "Expect closing bracket after list access");

                    left = new ListGetExpr(rightBracket, left, ind);
                }
            }

            return left;
        }

        private Expr Primary() {
            if(Match(TRUE)) {
                return new LiteralExpr(true);
            }
            if(Match(FALSE)) {
                return new LiteralExpr(false);
            }

            if(Match(NUMBER)) {
                double number;
                try {
                    number = double.Parse(Prev().lexeme);
                    return new LiteralExpr(number);
                } catch(Exception) {
                    throw Error(Prev(), "Number " + Prev().lexeme + " is not a valid number");
                }
            }

            if(Match(STRING)) {
                var lexeme = Prev().lexeme;
                var s = lexeme.Substring(1, lexeme.Length - 2);
                return new LiteralExpr(s);
            }

            if(Match(LEFT_BRACKET)) {
                var open = Prev();
                if(Match(RIGHT_BRACKET)) {
                    return new ListLiteralExpr(open, new Expr[0]);
                }

                var list = new List<Expr>();
                do {
                    list.Add(Expression());
                } while(Match(COMMA));

                Consume(RIGHT_BRACKET, "Expect closing bracket in list literal");
                return new ListLiteralExpr(open, list.ToArray());
            }


            if(Match(LEFT_BRACE)) {
                var open = Prev();
                if(Match(RIGHT_BRACE)) {
                    return new TableLiteralExpr(open, new List<Token>(), new List<Expr>());
                }

                var idents = new List<Token>();
                var exprs = new List<Expr>();
                do {
                    var ident = Consume(IDENTIFIER, "Expect name in table literal");
                    Consume(COLON, "Expect colon after name in table literal");
                    idents.Add(ident);
                    exprs.Add(Expression());
                } while(Match(COMMA));

                Consume(RIGHT_BRACE, "Expect closing brace in table literal");
                return new TableLiteralExpr(open, idents, exprs);
            }

            if(Match(IDENTIFIER)) {
                var ident = Prev();

                if(Match(PLUS_PLUS, MINUS_MINUS)) {
                    var op = Prev();
                    return new PostfixExpr(ident, op);
                }

                return new VariableExpr(ident);
            }

            if(Match(LEFT_PAREN)) {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Closing parenthesis not found");
                return new GroupingExpr(expr);
            }

            throw Error(Prev(), Peek().type.ToString() + " Incomplete expression");
        }

        private BlockStmt Block() {
            int outerIndentLevel = lastIndentLevel;
            int blockIndentLevel = thisIndentLevel;

            lastIndentLevel = blockIndentLevel;

            var statements = new List<Stmt>();
            while(!IsDone()) {
                if(Match(NL)) {
                    continue;
                }

                if(Peek().indentLevel != blockIndentLevel) {
                    break;
                }

                statements.Add(Declaration());
            }

            if(Peek().indentLevel > outerIndentLevel && !IsDone()) {
                throw Error(Peek(), "Indent level does not match any outer indent level");
            }

            if(Peek().type != EOF) {
                Rewind();
            }

            return new BlockStmt(blockIndentLevel, statements);
        }

        private Stmt If() {
            Expr condition = Expression();

            Consume(NL, "Expect line break following if");

            lastIndentLevel = thisIndentLevel;
            thisIndentLevel = Peek().indentLevel;

            Stmt then = Block();
             
            ConsumeOrEnd(NL, "Expect line break following if block");

            Stmt elseStmt = null;

            if(Match(ELSE)) {
                if(Match(IF)) {
                    elseStmt = If();
                } else {
                    Consume(NL, "Expect line break following else");
                    elseStmt = Block();
                }
            } else if(Peek().type != EOF) {
                Rewind();
            }

            return new IfStmt(condition, then, elseStmt);
        }

        private Stmt While() {
            Expr condition = Expression();

            Consume(NL, "Expect line break following while");

            lastIndentLevel = thisIndentLevel;
            thisIndentLevel = Peek().indentLevel;

            inLoop = true;

            Stmt loop = Block();

            inLoop = false;

            ConsumeOrEnd(NL, "Expect line break following while block");

            if(Peek().type != EOF) {
                Rewind();
            }

            return new WhileStmt(condition, loop);
        }

        private Stmt For() {
            var id = Consume(IDENTIFIER, "For must be of form \"for variable = {lower}..{=}upper\", variable omitted");

            Consume(EQUAL, "For must be of form \"for variable = {lower}..{=}upper, equals sign omitted\"");

            Expr lower = new LiteralExpr(0);

            //Not skipping first argument
            if(!Check(DOT_DOT) && !Check(DOT_DOT_EQ)) {
                lower = Expression();
            }

            bool inclusive = false;
            if(Match(DOT_DOT_EQ)) {
                inclusive = true;
            } else if(!Match(DOT_DOT)) {
                throw Error(Prev(), "For statement must have a range with either .. or ..=");
            }

            Expr upper = Expression();

            Consume(NL, "Expect line break following for");

            lastIndentLevel = thisIndentLevel;
            thisIndentLevel = Peek().indentLevel;

            inLoop = true;

            Stmt loop = Block();

            inLoop = false;

            ConsumeOrEnd(NL, "Expect line break following for block");

            if(Peek().type != EOF) {
                Rewind();
            }

            return new ForStmt(id, lower, upper, loop, inclusive);
        }

        private void Synchronize() {
            Next();

            while(!IsDone()) {
                if(Prev().type == NL) {
                    return;
                }

                switch(Peek().type) {
                    case IF:
                    case FN:
                    case FOR:
                    case WHILE:
                    case RETURN:
                    case WAIT:
                    case BREAK:
                    case CONTINUE:
                        return;
                }

                Next();
            }
        }

        private bool Match(params TokenType[] types) {
            foreach(var type in types) {
                if(Check(type)) {
                    Next();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type) {
            if(IsDone()) {
                return false;
            }

            return Peek().type == type;
        }

        private Token Consume(TokenType type, string error) {
            if(Check(type)) {
                return Next();
            }

            throw Error(Peek(), error);
        }

        private Token ConsumeOrEnd(TokenType type, string error) {
            if(Check(type)) {
                return Next();
            }

            if(IsDone()) {
                return Peek();
            }

            throw Error(Peek(), error);
        }

        private ParseError Error(Token token, string error) {
            hadError = true;
            PicaError.Error(error, token.line);
            return new ParseError();
        }

        private Token Next() {
            if(!IsDone()) {
                current++;
            }

            return Prev();
        }

        private bool IsDone() {
            return Peek().type == EOF;
        }

        private Token Peek() {
            return tokens[current];
        }

        private Token Prev() {
            return tokens[current - 1];
        }

        private void Rewind() {
            current -= 1;
        }

        private class ParseError : Exception {}

    }

}