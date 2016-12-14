namespace Lahda.Parser.Impl
{
    public sealed class DropNode : AbstractStatementNode
    {
        public DropNode() : base(NodeType.Drop)
        {
        }

        public override string ToString() => $"DROP";
    }
}