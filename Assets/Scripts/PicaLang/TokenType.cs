namespace Pica {

    public enum TokenType {
        //Blocks
        LEFT_PAREN, RIGHT_PAREN, COMMA, LEFT_BRACE, RIGHT_BRACE, LEFT_BRACKET, RIGHT_BRACKET, DOT, COLON,

        //Arithmatic
        MINUS, PLUS, STAR, SLASH,

        //The equals sign and friends
        NOT_EQUAL, NOT, EQUAL_EQUAL, EQUAL, LESS, LESS_EQUAL, GREATER, GREATER_EQUAL, PLUS_PLUS, MINUS_MINUS,
        STAR_EQUAL, SLASH_EQUAL, PLUS_EQUAL, MINUS_EQUAL,

        //Literals
        IDENTIFIER, STRING, NUMBER,

        DOT_DOT, DOT_DOT_EQ,

        //Reserved words
        AND, OR, IF, ELSE, FN, FOR, WHILE, RETURN, TRUE, FALSE, BREAK, CONTINUE, WAIT, UNTIL,

        //Statement Terminator
        NL,

        //End of file
        EOF
    }

}