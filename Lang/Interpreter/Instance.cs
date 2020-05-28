using System;
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

            throw new RuntimeException(name, $"Property '{name.WrappedSource}' is undefined.");
        }

        public override string ToString()
        {
            return $"{_class.Name} instance";
        }
    }
}
