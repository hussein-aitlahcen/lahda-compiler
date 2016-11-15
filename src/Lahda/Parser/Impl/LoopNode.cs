using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class LoopNode : AbstractLoopNode
    {
        public AbstractStatementNode StmtsBlock { get; }

        public LoopNode(AbstractExpressionNode condition, BlockNode stmts, bool reverse = false)
         : this
         (
            condition,
            reverse ? new BlockNode(ConcreteNode.BREAK) : stmts,
            reverse ? stmts : new BlockNode(ConcreteNode.BREAK)
        )
        {
        }

        public LoopNode(AbstractExpressionNode condition, BlockNode trueStmts, BlockNode falseStmts)
        {
            StmtsBlock = new ConditionalNode(condition, trueStmts, falseStmts);
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine("WHILE");
            sb.AppendLine(StmtsBlock.ToString(indent + 1));
            return sb.ToString();
        }
        public override string ToString() => ToString(0);
    }
}