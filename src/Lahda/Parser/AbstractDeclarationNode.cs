using Lahda.Common;

namespace Lahda.Parser
{
    public abstract class AbstractDeclarationNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; private set; }

        public AbstractDeclarationNode(AbstractExpressionNode expression) : base(NodeType.Declaration)
        {
            Expression = expression;
        }

        public override void OptimizeChilds()
        {
            Expression.OptimizeChilds();
            Expression = Expression.Optimize();
        }
    }
}