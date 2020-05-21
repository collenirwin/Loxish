namespace Lang.Interpreter
{
    /// <summary>
    /// Contract that requires a readonly <see cref="ErrorState"/> property.
    /// </summary>
    public interface IErrorRecorder
    {
        ErrorState ErrorState { get; }
    }
}
