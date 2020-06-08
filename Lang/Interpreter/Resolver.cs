using System.Collections.Generic;
using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// Walks the AST, resolving variable scope.
    /// </summary>
    public class Resolver : IExpressionVisitor<object>, IStatementVisitor
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType _currentFunctionType = FunctionType.None;
        private ClassType _currentClassType = ClassType.None;

        public ErrorState ErrorState { get; }

        public Resolver(Interpreter interpreter, ErrorState errorState)
        {
            _interpreter = interpreter;
            ErrorState = errorState;
        }

        #region Expressions

        public object VisitAssignmentExpression(AssignmentExpression expression)
        {
            expression.Value.Accept(this);
            ResolveLocal(expression, expression.Name);
            return null;
        }

        public object VisitBinaryExpression(BinaryExpression expression)
        {
            expression.LeftOperand.Accept(this);
            expression.RightOperand.Accept(this);
            return null;
        }

        public object VisitCallExpression(CallExpression expression)
        {
            expression.Callee.Accept(this);

            foreach (var argument in expression.Arguments)
            {
                argument.Accept(this);
            }

            return null;
        }

        public object VisitFunctionExpression(FunctionExpression expression)
        {
            ResolveFunction(expression, FunctionType.Function);
            return null;
        }

        public object VisitGroupingExpression(GroupingExpression expression)
        {
            expression.Expression.Accept(this);
            return null;
        }

        public object VisitLiteralExpression(LiteralExpression expression) => null;

        public object VisitLogicalExpression(LogicalExpression expression)
        {
            expression.LeftOperand.Accept(this);
            expression.RightOperand.Accept(this);
            return null;
        }

        public object VisitUnaryExpression(UnaryExpression expression)
        {
            expression.Operand.Accept(this);
            return null;
        }

        public object VisitVariableExpression(VariableExpression expression)
        {
            if (_scopes.Any() &&
                _scopes.Peek().TryGetValue(expression.Name.WrappedSource, out bool initialized) &&
                !initialized)
            {
                ErrorState.AddError(expression.Name, "Cannot read from local variable in its own initializer.");
            }

            ResolveLocal(expression, expression.Name);
            return null;
        }

        public object VisitGetExpression(GetExpression expression)
        {
            expression.Object.Accept(this);
            return null;
        }

        public object VisitSetExpression(SetExpression expression)
        {
            expression.Object.Accept(this);
            expression.Value.Accept(this);
            return null;
        }

        public object VisitThisExpression(ThisExpression expression)
        {
            if (_currentClassType == ClassType.None)
            {
                ErrorState.AddError(expression.Keyword, "Cannot use 'this' outside of a class.");
            }
            else
            {
                ResolveLocal(expression, expression.Keyword);
            }
            
            return null;
        }

        #endregion

        #region Statements

        public void VisitBlockStatement(BlockStatement statement)
        {
            BeginScope();
            Resolve(statement.Statements);
            EndScope();
        }

        public void VisitBreakStatement(BreakStatement statement)
        {
        }

        public void VisitExpressionStatement(ExpressionStatement statement)
        {
            statement.Expression.Accept(this);
        }

        public void VisitFunctionStatement(FunctionStatement statement)
        {
            Declare(statement.Name);
            Define(statement.Name);
            ResolveFunction(statement.Function, FunctionType.Function);
        }

        public void VisitVarStatement(VarStatement statement)
        {
            Declare(statement.Name);
            statement.Initializer?.Accept(this);
            Define(statement.Name);
        }

        public void VisitIfStatement(IfStatement statement)
        {
            statement.Condition.Accept(this);
            statement.ThenBranch.Accept(this);
            statement.ElseBranch?.Accept(this);
        }

        public void VisitWhileStatement(WhileStatement statement)
        {
            statement.Condition.Accept(this);
            statement.Body.Accept(this);
        }

        public void VisitPrintStatement(PrintStatement statement)
        {
            statement.Expression.Accept(this);
        }

        public void VisitReturnStatement(ReturnStatement statement)
        {
            if (_currentFunctionType == FunctionType.None)
            {
                ErrorState.AddError(statement.Keyword, "Cannot return from top-level code.");
            }

            statement.Value?.Accept(this);
        }

        public void VisitClassStatement(ClassStatement statement)
        {
            var enclosingClassType = _currentClassType;
            _currentClassType = ClassType.Class;

            Declare(statement.Name);
            Define(statement.Name);

            BeginScope();
            // we'll directly add 'this' as a variable scoped locally to the class
            _scopes.Peek().Add("this", true);

            foreach (var method in statement.Methods)
            {
                ResolveFunction(method.Function, FunctionType.Method);
            }

            EndScope();
            _currentClassType = enclosingClassType;
        }

        #endregion

        #region Helpers

        public void Resolve(IEnumerable<StatementBase> statements)
        {
            foreach (var statement in statements)
            {
                statement.Accept(this);
            }
        }

        private void ResolveLocal(ExpressionBase expression, Token name)
        {
            int index = 0;
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(name.WrappedSource))
                {
                    _interpreter.Resolve(expression, index);
                    return;
                }

                index++;
            }
        }
        private void ResolveFunction(FunctionExpression function, FunctionType type)
        {
            var enclosingFunctionType = _currentFunctionType;
            _currentFunctionType = type;
            BeginScope();

            foreach (var param in function.Params)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.Body);
            EndScope();
            _currentFunctionType = enclosingFunctionType;
        }


        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (_scopes.Any())
            {
                var scope = _scopes.Peek();

                if (scope.ContainsKey(name.WrappedSource))
                {
                    ErrorState.AddError(name, $"Variable '{name.WrappedSource}' already delcared in this scope.");
                }

                scope[name.WrappedSource] = false;
            }
        }

        private void Define(Token name)
        {
            if (_scopes.Any())
            {
                _scopes.Peek()[name.WrappedSource] = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Defines the available types of functions.
    /// </summary>
    enum FunctionType
    {
        None,
        Function,
        Method
    }

    /// <summary>
    /// Defines the available types of classes.
    /// </summary>
    enum ClassType
    {
        None,
        Class
    }
}
