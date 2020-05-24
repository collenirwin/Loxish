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
        public IEnumerable<StatementBase> Statements { get; }

        public BlockStatement(IEnumerable<StatementBase> statements)
        {
            Statements = statements;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlockStatement(this);
        }
    }

    public class IfStatement : StatementBase
    {
        public ExpressionBase Condition { get; }
        public StatementBase ThenBranch { get; }
        public StatementBase ElseBranch { get; }

        public IfStatement(ExpressionBase condition, StatementBase thenBranch, StatementBase elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitIfStatement(this);
        }
    }

    public class WhileStatement : StatementBase
    {
        public ExpressionBase Condition { get; }
        public StatementBase Body { get; }

        public WhileStatement(ExpressionBase condition, StatementBase body)
        {
            Condition = condition;
            Body = body;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitWhileStatement(this);
        }
    }

    public class BreakStatement : StatementBase
    {
        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBreakStatement(this);
        }
    }

    public class FunctionStatement : StatementBase
    {
        public Token Name { get; }
        public IEnumerable<Token> Params { get; }
        public IEnumerable<StatementBase> Body { get; }

        public FunctionStatement(Token name,
            IEnumerable<Token> @params, IEnumerable<StatementBase> body)
        {
            Name = name;
            Params = @params;
            Body = body;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitFunctionStatement(this);
        }
    }

    public class ReturnStatement : StatementBase
    {
        public Token Keyword { get; }
        public ExpressionBase Value { get; }

        public ReturnStatement(Token keyword, ExpressionBase value)
        {
            Keyword = keyword;
            Value = value;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }
    }
}
