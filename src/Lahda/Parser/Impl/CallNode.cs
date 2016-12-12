using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class CallNode : AbstractExpressionNode
    {
        public FunctionIdentifierNode Target { get; }
        public List<AbstractExpressionNode> Parameters { get; }

        public CallNode(FunctionIdentifierNode target, List<AbstractExpressionNode> parameters) : base(NodeType.Call)
        {
            Target = target;
            Parameters = parameters;
        }

        public override string ToString() => $"CALL {Target.Symbol.Name}";
    }
}