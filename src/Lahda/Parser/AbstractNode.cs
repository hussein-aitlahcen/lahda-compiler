using System.Collections.Generic;
using System.Text;
using Lahda.Lexer;

namespace Lahda.Parser
{
    public abstract class AbstractNode
    {
        public uint Id { get; set; }
        public NodeType Type { get; }

        public AbstractNode(NodeType type)
        {
            Type = type;
        }

        public virtual string ToString(int indent)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indent; i++)
                sb.Append("\t");
            sb.Append(ToString());
            return sb.ToString();
        }
    }
}