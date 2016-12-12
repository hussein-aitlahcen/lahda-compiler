using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class ArrayDeclarationNode : AbstractDeclarationNode<ArrayIdentifierNode, ArrayVariableSymbol>
    {
        public ArrayDeclarationNode(ArrayIdentifierNode identifier) : base(identifier, LiteralNode.Zero) { }
    }
}