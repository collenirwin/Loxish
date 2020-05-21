using System.Collections.Generic;
using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// Keeps track of <see cref="SyntaxError"/>s detected during interpretation of source code.
    /// </summary>
    public class ErrorState
    {
        /// <summary>
        /// All detected errors.
        /// </summary>
        public List<SyntaxError> Errors { get; } = new List<SyntaxError>();

        /// <summary>
        /// Have any errors been detected?
        /// </summary>
        public bool HasErrors => Errors.Any();

        /// <summary>
        /// Adds an error to the list.
        /// </summary>
        /// <param name="line">Line the error was detected.</param>
        /// <param name="message">Message describing the detected error.</param>
        public void AddError(int line, string message) => Errors.Add(new SyntaxError(line, message));

        /// <summary>
        /// Adds an error to the list.
        /// </summary>
        /// <param name="error">Error to add.</param>
        public void AddError(SyntaxError error) => Errors.Add(error);
    }
}
