namespace Lahda.Parser.Impl
{
    public sealed class LiteralNode : AbstractExpressionNode
    {
        public static LiteralNode Zero = new LiteralNode(0);
        public static LiteralNode One = new LiteralNode(1);
        public static LiteralNode False = Zero;
        public static LiteralNode True = One;

        public float Value { get; }

        public LiteralNode(bool v) : this(v ? 1 : 0)
        {
        }

        public LiteralNode(float v) : base(NodeType.Literal)
        {
            Value = v;
        }

        public override string ToString() => $"{Value}";
    }
}