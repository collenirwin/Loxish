namespace Lang.Interpreter
{
    /// <summary>
    /// Base class for all statements.
    /// </summary>
    public abstract class StatementBase
    {
    }

    public class ExpressionStatement : StatementBase
    {
        public ExpressionBase Expression { get; }

        public ExpressionStatement(ExpressionBase expression)
        {
            Expression = expression;
        }
    }

    public class PrintStatement : StatementBase
    {
        public ExpressionBase Expression { get; }

        public PrintStatement(ExpressionBase expression)
        {
            Expression = expression;
        }
    }
}
