namespace Lahda.Parser
{
    public enum NodeType
    {
        None,
        Root,
        Drop,
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