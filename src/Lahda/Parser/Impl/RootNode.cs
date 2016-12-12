using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class RootNode : AbstractStatementNode
    {
        public List<FunctionNode> Functions { get; }

        public RootNode(List<FunctionNode> functions) : base(NodeType.Root)
        {
            Functions = functions;
        }

        public override void OptimizeChilds()
        {
            Functions.ForEach(f => f.OptimizeChilds());
        }

        public override string ToString() => $"ROOT";
    }
}