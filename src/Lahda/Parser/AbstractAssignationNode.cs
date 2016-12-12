using Lahda.Common;

namespace Lahda.Parser
{
    public abstract class AbstractAssignationNode<T, K> : AbstractStatementNode
        where T : AbstractIdentifierNode<K>
        where K : AbstractSymbol
    {
        public T Identifier { get; private set; }

        public AbstractExpressionNode Expression { get; private set; }

        public AbstractAssignationNode(T identifier, AbstractExpressionNode expression) : base(NodeType.Assignation)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
        }

        public override string ToString() => $"ASSIGN {Identifier} = {Expression}";
    }
}