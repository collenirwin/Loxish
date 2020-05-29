using System.Collections.Generic;

namespace Lang.Interpreter
{
    /// <summary>
    /// Base class for all expressions.
    /// </summary>
    public abstract class ExpressionBase
    {
        public abstract T Accept<T>(IExpressionVisitor<T> visitor);
    }

    /// <summary>
    /// Represents a unary expression (an operator and an operand).
    /// </summary>
    public class UnaryExpression : ExpressionBase
    {
        public ExpressionBase Operand { get; }
        public Token Operator { get; }

        public UnaryExpression(ExpressionBase operand, Token @operator)
        {
            Operand = operand;
            Operator = @operator;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }

    /// <summary>
    /// Represents a binary expression (2 operands and an operator).
    /// </summary>
    public class BinaryExpression : ExpressionBase
    {
        public ExpressionBase LeftOperand { get; }
        public ExpressionBase RightOperand { get; }
        public Token Operator { get; }

        public BinaryExpression(ExpressionBase left, ExpressionBase right, Token @operator)
        {
            LeftOperand = left;
            RightOperand = right;
            Operator = @operator;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }

    /// <summary>
    /// Represents an expression enclosed by parens.
    /// </summary>
    public class GroupingExpression : ExpressionBase
    {
        public ExpressionBase Expression { get; }

        public GroupingExpression(ExpressionBase expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }

    /// <summary>
    /// An object literal with a value.
    /// </summary>
    public class LiteralExpression : ExpressionBase
    {
        public object Value { get; }

        public LiteralExpression(object value)
        {
            Value = value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }

    public class VariableExpression : ExpressionBase
    {
        public Token Name { get; }

        public VariableExpression(Token name)
        {
            Name = name;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }

    public class AssignmentExpression : ExpressionBase
    {
        public Token Name { get; }
        public Token Operator { get; }
        public ExpressionBase Value { get; }

        public AssignmentExpression(Token name, Token @operator, ExpressionBase value)
        {
            Name = name;
            Operator = @operator;
            Value = value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }

    public class LogicalExpression : ExpressionBase
    {
        public ExpressionBase LeftOperand { get; }
        public ExpressionBase RightOperand { get; }
        public Token Operator { get; }

        public LogicalExpression(ExpressionBase left, ExpressionBase right, Token @operator)
        {
            LeftOperand = left;
            RightOperand = right;
            Operator = @operator;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpression(this);
        }
    }

    public class CallExpression : ExpressionBase
    {
        public ExpressionBase Callee { get; }
        public IEnumerable<ExpressionBase> Arguments { get; }
        public Token ClosingParen { get; }

        public CallExpression(ExpressionBase callee,
            IEnumerable<ExpressionBase> arguments, Token closingParen)
        {
            Callee = callee;
            Arguments = arguments;
            ClosingParen = closingParen;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }

    public class FunctionExpression : ExpressionBase
    {
        public IEnumerable<Token> Params { get; }
        public IEnumerable<StatementBase> Body { get; }

        public FunctionExpression(IEnumerable<Token> @params, IEnumerable<StatementBase> body)
        {
            Params = @params;
            Body = body;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitFunctionExpression(this);
        }
    }

    public class SingleLineFunctionExpression : FunctionExpression
    {
        public SingleLineFunctionExpression(IEnumerable<Token> @params, StatementBase body)
            : base(@params, new[] { body })
        {
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSingleLineFunctionExpression(this);
        }
    }

    public class GetExpression : ExpressionBase
    {
        public ExpressionBase Object { get; }
        public Token Name { get; }

        public GetExpression(ExpressionBase @object, Token name)
        {
            Object = @object;
            Name = name;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGetExpression(this);
        }
    }

    public class SetExpression : ExpressionBase
    {
        public ExpressionBase Object { get; }
        public Token Name { get; }
        public ExpressionBase Value { get; }

        public SetExpression(ExpressionBase @object, Token name, ExpressionBase value)
        {
            Object = @object;
            Name = name;
            Value = value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetExpression(this);
        }
    }
}
