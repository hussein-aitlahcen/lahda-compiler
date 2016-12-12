using System.Collections.Generic;
using System.Linq;

namespace Lahda.Parser.Impl
{
    public sealed class CallNode : AbstractExpressionNode
    {
        public FunctionIdentifierNode Target { get; }
        public List<AbstractExpressionNode> Parameters { get; private set; }

        public CallNode(FunctionIdentifierNode target, List<AbstractExpressionNode> parameters) : base(NodeType.Call)
        {
            Target = target;
            Parameters = parameters;
        }

        public override void OptimizeChilds()
        {
            Target.OptimizeChilds();
            var optimizedParams = new List<AbstractExpressionNode>();
            foreach (var param in Parameters)
            {
                optimizedParams.Add(param.Optimize());
            }
            Parameters = optimizedParams;
        }

        public override string ToString() => $"CALL {Target.Symbol.Name}(" + string.Join(", ", Parameters) + ")";
    }
}