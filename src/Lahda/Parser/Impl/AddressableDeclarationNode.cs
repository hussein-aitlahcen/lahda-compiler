namespace Lahda.Parser
{
    public sealed class AddressableDeclarationNode : AbstractDeclarationNode
    {
        public AddressableIdentifierNode Identifier { get; private set; }

        public AddressableDeclarationNode(AddressableIdentifierNode identifier, AbstractExpressionNode expression) : base(expression)
        {
            Identifier = identifier;
        }

        public override string ToString() => $"DECL {Identifier} = {Expression}";
    }
}