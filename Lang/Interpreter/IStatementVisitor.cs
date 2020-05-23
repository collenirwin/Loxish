namespace Lang.Interpreter
{
    /// <summary>
    /// Definines a visitor contract for each <see cref="StatementBase"/> type.
    /// </summary>
    /// <typeparam name="T">Type resulting from each Visit method.</typeparam>
    public interface IStatementVisitor<T>
    {
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitPrintStatement(PrintStatement statement);
    }
}
