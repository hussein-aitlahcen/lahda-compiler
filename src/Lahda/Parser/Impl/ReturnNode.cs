namespace Lahda.Parser.Impl
{
    public sealed class ReturnNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; private set; }

        public ReturnNode(AbstractExpressionNode expression) : base(NodeType.Return)
        {
            Expression = expression;
        }

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
        }

        public override string ToString() => $"RET {Expression}";
    }
}