using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class DeclarationNode : AbstractStatementNode
    {
        public IdentifierNode Identifier { get; private set; }
        public AbstractExpressionNode Expression { get; private set; }

        public DeclarationNode(IdentifierNode identifier, AbstractExpressionNode expression) : base(NodeType.Declaration)
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