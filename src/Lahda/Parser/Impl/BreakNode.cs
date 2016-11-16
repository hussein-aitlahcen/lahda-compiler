namespace Lahda.Parser.Impl
{
    public sealed class BreakNode : AbstractStatementNode
    {
        public LoopNode Parent { get; }

        public override uint Id => Parent.Id;

        public BreakNode(LoopNode parent) : base(NodeType.Break)
        {
            Parent = parent;
        }

        public override string ToString() => $"BREAK";
    }
}