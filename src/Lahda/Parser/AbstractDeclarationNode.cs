using Lahda.Common;

namespace Lahda.Parser
{
    public abstract class AbstractDeclarationNode<T, K> : AbstractStatementNode
        where K : AbstractSymbol
        where T : AbstractIdentifierNode<K>
    {
        public T Identifier { get; private set; }
        public AbstractExpressionNode Expression { get; private set; }

        public AbstractDeclarationNode(T identifier, AbstractExpressionNode expression) : base(NodeType.Declaration)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override string ToString() => $"DECL {Identifier} = {Expression}";

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
        }
    }
}