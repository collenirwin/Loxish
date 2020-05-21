using System;
using System.Text;

namespace Lang.Utils
{
    /// <summary>
    /// Contains extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets a piece of the passed string.
        /// </summary>
        /// <param name="text">String we're working with.</param>
        /// <param name="start">Start index of the piece to slice.</param>
        /// <param name="end">Exclusive end index of the piece to slice.</param>
        /// <returns>The requested text slice.</returns>
        public static string Slice(this string text, int start, int end)
        {
            if (start < 0 || end > text.Length)
            {
                throw new ArgumentOutOfRangeException("Start and end must fall within the bounds of the string.");
            }

            var builder = new StringBuilder();

            for (int index = start; index < end; index++)
            {
                builder.Append(text[index]);
            }

            return builder.ToString();
        }
    }
}
