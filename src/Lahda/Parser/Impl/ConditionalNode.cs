using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class ConditionalNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; private set; }
        public AbstractStatementNode TrueStatement { get; private set; }
        public AbstractStatementNode FalseStatement { get; private set; }

        public ConditionalNode(AbstractExpressionNode expression, AbstractStatementNode trueStatement, AbstractStatementNode falseStatement) : base(NodeType.Conditional)
        {
            Expression = expression;
            TrueStatement = trueStatement;
            FalseStatement = falseStatement;
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine($"IF {Expression} THEN");
            sb.AppendLine(TrueStatement.ToString(indent + 1));
            for (int i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine("ELSE");
            sb.AppendLine(FalseStatement.ToString(indent + 1));
            return sb.ToString();
        }

        public override string ToString() => ToString(0);

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
            TrueStatement.OptimizeChilds();
            FalseStatement.OptimizeChilds();
        }
    }
}