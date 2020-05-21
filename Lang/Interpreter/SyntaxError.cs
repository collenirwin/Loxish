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
        /// Initializes a <see cref="SyntaxError"/> with a line number and message.
        /// </summary>
        /// <param name="line">The line the error was detected on.</param>
        /// <param name="message">Message describing the detected error.</param>
        public SyntaxError(int line, string message)
        {
            Line = line;
            Message = message;
        }
    }
}
