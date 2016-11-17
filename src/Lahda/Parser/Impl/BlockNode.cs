using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lahda.Parser.Impl
{
    public sealed class BlockNode : AbstractStatementNode
    {
        public List<AbstractStatementNode> Statements { get; }

        public BlockNode() : this(new AbstractStatementNode[] { })
        {
        }

        public BlockNode(params AbstractStatementNode[] nodes) : this(nodes.ToList())
        {
        }

        public BlockNode(IEnumerable<AbstractStatementNode> statements) : base(NodeType.Block)
        {
            Statements = new List<AbstractStatementNode>(statements);
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            foreach (var statement in Statements)
                sb.AppendLine(statement.ToString(indent));
            return sb.ToString();
        }

        public override string ToString() => ToString(0);

        public override void OptimizeChilds() => Statements.ForEach(s => s.OptimizeChilds());
    }
}