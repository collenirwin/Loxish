namespace Lang.Interpreter
{
    /// <summary>
    /// Executes expressions.
    /// </summary>
    public class Interpreter : IExpressionVisitor<object>
    {
        /// <summary>
        /// Evaluates a binary expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitBinaryExpression(BinaryExpression expression)
        {
            var left = Evaluate(expression.LeftOperand);
            var right = Evaluate(expression.RightOperand);

            switch (expression.Operator.Type)
            {
                case TokenType.Plus:
                    if (left is double leftNumber && right is double rightNumber)
                    {
                        return leftNumber + rightNumber;
                    }

                    if (left is string leftString)
                    {
                        return leftString + right.ToString();
                    }

                    throw new RuntimeException(expression.Operator, "Invalid operand(s) for '+'.");
                case TokenType.Minus:
                    AssertOperandsAreNumbers(left, right, expression.Operator);
                    return (double)left - (double)right;
                case TokenType.Star:
                    AssertOperandsAreNumbers(left, right, expression.Operator);
                    return (double)left * (double)right;
                case TokenType.Slash:
                    AssertOperandsAreNumbers(left, right, expression.Operator);
                    return (double)left / (double)right;

                case TokenType.DoubleEqual:
                    return AreEqual(left, right);
                case TokenType.BangEqual:
                    return !AreEqual(left, right);

                case TokenType.LessThan:
                    AssertOperandsAreComparable(left, right, expression.Operator);
                    return (double)left < (double)right;
                case TokenType.LessThanOrEqual:
                    AssertOperandsAreComparable(left, right, expression.Operator);
                    return (double)left <= (double)right;
                case TokenType.GreaterThan:
                    AssertOperandsAreComparable(left, right, expression.Operator);
                    return (double)left > (double)right;
                case TokenType.GreaterThanOrEqual:
                    AssertOperandsAreComparable(left, right, expression.Operator);
                    return (double)left >= (double)right;
            }

            throw new RuntimeException(expression.Operator, "Invalid binary expression.");
        }

        /// <summary>
        /// Evaluates a grouping expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitGroupingExpression(GroupingExpression expression)
        {
            return Evaluate(expression.Expression);
        }

        /// <summary>
        /// Evaluates a literal expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitLiteralExpression(LiteralExpression expression)
        {
            return expression.Value;
        }

        /// <summary>
        /// Evaluates a unary expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitUnaryExpression(UnaryExpression expression)
        {
            var operand = Evaluate(expression.Operand);

            return expression.Operator.Type switch
            {
                TokenType.Minus => -CastOperandToNumber(operand, expression.Operator),
                TokenType.Bang => !IsTruthy(operand),
                _ => throw new RuntimeException(expression.Operator, "Invalid unary expression."),
            };
        }

        /// <summary>
        /// Runs the expression's <see cref="ExpressionBase.Accept{T}(IExpressionVisitor{T})"/>
        /// method.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of <see cref="ExpressionBase.Accept{T}(IExpressionVisitor{T})"/>.</returns>
        private object Evaluate(ExpressionBase expression)
        {
            return expression.Accept(this);
        }

        /// <summary>
        /// Determines if an object is 'truthy',
        /// which is to say that it is not null, not false, and not 0.
        /// </summary>
        /// <param name="value">Object to check.</param>
        /// <returns>True if the object is determined to be 'truthy'.</returns>
        private bool IsTruthy(object value)
        {
            if (value is bool b)
            {
                return b;
            }

            if (value is double d)
            {
                return d != 0.0;
            }

            return true;
        }

        /// <summary>
        /// Throws a <see cref="RuntimeException"/> if either operand is not a number.
        /// </summary>
        /// <param name="leftOperand">Left operand to check.</param>
        /// <param name="rightOperand">Right operand to check.</param>
        /// <param name="operator">Operator to include in the exception on failure.</param>
        /// <exception cref="RuntimeException"/>
        private void AssertOperandsAreNumbers(object leftOperand, object rightOperand, Token @operator)
        {
            if (!(leftOperand is double) || !(rightOperand is double))
            {
                throw new RuntimeException(@operator, "Operands must be a numbers.");
            }
        }

        /// <summary>
        /// Throws a <see cref="RuntimeException"/> if operands are not both doubles or both strings.
        /// </summary>
        /// <param name="leftOperand">Left operand to check.</param>
        /// <param name="rightOperand">Right operand to check.</param>
        /// <param name="operator">Operator to include in the exception on failure.</param>
        /// <exception cref="RuntimeException"/>
        private void AssertOperandsAreComparable(object leftOperand, object rightOperand, Token @operator)
        {
            if ((!(leftOperand is double) || !(rightOperand is double)) &&
                (!(leftOperand is string) || !(rightOperand is string)))
            {
                throw new RuntimeException(@operator, "Operands must be a numbers or strings.");
            }
        }

        /// <summary>
        /// Casts the operand to a number.
        /// Throws a <see cref="RuntimeException"/> if the operand is not a number.
        /// </summary>
        /// <param name="operand">Operand to cast.</param>
        /// <param name="operator">Operator to include in the exception on failure.</param>
        /// <exception cref="RuntimeException"/>
        private double CastOperandToNumber(object operand, Token @operator)
        {
            if (operand is double number)
            {
                return number;
            }

            throw new RuntimeException(@operator, "Operand must be a number.");
        }

        /// <summary>
        /// Determines if two object are equal.
        /// </summary>
        /// <param name="value1">First object.</param>
        /// <param name="value2">Second object.</param>
        /// <returns>True if the objects are equal.</returns>
        private bool AreEqual(object value1, object value2)
        {
            if (value1 is null && value2 is null)
            {
                return true;
            }

            return value1?.Equals(value2) ?? false;
        }
    }
}
