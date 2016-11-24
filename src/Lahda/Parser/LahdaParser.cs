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
            if (!IsValue(TokenType.SpecialCharacter, Lexer.Configuration.EndOfStatement()))
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
                        if (Lexer.Configuration.IsOperator(OperatorType.BraceOpen, token.Value))
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
            switch (GetKeywordType())
            {
                case KeywordType.Var: return Statement(DeclarationExpression);
                case KeywordType.Print: return Statement(PrintExpression);
                case KeywordType.While: return WhileExpression();
                case KeywordType.For: return ForExpression();
                case KeywordType.Do: return Statement(DoExpression);
                case KeywordType.If: return ConditionalExpression();
                case KeywordType.Break: return Statement(() => new BreakNode());
                case KeywordType.Continue: return Statement(() => new ContinueNode());
            }
            throw new InvalidOperationException($"keyword expected");
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
            var statement = NextStatement();
            switch (GetKeywordType())
            {
                case KeywordType.While: return DoWhileExpression(statement);
                case KeywordType.Until: return DoUntilExpression(statement);
                case KeywordType.Forever: return DoForeverExpression(statement);
            }
            throw new InvalidOperationException("invalid do expression");
        }

        public AbstractStatementNode DoWhileExpression(AbstractStatementNode statement)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(stopCondition, statement);
        }

        public AbstractStatementNode DoUntilExpression(AbstractStatementNode statement)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(stopCondition, statement, true);
        }

        public AbstractStatementNode DoForeverExpression(AbstractStatementNode statement)
        {
            return new LoopNode(new LiteralNode(1), statement);
        }

        public AbstractStatementNode WhileExpression()
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            var stmt = NextStatement();
            return new LoopNode(stopCondition, stmt);
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
            Symbols.PushScope();
            var data = ParentheseEnclosed(() => new ForExpressionData()
            {
                Initialization = Statement(DeclarationExpression),
                StopCondition = Statement(ArithmeticExpression),
                Iteration = AssignationExpression()
            });
            var stmt = NextStatement();
            Symbols.PopScope();
            return new BlockNode(data.Initialization, new LoopNode(data.StopCondition, data.Iteration, stmt));
        }

        /*
            Read a conditional expression (optional 'else' keyword followed by a statements block).
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
            var condition = ParentheseEnclosed(ArithmeticExpression);
            var trueStmts = NextStatement();
            AbstractStatementNode falseStmts = new BlockNode();
            if (IsKeyword(KeywordType.Else))
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
                // Why don't we use IsOperator here ? Because we don't want to consume it, the 'BraceEnclosed' function will do it for us
                ValueToken<string> tok;
                while ((tok = PeekToken() as ValueToken<string>) != null &&
                        tok.Type != TokenType.Operator &&
                        !Lexer.Configuration.IsOperator(OperatorType.BraceClose, tok.Value))
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
        public T ParentheseEnclosed<T>(Func<T> fun) => Enclosed(OperatorType.ParentheseOpen, OperatorType.ParentheseClose, fun);

        /*
            Simple overload, ensure that our function is surrounded by two braces.
        */
        public T BraceEnclosed<T>(Func<T> fun) => Enclosed(OperatorType.BraceOpen, OperatorType.BraceClose, fun);

        /*
            Ensure that the function will be enclosed by the openning/closing operators given :
            e.g. : Enclosed('(', ')', ArithmeticExpression) will ensure that our arithmetic expression is surrounded by ()
        */
        public T Enclosed<T>(OperatorType open, OperatorType close, Func<T> fun)
        {
            if (!IsOperator(open))
            {
                throw new InvalidOperationException($"missing enclosed begin operator: expected={open}, obtained={PeekToken()}");
            }
            var value = fun();
            if (!IsOperator(close))
            {
                throw new InvalidOperationException($"missing enclosed final operator: expected={close}, obtained={PeekToken()}");
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
            if (IsKeyword(KeywordType.Var)) ;

            var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"declaration identifier not found");

            if (!IsOperator(OperatorType.Assign))
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

            var assignationOperators = new[]
            {
                OperatorType.Assign,
                OperatorType.AddAssign,
                OperatorType.SubAssign,
                OperatorType.MulAssign,
                OperatorType.DivAssign,
                OperatorType.ModAssign,
                OperatorType.Increment,
                OperatorType.Decrement,
            };

            var op = assignationOperators.FirstOrDefault(IsOperator);
            if (op == OperatorType.None)
                throw new InvalidOperationException($"wrong assignation operator {PeekToken()}");

            var symbol = Symbols.Search(ident);
            if (symbol.IsUnknow)
                throw new InvalidOperationException($"unknow symbol {ident}");

            AbstractExpressionNode expression = null;
            switch (op)
            {
                case OperatorType.Assign:
                    expression = ArithmeticExpression();
                    break;

                case OperatorType.AddAssign:
                    expression = new OperationNode(OperatorType.Add, new IdentifierNode(symbol), ArithmeticExpression());
                    break;

                case OperatorType.SubAssign:
                    expression = new OperationNode(OperatorType.Sub, new IdentifierNode(symbol), ArithmeticExpression());
                    break;

                case OperatorType.MulAssign:
                    expression = new OperationNode(OperatorType.Mul, new IdentifierNode(symbol), ArithmeticExpression());
                    break;

                case OperatorType.DivAssign:
                    expression = new OperationNode(OperatorType.Div, new IdentifierNode(symbol), ArithmeticExpression());
                    break;

                case OperatorType.ModAssign:
                    expression = new OperationNode(OperatorType.Mod, new IdentifierNode(symbol), ArithmeticExpression());
                    break;

                case OperatorType.Increment:
                    expression = new OperationNode(OperatorType.Add, new IdentifierNode(symbol), new LiteralNode(1));
                    break;

                case OperatorType.Decrement:
                    expression = new OperationNode(OperatorType.Sub, new IdentifierNode(symbol), new LiteralNode(1));
                    break;
            }

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
        private IEnumerable<OperatorType> GetOperators(ArithmeticLevel level)
        {
            switch (level)
            {
                case ArithmeticLevel.Divisible:
                    yield return OperatorType.Div;
                    break;

                case ArithmeticLevel.Multiplicative:
                    yield return OperatorType.Mod;
                    yield return OperatorType.Mul;
                    break;

                case ArithmeticLevel.Additive:
                    yield return OperatorType.Add;
                    yield return OperatorType.Sub;
                    break;

                case ArithmeticLevel.BitwiseAnd:
                    yield return OperatorType.BitwiseAnd;
                    break;

                case ArithmeticLevel.BitwiseOr:
                    yield return OperatorType.BitwiseOr;
                    break;

                case ArithmeticLevel.Comparative:
                    yield return OperatorType.Equals;
                    yield return OperatorType.NotEquals;
                    yield return OperatorType.Greater;
                    yield return OperatorType.NotGreater;
                    yield return OperatorType.Less;
                    yield return OperatorType.NotLess;
                    break;

                case ArithmeticLevel.LogicalAnd:
                    yield return OperatorType.AndAlso;
                    break;

                case ArithmeticLevel.LogicalOr:
                    yield return OperatorType.OrElse;
                    break;
            }
        }

        // If we find an operator on the right, we produce a new operation node, otherwise, the primitive will be returned
        private AbstractExpressionNode ExecuteIfOperator(AbstractExpressionNode prime, Func<AbstractExpressionNode> operation, IEnumerable<OperatorType> operators)
        {
            var op = operators.FirstOrDefault(IsOperator);
            if (op != OperatorType.None)
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
                    if (IsKeyword(KeywordType.True))
                    {
                        return new LiteralNode(1);
                    }
                    else if (IsKeyword(KeywordType.False))
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
                    var op = ((ValueToken<string>)token).Value;
                    switch (Lexer.Configuration.GetOperatorType(op))
                    {
                        case OperatorType.ParentheseOpen:
                            // here is the recursive call of our arithmetic function
                            return ParentheseEnclosed(ArithmeticExpression);

                        case OperatorType.Sub:
                            NextToken();
                            return new OperationNode(OperatorType.Sub, new LiteralNode(0), ArithmeticPrimitive());

                        // negate the expression
                        case OperatorType.Negate:
                            NextToken();
                            return new OperationNode(OperatorType.Equals, new LiteralNode(0), ArithmeticPrimitive());

                    }
                    break;
            }
            throw new InvalidOperationException($"arithmetic primitive unknow type {token}");
        }

        private bool IsType(TokenType type) => PeekToken().Type == type;

        private bool IsKeyword(KeywordType key) => IsValue<string>(TokenType.Keyword, Lexer.Configuration.GetKeyword(key));

        private bool IsOperator(OperatorType op) => IsValue<string>(TokenType.Operator, Lexer.Configuration.GetOperator(op));

        private OperatorType GetOperatorType() => Lexer.Configuration.GetOperatorType(GetTokenValueSilent<string>(TokenType.Operator));

        private KeywordType GetKeywordType() => Lexer.Configuration.GetKeywordType(GetTokenValueSilent<string>(TokenType.Keyword));

        private T GetTokenValueSilent<T>(TokenType type, T def = default(T))
        {
            try
            {
                return GetTokenValueOrThrow<T>(type, "");
            }
            catch (Exception e) { }
            return def;
        }

        private T GetTokenValueOrThrow<T>(TokenType type, string exceptionMessage)
        {
            var tok = PeekToken();
            var casted = tok as ValueToken<T>;
            if (casted == null || tok.Type != type)
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