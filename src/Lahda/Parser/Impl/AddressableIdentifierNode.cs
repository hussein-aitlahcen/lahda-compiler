using Lahda.Common;

namespace Lahda.Parser
{
    public sealed class AddressableIdentifierNode : AbstractIdentifierNode
    {
        public AbstractAddressableSymbol Symbol { get; }

        public AddressableIdentifierNode(AbstractAddressableSymbol symbol)
        {
            Symbol = symbol;
        }

        public override string ToString() => $"{Symbol.Name}";
    }
}