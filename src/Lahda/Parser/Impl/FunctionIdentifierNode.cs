using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class FunctionIdentifierNode : AbstractIdentifierNode<FunctionSymbol>
    {
        public FunctionIdentifierNode(FunctionSymbol symbol) : base(symbol) { }
    }
}