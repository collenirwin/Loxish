using System;

namespace Lang.Interpreter
{
    /// <summary>
    /// Used to return from a function.
    /// </summary>
    internal class ReturnException : Exception
    {
        /// <summary>
        /// The return value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a <see cref="ReturnException"/> with a return value.
        /// </summary>
        /// <param name="value">The return value.</param>
        public ReturnException(object value)
        {
            Value = value;
        }
    }
}
