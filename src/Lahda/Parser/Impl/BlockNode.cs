using System.Collections.Generic;
using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class BlockNode : AbstractNode
    {
        public List<AbstractStatementNode> Statements { get; }

        public BlockNode() : this(new AbstractStatementNode[] { })
        {
        }

        public BlockNode(IEnumerable<AbstractStatementNode> statements) : base(NodeType.Block)
        {
            Statements = new List<AbstractStatementNode>(statements);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SCOPE {");
            foreach (var stmt in Statements)
                sb.AppendLine("\t" + stmt);
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}