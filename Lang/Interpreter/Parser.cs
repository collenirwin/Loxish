using System;
using System.Collections.Generic;
using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// Parses tokens into statements.
    /// </summary>
    public class Parser : IErrorRecorder
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private int _loopDepth = 0;

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
        public Parser(IEnumerable<Token> tokens, ErrorState errorState)
        {
            _tokens = tokens.ToList();
            ErrorState = errorState;
        }

        /// <summary>
        /// Creates statements from tokens.
        /// </summary>
        /// <returns>All statements generated from the tokens.</returns>
        public IEnumerable<StatementBase> Parse()
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

                if (NextTokenMatches(TokenType.Class))
                {
                    return ClassDeclaration();
                }

                if (PeekMatches(TokenType.Fun) && PeekAfterMatches(TokenType.Identifier))
                {
                    NextToken();
                    return Function(kind: "function");
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
        /// Consumes a class declaration statement.
        /// </summary>
        private ClassStatement ClassDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expected class name.");
            Consume(TokenType.LeftCurlyBrace, "Expected '{' before class body.");

            var methods = new List<FunctionStatement>();
            while (!PeekMatches(TokenType.RightCurlyBrace) && !_atEndOfTokens)
            {
                methods.Add(Function(kind: "method"));
            }

            Consume(TokenType.RightCurlyBrace, "Expected '}' after class body.");
            return new ClassStatement(name, methods);
        }

        /// <summary>
        /// Consumes a function declaration statement.
        /// </summary>
        private FunctionStatement Function(string kind)
        {
            var name = Consume(TokenType.Identifier, $"Expected {kind} name.");
            return new FunctionStatement(name, FunctionBody(kind));
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

            if (NextTokenMatches(TokenType.While))
            {
                return WhileStatement();
            }

            if (NextTokenMatches(TokenType.For))
            {
                return ForStatement();
            }

            if (NextTokenMatches(TokenType.Return))
            {
                return ReturnStatement();
            }

            if (NextTokenMatches(TokenType.Break))
            {
                return BreakStatement();
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
        /// Consumes a while statement.
        /// </summary>
        private WhileStatement WhileStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'while'.");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expected ')' after 'while' condition.");

            try
            {
                _loopDepth++;
                var body = Statement();
                return new WhileStatement(condition, body);
            }
            finally
            {
                _loopDepth--;
            }
        }

        /// <summary>
        /// Consumes a for statement (desugars into a block-enclosed while loop).
        /// </summary>
        private StatementBase ForStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'for'.");

            StatementBase initializer = null;
            if (!NextTokenMatches(TokenType.SemiColon))
            {
                initializer = NextTokenMatches(TokenType.Var)
                    ? VarDeclaration()
                    : ExpressionStatement();
            }

            ExpressionBase condition = null;
            if (!PeekMatches(TokenType.SemiColon))
            {
                condition = Expression();
            }

            Consume(TokenType.SemiColon, "Expected ';' after for loop condition.");

            ExpressionBase increment = null;
            if (!PeekMatches(TokenType.RightParen))
            {
                increment = Expression();
            }

            Consume(TokenType.RightParen, "Expected ')' after for loop clauses.");

            try
            {
                _loopDepth++;
                var body = Statement();

                if (increment != null)
                {
                    // wrap the body in a block with the increment executing at the end
                    body = new BlockStatement(new[]
                    {
                        body,
                        new ExpressionStatement(increment)
                    });
                }

                // wire up a while loop using the condition and body
                condition ??= new LiteralExpression(true);
                body = new WhileStatement(condition, body);

                if (initializer != null)
                {
                    // wrap the body in another block with the increment executing first
                    body = new BlockStatement(new[]
                    {
                        initializer,
                        body
                    });
                }

                return body;
            }
            finally
            {
                _loopDepth--;
            }
        }

        /// <summary>
        /// Consumes a return statement.
        /// </summary>
        private ReturnStatement ReturnStatement()
        {
            var keyword = _currentToken;
            ExpressionBase value = null;

            if (!NextTokenMatches(TokenType.SemiColon))
            {
                value = Expression();
            }

            Consume(TokenType.SemiColon, "Expected ';' after return value.");
            return new ReturnStatement(keyword, value);
        }

        /// <summary>
        /// Consumes a break statement.
        /// </summary>
        private BreakStatement BreakStatement()
        {
            if (_loopDepth == 0)
            {
                Error(_currentToken, "'break' must be inside of a loop body.");
            }

            Consume(TokenType.SemiColon, "Expected ';' after 'break'.");
            return new BreakStatement();
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
        private IEnumerable<StatementBase> BlockStatement()
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
            var expression = LogicalOr();

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

        private ExpressionBase LogicalOr()
        {
            var expression = LogicalAnd();

            while (NextTokenMatches(TokenType.DoublePipe))
            {
                var @operator = _currentToken;
                var right = LogicalAnd();
                expression = new LogicalExpression(expression, right, @operator);
            }

            return expression;
        }

        private ExpressionBase LogicalAnd()
        {
            var expression = Binary();

            while (NextTokenMatches(TokenType.DoubleAmp))
            {
                var @operator = _currentToken;
                var right = Binary();
                expression = new LogicalExpression(expression, right, @operator);
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

            return Call();
        }

        private ExpressionBase Call()
        {
            var expression = Primary();

            while (NextTokenMatches(TokenType.LeftParen))
            {
                expression = FinishCall(expression);
            }

            return expression;
        }

        private ExpressionBase FinishCall(ExpressionBase expression)
        {
            var arguments = new List<ExpressionBase>();

            if (!PeekMatches(TokenType.RightParen)) // no args?
            {
                do // at least one arg
                {
                    if (arguments.Count > 255)
                    {
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    }

                    arguments.Add(Expression());
                } while (NextTokenMatches(TokenType.Comma)); // more args?
            }

            var closingParen = Consume(TokenType.RightParen, "Expected ')' after arguments.");
            return new CallExpression(expression, arguments, closingParen);
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

            if (NextTokenMatches(TokenType.Fun))
            {
                return FunctionBody(kind: "anonymous function");
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

        private FunctionExpression FunctionBody(string kind)
        {
            Consume(TokenType.LeftParen, kind == "anonymous function"
                ? $"Expected '(' after {kind} declaration."
                : $"Expected '(' after {kind} name.");
            var @params = new List<Token>();

            if (!PeekMatches(TokenType.RightParen)) // no params?
            {
                do // at least one param
                {
                    if (@params.Count > 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }

                    @params.Add(Consume(TokenType.Identifier, "Expected parameter name."));
                } while (NextTokenMatches(TokenType.Comma)); // more params?
            }

            Consume(TokenType.RightParen, "Expected ')' after parameters.");
            Consume(TokenType.LeftCurlyBrace, $"Expected '{{' before {kind} body.");
            return new FunctionExpression(@params, body: BlockStatement());
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
        /// Checks to see if the token after the next token matches the passed token type.
        /// </summary>
        /// <param name="tokenType">Token type to match.</param>
        /// <returns>True if the token after next token matches the passed token type</returns>
        private bool PeekAfterMatches(TokenType tokenType) => !_atEndOfTokens && _tokens[_current + 1].Type == tokenType;

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
