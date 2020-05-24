using Lang.Interpreter.NativeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// Executes expressions.
    /// </summary>
    public class Interpreter : IExpressionVisitor<object>, IStatementVisitor
    {
        private readonly EnvironmentState _globalScope = new EnvironmentState();
        private EnvironmentState _environment;

        /// <summary>
        /// Has a <see cref="RuntimeException"/> been thrown?
        /// </summary>
        public bool RuntimeExceptionThrown { get; private set; }

        #region Execution

        /// <summary>
        /// Interprets and executes the given statements.
        /// </summary>
        /// <param name="statements">Statements to execute.</param>
        public void Interpret(IEnumerable<StatementBase> statements)
        {
            RuntimeExceptionThrown = false;
            _environment = _globalScope;
            PrepareGlobalScope();

            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeException ex)
            {
                RuntimeExceptionThrown = true;
                ErrorReporter.ReportRuntimeException(ex);
            }
        }

        /// <summary>
        /// Runs the statement's <see cref="StatementBase.Accept(IStatementVisitor)"/> method.
        /// </summary>
        /// <param name="statement">Statement to execute.</param>
        private void Execute(StatementBase statement)
        {
            statement.Accept(this);
        }

        /// <summary>
        /// <see cref="Execute(StatementBase)"/>s all statements in a block.
        /// </summary>
        /// <param name="statements">Statements to execute.</param>
        /// <param name="environment">The block's environment.</param>
        private void ExecuteBlock(IEnumerable<StatementBase> statements, EnvironmentState environment)
        {
            // hang on to the current scope
            var outerEnvironment = _environment;

            try
            {
                // our scope is now this block's scope
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                // restore to the first scope
                _environment = outerEnvironment;
            }
        }

        /// <summary>
        /// Runs the expression's <see cref="ExpressionBase.Accept{T}(IExpressionVisitor{T})"/> method.
        /// This will essentially compute the value of the expression and return it.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of <see cref="ExpressionBase.Accept{T}(IExpressionVisitor{T})"/>.</returns>
        private object Evaluate(ExpressionBase expression)
        {
            return expression.Accept(this);
        }

        #endregion

        #region Expression visitation

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

                case TokenType.Amp:
                    return RunOperationAsNumbers(left, right, expression.Operator,
                        (l, r) => (double)((int)l & (int)r));
                case TokenType.Pipe:
                    return RunOperationAsNumbers(left, right, expression.Operator,
                        (l, r) => (double)((int)l | (int)r));
                case TokenType.Caret:
                    return RunOperationAsNumbers(left, right, expression.Operator,
                        (l, r) => (double)((int)l ^ (int)r));

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
        /// Evaluates a variable expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitVariableExpression(VariableExpression expression)
        {
            return _environment.GetValue(expression.Name);
        }

        /// <summary>
        /// Evaluates an assignment expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitAssignmentExpression(AssignmentExpression expression)
        {
            var value = Evaluate(expression.Value);

            if (expression.Operator.Type == TokenType.PlusEqual)
            {
                value = RunOperationAsNumbers(_environment.GetValue(expression.Name), value, expression.Operator,
                    (var, val) => var + val);
            }
            else if (expression.Operator.Type == TokenType.MinusEqual)
            {
                value = RunOperationAsNumbers(_environment.GetValue(expression.Name), value, expression.Operator,
                    (var, val) => var - val);
            }

            _environment.Assign(expression.Name, value);
            return value;
        }

        /// <summary>
        /// Evaluates a logical expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitLogicalExpression(LogicalExpression expression)
        {
            var leftValue = Evaluate(expression.LeftOperand);

            // see if we can short-circuit
            if (expression.Operator.Type == TokenType.DoublePipe) // or
            {
                if (IsTruthy(leftValue))
                {
                    return leftValue;
                }
            }
            else // and
            {
                if (!IsTruthy(leftValue))
                {
                    return leftValue;
                }
            }

            return Evaluate(expression.RightOperand);
        }

        /// <summary>
        /// Evaluates a call expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The result of the expression.</returns>
        public object VisitCallExpression(CallExpression expression)
        {
            var callee = Evaluate(expression.Callee);
            var argumentValues = expression.Arguments
                .Select(argument => Evaluate(argument))
                .ToList();

            if (!(callee is ICallable function))
            {
                throw new RuntimeException(expression.ClosingParen, "Expression does not support calling.");
            }

            if (function.ArgumentCount != argumentValues.Count)
            {
                throw new RuntimeException(expression.ClosingParen,
                    $"Expected {function.ArgumentCount} arguments but got {argumentValues.Count}.");
            }

            return function.Call(this, argumentValues);
        }

        #endregion

        #region Statement visitation

        /// <summary>
        /// Runs an expression statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitExpressionStatement(ExpressionStatement statement)
        {
            Evaluate(statement.Expression);
        }

        /// <summary>
        /// Runs a print statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitPrintStatement(PrintStatement statement)
        {
            var value = Evaluate(statement.Expression);
            Console.WriteLine(Stringify(value));
        }

        /// <summary>
        /// Runs a var statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitVarStatement(VarStatement statement)
        {
            object value = null;
            if (statement.Initializer != null)
            {
                value = Evaluate(statement.Initializer);
            }

            _environment.Define(statement.Name.WrappedSource, value);
        }

        /// <summary>
        /// Runs a block statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitBlockStatement(BlockStatement statement)
        {
            ExecuteBlock(statement.Statements, new EnvironmentState(_environment));
        }

        /// <summary>
        /// Runs an if statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitIfStatement(IfStatement statement)
        {
            if (IsTruthy(Evaluate(statement.Condition)))
            {
                Execute(statement.ThenBranch);
            }
            else if (statement.ElseBranch != null)
            {
                Execute(statement.ElseBranch);
            }
        }

        /// <summary>
        /// Runs a while statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitWhileStatement(WhileStatement statement)
        {
            try
            {
                while (IsTruthy(Evaluate(statement.Condition)))
                {
                    Execute(statement.Body);
                }
            }
            catch (BreakException) { /* exit the loop */ }
        }

        /// <summary>
        /// Runs a break statement.
        /// </summary>
        /// <param name="statement">Statement to run.</param>
        public void VisitBreakStatement(BreakStatement statement)
        {
            throw new BreakException();
        }

        #endregion

        #region Helpers

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

            return !(value is null);
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

        /// <summary>
        /// Gets a string representation of an object.
        /// </summary>
        /// <param name="value">Object to stringify.</param>
        /// <returns>A string representation of the passed object.</returns>
        private string Stringify(object value)
        {
            if (value is null)
            {
                return "null";
            }

            return value.ToString();
        }

        /// <summary>
        /// Adds all native functions to the <see cref="_globalScope"/>.
        /// </summary>
        private void PrepareGlobalScope()
        {
            _globalScope.Define("__SysClockSeconds", new SysClockSeconds());
        }

        #endregion
    }

    /// <summary>
    /// Used to signal a loop break.
    /// </summary>
    class BreakException : Exception
    {
    }
}
