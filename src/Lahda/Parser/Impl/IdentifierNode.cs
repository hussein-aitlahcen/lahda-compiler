using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class IdentifierNode : AbstractExpressionNode
    {
        public Symbol Symbol { get; }

        public IdentifierNode(Symbol symbol) : base(NodeType.Identifier)
        {
            Symbol = symbol;
        }

        public override string ToString() => $"{Symbol.Name}";
    }
}