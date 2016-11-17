using System.Text;
using Lahda.Lexer;

namespace Lahda.Parser.Impl
{
    public sealed class LoopNode : AbstractLoopNode
    {
        public ConditionalNode Cond { get; private set; }

        public LoopNode(AbstractExpressionNode condition, BlockNode code, bool reverse = false)
        {
            if (reverse)
                condition = new OperationNode(Operators.EQUALS, condition, new LiteralNode(0));
            Cond = new ConditionalNode(condition, code, new BlockNode(new BreakNode()));
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine("WHILE");
            sb.AppendLine(Cond.ToString(indent + 1));
            return sb.ToString();
        }

        public override string ToString() => ToString(0);

        public override void OptimizeChilds()
        {
            Cond.OptimizeChilds();
        }
    }
}