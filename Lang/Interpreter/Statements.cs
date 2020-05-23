using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// Base class for all statements.
    /// </summary>
    public abstract class StatementBase
    {
        public abstract void Accept(IStatementVisitor visitor);
    }

    public class ExpressionStatement : StatementBase
    {
        public ExpressionBase Expression { get; }

        public ExpressionStatement(ExpressionBase expression)
        {
            Expression = expression;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }
    }

    public class PrintStatement : StatementBase
    {
        public ExpressionBase Expression { get; }

        public PrintStatement(ExpressionBase expression)
        {
            Expression = expression;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitPrintStatement(this);
        }
    }

    public class VarStatement : StatementBase
    {
        public Token Name { get; }
        public ExpressionBase Initializer { get; }

        public VarStatement(Token name, ExpressionBase initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitVarStatement(this);
        }
    }

    public class BlockStatement : StatementBase
    {
        public List<StatementBase> Statements { get; }

        public BlockStatement(List<StatementBase> statements)
        {
            Statements = statements;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlockStatement(this);
        }
    }
}
