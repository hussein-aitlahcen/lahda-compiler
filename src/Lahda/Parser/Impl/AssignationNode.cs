
namespace Lahda.Parser
{
    public sealed class AssignationNode : AbstractStatementNode
    {
        public AddressableIdentifierNode Identifier { get; }

        public AbstractExpressionNode ValueExpression { get; private set; }

        public AssignationNode(AddressableIdentifierNode identifier, AbstractExpressionNode value) : base(NodeType.Assignation)
        {
            Identifier = identifier;
            ValueExpression = value;
        }

        public override void OptimizeChilds()
        {
            ValueExpression.OptimizeChilds();
            ValueExpression = ValueExpression.Optimize();
        }

        public override string ToString() => $"ASSIGN {Identifier} = {ValueExpression}";
    }
}