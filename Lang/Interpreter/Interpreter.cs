using System;

namespace Lang.Interpreter
{
    /// <summary>
    /// Executes expressions.
    /// </summary>
    public class Interpreter : IExpressionVisitor<object>
    {
        /// <summary>
        /// Has a <see cref="RuntimeException"/> been thrown?
        /// </summary>
        public bool RuntimeExceptionThrown { get; private set; }

        public void Interpret(ExpressionBase expression)
        {
            RuntimeExceptionThrown = false;

            try
            {
                var value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeException ex)
            {
                RuntimeExceptionThrown = true;
                ErrorReporter.ReportRuntimeException(ex);
            }
        }

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
                    return RunOperationAsNumbers(left, right, expression.Operator, (l, r) => l - r);
                case TokenType.Star:
                    return RunOperationAsNumbers(left, right, expression.Operator, (l, r) => l * r);
                case TokenType.Slash:
                    return RunOperationAsNumbers(left, right, expression.Operator, (l, r) => l / r);

                case TokenType.DoubleEqual:
                    return AreEqual(left, right);
                case TokenType.BangEqual:
                    return !AreEqual(left, right);

                case TokenType.LessThan:
                    return Compare(left, right, expression.Operator) < 0;
                case TokenType.LessThanOrEqual:
                    return Compare(left, right, expression.Operator) <= 0;
                case TokenType.GreaterThan:
                    return Compare(left, right, expression.Operator) > 0;
                case TokenType.GreaterThanOrEqual:
                    return Compare(left, right, expression.Operator) >= 0;
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
        /// Runs the operation if both operands are numbers, otherise throws a <see cref="RuntimeException"/>.
        /// </summary>
        /// <typeparam name="T">Return type of the operation.</typeparam>
        /// <param name="leftOperand">Left operand to cast to a number.</param>
        /// <param name="rightOperand">Right operand to cast to a number.</param>
        /// <param name="operator">Operator to include in the exception on failure.</param>
        /// <param name="operation">Operation to perform with the operands.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The result of the operation.</returns>
        private T RunOperationAsNumbers<T>(object leftOperand, object rightOperand, Token @operator,
            Func<double, double, T> operation)
        {
            if (leftOperand is double leftNumber && rightOperand is double rightNumber)
            {
                return operation(leftNumber, rightNumber);
            }

            throw new RuntimeException(@operator, "Operands must be a numbers.");
        }

        /// <summary>
        /// Returns the result of a comparison between the left and right operands,
        /// or throw a <see cref="RuntimeException"/> if the operands are not comparable.
        /// </summary>
        /// <param name="leftOperand">Left operand to compare.</param>
        /// <param name="rightOperand">Right operand to compare.</param>
        /// <param name="operator">Operator to include in the exception on failure.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The result of the comparison.</returns>
        private int Compare(object leftOperand, object rightOperand, Token @operator)
        {
            if (leftOperand is double leftNumber && rightOperand is double rightNumber)
            {
                return leftNumber.CompareTo(rightNumber);
            }

            if (leftOperand is string leftString && rightOperand is string rightString)
            {
                return leftString.CompareTo(rightString);
            }

            throw new RuntimeException(@operator, "Both operands must be comparable to each other.");
        }

        /// <summary>
        /// Casts the operand to a number.
        /// Throws a <see cref="RuntimeException"/> if the operand is not a number.
        /// </summary>
        /// <param name="operand">Operand to cast.</param>
        /// <param name="operator">Operator to include in the exception on failure.</param>
        /// <exception cref="RuntimeException"/>
        /// <returns>The operand as a double.</returns>
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

        private string Stringify(object value)
        {
            if (value is null)
            {
                return "null";
            }

            return value.ToString();
        }
    }
}
