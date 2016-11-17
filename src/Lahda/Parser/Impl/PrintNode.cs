namespace Lahda.Parser.Impl
{
    public sealed class PrintNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; private set; }

        public PrintNode(AbstractExpressionNode expression) : base(NodeType.Print)
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