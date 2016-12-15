namespace Lahda.Parser
{
    public enum NodeType
    {
        None,
        Root,
        Drop,
        MultiExpression,
        Literal,
        Return,
        Operation,
        Identifier,
        Declaration,
        PointerAssignation,
        Assignation,
        Block,
        Conditional,
        Loop,
        Break,
        Continue,
        Print,
        Function,
        Call,
        Dereference,
        Reference
    }
}