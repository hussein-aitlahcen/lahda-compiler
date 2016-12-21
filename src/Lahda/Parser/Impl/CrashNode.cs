namespace Lahda.Parser.Impl
{
    public sealed class CrashNode : AbstractStatementNode
    {
        public CrashNode() : base(NodeType.Crash)
        {
        }

        public override string ToString() => $"CRASH";
    }
}