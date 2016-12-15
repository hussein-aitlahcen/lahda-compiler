using Lahda.Common;
using Lahda.Parser;
using Lahda.Parser.Impl;

namespace Lahda
{
    public static class BuiltinFunctions
    {
        private static AbstractStatementNode DropReturn(AbstractStatementNode node) =>
            new BlockNode
            (
                node,
                new DropNode()
            );

        public static AbstractStatementNode InitArray(AbstractExpressionNode pointer, AbstractExpressionNode dimPointer, AbstractExpressionNode dimPointerSize) =>
            DropReturn(new CallNode
            (
                new FunctionIdentifierNode(new FunctionSymbol("init_array")),
                pointer,
                dimPointer,
                dimPointerSize
            ));

        public static AbstractStatementNode BorrowMemory(AbstractExpressionNode size) =>
            new CallNode
            (
                new FunctionIdentifierNode(new FunctionSymbol("bmem")),
                size
            );

        public static AbstractStatementNode RecoverMemory(AddressableIdentifierNode ident) =>
            DropReturn(new CallNode
            (
                new FunctionIdentifierNode(new FunctionSymbol("rmem")),
                ident
            ));

        public static AbstractStatementNode Pow(AbstractExpressionNode a, AbstractExpressionNode b) =>
            new CallNode
            (
                new FunctionIdentifierNode(new FunctionSymbol("pow")),
                a,
                b
            );
    }
}