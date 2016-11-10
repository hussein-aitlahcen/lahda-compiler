namespace Lahda.Parser.Impl
{
    public sealed class DeclarationNode : AbstractStatementNode
    {
        public IdentifierNode Identifier { get; }
        public AbstractExpressionNode Expression { get; }

        public DeclarationNode(IdentifierNode identifier, AbstractExpressionNode expression) : base(NodeType.Declaration)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override string ToString() => $"DECL {Identifier} = {Expression}";
    }
}