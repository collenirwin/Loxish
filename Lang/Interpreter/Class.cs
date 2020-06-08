using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// A lang class.
    /// </summary>
    public class Class : ICallable
    {
        private readonly Dictionary<string, Function> _methods;

        /// <summary>
        /// The name of the class.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The (optional) superclass of this class.
        /// </summary>
        public Class SuperClass { get; }

        /// <summary>
        /// The number of arguments the constructor requires.
        /// </summary>
        public int ParamCount => TryGetMethod("init")?.ParamCount ?? 0;

        /// <summary>
        /// Initializes a <see cref="Class"/> with a name.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        public Class(string name, Dictionary<string, Function> methods, Class superClass = null)
        {
            Name = name;
            _methods = methods;
            SuperClass = superClass;
        }

        /// <summary>
        /// Calls the constructor of this class.
        /// </summary>
        /// <param name="interpreter">Interpreter calling the constructor.</param>
        /// <param name="arguments">Arguments for the constructor.</param>
        /// <returns></returns>
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var instance = new Instance(this);

            // run constructor if it exists
            TryGetMethod("init")?.Bind(instance).Call(interpreter, arguments);
            return instance;
        }

        /// <summary>
        /// Gets the method with the given name, or null if it is not defined.
        /// </summary>
        /// <param name="name">Name of the method.</param>
        /// <returns>The method <see cref="Function"/> object, or null if not found.</returns>
        public Function TryGetMethod(string name)
        {
            _methods.TryGetValue(name, out Function function);
            return function ?? SuperClass?.TryGetMethod(name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
