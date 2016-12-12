using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class PrimitiveIdentifierNode : AbstractIdentifierNode<PrimitiveVariableSymbol>
    {
        public PrimitiveIdentifierNode(PrimitiveVariableSymbol symbol) : base(symbol) { }
    }
}