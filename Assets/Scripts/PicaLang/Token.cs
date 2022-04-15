namespace Pica {

    public struct Token {

        public TokenType type;

        public int indentLevel;
        public int line;

        public string lexeme;

        public Token(TokenType type, int indentLevel, int line, string lexeme) {
            this.type = type;
            this.indentLevel = indentLevel;
            this.line = line;
            this.lexeme = lexeme;
        }

    }

}