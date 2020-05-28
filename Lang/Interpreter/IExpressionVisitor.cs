namespace Lang.Interpreter
{
    /// <summary>
    /// Definines a visitor contract for each <see cref="ExpressionBase"/> type.
    /// </summary>
    /// <typeparam name="T">Type resulting from each Visit method.</typeparam>
    public interface IExpressionVisitor<T>
    {
        T VisitUnaryExpression(UnaryExpression expression);
        T VisitBinaryExpression(BinaryExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        T VisitVariableExpression(VariableExpression expression);
        T VisitAssignmentExpression(AssignmentExpression expression);
        T VisitLogicalExpression(LogicalExpression expression);
        T VisitCallExpression(CallExpression expression);
        T VisitFunctionExpression(FunctionExpression expression);
        T VisitGetExpression(GetExpression expression);
    }
}
