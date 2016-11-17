namespace Lahda.Parser.Impl
{
    public sealed class BreakNode : AbstractStatementNode
    {
        public BreakNode() : base(NodeType.Break)
        {
        }

        public override string ToString() => $"BREAK";
    }
}