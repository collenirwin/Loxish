using System;

namespace Lang.Interpreter
{
    /// <summary>
    /// Contains methods for reporting error messages over Standard Error.
    /// </summary>
    public static class ErrorReporter
    {
        /// <summary>
        /// Writes all <see cref="SyntaxError.FullMessage"/>s in the error state over Standard Error.
        /// </summary>
        /// <param name="errorState">Error state to report about.</param>
        public static void ReportSyntaxErrors(ErrorState errorState)
        {
            foreach (var error in errorState.Errors)
            {
                Console.Error.WriteLine(error.FullMessage);
            }
        }

        /// <summary>
        /// Writes an exception message to Standard Error.
        /// </summary>
        /// <param name="ex">Exception to base the message on.</param>
        public static void ReportRuntimeException(RuntimeException ex)
        {
            Console.Error.WriteLine($"[Line {ex.Token.Line}] Error: {ex.Message}");
        }
    }
}
