using System;
using Lahda.Lexer;

namespace Lahda.Parser
{
    public sealed class LahdaParser 
    {
        public ILexer Lexer { get; }

        public LahdaParser(ILexer lexer) 
        {
            Lexer = lexer;
        }

        private IToken PeekToken() => Lexer.PeekToken();
        private IToken NextToken() => Lexer.NextToken();

        public Tree Expression() => Additive();

        private Tree Additive()
        {
            var mul = Multiplicative();
            if(IsOperator(Operators.ADD) || IsOperator(Operators.SUB)) 
            {
                return new Tree(NextToken(), mul, Additive());
            }
            return mul;
        }

        private Tree Multiplicative() 
        {
            var prim = Primitive();
            if(IsOperator(Operators.MUL) || IsOperator(Operators.DIV))
            {
                return new Tree(NextToken(), prim, Multiplicative());
            }
            return prim;
        }

        private Tree Primitive() 
        {
            var token = PeekToken();
            switch(token.Type)
            {
                case TokenType.Floating:
                case TokenType.Integer:
                case TokenType.Identifier:
                    return new Tree(NextToken());
                
                case TokenType.Operator:
                    if(IsOperator(Operators.BRACKET_OPEN))
                    {
                        NextToken(); // Bracket consumed
                        var exp = Expression();
                        if(IsOperator(Operators.BRACKET_CLOSE))
                        {
                            NextToken(); // Bracket consumed
                            return exp;
                        }
                    }
                    break;
            }
            throw new InvalidOperationException("you should consider going back to php ))))");
        }

        private bool IsOperator(string op)
        {
            var opToken = PeekToken() as ValueToken<string>;
            return opToken != null && 
                    opToken.Type == TokenType.Operator && 
                    opToken.Value == op;
        }
    }
}