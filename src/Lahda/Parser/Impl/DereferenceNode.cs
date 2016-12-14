namespace Lahda.Parser.Impl
{
    public sealed class DereferenceNode : AbstractExpressionNode
    {
        public AbstractExpressionNode Expression { get; }

        public DereferenceNode(AbstractExpressionNode expression) : base(NodeType.Dereference)
        {
            Expression = expression;
        }

        public override string ToString() => $"*{Expression}";
    }
}