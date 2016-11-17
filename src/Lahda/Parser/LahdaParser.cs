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
            var token = PeekToken();
            if (!IsValue(TokenType.SpecialCharacter, ";"))
            {
                throw new InvalidOperationException("end of statement");
            }
            NextToken();
        }

        public AbstractStatementNode NextExpression()
        {
            var token = PeekToken() as ValueToken<string>;
            switch (token.Type)
            {
                case TokenType.Operator:
                    if (token.Value == Operators.BRACE_OPEN)
                        return StatementsBlock();
                    break;
                case TokenType.Keyword:
                    return DetermineKeywordExpression(token.Value);

                case TokenType.Identifier:
                    return Statement(AssignationExpression);
            }
            throw new InvalidOperationException($"next expression {token.Type}");
        }

        public AbstractStatementNode DetermineKeywordExpression(string keyword)
        {
            switch (keyword)
            {
                case Keywords.VAR:
                    return Statement(DeclarationExpression);

                case Keywords.PRINT:
                    return Statement(PrintExpression);

                case Keywords.WHILE:
                case Keywords.FOR:
                case Keywords.DO:
                    return LoopExpression();

                case Keywords.IF:
                    return ConditionalExpression();
            }
            throw new InvalidOperationException($"keyword expression {keyword}");
        }

        public AbstractStatementNode LoopExpression()
        {
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
            throw new InvalidOperationException("unknow loop type");
        }

        public AbstractStatementNode DoExpression()
        {
            NextToken();
            var block = StatementsBlock();
            if (IsKeyword(Keywords.WHILE))
            {
                NextToken();
                if (IsOperator(Operators.PARENTHESE_OPEN))
                {
                    NextToken();
                    var exp = ArithmeticExpression();
                    if (IsOperator(Operators.PARENTHESE_CLOSE))
                    {
                        NextToken();
                        return new LoopNode(exp, block);
                    }
                }
            }
            else if (IsKeyword(Keywords.UNTIL))
            {
                NextToken();
                if (IsOperator(Operators.PARENTHESE_OPEN))
                {
                    NextToken();
                    var exp = ArithmeticExpression();
                    if (IsOperator(Operators.PARENTHESE_CLOSE))
                    {
                        NextToken();
                        return new LoopNode(exp, block, true);
                    }
                }
            }
            else if (IsKeyword(Keywords.FOREVER))
            {
                NextToken();
                return new LoopNode(new LiteralNode(1), block);
            }
            throw new InvalidOperationException("invalid do expression");
        }

        public AbstractStatementNode WhileExpression()
        {
            NextToken();
            if (IsOperator(Operators.PARENTHESE_OPEN))
            {
                NextToken();
                var exp = ArithmeticExpression();
                if (IsOperator(Operators.PARENTHESE_CLOSE))
                {
                    NextToken();
                    var block = StatementsBlock();
                    return new LoopNode(exp, block);
                }
            }
            throw new InvalidOperationException("invalid while expression");
        }

        public AbstractStatementNode ForExpression()
        {
            NextToken();
            if (IsOperator(Operators.PARENTHESE_OPEN))
            {
                NextToken();
                var initialization = Statement(DeclarationExpression);
                var condition = Statement(ArithmeticExpression);
                var iteration = AssignationExpression();
                if (IsOperator(Operators.PARENTHESE_CLOSE))
                {
                    NextToken();
                    var block = StatementsBlock();
                    return new BlockNode(initialization, new LoopNode(condition, new BlockNode(block, iteration)));
                }
            }
            throw new InvalidOperationException("invalid for expression");
        }

        public ConditionalNode ConditionalExpression()
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
                    var trueStatements = StatementsBlock();
                    var falseStatements = new BlockNode();

                    // second block
                    if (IsKeyword(Keywords.ELSE))
                    {
                        NextToken(); // consume else
                        falseStatements = StatementsBlock();
                    }

                    return new ConditionalNode(condition, trueStatements, falseStatements);
                }
            }
            throw new InvalidOperationException($"conditional expression");
        }

        public BlockNode StatementsBlock()
        {
            Symbols.PushScope();
            var expressions = new List<AbstractStatementNode>();
            if (IsOperator(Operators.BRACE_OPEN))
            {
                NextToken(); // consumed brace open
                while (!IsOperator(Operators.BRACE_CLOSE))
                {
                    expressions.Add(NextExpression());
                }
                NextToken(); // consumed brace close
            }
            else
            {
                throw new Exception("instruction block");
            }
            Symbols.PopScope();
            return new BlockNode(expressions);
        }

        public T Statement<T>(Func<T> fun)
        {
            T expression = fun();
            EnsureEndOfStatement();
            return expression;
        }

        public PrintNode PrintExpression()
        {
            NextToken();
            return new PrintNode(ArithmeticExpression());
        }

        public DeclarationNode DeclarationExpression()
        {
            NextToken(); // consume 'var';

            var ident = (ValueToken<string>)NextToken();
            if (ident.Type != TokenType.Identifier)
                throw new InvalidOperationException($"declaration identifier {ident.Type}");

            // consume the operator
            if (!IsValue(TokenType.Operator, Operators.ASSIGN))
                throw new InvalidOperationException($"declaration operator missing");
            NextToken();

            var expression = ArithmeticExpression();

            var symbol = new Symbol(SymbolType.Floating, ident.Value);
            Symbols.DefineSymbol(symbol);

            return new DeclarationNode(new IdentifierNode(symbol), expression);
        }

        public AssignationNode AssignationExpression()
        {
            var ident = (ValueToken<string>)NextToken();
            if (ident.Type != TokenType.Identifier)
                throw new InvalidOperationException($"assignation identifier {ident.Type}");

            if (!IsValue(TokenType.Operator, Operators.ASSIGN))
                throw new InvalidOperationException($"assignation operator missing");
            NextToken();

            var symbol = Symbols.Search(ident.Value);
            if (symbol.IsUnknow)
                throw new InvalidOperationException($"unknow symbol {ident.Value}");

            var expression = ArithmeticExpression();

            return new AssignationNode(new IdentifierNode(symbol), expression);
        }

        public AbstractExpressionNode ArithmeticExpression() => ArithmeticOperation(ArithmeticLevel.LogicalOr)();

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

        private AbstractExpressionNode ExecuteIfOperator(AbstractExpressionNode prime, Func<AbstractExpressionNode> operation, IEnumerable<string> operators)
        {
            var op = operators.FirstOrDefault(IsOperator);
            if (op != null)
            {
                NextToken();
                return new OperationNode(op, prime, operation());
            }
            return prime;
        }

        private AbstractExpressionNode ArithmeticPrimitive()
        {
            var token = PeekToken();
            switch (token.Type)
            {
                case TokenType.Keyword:
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

        private bool IsType(TokenType type) => PeekToken().Type == type;

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