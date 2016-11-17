using System.Collections.Generic;

namespace Lahda.Parser.Impl
{
    public sealed class AssignationNode : AbstractStatementNode
    {
        public IdentifierNode Identifier { get; private set; }

        public AbstractExpressionNode Expression { get; private set; }

        public AssignationNode(IdentifierNode identifier, AbstractExpressionNode expression) : base(NodeType.Assignation)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override void OptimizeChilds()
        {
            Expression = Expression.Optimize();
        }

        public override string ToString() => $"ASSIGN {Identifier} = {Expression}";
    }
}