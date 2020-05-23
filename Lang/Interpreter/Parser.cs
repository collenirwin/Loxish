using System;
using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// Parses tokens into statements.
    /// </summary>
    public class Parser : IErrorRecorder
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        private bool _atEndOfTokens => Peek().Type == TokenType.EndOfFile;
        private Token _currentToken => _tokens[_current - 1];

        /// <summary>
        /// Contains any errors thrown during a <see cref="Parse"/> call.
        /// </summary>
        public ErrorState ErrorState { get; }

        /// <summary>
        /// Initializes a <see cref="Parser"/> with tokens and an error state.
        /// </summary>
        /// <param name="tokens">Tokens to parse.</param>
        /// <param name="errorState">Error state object to record detected errors.</param>
        public Parser(List<Token> tokens, ErrorState errorState)
        {
            _tokens = tokens;
            ErrorState = errorState;
        }

        /// <summary>
        /// Creates statements from tokens.
        /// </summary>
        /// <returns>All statements generated from the tokens.</returns>
        public List<StatementBase> Parse()
        {
            var statements = new List<StatementBase>();

            while (!_atEndOfTokens)
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        /// <summary>
        /// Gets to the next statement after an error has occured so that parsing may continue.
        /// </summary>
        private void Synchronize()
        {
            NextToken();

            // look for the end of the current statement or the beginning of the next
            // so that we can continue parsing after an error has been detected
            while (!_atEndOfTokens)
            {
                if (_currentToken.Type == TokenType.SemiColon)
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

        #region Statements

        /// <summary>
        /// The most general classification of a statement.
        /// Every statement starts here.
        /// Synchronization is done here.
        /// </summary>
        private StatementBase Declaration()
        {
            try
            {
                if (NextTokenMatches(TokenType.Var))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParserException)
            {
                Synchronize();
                return null;
            }
        }

        /// <summary>
        /// Consumes a variable declaration statement.
        /// </summary>
        private StatementBase VarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expected identifier.");
            ExpressionBase initializer = null;

            if (NextTokenMatches(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.SemiColon, "Expected ';' after variable declaration.");
            return new VarStatement(name, initializer);
        }

        /// <summary>
        /// Branches to <see cref="PrintStatement"/>,
        /// <see cref="BlockStatement"/>,
        /// or <see cref="ExpressionStatement"/>.
        /// </summary>
        private StatementBase Statement()
        {
            if (NextTokenMatches(TokenType.If))
            {
                return IfStatement();
            }

            if (NextTokenMatches(TokenType.Print))
            {
                return PrintStatement();
            }

            if (NextTokenMatches(TokenType.LeftCurlyBrace))
            {
                return new BlockStatement(BlockStatement());
            }

            return ExpressionStatement();
        }

        /// <summary>
        /// Consumes an if statement.
        /// </summary>
        private IfStatement IfStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expected ')' after 'if' condition.");

            var thenBranch = Statement();
            StatementBase elseBranch = null;

            if (NextTokenMatches(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// Consumes a print statement.
        /// </summary>
        private PrintStatement PrintStatement()
        {
            var expression = Expression();
            Consume(TokenType.SemiColon, "Expected ';' after value to print.");
            return new PrintStatement(expression);
        }

        /// <summary>
        /// Consumes a block.
        /// </summary>
        private List<StatementBase> BlockStatement()
        {
            var statements = new List<StatementBase>();

            while (!PeekMatches(TokenType.RightCurlyBrace) && !_atEndOfTokens)
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RightCurlyBrace, "Expected closing '}'.");
            return statements;
        }

        /// <summary>
        /// Consumes an expression.
        /// </summary>
        private ExpressionStatement ExpressionStatement()
        {
            var expression = Expression();
            Consume(TokenType.SemiColon, "Expected ';' after expression.");
            return new ExpressionStatement(expression);
        }

        #endregion

        #region Expressions

        /// <summary>
        /// Kicks off the expression classfication by calling the expression method of lowest precedence.
        /// </summary>
        private ExpressionBase Expression()
        {
            return Assignment();
        }

        private ExpressionBase Assignment()
        {
            var expression = Binary();

            if (NextTokenMatches(TokenType.Equal, TokenType.PlusEqual, TokenType.MinusEqual))
            {
                var @operator = _currentToken;
                var value = Assignment();

                if (expression is VariableExpression variableExpression)
                {
                    var name = variableExpression.Name;
                    return new AssignmentExpression(name, @operator, value);
                }

                Error(@operator, "Invalid assignment target.");
            }

            return expression;
        }

        private ExpressionBase Binary()
        {
            var expression = Equality();

            while (NextTokenMatches(TokenType.Amp, TokenType.Pipe, TokenType.Caret))
            {
                var @operator = _currentToken;
                var right = Equality();
                expression = new BinaryExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase Equality()
        {
            // we're calling Comparison because it is the pattern with the next-higher precedence to equality
            var expression = Comparison();

            while (NextTokenMatches(TokenType.DoubleEqual, TokenType.BangEqual))
            {
                var @operator = _currentToken;
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
                var @operator = _currentToken;
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
                var @operator = _currentToken;
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
                var @operator = _currentToken;
                var right = Unary();
                expression = new BinaryExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase Unary()
        {
            if (NextTokenMatches(TokenType.Bang, TokenType.Minus))
            {
                var @operator = _currentToken;
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

            if (NextTokenMatches(TokenType.String, TokenType.Number))
            {
                return new LiteralExpression(_currentToken.Value);
            }

            if (NextTokenMatches(TokenType.Identifier))
            {
                return new VariableExpression(_currentToken);
            }

            if (NextTokenMatches(TokenType.LeftParen))
            {
                NextToken();
                var expression = Expression();
                Consume(TokenType.RightParen, errorMessage: "Expected ')' after expression.");
                return new GroupingExpression(expression);
            }

            throw Error(Peek(), "Expression expected.");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Advances <see cref="_current"/> if possible, and grabs the new <see cref="_currentToken"/>.
        /// </summary>
        /// <returns><see cref="_currentToken"/></returns>
        private Token NextToken()
        {
            if (!_atEndOfTokens)
            {
                _current++;
            }

            return _currentToken;
        }

        /// <summary>
        /// Peeks the next token without advancing.
        /// </summary>
        /// <returns>The next token.</returns>
        private Token Peek() => _tokens[_current];

        /// <summary>
        /// Checks to see if the next token matches one of the passed token types.
        /// Advances to it if it does.
        /// </summary>
        /// <param name="tokenTypes">Token types to match.</param>
        /// <returns>True if the next token matches one of the passed token types.</returns>
        private bool NextTokenMatches(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (PeekMatches(tokenType))
                {
                    NextToken();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the next token matches the passed token type.
        /// </summary>
        /// <param name="tokenType">Token type to match.</param>
        /// <returns>True if the next token matches the passed token type</returns>
        private bool PeekMatches(TokenType tokenType) => !_atEndOfTokens && Peek().Type == tokenType;

        /// <summary>
        /// Consumes the next token if it is of the passed type.
        /// Otherwise, <see cref="Error(Token, string)"/> is thrown.
        /// </summary>
        /// <param name="tokenType">Token type to match.</param>
        /// <param name="errorMessage">Error message to display if the token type is not matched.</param>
        /// <returns>The consumed token.</returns>
        private Token Consume(TokenType tokenType, string errorMessage)
        {
            if (PeekMatches(tokenType))
            {
                return NextToken();
            }

            throw Error(Peek(), errorMessage);
        }

        /// <summary>
        /// Adds the token and message to the <see cref="ErrorState"/>,
        /// then returns a <see cref="ParserException"/>.
        /// </summary>
        /// <param name="token">Token that was involved in the error.</param>
        /// <param name="message">Error message to add with the token.</param>
        /// <returns>A <see cref="ParserException"/>.</returns>
        private ParserException Error(Token token, string message)
        {
            ErrorState.AddError(token, message);
            return new ParserException();
        }

        #endregion
    }

    /// <summary>
    /// An exception thrown when a parsing error is encountered.
    /// </summary>
    class ParserException : Exception
    {
    }
}
