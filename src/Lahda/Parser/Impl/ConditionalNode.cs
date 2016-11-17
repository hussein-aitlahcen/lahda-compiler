using System.Text;
using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class ConditionalNode : AbstractStatementNode
    {
        public AbstractExpressionNode Expression { get; private set; }
        public BlockNode TrueStatements { get; private set; }
        public BlockNode FalseStatements { get; private set; }

        public ConditionalNode(AbstractExpressionNode expression, BlockNode trueStatements, BlockNode falseStatements) : base(NodeType.Conditional)
        {
            Expression = expression;
            TrueStatements = trueStatements;
            FalseStatements = falseStatements;
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine($"IF {Expression} THEN");
            sb.AppendLine(TrueStatements.ToString(indent + 1));
            for (int i = 0; i < indent; i++)
                sb.Append("\t");
            sb.AppendLine("ELSE");
            sb.AppendLine(FalseStatements.ToString(indent + 1));
            return sb.ToString();
        }

        public override string ToString() => ToString(0);

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
            TrueStatements.OptimizeChilds();
            FalseStatements.OptimizeChilds();
        }
    }
}