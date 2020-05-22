using Lang.Utils;
using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// Object for scanning source code and turning it into parsable <see cref="Token"/>s.
    /// </summary>
    public class Lexer : IErrorRecorder
    {
        /// <summary>
        /// Contains any errors thrown during a <see cref="Tokenize"/> call.
        /// </summary>
        public ErrorState ErrorState { get; }

        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            { "class",  TokenType.Class },
            { "and",    TokenType.And },
            { "or",     TokenType.Or },
            { "if",     TokenType.If },
            { "else",   TokenType.Else },
            { "true",   TokenType.True },
            { "false",  TokenType.False },
            { "this",   TokenType.This },
            { "super",  TokenType.Super },
            { "var",    TokenType.Var },
            { "return", TokenType.Return },
            { "for",    TokenType.For },
            { "while",  TokenType.While },
            { "print",  TokenType.Print },
            { "null",   TokenType.Null }
        };

        private bool _atEndOfSource => _current >= _source.Length;

        /// <summary>
        /// Initializes a <see cref="Lexer"/> with source code and an <see cref="ErrorState"/> object.
        /// </summary>
        /// <param name="source">Source code of the program.</param>
        /// <param name="errorState">Error state object to record detected errors.</param>
        public Lexer(string source, ErrorState errorState)
        {
            _source = source;
            ErrorState = errorState;
        }

        public List<Token> Tokenize()
        {
            while (!_atEndOfSource)
            {
                _start = _current;
                ScanNextToken();
            }

            _tokens.Add(new Token(TokenType.EndOfFile, _line));
            return _tokens;
        }

        private void ScanNextToken()
        {
            char c = NextChar();

            switch (c)
            {
                // white space
                case ' ':
                case '\r':
                case '\t':                    
                    break;
                case '\n':
                    _line++;
                    break;

                // single-char tokens
                case '(':
                    AddToken(TokenType.LeftParen);
                    break;
                case ')':
                    AddToken(TokenType.RightParen);
                    break;
                case '{':
                    AddToken(TokenType.LeftCurlyBrace);
                    break;
                case '}':
                    AddToken(TokenType.RightCurlyBrace);
                    break;
                case ',':
                    AddToken(TokenType.Comma);
                    break;
                case '.':
                    AddToken(TokenType.Dot);
                    break;
                case '+':
                    AddToken(TokenType.Plus);
                    break;
                case '-':
                    AddToken(TokenType.Minus);
                    break;
                case '*':
                    AddToken(TokenType.Star);
                    break;
                case ';':
                    AddToken(TokenType.SemiColon);
                    break;

                // single or double-char tokens
                case '!':
                    AddToken(NextCharIs('=') ? TokenType.BangEqual : TokenType.Bang);
                    break;
                case '=':
                    AddToken(NextCharIs('=') ? TokenType.DoubleEqual : TokenType.Equal);
                    break;
                case '<':
                    AddToken(NextCharIs('=') ? TokenType.LessThanOrEqual : TokenType.LessThan);
                    break;
                case '>':
                    AddToken(NextCharIs('=') ? TokenType.GreaterThanOrEqual : TokenType.GreaterThan);
                    break;

                // longer tokens
                case '/':
                    // see if we've got a comment
                    if (NextCharIs('/'))
                    {
                        // consume all of the chars of this line without creating a token
                        while (Peek() != '\n' && !_atEndOfSource)
                        {
                            NextChar();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }

                    break;
                case '"':
                    AddStringToken();
                    break;

                default:
                    if (char.IsDigit(c))
                    {
                        AddNumberToken();
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        AddIndentifierToken();
                    }
                    else
                    {
                        ErrorState.AddError(_line, $"Unexpected token: '{c}'.");
                    }
                    
                    break;
            }
        }

        private char NextChar() => _source[_current++];

        private bool NextCharIs(char expected)
        {
            if (_atEndOfSource || _source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }

        private char Peek(int length = 0)
        {
            if (_current + length >= _source.Length)
            {
                return '\0';
            }

            return _source[_current + length];
        }

        private void AddToken(TokenType type, object value = null)
        {
            string text = _source.Slice(_start, _current);
            _tokens.Add(new Token(type, text, value, _line));
        }

        private void AddStringToken()
        {
            char next;
            while ((next = Peek()) != '"' && !_atEndOfSource)
            {
                if (next == '\n')
                {
                    _line++;
                }

                NextChar();
            }

            if (_atEndOfSource)
            {
                ErrorState.AddError(_line, "Unterminated string.");
                return;
            }

            // consume the ending "
            NextChar();
            AddToken(TokenType.String, _source.Slice(_start + 1, _current - 1));
        }

        private void AddNumberToken()
        {
            while (char.IsDigit(Peek()))
            {
                NextChar();
            }

            if (Peek() == '.' && char.IsDigit(Peek(length: 1)))
            {
                // consume the .
                NextChar();

                while (char.IsDigit(Peek()))
                {
                    NextChar();
                }
            }

            AddToken(TokenType.Number, double.Parse(_source.Slice(_start, _current)));
        }

        private void AddIndentifierToken()
        {
            char next;
            while (char.IsLetterOrDigit(next = Peek()) || next == '_')
            {
                NextChar();
            }

            string text = _source.Slice(_start, _current);
            AddToken(_keywords.ContainsKey(text) ? _keywords[text] : TokenType.Identifier);
        }
    }
}
