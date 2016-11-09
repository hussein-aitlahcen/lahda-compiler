using System;
using System.Collections.Generic;
using Lahda.Lexer;
using System.Linq;

namespace Lahda.Parser
{
    public enum ArithmeticLevel
    {
        Primitive = 0,
        Divisible,
        Multiplicative,
        Additive,
        And,
        Or,
        Comparative,
        AndAlso,
        OrElse
    }

    public sealed class LahdaParser
    {
        public ILexer Lexer { get; }

        public LahdaParser(ILexer lexer)
        {
            Lexer = lexer;
        }

        private IToken PeekToken() => Lexer.PeekToken();
        private IToken NextToken() => Lexer.NextToken();

        public Tree NextExpression()
        {
            var token = PeekToken() as ValueToken<string>;
            switch (token.Type)
            {
                case TokenType.Keyword:
                    return DetermineKeywordExpression(token.Value);

                case TokenType.Identifier:
                    return AssignationExpression();
            }
            throw new InvalidOperationException($"next expression {token.Type}");
        }

        public Tree DetermineKeywordExpression(string keyword)
        {
            switch (keyword)
            {
                case Keywords.VAR:
                    return DeclarationExpression();

                case Keywords.IF:
                    return ConditionalExpression();
            }
            throw new InvalidOperationException($"keyword expression {keyword}");
        }

        public Tree ConditionalExpression()
        {
            var token = NextToken();
            if (IsOperator(Operators.PARENTHESE_OPEN))
            {
                NextToken();
                var condition = ArithmeticExpression();
                if (IsOperator(Operators.PARENTHESE_CLOSE))
                {
                    NextToken();

                    // two childs
                    var trueInstructions = InstructionsBlock();
                    var falseInstructions = new List<Tree>();

                    // second block
                    if (IsKeyword(Keywords.ELSE))
                    {
                        NextToken(); // consume else
                        falseInstructions.AddRange(InstructionsBlock());
                    }

                    return new Tree(token, condition, new Tree(trueInstructions), new Tree(falseInstructions));
                }
            }
            throw new InvalidOperationException($"conditional expression");
        }

        public IEnumerable<Tree> InstructionsBlock()
        {
            if (IsOperator(Operators.BRACE_OPEN))
            {
                NextToken(); // consumed brace open
                while (!IsOperator(Operators.BRACE_CLOSE))
                {
                    yield return NextExpression();
                }
                NextToken(); // consumed brace close
            }
            else
            {
                throw new Exception("instruction block");
            }
        }

        public Tree DeclarationExpression()
        {
            var token = NextToken();
            if (token.Type != TokenType.Identifier)
                throw new InvalidOperationException($"declaration identifier {token.Type}");
            return null;
        }

        public Tree AssignationExpression()
        {
            var token = NextToken();
            return null;
        }

        public Tree ArithmeticExpression() => ArithmeticOperation(ArithmeticLevel.OrElse)();

        private Func<Tree> ArithmeticOperation(ArithmeticLevel level)
        {
            return () =>
            {
                if (level == ArithmeticLevel.Primitive)
                    return ArithmeticPrimitive();

                return ExecuteIfOperator
                (
                    ArithmeticOperation(level - 1)(),
                    ArithmeticOperation(level),
                    GetOperators(level)
                );
            };
        }

        private IEnumerable<string> GetOperators(ArithmeticLevel level)
        {
            switch (level)
            {
                case ArithmeticLevel.Divisible:
                    yield return Operators.DIV;
                    break;

                case ArithmeticLevel.Multiplicative:
                    yield return Operators.MUL;
                    break;

                case ArithmeticLevel.Additive:
                    yield return Operators.ADD;
                    yield return Operators.SUB;
                    break;

                case ArithmeticLevel.And:
                    yield return Operators.AND;
                    break;

                case ArithmeticLevel.Or:
                    yield return Operators.OR;
                    break;

                case ArithmeticLevel.Comparative:
                    yield return Operators.EQUALS;
                    yield return Operators.NOT_EQUALS;
                    yield return Operators.GREATER;
                    yield return Operators.NOT_GREATER;
                    yield return Operators.LESS;
                    yield return Operators.NOT_LESS;
                    break;

                case ArithmeticLevel.AndAlso:
                    yield return Operators.ANDALSO;
                    break;

                case ArithmeticLevel.OrElse:
                    yield return Operators.ORELSE;
                    break;
            }
        }

        private Tree ExecuteIfOperator(Tree prime, Func<Tree> operation, IEnumerable<string> operators)
        {
            if (operators.Any(IsOperator))
            {
                return new Tree(NextToken(), prime, operation());
            }
            return prime;
        }

        private Tree ArithmeticPrimitive()
        {
            var token = PeekToken();
            switch (token.Type)
            {
                case TokenType.Keyword:
                    if (IsKeyword(Keywords.TRUE))
                    {
                        return new Tree(new ValueToken<int>(TokenType.Integer, NextToken().Position, 1));
                    }
                    else if (IsKeyword(Keywords.FALSE))
                    {
                        return new Tree(new ValueToken<int>(TokenType.Integer, NextToken().Position, 0));
                    }
                    break;

                case TokenType.Floating:
                case TokenType.Integer:
                case TokenType.Identifier:
                    return new Tree(NextToken());

                case TokenType.Operator:
                    if (IsOperator(Operators.PARENTHESE_OPEN))
                    {
                        NextToken(); // Bracket consumed
                        var exp = ArithmeticExpression();
                        if (IsOperator(Operators.PARENTHESE_CLOSE))
                        {
                            NextToken(); // Bracket consumed
                            return exp;
                        }
                    }
                    break;
            }
            throw new InvalidOperationException("arithmetic primitive");
        }

        private bool IsKeyword(string op) => IsValue<string>(TokenType.Keyword, op);

        private bool IsOperator(string op) => IsValue<string>(TokenType.Operator, op);

        private bool IsValue<T>(TokenType type, T v) where T : class
        {
            var opToken = PeekToken();
            var casted = opToken as ValueToken<T>;
            return casted != null &&
                    opToken.Type == type &&
                    casted.Value.Equals(v);
        }
    }
}