namespace Lahda.Parser.Impl
{
    public sealed class IdentifierNode : AbstractExpressionNode
    {
        public string Value { get; }

        public IdentifierNode(string v) : base(NodeType.Identifier)
        {
            Value = v;
        }

        public override string ToString() => $"[{Value}]";
    }
}