namespace Lang.Interpreter
{
    /// <summary>
    /// Represents an atomic unit of source code.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The type of this token.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// The source code text we're wrapping into this token.
        /// </summary>
        public string WrappedSource { get; }

        /// <summary>
        /// The value we're interpreting this token to have, if applicable.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// The line this token resides on in the source file.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Initializes a <see cref="Token"/> with a type and a line,
        /// with default values for wrapped source and literal.
        /// </summary>
        /// <param name="type">The type of this token.</param>
        /// <param name="line">The line this token resides on in the source file.</param>
        public Token(TokenType type, int line) : this(type, "", null, line)
        {
        }

        /// <summary>
        /// Initializes a <see cref="Token"/> with explicit property values.
        /// </summary>
        /// <param name="type">The type of this token.</param>
        /// <param name="wrappedSource">The source code text we're wrapping into this token.</param>
        /// <param name="value">The value we're interpreting this token to have, if applicable.</param>
        /// <param name="line">The line this token resides on in the source file.</param>
        public Token(TokenType type, string wrappedSource, object value, int line)
        {
            Type = type;
            WrappedSource = wrappedSource;
            Value = value;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {WrappedSource} {Value}";
        }
    }
}
