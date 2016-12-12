using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class ArrayIdentifierNode : AbstractIdentifierNode<ArrayVariableSymbol>
    {
        public AbstractExpressionNode IndexExpression { get; }
        public ArrayIdentifierNode(ArrayVariableSymbol symbol, AbstractExpressionNode indexExpression) : base(symbol)
        {
            IndexExpression = indexExpression;
        }

        public override string ToString() => $"{Symbol.Name}[{IndexExpression}]";
    }
}