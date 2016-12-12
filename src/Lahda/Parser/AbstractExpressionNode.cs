namespace Lahda.Parser
{
    public abstract class AbstractExpressionNode : AbstractNode
    {
        public AbstractExpressionNode(NodeType type) : base(type)
        {
        }

        public virtual AbstractExpressionNode Optimize() => this;
    }
}