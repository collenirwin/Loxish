using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// Keeps variable state.
    /// </summary>
    public class EnvironmentState
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// Add a variable to the internal state dictionary.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="value">Value of the variable.</param>
        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        /// <summary>
        /// Assigns a value to an existing variable.
        /// </summary>
        /// <param name="name">The variable name token.</param>
        /// <param name="value">Value of the variable.</param>
        /// <exception cref="RuntimeException"/>
        public void Assign(Token name, object value)
        {
            if (!_values.ContainsKey(name.WrappedSource))
            {
                throw new RuntimeException(name, $"'{name.WrappedSource}' is undefined.");
            }

            _values[name.WrappedSource] = value;
        }

        /// <summary>
        /// Gets the value the corresponds with the name token.
        /// </summary>
        /// <param name="name">The variable name token.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The value the corresponds with the name token.</returns>
        public object GetValue(Token name)
        {
            if (_values.ContainsKey(name.WrappedSource))
            {
                return _values[name.WrappedSource];
            }

            throw new RuntimeException(name, $"'{name.WrappedSource}' is undefined.");
        }
    }
}
