using System.Collections.Generic;
using System.Linq;

namespace Lahda.Parser.Impl
{
    public sealed class FunctionNode : AbstractStatementNode
    {
        public ObjectType ReturnType { get; }
        public FunctionIdentifierNode Identifier { get; }
        public List<AbstractExpressionNode> Arguments { get; }
        public AbstractStatementNode Statement { get; }

        public FunctionNode(ObjectType returnType, FunctionIdentifierNode identifier, List<AbstractExpressionNode> arguments, AbstractStatementNode stmt) : base(NodeType.Function)
        {
            ReturnType = returnType;
            Identifier = identifier;
            Arguments = arguments;
            Statement = stmt;
        }

        public override void OptimizeChilds()
        {
            Identifier.OptimizeChilds();
            Arguments.ForEach(a => a.OptimizeChilds());
            Statement.OptimizeChilds();
        }

        public override string ToString() => $"FUN {Identifier.Symbol.Name}: {ReturnType} (" + string.Join(", ", Arguments) + $")\n {Statement}";
    }
}