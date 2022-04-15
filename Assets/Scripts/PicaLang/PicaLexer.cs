using System.Collections.Generic;
using static Pica.TokenType;

namespace Pica {

    public class PicaLexer {

        private string input;
        private List<Token> tokens;

        private Dictionary<string, TokenType> keywords;

        private int lexemeStart;
        private int lexemeCurrent;
        private int line;
        private int indentLevel;
        private bool startOfLine;

        private bool foundTabSpace;
        private bool tabs;

        public bool hadError;

        public PicaLexer(string input) {
            this.input = input;
            tokens = new List<Token>();

            lexemeStart = 0;
            lexemeCurrent = 0;
            line = 0;
            indentLevel = 0;
            startOfLine = true;

            foundTabSpace = false;
            tabs = false;

            hadError = false;

            SetupKeywords();
        }

        public List<Token> Lex() {
            while(!IsDone()) {
                lexemeStart = lexemeCurrent;

                ScanToken();
            }

            lexemeStart = lexemeCurrent;
            AddToken(EOF);

            return tokens;
        }

        private bool IsDone() {
            return lexemeCurrent >= input.Length;
        }

        private void ScanToken() {
            char c = Next();

            if(!IsTabOrSpace(c)) {
                startOfLine = false;
            }

            switch(c) {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case '[': AddToken(LEFT_BRACKET); break;
                case ']': AddToken(RIGHT_BRACKET); break;
                case ':': AddToken(COLON); break;
                case ',': AddToken(COMMA); break;
                case '-': AddToken(Match('-') ? MINUS_MINUS : Match('=') ? MINUS_EQUAL : MINUS); break;
                case '+': AddToken(Match('+') ? PLUS_PLUS : Match('=') ? PLUS_EQUAL : PLUS); break;
                case '*': AddToken(Match('=') ? STAR_EQUAL : STAR); break;
                case '/': AddToken(Match('=') ? SLASH_EQUAL : SLASH); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '~': AddToken(Match('=') ? NOT_EQUAL : NOT); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
                case '.': {
                    if(IsDigit(Peek())) {
                        NumericLiteral();
                    } else if(Match('.')) {
                        AddToken(Match('=') ? DOT_DOT_EQ : DOT_DOT);
                    } else {
                        AddToken(DOT);
                    }
                    break;
                }
                case ' ':
                case '\t': LeadingWhitespace(c); break;
                case '\r': break;
                case '\n': {
                    AddToken(NL);
                    line++;
                    startOfLine = true;
                    indentLevel = 0;
                    break;
                }
                case '"': StringLiteral(); break;
                case '#': Comment(); break;
                default: {
                    if(IsDigit(c)) {
                        NumericLiteral();
                    } else if(CanStartIdentifier(c)) {
                        Identifier();
                    } else {
                        Error("Unexpected character", line);
                    }

                    break;
                }
            }
        }

        private void SetupKeywords() {
            keywords = new Dictionary<string, TokenType>();

            keywords.Add("and", AND);
            keywords.Add("or", OR);
            keywords.Add("if", IF);
            keywords.Add("else", ELSE);
            keywords.Add("fn", FN);
            keywords.Add("for", FOR);
            keywords.Add("while", WHILE);
            keywords.Add("return", RETURN);
            keywords.Add("true", TRUE);
            keywords.Add("false", FALSE);
            keywords.Add("wait", WAIT);
            keywords.Add("until", UNTIL);
            keywords.Add("break", BREAK);
            keywords.Add("continue", CONTINUE);
        }

        private void LeadingWhitespace(char c) {
            if(!startOfLine) {
                return;
            }

            bool isTab = c == '\t';

            if(!foundTabSpace) {
                tabs = isTab;
            }

            if(isTab != tabs) {
                Error("Cannot mix spaces and tabs in a script", line);
                return;
            }

            var count = 1;

            while(Match(c)) {
                count++;
            }

            if(isTab) {
                indentLevel = count;
                return;
            }

            if(count % 4 != 0) {
                Error("Leading spaces must be a multiple of four", line);
            }

            indentLevel = count / 4;

        }

        public void StringLiteral() {
            while(Peek() != '"' && !IsDone()) {
                if(Peek() == '\n') {
                    line++;
                }

                Next();
            }

            if(IsDone()) {
                Error("Unterminated string", line);
                return;
            }

            //Hit the closing thing
            Next();

            AddToken(TokenType.STRING);
        }

        private void NumericLiteral() {
            while(IsDigit(Peek())) {
                Next();
            }

            if(Peek() == '.' && IsDigit(PeekNext())) {
                Next();

                while(IsDigit(Peek())) {
                    Next();
                }
            }

            AddToken(NUMBER);
        }

        private void Identifier() {
            while(IsAlphaNumeric(Peek())) {
                Next();
            }

            if(keywords.TryGetValue(GetLexeme(), out var keyword)) {
                AddToken(keyword);
                return;
            }

            AddToken(IDENTIFIER);
        }

        private void Comment() {
            while(!(Peek() == '\n') && !IsDone()) {
                Next();
            }
        }

        private void AddToken(TokenType type) {
            tokens.Add(new Token(type, indentLevel, line, GetLexeme()));
        }

        private string GetLexeme() {
            return input.Substring(lexemeStart, lexemeCurrent - lexemeStart);
        }

        private char Next() {
            return input[lexemeCurrent++];
        }

        private bool Match(char c) {
            if(Peek() != c) {
                return false;
            }

            lexemeCurrent++;
            return true;
        }

        private char Peek() {
            if(IsDone()) {
                return '\0';
            }
            return input[lexemeCurrent];
        }

        private char PeekNext() {
            if(lexemeCurrent + 1 >= input.Length) {
                return '\0';
            }
            return input[lexemeCurrent + 1];
        }

        private bool CanStartIdentifier(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool IsAlphaNumeric(char c) {
            return CanStartIdentifier(c) || IsDigit(c);
        }

        private bool IsTabOrSpace(char c) {
            return c == ' ' || c == '\t';
        }

        private bool IsDigit(char c) {
            return c >= '0' && c <= '9';
        }

        private void Error(string s, int line) {
            hadError = true;
            PicaError.Error(s, line);
        }

    }

}