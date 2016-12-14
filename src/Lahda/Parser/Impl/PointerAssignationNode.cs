
namespace Lahda.Parser
{
    public sealed class PointerAssignationNode : AbstractStatementNode
    {
        public AbstractExpressionNode AddressExpression { get; private set; }

        public AbstractExpressionNode ValueExpression { get; private set; }

        public PointerAssignationNode(AbstractExpressionNode address, AbstractExpressionNode value) : base(NodeType.PointerAssignation)
        {
            AddressExpression = address;
            ValueExpression = value;
        }

        public override void OptimizeChilds()
        {
            AddressExpression.OptimizeChilds();
            AddressExpression = AddressExpression.Optimize();
            ValueExpression.OptimizeChilds();
            ValueExpression = ValueExpression.Optimize();
        }

        public override string ToString() => $"ASSIGN *({AddressExpression}) = {ValueExpression}";
    }
}