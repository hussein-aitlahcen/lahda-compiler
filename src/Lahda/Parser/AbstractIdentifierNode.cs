using Lahda.Common;

namespace Lahda.Parser
{
    public abstract class AbstractIdentifierNode<T> : AbstractExpressionNode
        where T : AbstractSymbol
    {
        public T Symbol { get; }

        public AbstractIdentifierNode(T symbol) : base(NodeType.Identifier)
        {
            Symbol = symbol;
        }

        public override string ToString() => $"{Symbol.Name}";
    }
}