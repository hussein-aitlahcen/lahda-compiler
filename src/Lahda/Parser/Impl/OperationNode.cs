using System;

namespace Lahda.Parser.Impl
{
    public sealed class OperationNode : AbstractExpressionNode
    {
        public AbstractExpressionNode Left { get; private set; }
        public AbstractExpressionNode Right { get; private set; }
        public OperatorType Operator { get; set; }

        public OperationNode(OperatorType op, AbstractExpressionNode left, AbstractExpressionNode right)
            : base(NodeType.Operation)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override AbstractExpressionNode Optimize()
        {
            Left = Left.Optimize();
            Right = Right.Optimize();
            var leftLit = Left as LiteralNode;
            var rightLit = Right as LiteralNode;
            if (leftLit != null && rightLit != null)
            {
                switch (Operator)
                {
                    case OperatorType.Add: return new LiteralNode(leftLit.Value + rightLit.Value);
                    case OperatorType.Mul: return new LiteralNode(leftLit.Value * rightLit.Value);
                    case OperatorType.Sub: return new LiteralNode(leftLit.Value - rightLit.Value);
                    case OperatorType.Div: return new LiteralNode(leftLit.Value / rightLit.Value);
                    case OperatorType.BitwiseAnd: return new LiteralNode((int)leftLit.Value & (int)rightLit.Value);
                    case OperatorType.BitwiseOr: return new LiteralNode((int)leftLit.Value | (int)rightLit.Value);
                    case OperatorType.AndAlso: return new LiteralNode(leftLit.Value != 0 && rightLit.Value != 0 ? 1f : 0f);
                    case OperatorType.OrElse: return new LiteralNode(leftLit.Value != 0 || rightLit.Value != 0 ? 1f : 0f);
                    case OperatorType.Mod: return new LiteralNode(leftLit.Value % rightLit.Value);
                    case OperatorType.Greater: return new LiteralNode(leftLit.Value > rightLit.Value ? 1f : 0f);
                    case OperatorType.NotGreater: return new LiteralNode(leftLit.Value <= rightLit.Value ? 1f : 0f);
                    case OperatorType.Less: return new LiteralNode(leftLit.Value < rightLit.Value ? 1f : 0f);
                    case OperatorType.NotLess: return new LiteralNode(leftLit.Value >= rightLit.Value ? 1f : 0f);
                    case OperatorType.Equals: return new LiteralNode(leftLit.Value == rightLit.Value ? 1f : 0f);
                    case OperatorType.NotEquals: return new LiteralNode(leftLit.Value != rightLit.Value ? 1f : 0f);
                    case OperatorType.Pow: return new LiteralNode((float)Math.Pow(leftLit.Value, rightLit.Value));
                }
            }
            return this;
        }

        public override string ToString() => $"({Left} {Operator} {Right})";
    }
}