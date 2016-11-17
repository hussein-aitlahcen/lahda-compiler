using System;
using System.Collections.Generic;
using Lahda.Lexer;

namespace Lahda.Parser.Impl
{
    public sealed class OperationNode : AbstractExpressionNode
    {
        public AbstractExpressionNode Left { get; private set; }
        public AbstractExpressionNode Right { get; private set; }
        public string Operator { get; set; }

        public OperationNode(string op, AbstractExpressionNode left, AbstractExpressionNode right)
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
                    case Operators.ADD: return new LiteralNode(leftLit.Value + rightLit.Value);
                    case Operators.MUL: return new LiteralNode(leftLit.Value * rightLit.Value);
                    case Operators.SUB: return new LiteralNode(leftLit.Value - rightLit.Value);
                    case Operators.DIV: return new LiteralNode(leftLit.Value / rightLit.Value);
                    case Operators.AND: return new LiteralNode((int)leftLit.Value & (int)rightLit.Value);
                    case Operators.OR: return new LiteralNode((int)leftLit.Value | (int)rightLit.Value);
                    case Operators.ANDALSO: return new LiteralNode(leftLit.Value != 0 && rightLit.Value != 0 ? 1f : 0f);
                    case Operators.ORELSE: return new LiteralNode(leftLit.Value != 0 || rightLit.Value != 0 ? 1f : 0f);
                    case Operators.MOD: return new LiteralNode(leftLit.Value % rightLit.Value);
                    case Operators.GREATER: return new LiteralNode(leftLit.Value > rightLit.Value ? 1f : 0f);
                    case Operators.NOT_GREATER: return new LiteralNode(leftLit.Value <= rightLit.Value ? 1f : 0f);
                    case Operators.LESS: return new LiteralNode(leftLit.Value < rightLit.Value ? 1f : 0f);
                    case Operators.NOT_LESS: return new LiteralNode(leftLit.Value >= rightLit.Value ? 1f : 0f);
                    case Operators.EQUALS: return new LiteralNode(leftLit.Value == rightLit.Value ? 1f : 0f);
                    case Operators.NOT_EQUALS: return new LiteralNode(leftLit.Value != rightLit.Value ? 1f : 0f);
                    case Operators.POW: return new LiteralNode((float)Math.Pow(leftLit.Value, rightLit.Value));
                }
            }
            return this;
        }

        public override string ToString() => $"({Left} {Operator} {Right})";
    }
}