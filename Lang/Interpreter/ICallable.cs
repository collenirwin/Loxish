using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// Contract for all lang objects that can be invoked.
    /// </summary>
    public interface ICallable
    {
        int ArgumentCount { get; }
        object Call(Interpreter interpreter, IEnumerable<object> arguments);
    }
}
