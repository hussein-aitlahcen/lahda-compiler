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
                throw new InvalidOperationException($"end of statement: expected={Lexer.Configuration.EndOfStatement()}, found={PeekToken()}");
            }
        }

        public AbstractStatementNode Root()
        {
            var funcs = new List<FunctionNode>();
            while (PeekToken().Type != TokenType.EOF)
            {
                funcs.Add(FunctionExpression());
            }
            return new RootNode(funcs);
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
            throw new InvalidOperationException($"next statement expected: found={PeekToken()}");
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
                case KeywordType.Break: return Statement<BreakNode>();
                case KeywordType.Continue: return Statement<ContinueNode>();
                case KeywordType.Return: return Statement(ReturnExpression);
            }
            throw new InvalidOperationException($"keyword expected");
        }

        public ReturnNode ReturnExpression() => new ReturnNode(ArithmeticExpression());

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
        public LoopNode DoExpression()
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

        public LoopNode DoWhileExpression(AbstractStatementNode statement)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(stopCondition, statement);
        }

        public LoopNode DoUntilExpression(AbstractStatementNode statement)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(OperationNode.Negate(stopCondition), statement);
        }

        public LoopNode DoForeverExpression(AbstractStatementNode statement)
        {
            return new LoopNode(LiteralNode.True, statement);
        }

        public LoopNode WhileExpression()
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            var stmt = NextStatement();
            return new LoopNode(stopCondition, stmt);
        }

        private class ForExpressionData
        {
            public AbstractStatementNode Initialization { get; set; }
            public AbstractExpressionNode StopCondition { get; set; }
            public AbstractStatementNode Iteration { get; set; }
        }

        /*
            Read a for expression.
            e.g. : 
            for(var i = 0; i < 10; i = i + 1)
            {
                x = x + 2;
            }
        */
        public BlockNode ForExpression() =>
            Scoped(() =>
            {
                var data = ParentheseEnclosed(() => new ForExpressionData()
                {
                    Initialization = Statement(DeclarationExpression),
                    StopCondition = Statement(ArithmeticExpression),
                    Iteration = AssignationExpression()
                });
                var stmt = NextStatement();
                return new BlockNode(data.Initialization, new LoopNode(data.StopCondition, data.Iteration, stmt));
            });

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
        public BlockNode StatementsBlock() =>
            Scoped(() => BraceEnclosed(() =>
            {
                var statements = new List<AbstractStatementNode>();
                while (!IsOperatorUnconsumed(OperatorType.BraceClose))
                {
                    statements.Add(NextStatement());
                }
                return new BlockNode(statements);
            }));

        public PrintNode PrintExpression() => new PrintNode(ArithmeticExpression());

        public CallNode CallExpression(FunctionSymbol symbol)
            => ParentheseEnclosed(() =>
            {
                var expressions = new List<AbstractExpressionNode>();
                while (!IsOperatorUnconsumed(OperatorType.ParentheseClose))
                {
                    expressions.Add(ArithmeticExpression());
                    if (IsOperator(OperatorType.Comma)) ;
                }
                if (expressions.Count != symbol.ParameterCount)
                {
                    throw new InvalidOperationException($"argument count mismatch for function {symbol.Name}");
                }
                return new CallNode(new FunctionIdentifierNode(symbol), expressions);
            });

        public FunctionNode FunctionExpression()
        {
            var returnType = GetObjectType(GetKeywordType());
            var symb = DefineFunctionSymbol();
            return Scoped(() =>
            {
                var arguments = ParentheseEnclosed(() =>
                {
                    var args = new List<AbstractExpressionNode>();
                    while (IsType(TokenType.Keyword))
                    {
                        var argType = GetObjectType(GetKeywordType());
                        switch (argType)
                        {
                            case ObjectType.Floating:
                                args.Add(new PrimitiveIdentifierNode(DefinePrimitiveSymbol()));
                                break;

                            case ObjectType.Array:
                                // TODO: define size ? SHOULD BE A POINTER
                                args.Add(new ArrayIdentifierNode(DefineArraySymbol(0), new LiteralNode(0)));
                                break;

                            default:
                                throw new InvalidOperationException("unknow argument type");
                        }
                        if (IsOperator(OperatorType.Comma)) ;
                    }
                    return args;
                });
                symb.ParameterCount = arguments.Count;
                var stmt = NextStatement();
                return new FunctionNode(returnType, new FunctionIdentifierNode(symb), arguments, stmt);
            });
        }

        /*
            Our declaration expressions are very basic, javascript style :
                - var x = 2;
                - var y = "World Hello";
        */
        public AbstractStatementNode DeclarationExpression()
        {
            /*
                Pretty sad but in a for loop we know that our first statement will be a declaration, so, the 'var' keyword
                won't be consumed, this line consume it when necessary.
            */
            if (IsKeyword(KeywordType.Var)) ;
            var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"declaration identifier not found");
            if (IsOperatorUnconsumed(OperatorType.BracketOpen))
            {
                var size = BracketEnclosed(() =>
                {
                    if (!IsType(TokenType.Floating))
                    {
                        throw new InvalidOperationException("const size expected");
                    }
                    return (int)((ValueToken<float>)NextToken()).Value;
                });
                var arrSymbol = Symbols.DefineSymbol(new ArrayVariableSymbol(ident, size));
                return new ArrayDeclarationNode(new ArrayIdentifierNode(arrSymbol, new LiteralNode(size)));
            }
            if (!IsOperator(OperatorType.Assign))
            {
                throw new InvalidOperationException($"declaration operator missing {PeekToken()}");
            }
            var expression = ArithmeticExpression();
            var symbol = Symbols.DefineSymbol(new PrimitiveVariableSymbol(ident));
            return new PrimitiveDeclarationNode(new PrimitiveIdentifierNode(symbol), expression);
        }

        private AbstractStatementNode AbstractAssign<T>(AbstractExpressionNode left, Func<AbstractExpressionNode, AbstractStatementNode> outputNodeProvider)
        {
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
            {
                throw new InvalidOperationException($"wrong assignation operator {PeekToken()}");
            }
            AbstractExpressionNode expression = null;
            switch (op)
            {
                case OperatorType.Assign:
                    expression = ArithmeticExpression();
                    break;

                case OperatorType.AddAssign:
                    expression = new OperationNode(OperatorType.Add, left, ArithmeticExpression());
                    break;

                case OperatorType.SubAssign:
                    expression = new OperationNode(OperatorType.Sub, left, ArithmeticExpression());
                    break;

                case OperatorType.MulAssign:
                    expression = new OperationNode(OperatorType.Mul, left, ArithmeticExpression());
                    break;

                case OperatorType.DivAssign:
                    expression = new OperationNode(OperatorType.Div, left, ArithmeticExpression());
                    break;

                case OperatorType.ModAssign:
                    expression = new OperationNode(OperatorType.Mod, left, ArithmeticExpression());
                    break;

                case OperatorType.Increment:
                    expression = OperationNode.Increment(left);
                    break;

                case OperatorType.Decrement:
                    expression = OperationNode.Decrement(left);
                    break;
            }
            return outputNodeProvider(expression);
        }

        /*
            An assignation is in the form of : 
                - x = 5;
                - x = y * 1 / z;
        */
        public AbstractStatementNode AssignationExpression()
        {
            var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"assignation identifier not found");
            var symbol = Symbols.Search<AbstractAddressableSymbol>(ident);
            if (symbol.IsUnknow)
            {
                throw new InvalidOperationException($"unknow symbol {ident}");
            }
            if (symbol is PrimitiveVariableSymbol)
            {
                var primitiveIdent = new PrimitiveIdentifierNode((PrimitiveVariableSymbol)symbol);
                return AbstractAssign<PrimitiveVariableSymbol>(primitiveIdent, e => new PrimitiveAssignationNode(primitiveIdent, e));
            }
            else if (symbol is ArrayVariableSymbol)
            {
                var elementIndex = BracketEnclosed(ArithmeticExpression);
                var arrayIdent = new ArrayIdentifierNode((ArrayVariableSymbol)symbol, elementIndex);
                return AbstractAssign<ArrayVariableSymbol>(arrayIdent, e => new ArrayAssignationNode(arrayIdent, e));
            }
            throw new InvalidOperationException("unknow symbol assignation type");
        }

        // The arithmetic expression starts at the higher level, the Logical Or.
        public AbstractExpressionNode ArithmeticExpression() => ArithmeticOperation(ArithmeticLevel.LogicalOr)();

        // Recursively call the lower arithmetic stage until a primitive
        private Func<AbstractExpressionNode> ArithmeticOperation(ArithmeticLevel level)
        {
            return () =>
            {
                if (level == ArithmeticLevel.Primitive)
                {
                    return ArithmeticPrimitive();
                }
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
                - primitive (yes, again, but opposed/negated or enclosed in parentheses)
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
                        return LiteralNode.True;
                    }
                    else if (IsKeyword(KeywordType.False))
                    {
                        return LiteralNode.False;
                    }
                    break;

                case TokenType.Floating:
                    // we support floating numbers only
                    return new LiteralNode(((ValueToken<float>)NextToken()).Value);

                case TokenType.Identifier:
                    var ident = ((ValueToken<string>)NextToken()).Value;
                    var symbol = Symbols.Search<AbstractSymbol>(ident);
                    if (symbol.IsUnknow)
                    {
                        throw new InvalidOperationException($"unknow symbol {ident}");
                    }
                    if (IsOperatorUnconsumed(OperatorType.ParentheseOpen))
                    {
                        if (!(symbol is FunctionSymbol))
                        {
                            throw new InvalidOperationException("wrong symbol type for function call in primitive");
                        }
                        return CallExpression((FunctionSymbol)symbol);
                    }
                    if (symbol.Type == ObjectType.Function)
                    {
                        throw new InvalidOperationException($"wrong symbol type for atomic identifier in primitive {symbol.Type}");
                    }
                    var arraySymbol = symbol as ArrayVariableSymbol;
                    if (arraySymbol != null)
                    {
                        var index = BracketEnclosed(ArithmeticExpression);
                        return new ArrayIdentifierNode(arraySymbol, index);
                    }
                    return new PrimitiveIdentifierNode((PrimitiveVariableSymbol)symbol);

                case TokenType.Operator:
                    if (IsOperatorUnconsumed(OperatorType.ParentheseOpen))
                    {
                        return ParentheseEnclosed(ArithmeticExpression);
                    }
                    else if (IsOperator(OperatorType.Sub))
                    {
                        return OperationNode.Oppose(ArithmeticPrimitive());
                    }
                    else if (IsOperator(OperatorType.Negate))
                    {
                        return OperationNode.Negate(ArithmeticPrimitive());
                    }
                    break;
            }
            throw new InvalidOperationException($"arithmetic primitive unknow type {token}");
        }

        public ArrayVariableSymbol DefineArraySymbol(int size)
            => DefineSymbol<ArrayVariableSymbol>(ident => new ArrayVariableSymbol(ident, size));

        public PrimitiveVariableSymbol DefinePrimitiveSymbol()
            => DefineSymbol<PrimitiveVariableSymbol>(ident => new PrimitiveVariableSymbol(ident));

        public FunctionSymbol DefineFunctionSymbol()
            => DefineSymbol<FunctionSymbol>(ident => new FunctionSymbol(ident));

        public T DefineSymbol<T>(Func<string, T> fun)
            where T : AbstractSymbol
        {
            var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"identifier not found");
            var symbol = Symbols.DefineSymbol(fun(ident));
            return symbol;
        }

        public ObjectType GetObjectType(KeywordType type)
        {
            switch (type)
            {
                case KeywordType.Float: return ObjectType.Floating;
                default: throw new InvalidOperationException($"unknow object type={type}");
            }
        }

        /*
            Execute the given function in a new scope.
        */
        public T Scoped<T>(Func<T> fun)
        {
            Symbols.PushScope();
            T value = fun();
            Symbols.PopScope();
            return value;
        }

        public T BracketEnclosed<T>(Func<T> fun) => Enclosed(OperatorType.BracketOpen, OperatorType.BracketClose, fun);

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

        public T Statement<T>() where T : new() => Statement<T>(() => new T());

        private bool IsType(TokenType type) => PeekToken().Type == type;
        private bool IsKeywordUnconsumed(KeywordType key) => IsValueUnconsumed<string>(TokenType.Keyword, Lexer.Configuration.GetKeyword(key));
        private bool IsKeyword(KeywordType key) => IsValue<string>(TokenType.Keyword, Lexer.Configuration.GetKeyword(key));

        private bool IsOperatorUnconsumed(OperatorType op) => IsValueUnconsumed<string>(TokenType.Operator, Lexer.Configuration.GetOperator(op));
        private bool IsOperator(OperatorType op) => IsValue<string>(TokenType.Operator, Lexer.Configuration.GetOperator(op));

        private OperatorType GetOperatorType() => Lexer.Configuration.GetOperatorType(GetTokenValueSilent<string>(TokenType.Operator));

        private KeywordType GetKeywordType() => Lexer.Configuration.GetKeywordType(GetTokenValueSilent<string>(TokenType.Keyword));

        private T GetTokenValueSilent<T>(TokenType type, T def = default(T))
        {
            try
            {
                return GetTokenValueOrThrow<T>(type, "");
            }
            catch (Exception e)
            {
            }
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

        private bool IsValueUnconsumed<T>(TokenType type, T v) where T : class
        {
            var tok = PeekToken();
            var casted = tok as ValueToken<T>;
            if (casted == null || tok.Type != type || !casted.Value.Equals(v))
            {
                return false;
            }
            return true;
        }

        private bool IsValue<T>(TokenType type, T v) where T : class
        {
            if (IsValueUnconsumed<T>(type, v))
            {
                NextToken();
                return true;
            }
            return false;
        }
    }
}