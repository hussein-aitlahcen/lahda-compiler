using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class LoopNode : AbstractLoopNode
    {
        public ConditionalNode Conditional { get; }
        public AbstractStatementNode Iteration { get; }


        public LoopNode(AbstractExpressionNode condition, AbstractStatementNode iteration, AbstractStatementNode statement, bool reverse = false)
        {
            if (reverse)
                condition = new OperationNode(OperatorType.Equals, condition, new LiteralNode(0));
            Conditional = new ConditionalNode(condition, statement, new BlockNode(new BreakNode()));
            Iteration = iteration;
        }

        public LoopNode(AbstractExpressionNode condition, AbstractStatementNode statement, bool reverse = false)
            : this(condition, new BlockNode(), statement, reverse)
        {
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine("WHILE");
            sb.AppendLine(Conditional.ToString(indent + 1));
            return sb.ToString();
        }

        public override string ToString() => ToString(0);

        public override void OptimizeChilds()
        {
            Conditional.OptimizeChilds();
        }
    }
}