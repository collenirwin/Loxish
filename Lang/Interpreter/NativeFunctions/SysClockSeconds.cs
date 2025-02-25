﻿using System;
using System.Collections.Generic;

namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Native function that gets the current date and time as fractional seconds.
    /// </summary>
    public class SysClockSeconds : NativeFunctionBase
    {
        public override string Name { get; } = "__SysClockSeconds";
        public override int ParamCount { get; } = 0;

        public override object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }
    }
}
