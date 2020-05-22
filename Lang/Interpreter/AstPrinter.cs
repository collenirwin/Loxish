using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// Creates a string representation of AST nodes.
    /// </summary>
    internal class AstPrinter : IExpressionVisitor<string>
    {
        /// <summary>
        /// Creates a string representation of AST nodes.
        /// </summary>
        /// <param name="expression">Expression to print.</param>
        /// <returns>A string representation of AST nodes in the expression.</returns>
        public string Print(ExpressionBase expression)
        {
            return expression.Accept(this);
        }

        public string VisitBinaryExpression(BinaryExpression expression)
        {
            return Parenthesize(expression.Operator.WrappedSource, expression.LeftOperand, expression.RightOperand);
        }

        public string VisitGroupingExpression(GroupingExpression expression)
        {
            return Parenthesize("group", expression.Expression);
        }

        public string VisitLiteralExpression(LiteralExpression expression)
        {
            return expression.Value?.ToString() ?? "null";
        }

        public string VisitUnaryExpression(UnaryExpression expression)
        {
            return Parenthesize(expression.Operator.WrappedSource, expression.Operand);
        }

        private string Parenthesize(string name, params ExpressionBase[] expressions) =>
            $"({name} {string.Join(" ", expressions.Select(expression => expression.Accept(this)))})";
    }
}
