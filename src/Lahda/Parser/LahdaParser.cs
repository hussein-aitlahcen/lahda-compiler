using System;
using System.Collections.Generic;
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

        public Tree NextExpression() 
        {
            var token = PeekToken() as ValueToken<string>;
            switch(token.Type)
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
            switch(keyword)
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
            if(IsOperator(Operators.PARENTHESE_OPEN))
            {
                NextToken();
                var condition = BooleanExpression();
                if(IsOperator(Operators.PARENTHESE_CLOSE)) 
                {
                    NextToken();

                    // two childs
                    var trueInstructions = InstructionsBlock();
                    var falseInstructions = new List<Tree>();

                    // second block                  
                    if(IsKeyword(Keywords.ELSE))
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
            if(IsOperator(Operators.BRACE_OPEN))
            {
                NextToken(); // consumed brace open
                while(!IsOperator(Operators.BRACE_CLOSE))
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
            if(token.Type != TokenType.Identifier)
                throw new InvalidOperationException($"declaration identifier {token.Type}");
            return null;
        }

        public Tree AssignationExpression() 
        {
            var token = NextToken();
            return null;
        }

        public Tree BooleanExpression()
        {
            try
            {
                var left = ArithmeticExpression();
                if(IsOperator(Operators.EQUALS) || 
                    IsOperator(Operators.NOT_EQUALS) ||
                    IsOperator(Operators.GREATER) ||
                    IsOperator(Operators.NOT_GREATER) ||
                    IsOperator(Operators.LESS) ||
                    IsOperator(Operators.NOT_LESS)) 
                {
                    return new Tree(NextToken(), left, ArithmeticExpression());
                }
            }
            catch(Exception e)
            {
            }

            var prim = BooleanPrimitive();
            if(IsOperator(Operators.ANDALSO) || IsOperator(Operators.ORELSE))
            {
                return new Tree(NextToken(), prim, BooleanExpression());
            }
            return prim;
        }

        private Tree BooleanPrimitive()
        {
            var token = PeekToken();
            switch(token.Type)
            {                
                case TokenType.Identifier:
                    return new Tree(NextToken());

                case TokenType.Keyword:
                    if(IsKeyword(Keywords.TRUE) || IsKeyword(Keywords.FALSE))
                    {
                        return new Tree(NextToken());
                    }
                    break;
                
                case TokenType.Operator:
                    if(IsOperator(Operators.PARENTHESE_OPEN))
                    {
                        NextToken(); // Bracket consumed
                        var exp = BooleanExpression();
                        if(IsOperator(Operators.PARENTHESE_CLOSE))
                        {
                            NextToken(); // Bracket consumed
                            return exp;
                        }
                    }
                    break;
            }
            throw new InvalidOperationException("primitive");
        }

        public Tree ArithmeticExpression() => ArithmeticAdditive();

        private Tree ArithmeticAdditive()
        {
            var mul = ArithmeticMultiplicative();
            if(IsOperator(Operators.ADD) || IsOperator(Operators.SUB) || IsOperator(Operators.OR)) 
            {
                return new Tree(NextToken(), mul, ArithmeticAdditive());
            }
            return mul;
        }

        private Tree ArithmeticMultiplicative() 
        {
            var prim = ArithmeticPrimitive();
            if(IsOperator(Operators.MUL) || IsOperator(Operators.DIV) || IsOperator(Operators.AND))
            {
                return new Tree(NextToken(), prim, ArithmeticMultiplicative());
            }
            return prim;
        }

        private Tree ArithmeticPrimitive() 
        {
            var token = PeekToken();
            switch(token.Type)
            {
                case TokenType.Floating:
                case TokenType.Integer:
                case TokenType.Identifier:
                    return new Tree(NextToken());
                
                case TokenType.Operator:
                    if(IsOperator(Operators.PARENTHESE_OPEN))
                    {
                        NextToken(); // Bracket consumed
                        var exp = ArithmeticExpression();
                        if(IsOperator(Operators.PARENTHESE_CLOSE))
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