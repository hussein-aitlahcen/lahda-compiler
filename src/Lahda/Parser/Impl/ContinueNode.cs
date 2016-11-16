namespace Lahda.Parser.Impl
{
    public sealed class ContinueNode : AbstractStatementNode
    {
        public LoopNode Parent { get; }

        public override uint Id => Parent.Id;

        public ContinueNode(LoopNode parent) : base(NodeType.Continue)
        {
            Parent = parent;
        }

        public override string ToString() => $"CONTINUE";
    }
}