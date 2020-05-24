using System;
using System.Collections.Generic;

namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Native function that gets the current date and time as fractional seconds.
    /// </summary>
    public class SysClockSeconds : NativeFunctionBase, ICallable
    {
        public override string Name { get; } = "__SysClockSeconds";
        public int ArgumentCount { get; } = 0;

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }
    }
}
