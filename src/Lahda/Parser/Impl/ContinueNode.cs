namespace Lahda.Parser.Impl
{
    public sealed class ContinueNode : AbstractStatementNode
    {
        public int CondId { get; }
        public ContinueNode(int condId) : base(NodeType.Continue)
        {
            CondId = condId;
        }

        public override string ToString() => $"CONTINUE";
    }
}