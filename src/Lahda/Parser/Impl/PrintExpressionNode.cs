namespace Lahda.Parser.Impl
{
    public sealed class PrintExpressionNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; private set; }

        public PrintExpressionNode(AbstractExpressionNode expression) : base(NodeType.Print)
        {
            Expression = expression;
        }

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
        }

        public override string ToString() => $"PRINT {Expression}";
    }
}