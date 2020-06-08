using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// A lang instance of a <see cref="Class"/>.
    /// </summary>
    public class Instance
    {
        private readonly Class _class;
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        /// <summary>
        /// Initialize an <see cref="Instance"/> with the <see cref="Class"/> it is an instance of.
        /// </summary>
        /// <param name="class"><see cref="Class"/> this is an instance of.</param>
        public Instance(Class @class)
        {
            _class = @class;
        }

        /// <summary>
        /// Gets the value of an instance property by its name.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The value of the property.</returns>
        public object Get(Token name)
        {
            if (_properties.TryGetValue(name.WrappedSource, out object value))
            {
                return value;
            }

            return _class.TryGetMethod(name.WrappedSource)?.Bind(this) ??
                throw new RuntimeException(name, $"Property '{name.WrappedSource}' is undefined.");
        }

        /// <summary>
        /// Sets the value of an instance property by its name, creating it if it does not yet exist.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Value to assign to the property.</param>
        public void Set(Token name, object value)
        {
            if (_properties.ContainsKey(name.WrappedSource))
            {
                _properties[name.WrappedSource] = value;
            }
            else
            {
                _properties.Add(name.WrappedSource, value);
            }
        }

        public override string ToString()
        {
            return $"{_class.Name} instance";
        }
    }
}
