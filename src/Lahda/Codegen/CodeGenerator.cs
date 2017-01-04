using Lahda.Parser;
using Lahda.Parser.Impl;
using System.Collections.Generic;
using System.Linq;
using Lahda.Common;

namespace Lahda.Codegen
{
    public sealed class CodeGenerator
    {
        public AbstractNode RootNode { get; }

        public ICodeOutput Output { get; }

        private int PointerIndex { get; set; }

        public CodeGenerator(ICodeOutput output, AbstractNode rootNode)
        {
            Output = output;
            RootNode = rootNode;
            Optimize();
        }

        public void Build()
        {
            InitGenerate(RootNode);
        }

        public void Writes(params string[] lines) => lines.ToList().ForEach(Write);
        public void Write(string line) => Output.Write(line);
        public void Debug(string message)
        {
            Write($"; {message.Replace("\n", "\n; ")}");
        }
        private void Optimize() => RootNode.OptimizeChilds();

        private void InitGenerate(AbstractNode node)
        {
            Generate(node);
        }

        private void PreGenerate(AbstractNode node)
        {
            switch (node.Type)
            {
                case NodeType.Block:
                    ((BlockNode)node).Statements.ForEach(PreGenerate);
                    break;

                case NodeType.Operation:
                    var operation = (OperationNode)node;
                    PreGenerate(operation.LeftOperand);
                    PreGenerate(operation.RightOperand);
                    break;

                case NodeType.Declaration:
                    if (node is AddressableDeclarationNode)
                    {
                        var primDecl = (AddressableDeclarationNode)node;
                        if (primDecl.Identifier.Symbol.Pointer > PointerIndex)
                        {
                            PointerIndex++;
                            Write(Pushf());
                        }
                    }
                    break;

                case NodeType.Loop:
                    var loop = (LoopNode)node;
                    PreGenerate(loop.Conditional);
                    PreGenerate(loop.Iteration);
                    break;

                case NodeType.Conditional:
                    var cond = (ConditionalNode)node;
                    PreGenerate(cond.Expression);
                    PreGenerate(cond.TrueStatement);
                    PreGenerate(cond.FalseStatement);
                    break;
            }
        }

