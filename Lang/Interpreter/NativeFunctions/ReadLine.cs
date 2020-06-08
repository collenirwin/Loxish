using System;
using System.Collections.Generic;

namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Native function that returns a line of standard input from the user.
    /// </summary>
    public class ReadLine : NativeFunctionBase
    {
        public override string Name { get; } = "readline";
        public override int ParamCount { get; } = 0;

        public override object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return Console.ReadLine();
        }
    }
}
