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
}
