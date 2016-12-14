using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class FunctionIdentifierNode : AbstractIdentifierNode
    {
        public FunctionSymbol Symbol { get; }

        public FunctionIdentifierNode(FunctionSymbol symbol)
        {
            Symbol = symbol;
        }
    }
}