        private void Generate(AbstractNode node)
        {
            var debugNodeTypes = new NodeType[]
            {
                /*NodeType.Block,
                NodeType.Literal,
                NodeType.Identifier,
                NodeType.Loop,
                NodeType.Operation,*/
            };
            if (debugNodeTypes.Contains(node.Type))
                Debug(node.ToString());
            switch (node.Type)
            {
                case NodeType.Block:
                    ((BlockNode)node).Statements.ForEach(Generate);
                    break;

                case NodeType.Operation:
                    var operation = (OperationNode)node;

                    // Operations that require two integer as input
                    var integerOperations = new[]
                    {
                        OperatorType.BitwiseAnd,
                        OperatorType.AndAlso,
                        OperatorType.BitwiseOr,
                        OperatorType.OrElse
                    };

                    var outputIntegerOperations = new[]
                    {
                        OperatorType.Equals,
                        OperatorType.Greater,
                        OperatorType.NotGreater,
                        OperatorType.Less,
                        OperatorType.NotLess,
                        OperatorType.NotEquals
                    };

                    /*
                        We accept floating only, sometimes we need integer (logical and/or)
                        In this case, we cast the floating into an integer and cast back the result into a floating one.
                    */
                    var requireInteger = integerOperations.Contains(operation.Operator);
                    var requireOutputTransform = outputIntegerOperations.Contains(operation.Operator);

                    Generate(operation.LeftOperand);
                    if (requireInteger)
                        Write("ftoi");

                    Generate(operation.RightOperand);
                    if (requireInteger)
                        Write("ftoi");

                    switch (operation.Operator)
                    {
                        case OperatorType.Add:
                            Write("add.f");
                            break;
                        case OperatorType.Sub:
                            Write("sub.f");
                            break;
                        case OperatorType.Mul:
                            Write("mul.f");
                            break;
                        case OperatorType.Div:
                            Write("div.f");
                            break;
                        case OperatorType.BitwiseAnd:
                        case OperatorType.AndAlso:
                            Write("and");
                            break;
                        case OperatorType.BitwiseOr:
                        case OperatorType.OrElse:
                            Write("or");
                            break;
                        case OperatorType.Equals:
                            Write("cmpeq.f");
                            break;
                        case OperatorType.NotEquals:
                            Write("cmpne.f");
                            break;
                        case OperatorType.NotGreater:
                            Write("cmple.f");
                            break;
                        case OperatorType.Less:
                            Write("cmplt.f");
                            break;
                        case OperatorType.NotLess:
                            Write("cmpge.f");
                            break;
                        case OperatorType.Greater:
                            Write("cmpgt.f");
                            break;
                        case OperatorType.EuclidianDiv:
                            Write(Drop());
                            Write(Drop());
                            Generate(BuiltinFunctions.EuclidianDiv(operation.LeftOperand, operation.RightOperand));
                            break;
                        case OperatorType.Mod:
                            Write(Drop());
                            Write(Drop());
                            Generate(BuiltinFunctions.Mod(operation.LeftOperand, operation.RightOperand));
                            break;
                        case OperatorType.Pow:
                            Write(Drop());
                            Write(Drop());
                            Generate(BuiltinFunctions.Pow(operation.LeftOperand, operation.RightOperand));
                            break;
                    }
                    if (requireInteger || requireOutputTransform)
                        Write("itof");
                    break;

                case NodeType.Declaration:
                    var dcl = (AddressableDeclarationNode)node;
                    switch (dcl.Identifier.Symbol.Type)
                    {
                        case ObjectType.Floating:
                            Generate(dcl.Expression);
                            Write
                            (
                                Set(dcl.Identifier.Symbol.Pointer)
                            );
                            break;

                        case ObjectType.Pointer:
                            var indexExpressions = ((MultiExpressionNode)dcl.Expression).Expressions;
                            indexExpressions.Reverse();
                            AbstractExpressionNode finalExpression = LiteralNode.One;
                            for (var i = 0; i < indexExpressions.Count - 1; i++)
                            {
                                finalExpression = new OperationNode
                                (
                                    OperatorType.Mul,
                                    new OperationNode
                                    (
                                        OperatorType.Add,
                                        LiteralNode.One,
                                        indexExpressions[i]
                                    ),
                                    finalExpression
                                );
                            }
                            finalExpression = new OperationNode
                            (
                                OperatorType.Mul,
                                indexExpressions.Last(),
                                finalExpression
                            );
                            Generate(BuiltinFunctions.BorrowMemory(finalExpression));
                            Write(Set(dcl.Identifier.Symbol.Pointer));
                            var tempVar = new AddressableIdentifierNode(new PrimitiveVariableSymbol("", PointerIndex + 1));
                            Write(Pushf());
                            Generate(BuiltinFunctions.BorrowMemory(new LiteralNode(indexExpressions.Count)));
                            Write(Set(tempVar.Symbol.Pointer));
                            indexExpressions.Reverse();
                            for (var i = 0; i < indexExpressions.Count; i++)
                            {
                                Generate
                                (
                                    new PointerAssignationNode
                                    (
                                        new OperationNode
                                        (
                                            OperatorType.Add,
                                            tempVar,
                                            new LiteralNode(i)
                                        ),
                                        indexExpressions[i]
                                    )
                                );
                            }
                            /*
                                    var a = bmem(6);
                                    var b = bmem(2);
                                    :b = 2;
                                    :b + 1 = 2;
                                    init_array(a, b, 2);
                            */
                            Generate(BuiltinFunctions.InitArray(dcl.Identifier, tempVar, new LiteralNode(indexExpressions.Count)));
                            Write(Get(tempVar.Symbol.Pointer));
                            Generate(BuiltinFunctions.RecoverMemory(tempVar));
                            break;
                    }
                    break;

                case NodeType.Reference:
                    var refnode = (ReferenceNode)node;
                    Write
                    (
                        Pushf(refnode.Identifier.Symbol.Pointer)
                    );
                    break;

                case NodeType.Dereference:
                    var deref = (DereferenceNode)node;
                    Generate(deref.Expression);
                    Writes
                    (
                        Ftoi(),
                        MemRead(),
                        Itof()
                    );
                    break;

                case NodeType.Assignation:
                    var assign = (AssignationNode)node;
                    Generate(assign.ValueExpression);
                    Write
                    (
                        Set(assign.Identifier.Symbol.Pointer)
                    );
                    break;

                case NodeType.Drop:
                    Write(Drop());
                    break;

                case NodeType.PointerAssignation:
                    var ptrAssign = (PointerAssignationNode)node;
                    Generate(ptrAssign.AddressExpression);
                    Write
                    (
                        Ftoi()
                    );
                    Generate(ptrAssign.ValueExpression);
                    Writes
                    (
                        Ftoi(),
                        MemWrite()
                    );
                    break;

                case NodeType.Literal:
                    var lit = (LiteralNode)node;
                    Write(Pushf(lit.Value));
                    break;

                case NodeType.Identifier:
                    var ident = (AddressableIdentifierNode)node;
                    Write
                    (
                        Get(ident.Symbol.Pointer)
                    );
                    break;

                case NodeType.Crash:
                    Write("halt");
                    break;

                case NodeType.Print:
                    if (node is PrintExpressionNode)
                    {
                        var printe = (PrintExpressionNode)node;
                        Generate(printe.Expression);
                        Write("out.f");
                    }
                    else
                    {
                        var prints = (PrintStringNode)node;
                        foreach (var c in prints.Content)
                        {
                            Write($"push.i {(int)c}");
                            Write("out.c");
                        }
                    }
                    Write("push.i 10");
                    Write("out.c");
                    break;

                case NodeType.Loop:
                    var loop = (LoopNode)node;
                    {
                        Write(DeclareLabel(BeginLoop(loop.UniqueId)));
                        Generate(loop.Conditional);
                        Write(DeclareLabel(IterationLoop(loop.UniqueId)));
                        Generate(loop.Iteration);
                        Write(Jump(BeginLoop(loop.UniqueId)));
                        Write(DeclareLabel(EndLoop(loop.UniqueId)));
                    }
                    break;

                case NodeType.Break:
                    var breakNode = (BreakNode)node;
                    Write(Jump(EndLoop(breakNode.LoopId)));
                    break;

                case NodeType.Continue:
                    var cont = (ContinueNode)node;
                    Write(Jump(EndIf(cont.CondId)));
                    break;

                case NodeType.Conditional:
                    var cond = (ConditionalNode)node;
                    {
                        Generate(cond.Expression);
                        Write(JumpFalse(Else(cond.UniqueId)));
                        Generate(cond.TrueStatement);
                        Write(Jump(EndIf(cond.UniqueId)));
                        Write(DeclareLabel(Else(cond.UniqueId)));
                        Generate(cond.FalseStatement);
                        Write(DeclareLabel(EndIf(cond.UniqueId)));
                    }
                    break;

                case NodeType.MultiExpression:
                    var multi = (MultiExpressionNode)node;
                    foreach (var expression in multi.Expressions)
                        Generate(expression);
                    break;

                case NodeType.Function:
                    var fun = (FunctionNode)node;
                    Write(DeclareLabel(fun.Identifier.Symbol.Name));
                    PointerIndex = fun.Arguments.Count - 1;
                    PreGenerate(fun.Statement);
                    Generate(fun.Statement);
                    if (fun.Identifier.Symbol.Name != "start")
                    {
                        Write(Pushf());
                        Write(Ret());
                    }
                    else
                    {
                        Write(Halt());
                    }
                    break;

                case NodeType.Return:
                    var ret = (ReturnNode)node;
                    Generate(ret.Expression);
                    Write(Ret());
                    break;

                case NodeType.Call:
                    var call = (CallNode)node;
                    Write(Prep(call.Target.Symbol.Name));
                    foreach (var arg in call.Parameters)
                    {
                        Generate(arg);
                    }
                    Write(Call(call.Parameters.Count));
                    break;

                case NodeType.Root:
                    var root = (RootNode)node;
                    foreach (var f in root.Functions)
                        Generate(f);
                    break;
            }
        }

        private string Prep(string function) => $"prep {function}";
        private string Call(int argCount) => $"call {argCount}";
        private string Addf() => "add.f";
        private string Addi() => "add.i";
        private string Halt() => "halt";
        private string Ret() => "ret";
        private string MemRead() => "read";
        private string Drop() => "drop";
        private string Dup() => "dup";
        private string Ftoi() => "ftoi";
        private string Itof() => "itof";
        private string MemWrite() => "write";
        private string Set(int index) => $"set {index}";
        private string Get(int index) => $"get {index}";
        private string Pushi(int x = 0) => $"push.i {x}";
        private string Pushf(float x = 0) => $"push.f {x.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        private string JumpSpec(char type, string label) => $"jump{type} {label}";
        private string JumpFalse(string label) => JumpSpec('f', label);
        private string Jump(string label) => $"jump {label}";
        private string Else(int id) => $"else_{id}";
        private string EndIf(int id) => $"endif_{id}";
        private string BeginLoop(int id) => $"beginloop_{id}";
        private string IterationLoop(int id) => $"iterloop_{id}";
        private string EndLoop(int id) => $"endloop_{id}";
        private string DeclareLabel(string label) => $".{label}";
    }
}