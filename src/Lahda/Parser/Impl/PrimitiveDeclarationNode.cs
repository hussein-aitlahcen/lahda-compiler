using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class PrimitiveDeclarationNode : AbstractDeclarationNode<PrimitiveIdentifierNode, PrimitiveVariableSymbol>
    {
        public PrimitiveDeclarationNode(PrimitiveIdentifierNode identifier, AbstractExpressionNode expression) : base(identifier, expression) { }
    }
}