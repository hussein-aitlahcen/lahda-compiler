namespace Lahda.Parser.Impl
{
    public sealed class LiteralNode : AbstractExpressionNode
    {
        public float Value { get; }

        public LiteralNode(float v) : base(NodeType.Literal)
        {
            Value = v;
        }

        public override string ToString() => $"{Value}";
    }
}