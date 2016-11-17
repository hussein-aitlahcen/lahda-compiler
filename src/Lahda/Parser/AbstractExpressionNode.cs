namespace Lahda.Parser.Impl
{
    public abstract class AbstractExpressionNode : AbstractNode
    {
        public AbstractExpressionNode(NodeType type) : base(type)
        {
        }

        public virtual AbstractExpressionNode Optimize() => this;
    }
}