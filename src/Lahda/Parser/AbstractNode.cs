using System.Collections.Generic;
using System.Text;
using Lahda.Lexer;

namespace Lahda.Parser
{
    public abstract class AbstractNode
    {
        public NodeType Type { get; }

        public AbstractNode(NodeType type, params AbstractNode[] args)
        {
            Type = type;
        }
    }
}