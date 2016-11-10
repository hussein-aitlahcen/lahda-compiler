namespace Lahda.Parser.Impl
{
    public sealed class ConditionalNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; }
        public BlockNode TrueStatements { get; }
        public BlockNode FalseStatements { get; }

        public ConditionalNode(AbstractExpressionNode expression, BlockNode trueStatements, BlockNode falseStatements) : base(NodeType.Conditional)
        {
            Expression = expression;
            TrueStatements = trueStatements;
            FalseStatements = falseStatements;
        }

        public override string ToString() => $"IF {Expression} THEN\n{TrueStatements}\nELSE\n{FalseStatements}";
    }
}