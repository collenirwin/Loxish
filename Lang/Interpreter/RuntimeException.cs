using System;

namespace Lang.Interpreter
{
    /// <summary>
    /// Defines an exception caused by a failure at runtime.
    /// </summary>
    public class RuntimeException : Exception
    {
        /// <summary>
        /// The token that caused the exception to occur.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Initializes a <see cref="RuntimeException"/> with a token and a message.
        /// </summary>
        /// <param name="token">The token that caused the exception to occur.</param>
        /// <param name="message">Decription of the error.</param>
        public RuntimeException(Token token, string message) : base(message)
        {
            Token = token;
        }
    }
}
