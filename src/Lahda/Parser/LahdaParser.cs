using System;
using System.Collections.Generic;
using Lahda.Lexer;
using System.Linq;
using Lahda.Parser.Impl;
using Lahda.Common;

namespace Lahda.Parser
{
    public sealed class LahdaParser
    {
        public ILexer Lexer { get; }

        public SymbolTable Symbols { get; }

        public LahdaParser(ILexer lexer)
        {
            Lexer = lexer;
            Symbols = new SymbolTable();
        }

        private IToken PeekToken() => Lexer.PeekToken();
        private IToken NextToken() => Lexer.NextToken();

        private void EnsureEndOfStatement()
        {
            if (!IsValue(TokenType.SpecialCharacter, ";"))
            {
                throw new InvalidOperationException($"end of statement {PeekToken()}");
            }
        }

        public AbstractStatementNode NextStatement()
        {
            var token = PeekToken() as ValueToken<string>;
            if (token != null)
            {
                switch (token.Type)
                {
                    case TokenType.Operator:
                        if (token.Value == Operators.BRACE_OPEN)
                            return StatementsBlock();
                        break;

                    case TokenType.Keyword:
                        return DetermineKeywordExpression();

                    case TokenType.Identifier:
                        return Statement(AssignationExpression);
                }
            }
            throw new InvalidOperationException($"next statement {PeekToken()}");
        }

        public AbstractStatementNode DetermineKeywordExpression()
        {
            if (IsKeyword(Keywords.VAR))
            {
                return Statement(DeclarationExpression);
            }
            else if (IsKeyword(Keywords.PRINT))
            {
                return Statement(PrintExpression);
            }
            if (IsKeyword(Keywords.WHILE))
            {
                return WhileExpression();
            }
            else if (IsKeyword(Keywords.FOR))
            {
                return ForExpression();
            }
            else if (IsKeyword(Keywords.DO))
            {
                return Statement(DoExpression);
            }
            else if (IsKeyword(Keywords.IF))
            {
                return ConditionalExpression();
            }
            throw new InvalidOperationException($"keyword expression {PeekToken()}");
        }

        /*
            Read a loop expression starting with 'do'.
            e.g. : 
            do 
            {
                x = x + 1;
                y = y - 1;
            }
            while(y > 10);

            do
            {
                x = x + 1;
            }
            until(x > 10);

            do 
            {
                x = x + 1;
                break;
            }
            forever; 
        */
        public AbstractStatementNode DoExpression()
        {
            var stmts = StatementsBlock();
            if (IsKeyword(Keywords.WHILE))
            {
                return DoWhileExpression(stmts);
            }
            else if (IsKeyword(Keywords.UNTIL))
            {
                return DoUntilExpression(stmts);
            }
            else if (IsKeyword(Keywords.FOREVER))
            {
                return DoForeverExpression(stmts);
            }
            throw new InvalidOperationException("invalid do expression");
        }

        public AbstractStatementNode DoWhileExpression(BlockNode stmts)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(stopCondition, stmts);
        }

