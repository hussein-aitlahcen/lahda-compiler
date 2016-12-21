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

        private int NextLoopId;
        private int NextCondId;
        private Stack<int> CurrentLoop { get; }
        private Stack<int> CurrentCond { get; }

        public LahdaParser(ILexer lexer)
        {
            Lexer = lexer;
            Symbols = new SymbolTable();
            CurrentLoop = new Stack<int>();
            CurrentCond = new Stack<int>();
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
            var funcs = new List<AbstractNode>();
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
                        if (IsOperatorUnconsumed(OperatorType.BraceOpen))
                        {
                            return StatementsBlock();
                        }
                        else if (IsOperatorUnconsumed(OperatorType.Dereference))
                        {
                            return Statement(AssignationExpression);
                        }
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
                case KeywordType.Crash: return Statement(CrashExpression);
                case KeywordType.Var: return Statement(DeclarationExpression);
                case KeywordType.Print: return Statement(() => PrintExpression());
                case KeywordType.While: return WhileExpression();
                case KeywordType.For: return ForExpression();
                case KeywordType.Do: return Statement(DoExpression);
                case KeywordType.If: return ConditionalExpression();
                case KeywordType.Break: return Statement(() => new BreakNode(CurrentLoop.Peek()));
                case KeywordType.Continue: return Statement(() => new ContinueNode(CurrentCond.Peek()));
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
        public AbstractStatementNode DoExpression() =>
            Scoped(() =>
                IdentifiedLoop(loopId =>
                    IdentifiedCond(condId =>
                    {
                        var stmt = new BlockNode(NextStatement(), Symbols.CurrentScope.ReleaseStatements);
                        switch (GetKeywordType())
                        {
                            case KeywordType.While: return DoWhileExpression(loopId, condId, stmt);
                            case KeywordType.Until: return DoUntilExpression(loopId, condId, stmt);
                            case KeywordType.Forever: return DoForeverExpression(loopId, condId, stmt);
                            default: throw new InvalidOperationException("invalid do expression");
                        }
                    })));

        public LoopNode DoWhileExpression(int loopId, int condId, AbstractStatementNode statement)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(loopId, condId, stopCondition, statement);
        }

        public LoopNode DoUntilExpression(int loopId, int condId, AbstractStatementNode statement)
        {
            var stopCondition = ParentheseEnclosed(ArithmeticExpression);
            return new LoopNode(loopId, condId, OperationNode.Negate(stopCondition), statement);
        }

        public LoopNode DoForeverExpression(int loopId, int condId, AbstractStatementNode statement) =>
            new LoopNode(loopId, condId, LiteralNode.True, statement);


        public AbstractStatementNode WhileExpression() =>
            Scoped(() =>
                IdentifiedLoop(id =>
                    IdentifiedCond(condId =>
                    {
                        var stopCondition = ParentheseEnclosed(ArithmeticExpression);
                        var stmt = new BlockNode(NextStatement(), Symbols.CurrentScope.ReleaseStatements);
                        return new LoopNode(id, condId, stopCondition, stmt);
                    })));

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
        public AbstractStatementNode ForExpression() =>
            Scoped(() =>
            {
                var data = ParentheseEnclosed(() => new ForExpressionData()
                {
                    Initialization = Statement(DeclarationExpression),
                    StopCondition = Statement(ArithmeticExpression),
                    Iteration = AssignationExpression()
                });
                return IdentifiedLoop(loopId =>
                    IdentifiedCond(condId =>
                    {
                        var stmt = NextStatement();
                        return new BlockNode
                        (
                            data.Initialization,
                            new LoopNode
                            (
                                loopId,
                                condId,
                                data.StopCondition,
                                data.Iteration,
                                new BlockNode
                                (
                                    stmt,
                                    Symbols.CurrentScope.ReleaseStatements
                                )
                            )
                        );
                    }));
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
        public AbstractStatementNode ConditionalExpression() =>
            IdentifiedCond(id =>
            {
                var condition = ParentheseEnclosed(ArithmeticExpression);
                var trueStmts = Scoped(NextStatement);
                AbstractStatementNode falseStmts = new BlockNode();
                if (IsKeyword(KeywordType.Else))
                {
                    falseStmts = Scoped(NextStatement);
                }
                return new ConditionalNode(id, condition, trueStmts, falseStmts);
            });

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
        public AbstractStatementNode StatementsBlock() =>
            Scoped(() =>
                BraceEnclosed(() =>
                {
                    var statements = new List<AbstractStatementNode>();
                    while (!IsOperatorUnconsumed(OperatorType.BraceClose))
                    {
                        statements.Add(NextStatement());
                    }
                    return new BlockNode(new BlockNode(statements), Symbols.CurrentScope.ReleaseStatements);
                }));

        public AbstractStatementNode CrashExpression()
        {
            if (IsType(TokenType.String))
            {
                return new BlockNode(PrintExpression("FAILURE: "), new CrashNode());
            }
            return new CrashNode();
        }

        public AbstractStatementNode PrintExpression(string prefix = "")
        {
            if (IsType(TokenType.String))
            {
                return new PrintStringNode(prefix + GetTokenValueOrThrow<string>(TokenType.String, "unknow string type ?? wtf"));
            }
            return new PrintExpressionNode(ArithmeticExpression());
        }

        public CallNode CallExpression(FunctionSymbol symbol)
            => ParentheseEnclosed(() =>
            {
                var expressions = new List<AbstractExpressionNode>();
                if (!IsOperatorUnconsumed(OperatorType.ParentheseClose))
                {
                    do
                    {
                        expressions.Add(ArithmeticExpression());
                    }
                    while (IsOperator(OperatorType.Comma));
                }
                if (expressions.Count != symbol.ParameterCount)
                {
                    throw new InvalidOperationException($"argument count mismatch for function {symbol.Name}");
                }
                return new CallNode(new FunctionIdentifierNode(symbol), expressions);
            });

        public AbstractNode FunctionExpression()
        {
            var returnType = GetObjectType(GetKeywordType());
            var symb = DefineFunctionSymbol();
            return Scoped(() =>
            {
                var arguments = ParentheseEnclosed(() =>
                {
                    var args = new List<AbstractExpressionNode>();
                    if (IsType(TokenType.Keyword))
                    {
                        do
                        {
                            var argType = GetObjectType(GetKeywordType());
                            switch (argType)
                            {
                                case ObjectType.Floating:
                                    args.Add(new AddressableIdentifierNode(DefinePrimitiveSymbol()));
                                    break;

                                case ObjectType.Pointer:
                                    args.Add(new AddressableIdentifierNode(DefineArraySymbol()));
                                    break;

                                default:
                                    throw new InvalidOperationException("unknow argument type");
                            }
                        }
                        while (IsOperator(OperatorType.Comma));
                    }
                    return args;
                });
                symb.ParameterCount = arguments.Count;
                var stmt = new BlockNode(NextStatement(), Symbols.CurrentScope.ReleaseStatements);
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
                var arrSymbol = Symbols.DefineSymbol(new ArrayVariableSymbol(ident));
                var indexExpressions = new List<AbstractExpressionNode>();
                while (IsOperatorUnconsumed(OperatorType.BracketOpen))
                {
                    indexExpressions.Add(BracketEnclosed(ArithmeticExpression));
                }
                return new AddressableDeclarationNode(new AddressableIdentifierNode(arrSymbol), new MultiExpressionNode(indexExpressions));
            }
            if (!IsOperator(OperatorType.Assign))
            {
                throw new InvalidOperationException($"declaration operator missing {PeekToken()}");
            }
            var expression = ArithmeticExpression();
            var symbol = Symbols.DefineSymbol(new PrimitiveVariableSymbol(ident));
            return new AddressableDeclarationNode(new AddressableIdentifierNode(symbol), expression);
        }

        private AbstractExpressionNode AbstractAssign(AbstractExpressionNode left)
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
            return expression;
        }

        /*
            An assignation is in the form of : 
                - x = 5;
                - x = y * 1 / z;
        */
        public AbstractStatementNode AssignationExpression()
        {
            AbstractExpressionNode left;
            if (IsOperator(OperatorType.Dereference))
            {
                left = ArithmeticExpression();
            }
            else
            {
                var ident = GetTokenValueOrThrow<string>(TokenType.Identifier, $"assignation identifier not found");
                var symbol = Symbols.Search<AbstractSymbol>(ident);
                if (symbol is AbstractAddressableSymbol)
                {
                    var addressableSymbol = (AbstractAddressableSymbol)symbol;
                    var identNode = new AddressableIdentifierNode(addressableSymbol);
                    if (IsOperatorUnconsumed(OperatorType.BracketOpen))
                    {
                        var indexExpression = BracketEnclosed(ArithmeticExpression);
                        left = new OperationNode(OperatorType.Add, identNode, indexExpression);
                        // recursive ident[a][b][c]...
                        while (IsOperatorUnconsumed(OperatorType.BracketOpen))
                        {
                            indexExpression = BracketEnclosed(ArithmeticExpression);
                            left = new OperationNode(OperatorType.Add, new DereferenceNode(left), indexExpression);
                        }
                    }
                    else
                    {
                        return new AssignationNode(identNode, AbstractAssign(identNode));
                    }
                }
                else if (symbol is FunctionSymbol)
                {
                    return new BlockNode
                    (
                        CallExpression((FunctionSymbol)symbol),
                        new DropNode()
                    );
                }
                else
                {
                    throw new InvalidOperationException("assignation or call expression expected");
                }
            }
            return new PointerAssignationNode(left, AbstractAssign(left));
        }

        // The arithmetic expression starts at the higher level, the Logical Or.
        public AbstractExpressionNode ArithmeticExpression() =>
            ArithmeticOperation(ArithmeticLevel.LogicalOr)();

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
                    yield return OperatorType.Pow;
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

                    // function call
                    if (IsOperatorUnconsumed(OperatorType.ParentheseOpen))
                    {
                        if (!(symbol is FunctionSymbol))
                        {
                            throw new InvalidOperationException("wrong symbol type for function call in primitive");
                        }
                        return CallExpression((FunctionSymbol)symbol);
                    }

                    if (!(symbol is AbstractAddressableSymbol))
                    {
                        throw new InvalidOperationException($"wrong symbol type for atomic identifier in primitive {symbol.Name} {symbol.Type}");
                    }

                    // check for pointer index access
                    var addressableSymbol = symbol as AbstractAddressableSymbol;
                    if (addressableSymbol != null)
                    {
                        if (IsOperatorUnconsumed(OperatorType.BracketOpen))
                        {
                            var indexExpression = BracketEnclosed(ArithmeticExpression);
                            var dereferencedNode = new DereferenceNode(new OperationNode(OperatorType.Add, new AddressableIdentifierNode(addressableSymbol), indexExpression));
                            // recursive a[x][y][z]...
                            while (IsOperatorUnconsumed(OperatorType.BracketOpen))
                            {
                                indexExpression = BracketEnclosed(ArithmeticExpression);
                                dereferencedNode = new DereferenceNode(new OperationNode(OperatorType.Add, dereferencedNode, indexExpression));
                            }
                            return dereferencedNode;
                        }
                    }

                    return new AddressableIdentifierNode((AbstractAddressableSymbol)symbol);

                case TokenType.Operator:
                    // (E)
                    if (IsOperatorUnconsumed(OperatorType.ParentheseOpen))
                    {
                        return ParentheseEnclosed(ArithmeticExpression);
                    }
                    // -E
                    else if (IsOperator(OperatorType.Sub))
                    {
                        return OperationNode.Oppose(ArithmeticPrimitive());
                    }
                    // !E
                    else if (IsOperator(OperatorType.Negate))
                    {
                        return OperationNode.Negate(ArithmeticPrimitive());
                    }
                    // @a
                    else if (IsOperator(OperatorType.Reference))
                    {
                        var refIdent = GetTokenValueOrThrow<string>(TokenType.Identifier, "expected identifier");
                        var refSymbol = Symbols.Search<AbstractAddressableSymbol>(refIdent);
                        return new ReferenceNode(new AddressableIdentifierNode(refSymbol));
                    }
                    // :E
                    else if (IsOperator(OperatorType.Dereference))
                    {
                        return new DereferenceNode(ArithmeticExpression());
                    }
                    break;
            }
            throw new InvalidOperationException($"arithmetic primitive unknow type {token}");
        }

        public ArrayVariableSymbol DefineArraySymbol()
            => DefineSymbol<ArrayVariableSymbol>(ident => new ArrayVariableSymbol(ident));

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
        public AbstractStatementNode Scoped<T>(Func<T> fun)
            where T : AbstractStatementNode
        {
            Symbols.PushScope();
            T value = fun();
            Symbols.PopScope();
            return value;
        }

        public T IdentifiedCond<T>(Func<int, T> fun) =>
            UniquelyIdentified(ref NextCondId, CurrentCond, fun);

        public T IdentifiedLoop<T>(Func<int, T> fun) =>
            UniquelyIdentified(ref NextLoopId, CurrentLoop, fun);

        public T UniquelyIdentified<T>(ref int id, Stack<int> stack, Func<int, T> fun)
        {
            id++;
            stack.Push(id);
            var value = fun(id);
            stack.Pop();
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
            catch (Exception)
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