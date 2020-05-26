using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

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
            if (_values.ContainsKey(name))
            {
                _values[name] = value;
            }
            else
            {
                _values.Add(name, value);
            }
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
        /// Assigns a value to an existing variable.
        /// </summary>
        /// <param name="name">The variable name token.</param>
        /// <param name="value">Value of the variable.</param>
        /// <param name="distance">Scope depth difference.</param>
        /// <exception cref="RuntimeException"/>
        public void Assign(Token name, object value, int distance)
        {
            GetAncestor(distance)._values[name.WrappedSource] = value;
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

        /// <summary>
        /// Gets the value the corresponds with the name token at the given distance.
        /// </summary>
        /// <param name="name">The variable name token.</param>
        /// <param name="distance">Scope depth difference.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The value the corresponds with the name token.</returns>
        public object GetValue(Token name, int distance)
        {
            return GetAncestor(distance)._values[name.WrappedSource];
        }

        /// <summary>
        /// Gets an outer environment distance relatives away.
        /// </summary>
        /// <param name="distance">Distance the ancestor is away from this.</param>
        /// <returns>The ancestor at the given distance.</returns>
        private EnvironmentState GetAncestor(int distance)
        {
            var current = this;

            for (int depth = 0; depth < distance; depth++)
            {
                current = current.OuterEnvironment;
            }

            return current;
        }
    }
}
