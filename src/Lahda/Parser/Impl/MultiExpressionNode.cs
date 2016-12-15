using System;
using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class MultiExpressionNode : AbstractExpressionNode
    {
        public List<AbstractExpressionNode> Expressions { get; }
        public MultiExpressionNode(IEnumerable<AbstractExpressionNode> expressions) : base(NodeType.MultiExpression)
        {
            Expressions = new List<AbstractExpressionNode>(expressions);
        }
    }
}