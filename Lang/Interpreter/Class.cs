using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// A lang class.
    /// </summary>
    public class Class : ICallable
    {
        /// <summary>
        /// The name of the class.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of arguments the constructor requires.
        /// </summary>
        public int ParamCount { get; }

        /// <summary>
        /// Initializes a <see cref="Class"/> with a name.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        public Class(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Calls the constructor of this class.
        /// </summary>
        /// <param name="interpreter">Interpreter calling the constructor.</param>
        /// <param name="arguments">Arguments for the constructor.</param>
        /// <returns></returns>
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return new Instance(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
