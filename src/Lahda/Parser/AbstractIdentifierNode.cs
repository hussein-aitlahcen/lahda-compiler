using Lahda.Common;

namespace Lahda.Parser
{
    public abstract class AbstractIdentifierNode : AbstractExpressionNode
    {
        public AbstractIdentifierNode() : base(NodeType.Identifier)
        {
        }
    }
}