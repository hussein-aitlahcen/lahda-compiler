namespace Lahda.Parser.Impl
{
    public sealed class PrintStringNode : AbstractStatementNode
    {
        public string Content { get; private set; }

        public PrintStringNode(string content) : base(NodeType.Print)
        {
            Content = content;
        }

        public override string ToString() => $"PRINT {Content}";
    }
}