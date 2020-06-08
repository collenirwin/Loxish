using System.Collections.Generic;

namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Abstract base class for all native functions.
    /// </summary>
    public abstract class NativeFunctionBase : ICallable
    {
        public abstract string Name { get; }
        public abstract int ParamCount { get; }
        public abstract object Call(Interpreter interpreter, IEnumerable<object> arguments);

        public override string ToString()
        {
            return $"<native fun {Name}>";
        }
    }
}
