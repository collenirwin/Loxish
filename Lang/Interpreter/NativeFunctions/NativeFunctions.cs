namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Contains an <see cref="All"/>
    /// field with one of each <see cref="NativeFunctionBase"/> instance.
    /// </summary>
    public static class NativeFunctionInstances
    {
        /// <summary>
        /// Contains one of each <see cref="NativeFunctionBase"/> instance.
        /// </summary>
        public static readonly NativeFunctionBase[] All = new[]
        {
            new SysClockSeconds()
        };
    }
}
