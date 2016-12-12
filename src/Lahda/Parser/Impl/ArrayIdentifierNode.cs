using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class ArrayIdentifierNode : AbstractIdentifierNode<ArrayVariableSymbol>
    {
        public ArrayIdentifierNode(ArrayVariableSymbol symbol) : base(symbol) { }
    }
}