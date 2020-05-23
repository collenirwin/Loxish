namespace Lang.Interpreter
{
    /// <summary>
    /// Definines a visitor contract for each <see cref="StatementBase"/> type.
    /// </summary>
    public interface IStatementVisitor
    {
        void VisitExpressionStatement(ExpressionStatement statement);
        void VisitPrintStatement(PrintStatement statement);
        void VisitVarStatement(VarStatement statement);
        void VisitBlockStatement(BlockStatement statement);
    }
}
