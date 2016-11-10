namespace Lahda.Parser.Impl
{
    public sealed class OperationNode : AbstractExpressionNode
    {
        public AbstractExpressionNode Left { get; }
        public AbstractExpressionNode Right { get; }
        public string Operator { get; set; }

        public OperationNode(string op, AbstractExpressionNode left, AbstractExpressionNode right)
            : base(NodeType.Operation)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override string ToString() => $"({Left} {Operator} {Right})";
    }
}