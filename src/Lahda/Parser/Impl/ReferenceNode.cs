namespace Lahda.Parser.Impl
{
    public sealed class ReferenceNode : AbstractExpressionNode
    {
        public AddressableIdentifierNode Identifier { get; }

        public ReferenceNode(AddressableIdentifierNode identifier) : base(NodeType.Reference)
        {
            Identifier = identifier;
        }

        public override string ToString() => $"@{Identifier}";
    }
}