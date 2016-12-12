using Lahda.Parser;
using Lahda.Lexer;
using Lahda.Parser.Impl;
using System.Linq;

namespace Lahda.Codegen
{
    public sealed class CodeGenerator
    {
        public AbstractNode RootNode { get; }

        public ICodeOutput Output { get; }

        private ScopeLabels Labels { get; set; }

        private string CurrentLabel(ScopeType type) => Labels[type].CurrentLabel;

        private int PointerIndex { get; set; }

        public CodeGenerator(ICodeOutput output, AbstractNode rootNode)
        {
            Output = output;
            RootNode = rootNode;
            Labels = new ScopeLabels();
            Optimize();
        }

        public void Build()
        {
            InitGenerate(RootNode);
            Write("halt");
        }

        public void Write(string line) => Output.Write(line);
        public void Debug(string message)
        {
            Write($"; {message.Replace("\n", "\n; ")}");
        }
        private void Optimize() => RootNode.OptimizeChilds();
        private void PushLabel(ScopeType type) => Labels[type].Push(0);
        private void IncrementLabel(ScopeType type) => Labels[type].Increment();
        private int PopLabel(ScopeType type) => Labels[type].Pop();

        private void InitGenerate(AbstractNode node)
        {
            PointerIndex = -1;
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
                    if (node is PrimitiveDeclarationNode)
                    {
                        var primDecl = (PrimitiveDeclarationNode)node;
                        if (primDecl.Identifier.Symbol.Pointer > PointerIndex)
                        {
                            PointerIndex++;
                            Write(Push());
                        }
                    }
                    else if (node is ArrayDeclarationNode)
                    {
                        var arrDecl = (ArrayDeclarationNode)node;
                        if (arrDecl.Identifier.Symbol.Pointer > PointerIndex)
                        {
                            for (var i = 0; i < arrDecl.Identifier.Symbol.Size; i++)
                            {
                                PointerIndex++;
                                Write(Push());
                            }
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
            var debugNodeTypesIgnore = new[]
            {
                NodeType.Block,
                NodeType.Literal,
                NodeType.Identifier,
                NodeType.Loop,
                NodeType.Operation,
            };
            if (!debugNodeTypesIgnore.Contains(node.Type))
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

                    /*
                        We accept floating only, sometimes we need integer (logical and/or)
                        In this case, we cast the floating into an integer and cast back the result into a floating one.
                    */
                    var requireInteger = integerOperations.Contains(operation.Operator);

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
                        case OperatorType.Mod:
                            // TODO: modulo
                            break;
                        case OperatorType.Pow:
                            // TODO: pow
                            break;
                    }
                    if (requireInteger)
                        Write("itof");
                    break;

                case NodeType.Declaration:
                    if (node is PrimitiveDeclarationNode)
                    {
                        var primDecl = (PrimitiveDeclarationNode)node;
                        Generate(primDecl.Expression);
                        Write(Set(primDecl.Identifier.Symbol.Pointer));
                    }
                    else if (node is ArrayDeclarationNode)
                    {
                        var arrDecl = (ArrayDeclarationNode)node;
                        Generate(arrDecl.Expression);
                        Write(Set(arrDecl.Identifier.Symbol.Pointer));
                    }
                    break;

                case NodeType.Assignation:
                    var assign = (AssignationNode)node;
                    Generate(assign.Expression);
                    Write($"set {assign.Identifier.Symbol.Pointer}");
                    break;

                case NodeType.Literal:
                    var lit = (LiteralNode)node;
                    Write($"push.f {lit.Value}");
                    break;

                case NodeType.Identifier:
                    if (node is PrimitiveIdentifierNode)
                    {
                        Write(Get(((PrimitiveIdentifierNode)node).Symbol.Pointer));
                    }
                    else if (node is ArrayIdentifierNode)
                    {
                        // TODO: push address
                    }
                    break;

                case NodeType.Print:
                    var print = (PrintNode)node;
                    Generate(print.Expression);
                    Write("out.f");

                    // make sure we get to a new line after the print
                    Write("push.i 10");
                    Write("out.c");
                    break;

                case NodeType.Loop:
                    var loop = (LoopNode)node;
                    IncrementLabel(ScopeType.Loop);
                    PushLabel(ScopeType.Loop);
                    var loopId = CurrentLabel(ScopeType.Loop);
                    {
                        Write(DeclareLabel(BeginLoop(loopId)));
                        Generate(loop.Conditional);
                        Write(DeclareLabel(IterationLoop(loopId)));
                        Generate(loop.Iteration);
                        Write(Jump(BeginLoop(loopId)));
                        Write(DeclareLabel(EndLoop(loopId)));
                    }
                    PopLabel(ScopeType.Loop);
                    break;

                case NodeType.Break:
                    Write(Jump(EndLoop(CurrentLabel(ScopeType.Loop))));
                    break;

                case NodeType.Continue:
                    Write(Jump(IterationLoop(CurrentLabel(ScopeType.Loop))));
                    break;

                case NodeType.Conditional:
                    var cond = (ConditionalNode)node;
                    IncrementLabel(ScopeType.Conditional);
                    PushLabel(ScopeType.Conditional);
                    var condId = CurrentLabel(ScopeType.Conditional);
                    {
                        Generate(cond.Expression);
                        Write(JumpFalse(Else(condId)));
                        Generate(cond.TrueStatement);
                        Write(Jump(EndIf(condId)));
                        Write(DeclareLabel(Else(condId)));
                        Generate(cond.FalseStatement);
                        Write(DeclareLabel(EndIf(condId)));
                    }
                    PopLabel(ScopeType.Conditional);
                    break;

                case NodeType.Function:
                    var fun = (FunctionNode)node;
                    Write(DeclareLabel(fun.Identifier.Symbol.Name));
                    // tree traversal 
                    for (var i = 0; i < fun.Arguments.Count; i++)
                        Write(Push());
                    PreGenerate(fun.Statement);
                    Generate(fun.Statement);
                    Write(Push());
                    Write(Ret());
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
        private string Ret() => "ret";
        private string Set(int index) => $"set {index}";
        private string Get(int index) => $"get {index}";
        private string Push() => Push(0);
        private string Push(float x) => $"push.f {x}";
        private string JumpSpec(char type, string label) => $"jump{type} {label}";
        private string JumpFalse(string label) => JumpSpec('f', label);
        private string Jump(string label) => $"jump {label}";
        private string Else(string id) => $"else_{id}";
        private string EndIf(string id) => $"endif_{id}";
        private string BeginLoop(string id) => $"beginloop_{id}";
        private string IterationLoop(string id) => $"iterloop_{id}";
        private string EndLoop(string id) => $"endloop_{id}";
        private string DeclareLabel(string label) => $".{label}";
    }
}