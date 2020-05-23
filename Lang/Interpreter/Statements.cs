namespace Lang.Interpreter
{
    /// <summary>
    /// Base class for all statements.
    /// </summary>
    public abstract class StatementBase
    {
        public abstract T Accept<T>(IStatementVisitor<T> visitor);
    }

    public class ExpressionStatement : StatementBase
    {
        public ExpressionBase Expression { get; }

        public ExpressionStatement(ExpressionBase expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }

    public class PrintStatement : StatementBase
    {
        public ExpressionBase Expression { get; }

        public PrintStatement(ExpressionBase expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitPrintStatement(this);
        }
    }
}
