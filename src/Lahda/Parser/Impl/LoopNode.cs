using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class LoopNode : AbstractStatementNode
    {
        public int UniqueId { get; }
        public ConditionalNode Conditional { get; }
        public AbstractStatementNode Iteration { get; }


        public LoopNode(int uniqueId, int uniqueCond, AbstractExpressionNode condition, AbstractStatementNode iteration, AbstractStatementNode statement)
            : base(NodeType.Loop)
        {
            UniqueId = uniqueId;
            Conditional = new ConditionalNode(uniqueCond, condition, statement, new BlockNode(new BreakNode(UniqueId)));
            Iteration = iteration;
        }

        public LoopNode(int uniqueId, int uniqueCond, AbstractExpressionNode condition, AbstractStatementNode statement)
            : this(uniqueId, uniqueCond, condition, new BlockNode(), statement)
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
            Iteration.OptimizeChilds();
        }
    }
}