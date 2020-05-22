using System;
using System.Collections.Generic;

namespace Lang.Interpreter
{
    public class Parser : IErrorRecorder
    {
        public ErrorState ErrorState { get; }
        private readonly List<Token> _tokens;
        private int _current = 0;

        private bool _atEndOfTokens => Peek().Type == TokenType.EndOfFile;
        private Token CurrentToken => _tokens[_current - 1];

        public Parser(List<Token> tokens, ErrorState errorState)
        {
            _tokens = tokens;
            ErrorState = errorState;
        }

        private ExpressionBase Equality()
        {
            // we're calling Comparison because it is the pattern with the next-higher precedence to equality
            var expression = Comparison();

            while (NextTokenMatches(TokenType.DoubleEqual, TokenType.BangEqual))
            {
                var @operator = CurrentToken;
                var right = Comparison();

                // wrap all expressions into the outer one
                // storing each added expression in the left operand, because we're left-associating
                expression = new BinaryExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase Comparison()
        {
            var expression = Addition();

            while (NextTokenMatches(TokenType.LessThan, TokenType.LessThanOrEqual,
                TokenType.GreaterThan, TokenType.GreaterThanOrEqual))
            {
                var @operator = CurrentToken;
                var right = Addition();
                expression = new BinaryExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase Addition()
        {
            var expression = Multiplication();

            while (NextTokenMatches(TokenType.Plus, TokenType.Minus))
            {
                var @operator = CurrentToken;
                var right = Multiplication();
                expression = new BinaryExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase Multiplication()
        {
            var expression = Unary();

            while (NextTokenMatches(TokenType.Star, TokenType.Slash))
            {
                var @operator = CurrentToken;
                var right = Unary();
                expression = new BinaryExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase Unary()
        {
            if (NextTokenMatches(TokenType.Bang, TokenType.Minus))
            {
                var @operator = CurrentToken;
                var right = Unary();
                return new UnaryExpression(right, @operator);
            }

            return Primary();
        }

        private ExpressionBase Primary()
        {
            if (NextTokenMatches(TokenType.True))
            {
                return new LiteralExpression(true);
            }

            if (NextTokenMatches(TokenType.False))
            {
                return new LiteralExpression(false);
            }

            if (NextTokenMatches(TokenType.Null))
            {
                return new LiteralExpression(null);
            }

            if (NextTokenMatches(TokenType.String))
            {
                return new LiteralExpression(CurrentToken.Value);
            }

            if (NextTokenMatches(TokenType.LeftParen))
            {
                var expression = Expression();
                Consume(TokenType.RightParen, errorMessage: "Expected ')' after expression.");
                return new GroupingExpression(expression);
            }

            throw new NotImplementedException();
        }

        private ExpressionBase Expression()
        {
            throw new NotImplementedException();
        }

        private void Synchronize()
        {
            NextToken();

            // look for the end of the current statement or the beginning of the next
            // so that we can continue parsing after an error has been detected
            while (!_atEndOfTokens)
            {
                if (CurrentToken.Type == TokenType.SemiColon)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.If:
                    case TokenType.For:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }

                NextToken();
            }
        }

        private Token NextToken()
        {
            if (!_atEndOfTokens)
            {
                _current++;
            }

            return CurrentToken;
        }

        private Token Peek() => _tokens[_current];

        private bool NextTokenMatches(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (NextTokenMatches(tokenType))
                {
                    NextToken();
                    return true;
                }
            }

            return false;
        }

        private bool NextTokenMatches(TokenType tokenType) => !_atEndOfTokens && Peek().Type == tokenType;

        private Token Consume(TokenType tokenType, string errorMessage)
        {
            if (NextTokenMatches(tokenType))
            {
                return NextToken();
            }

            throw Error(Peek(), errorMessage);
        }

        private ParserException Error(Token token, string message)
        {
            ErrorState.AddError(token, message);
            return new ParserException();
        }
    }

    class ParserException : Exception
    {
    }
}
