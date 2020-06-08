using System;
using System.Collections.Generic;

namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Native function that returns a character of standard input from the user.
    /// </summary>
    public class ReadChar : NativeFunctionBase
    {
        public override string Name { get; } = "readchar";
        public override int ParamCount { get; } = 0;

        public override object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return Console.ReadKey().KeyChar.ToString();
        }
    }
}
