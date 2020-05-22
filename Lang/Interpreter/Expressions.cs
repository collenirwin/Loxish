namespace Lang.Interpreter
{
    /// <summary>
    /// Base class for all expressions.
    /// </summary>
    public abstract class ExpressionBase
    {
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
    }
}
