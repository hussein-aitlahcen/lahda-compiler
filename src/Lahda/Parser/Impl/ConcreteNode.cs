namespace Lahda.Parser.Impl
{
    public sealed class ConcreteNode : AbstractStatementNode
    {
        public static ConcreteNode CONTINUE = new ConcreteNode(NodeType.Continue);
        public static ConcreteNode BREAK = new ConcreteNode(NodeType.Break);

        public ConcreteNode(NodeType type) : base(type)
        {
        }

        public override string ToString() => $"{Type}";
    }
}