namespace Lang.Interpreter.NativeFunctions
{
    /// <summary>
    /// Abstract base class for all native functions.
    /// </summary>
    public abstract class NativeFunctionBase
    {
        public abstract string Name { get; }

        public override string ToString()
        {
            return $"<native fun {Name}>";
        }
    }
}
