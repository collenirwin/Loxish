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
        /// The <see cref="EnvironmentState"/> enclosing this one.
        /// </summary>
        public EnvironmentState OuterEnvironment { get; }

        /// <summary>
        /// Initializes an <see cref="EnvironmentState"/>, optionally with an enclosing environment.
        /// </summary>
        /// <param name="outerEnvironment">The <see cref="EnvironmentState"/> enclosing this one.</param>
        public EnvironmentState(EnvironmentState outerEnvironment = null)
        {
            OuterEnvironment = outerEnvironment;
        }

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
            if (_values.ContainsKey(name.WrappedSource))
            {
                _values[name.WrappedSource] = value;
                return;
            }

            if (OuterEnvironment != null)
            {
                OuterEnvironment.Assign(name, value);
                return;
            }

            throw new RuntimeException(name, $"'{name.WrappedSource}' is undefined.");
        }

        /// <summary>
        /// Gets the value the corresponds with the name token.
        /// </summary>
        /// <param name="name">The variable name token.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The value the corresponds with the name token.</returns>
        public object GetValue(Token name)
        {
            // prioritize this environment's variable
            if (_values.ContainsKey(name.WrappedSource))
            {
                return _values[name.WrappedSource];
            }
            
            // then go to the outside world to find the variable
            if (OuterEnvironment != null)
            {
                return OuterEnvironment.GetValue(name);
            }

            throw new RuntimeException(name, $"'{name.WrappedSource}' is undefined.");
        }
    }
}
