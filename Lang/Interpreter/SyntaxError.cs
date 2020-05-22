namespace Lang.Interpreter
{
    /// <summary>
    /// Represents a syntax error, with a message and line number.
    /// </summary>
    public class SyntaxError
    {
        /// <summary>
        /// Line the error was detected.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Message describing the detected error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The token involved in the error, if applicable.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Builds an error message using
        /// <see cref="Line"/>, <see cref="Message"/>, and <see cref="Token"/> (if available).
        /// </summary>
        public string FullMessage
        {
            get
            {
                string tokenMessage = "";
                if (Token != null)
                {
                    tokenMessage = Token.Type == TokenType.EndOfFile
                        ? " at end"
                        : $" at '{Token.WrappedSource}'";
                }

                return $"[Line {Line}] Error{tokenMessage}: {Message}";
            }
        }

        private SyntaxError(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a <see cref="SyntaxError"/> with a line number and message.
        /// </summary>
        /// <param name="line">The line the error was detected on.</param>
        /// <param name="message">Message describing the detected error.</param>
        public SyntaxError(int line, string message) : this(message)
        {
            Line = line;
        }

        /// <summary>
        /// Initializes a <see cref="SyntaxError"/> with a token and message.
        /// </summary>
        /// <param name="token">The token involved in the error.</param>
        /// <param name="message">Message describing the detected error.</param>
        public SyntaxError(Token token, string message) : this(message)
        {
            Line = token.Line;
        }
    }
}
