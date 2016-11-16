using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class LoopNode : AbstractLoopNode
    {
        public uint Id { get; set; }

        public AbstractStatementNode StmtsBlock { get; }

        public LoopNode(AbstractExpressionNode condition, BlockNode code, bool reverse = false)
        {
            var brk = new BlockNode(new BreakNode(this));
            var cont = new BlockNode(code, new ContinueNode(this));
            var trueStmts = reverse ? brk : cont;
            var falseStmts = reverse ? cont : brk;
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