        public AbstractStatementNode DoUntilExpression(BlockNode stmts)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(stopCondition, stmts, true);
        }

        public AbstractStatementNode DoForeverExpression(BlockNode stmts)
        {
            return new LoopNode(new LiteralNode(1), stmts);
        }

        public AbstractStatementNode WhileExpression()
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            var stmts = StatementsBlock();
            return new LoopNode(stopCondition, stmts);
        }

        private class ForExpressionData
        {
            public DeclarationNode Initialization { get; set; }
            public AbstractExpressionNode StopCondition { get; set; }
            public AssignationNode Iteration { get; set; }
        }

        /*
            Read a for expression.
            e.g. : 
            for(var i = 0; i < 10; i = i + 1)
            {
                x = x + 2;
            }
        */
        public AbstractStatementNode ForExpression()
        {
            var data = ParentheseEnclosed(() => new ForExpressionData()
            {
                Initialization = Statement(DeclarationExpression),
                StopCondition = Statement(ArithmeticExpression),
                Iteration = AssignationExpression()
            });
            var stmts = StatementsBlock();
            return new BlockNode(data.Initialization, new LoopNode(data.StopCondition, new BlockNode(stmts, data.Iteration)));
        }

        /*
            Read a conditional expression (optional 'else' keyword followeb by a statements block).
            e.g. :
            if(x < 10 && y > 5) 
            {
                z = 1;
            }
            else 
            {
                z = 2;
            }
        */
        public ConditionalNode ConditionalExpression()
        {
            NextToken();
            var condition = ParentheseEnclosed(ArithmeticExpression);
            var trueStmts = NextStatement();
            AbstractStatementNode falseStmts = new BlockNode();
            if (IsKeyword(Keywords.ELSE))
            {
                falseStmts = NextStatement();
            }
            return new ConditionalNode(condition, trueStmts, falseStmts);
        }

        /*
            Read a statements block (note that we call 'BraceEnclosed')
            e.g. :       
            {
                var x = 0;
                for(var i = 0; i < 10; i = i + 1)
                {
                    x = x + 1;
                }
                print x;
            }
        */
        public BlockNode StatementsBlock()
        {
            Symbols.PushScope();
            var stmts = BraceEnclosed(() =>
            {
                var statements = new List<AbstractStatementNode>();
                // Why we don't use IsOperator here ? Because we don't want to consume it, the 'BraceEnclosed' function will do it for us
                while (PeekToken().Type != TokenType.Operator && ((ValueToken<string>)PeekToken()).Value != Operators.BRACE_CLOSE)
                {
                    statements.Add(NextStatement());
                }
                return statements;
            });
            Symbols.PopScope();
            return new BlockNode(stmts);
        }

        /*
            Simple overload, ensure that our function is surrounded by two parentheses.
        */
        public T ParentheseEnclosed<T>(Func<T> fun) => Enclosed(Operators.PARENTHESE_OPEN, Operators.PARENTHESE_CLOSE, fun);

        /*
            Simple overload, ensure that our function is surrounded by two braces.
        */
        public T BraceEnclosed<T>(Func<T> fun) => Enclosed(Operators.BRACE_OPEN, Operators.BRACE_CLOSE, fun);

        /*
            Ensure that the function will be enclosed by the openning/closing operators given :
            e.g. : Enclosed('(', ')', ArithmeticExpression) will ensure that our arithmetic expression is surrounded by ()
        */
        public T Enclosed<T>(string open, string close, Func<T> fun)
        {
            if (!IsOperator(open))
            {
                throw new InvalidOperationException($"missing enclosed begin operator: {open}");
            }
            var value = fun();
            if (!IsOperator(close))
            {
                throw new InvalidOperationException($"missing enclosed final operator: {close}");
            }
            return value;
        }

        /*
            Ensure that a statement is followed by an end of statement character (';' in our case)
        */
        public T Statement<T>(Func<T> fun)
        {
            var expression = fun();
            EnsureEndOfStatement();
            return expression;
        }

        public PrintNode PrintExpression() => new PrintNode(ArithmeticExpression());

        /*
            Our declaration expressions are very basic, javascript style :
                - var x = 2;
                - var y = "World Hello";
        */
        public DeclarationNode DeclarationExpression()
        {
            /*
                Pretty sad but in a for loop we know that our first statement will be a declaration, so, the 'var' keyword
                won't be consumed, this line consume it when necessary.
            */
            if (IsKeyword(Keywords.VAR)) ;

            var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"declaration identifier not found");

            if (!IsValue(TokenType.Operator, Operators.ASSIGN))
                throw new InvalidOperationException($"declaration operator missing {PeekToken()}");

            var expression = ArithmeticExpression();

            // define the new symbol
            var symbol = new Symbol(SymbolType.Floating, ident);
            Symbols.DefineSymbol(symbol);

            return new DeclarationNode(new IdentifierNode(symbol), expression);
        }

        /*
            An assignation is in the form of : 
                - x = 5;
                - x = y * 1 / x;
        */
        public AssignationNode AssignationExpression()
        {
            var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"assignation identifier not found");

            if (!IsValue(TokenType.Operator, Operators.ASSIGN))
                throw new InvalidOperationException($"assignation operator missing");

            var symbol = Symbols.Search(ident);
            if (symbol.IsUnknow)
                throw new InvalidOperationException($"unknow symbol {ident}");

            var expression = ArithmeticExpression();

            return new AssignationNode(new IdentifierNode(symbol), expression);
        }

        // The arithmetic expression starts at the higher level, the Logical Or.
        public AbstractExpressionNode ArithmeticExpression() => ArithmeticOperation(ArithmeticLevel.LogicalOr)();

        // Recursively call the lower arithmetic stage until a primitive
        private Func<AbstractExpressionNode> ArithmeticOperation(ArithmeticLevel level)
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

        // Retrieve the operators that belong to the arithmetic level
        private IEnumerable<string> GetOperators(ArithmeticLevel level)
        {
            switch (level)
            {
                case ArithmeticLevel.Divisible:
                    yield return Operators.DIV;
                    break;

                case ArithmeticLevel.Multiplicative:
                    yield return Operators.MOD;
                    yield return Operators.MUL;
                    break;

                case ArithmeticLevel.Additive:
                    yield return Operators.ADD;
                    yield return Operators.SUB;
                    break;

                case ArithmeticLevel.BitwiseAnd:
                    yield return Operators.AND;
                    break;

                case ArithmeticLevel.BitwiseOr:
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

                case ArithmeticLevel.LogicalAnd:
                    yield return Operators.ANDALSO;
                    break;

                case ArithmeticLevel.LogicalOr:
                    yield return Operators.ORELSE;
                    break;
            }
        }

        // If we find an operator on the right, we produce a new operation node, otherwise, the primitive will be returned
        private AbstractExpressionNode ExecuteIfOperator(AbstractExpressionNode prime, Func<AbstractExpressionNode> operation, IEnumerable<string> operators)
        {
            var op = operators.FirstOrDefault(IsOperator);
            if (op != null)
            {
                return new OperationNode(op, prime, operation());
            }
            return prime;
        }

        /*
            This is the deepest point of our arithmetic recursion. 
            A primitive, in our programming language, can be a(n) :
                - boolean (true/false)
                - number (floating)
                - identifier (variable reference)
                - arithmetic expression (enclosed in parentheses)
        */
        private AbstractExpressionNode ArithmeticPrimitive()
        {
            var token = PeekToken();
            switch (token.Type)
            {
                case TokenType.Keyword:
                    // boolean expressions are transformed into arithmetics ones
                    if (IsKeyword(Keywords.TRUE))
                    {
                        return new LiteralNode(1);
                    }
                    else if (IsKeyword(Keywords.FALSE))
                    {
                        return new LiteralNode(0);
                    }
                    break;

                case TokenType.Floating:
                    // we support floating numbers only
                    return new LiteralNode(((ValueToken<float>)NextToken()).Value);

                case TokenType.Identifier:
                    var ident = ((ValueToken<string>)NextToken()).Value;
                    var symbol = Symbols.Search(ident);
                    if (symbol.IsUnknow)
                    {
                        throw new InvalidOperationException($"unknow symbol {ident}");
                    }
                    return new IdentifierNode(symbol);

                case TokenType.Operator:
                    // here is the recursive call of our arithmetic function
                    return ParentheseEnclosed(ArithmeticExpression);
            }
            throw new InvalidOperationException("arithmetic primitive");
        }

        private bool IsType(TokenType type) => PeekToken().Type == type;

        private bool IsKeyword(string op) => IsValue<string>(TokenType.Keyword, op);

        private bool IsOperator(string op) => IsValue<string>(TokenType.Operator, op);

        private T GetTokenValueOrThrow<T>(TokenType type, string exceptionMessage)
        {
            var tok = PeekToken();
            var casted = tok as ValueToken<T>;
            if (casted == null)
            {
                throw new InvalidOperationException($"unexpected token type: {tok.Type}, expected: {type}, msg: {exceptionMessage}");
            }
            NextToken();
            return casted.Value;
        }

        private bool IsValue<T>(TokenType type, T v) where T : class
        {
            var tok = PeekToken();
            var casted = tok as ValueToken<T>;
            if (casted == null || tok.Type != type || !casted.Value.Equals(v))
            {
                return false;
            }
            NextToken();
            return true;
        }
    }
}