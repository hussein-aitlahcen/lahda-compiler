namespace Lahda.Parser.Impl
{
    public sealed class BreakNode : AbstractStatementNode
    {
        public int LoopId { get; }

        public BreakNode(int loopId) : base(NodeType.Break)
        {
            LoopId = loopId;
        }

        public override string ToString() => $"BREAK";
    }
}