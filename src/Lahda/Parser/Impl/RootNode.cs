using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class RootNode : AbstractStatementNode
    {
        public List<AbstractNode> Functions { get; }

        public RootNode(List<AbstractNode> functions) : base(NodeType.Root)
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