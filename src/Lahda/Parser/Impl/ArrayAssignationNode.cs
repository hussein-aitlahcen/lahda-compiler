using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class ArrayAssignationNode : AbstractAssignationNode<ArrayIdentifierNode, ArrayVariableSymbol>
    {
        public ArrayAssignationNode(ArrayIdentifierNode identifier, AbstractExpressionNode expression) : base(identifier, expression)
        {
        }
    }
}