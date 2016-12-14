namespace Lahda.Parser
{
    public abstract class AbstractExpressionNode : AbstractStatementNode
    {
        public AbstractExpressionNode(NodeType type) : base(type)
        {
        }

        public virtual AbstractExpressionNode Optimize() => this;
    }
}