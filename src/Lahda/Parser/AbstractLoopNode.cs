namespace Lahda.Parser.Impl
{
    public abstract class AbstractLoopNode : AbstractStatementNode
    {
        public AbstractLoopNode() : base(NodeType.Loop)
        {
        }
    }
}