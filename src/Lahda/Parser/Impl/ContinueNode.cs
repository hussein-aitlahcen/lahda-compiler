namespace Lahda.Parser.Impl
{
    public sealed class ContinueNode : AbstractStatementNode
    {
        public ContinueNode() : base(NodeType.Continue)
        {
        }

        public override string ToString() => $"CONTINUE";
    }
}