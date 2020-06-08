namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Contains an <see cref="All"/>
    /// field with one instance of each <see cref="NativeFunctionBase"/> class.
    /// </summary>
    public static class NativeFunctionInstances
    {
        /// <summary>
        /// Contains one of each <see cref="NativeFunctionBase"/> instance.
        /// </summary>
        public static readonly NativeFunctionBase[] All = new NativeFunctionBase[]
        {
            new SysClockSeconds(),
            new ReadLine(),
            new ReadChar()
        };
    }
}
