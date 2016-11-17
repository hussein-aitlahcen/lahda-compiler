namespace Lahda.Parser.Impl
{
    public sealed class PrintNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; }

        public PrintNode(AbstractExpressionNode expression) : base(NodeType.Print)
        {
            Expression = expression;
        }

        public override string ToString() => $"PRINT {Expression}";
    }
}