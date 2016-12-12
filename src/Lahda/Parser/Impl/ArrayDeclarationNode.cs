using Lahda.Common;

namespace Lahda.Parser.Impl
{
    public sealed class ArrayDeclarationNode : AbstractDeclarationNode<ArrayIdentifierNode, ArrayVariableSymbol>
    {
        public ArrayDeclarationNode(ArrayIdentifierNode identifier) : base(identifier, LiteralNode.Zero) { }

        public override string ToString() => $"DECL {Identifier.Symbol.Name} = new [{Identifier.IndexExpression}]";
    }
}