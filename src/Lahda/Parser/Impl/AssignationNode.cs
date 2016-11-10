namespace Lahda.Parser.Impl
{
    public sealed class AssignationNode : AbstractStatementNode
    {
        public IdentifierNode Identifier { get; }

        public AbstractExpressionNode Expression { get; }

        public AssignationNode(IdentifierNode identifier, AbstractExpressionNode expression) : base(NodeType.Assignation)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override string ToString() => $"ASSIGN {Identifier} = {Expression}";
    }
}