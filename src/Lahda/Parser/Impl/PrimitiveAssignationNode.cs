using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class PrimitiveAssignationNode : AbstractAssignationNode<PrimitiveIdentifierNode, PrimitiveVariableSymbol>
    {
        public PrimitiveAssignationNode(PrimitiveIdentifierNode identifier, AbstractExpressionNode expression) : base(identifier, expression) { }
    }
}