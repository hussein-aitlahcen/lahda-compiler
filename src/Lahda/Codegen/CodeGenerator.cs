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

        public CodeGenerator(ICodeOutput output, AbstractNode rootNode)
        {
            Output = output;
            RootNode = rootNode;
            Labels = new ScopeLabels();
            Optimize();
        }

        public void Build()
        {
            Write(".start");
            Generate(RootNode);
            Write("halt");
        }

        public void Write(string line) => Output.Write(line);
        private void Optimize() => RootNode.OptimizeChilds();
        private void PushLabel(ScopeType type) => Labels[type].Push(0);
        private void IncrementLabel(ScopeType type) => Labels[type].Increment();
        private int PopLabel(ScopeType type) => Labels[type].Pop();

        private void Generate(AbstractNode node)
        {
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

                    Generate(operation.Left);
                    if (requireInteger)
                        Write("ftoi");

                    Generate(operation.Right);
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
                    var decl = (DeclarationNode)node;
                    Write(";----------");
                    Write($"; var {decl.Identifier.Symbol.Name} = {decl.Expression}");
                    Write(";----------");
                    Write("push.f 0");
                    Generate(decl.Expression);
                    Write($"set {decl.Identifier.Symbol.Pointer}");
                    break;

                case NodeType.Assignation:
                    var assign = (AssignationNode)node;
                    Write(";----------");
                    Write($"; {assign.Identifier.Symbol.Name} = {assign.Expression}");
                    Write(";----------");
                    Generate(assign.Expression);
                    Write($"set {assign.Identifier.Symbol.Pointer}");
                    break;

                case NodeType.Literal:
                    var lit = (LiteralNode)node;
                    Write($"push.f {lit.Value}");
                    break;

                case NodeType.Identifier:
                    var ident = (IdentifierNode)node;
                    Write($"get {ident.Symbol.Pointer}");
                    break;

                case NodeType.Print:
                    var print = (PrintNode)node;
                    Write(";--------");
                    Write($"; print({print.Expression})");
                    Write(";--------");
                    Generate(print.Expression);
                    Write("out.f");

                    // make sure we get to a new line after the print
                    Write("push.i 10");
                    Write("out.c");
                    break;

                case NodeType.Loop:
                    var loop = (LoopNode)node;
                    Write(";--------");
                    Write("; loop");
                    Write(";--------");
                    IncrementLabel(ScopeType.Loop);
                    PushLabel(ScopeType.Loop);
                    var loopId = CurrentLabel(ScopeType.Loop);
                    Write(DeclareLabel(BeginLoop(loopId)));
                    Generate(loop.Conditional);
                    Write(Jump(BeginLoop(loopId)));
                    Write(DeclareLabel(EndLoop(loopId)));
                    PopLabel(ScopeType.Loop);
                    break;

                case NodeType.Break:
                    Write(Jump(EndLoop(CurrentLabel(ScopeType.Loop))));
                    break;

                case NodeType.Continue:
                    Write(Jump(BeginLoop(CurrentLabel(ScopeType.Loop))));
                    break;

                case NodeType.Conditional:
                    var cond = (ConditionalNode)node;
                    Write(";----------");
                    Write($"; if {cond.Expression}");
                    Write(";----------");
                    IncrementLabel(ScopeType.Conditional);
                    PushLabel(ScopeType.Conditional);
                    var condId = CurrentLabel(ScopeType.Conditional);
                    Generate(cond.Expression);
                    Write(JumpFalse(Else(condId)));
                    Generate(cond.TrueStatement);
                    Write(Jump(EndIf(condId)));
                    Write(DeclareLabel(Else(condId)));
                    Generate(cond.FalseStatement);
                    Write(DeclareLabel(EndIf(condId)));
                    PopLabel(ScopeType.Conditional);
                    break;
            }
        }


        private string JumpSpec(char type, string label) => $"jump{type} {label}";
        private string JumpFalse(string label) => JumpSpec('f', label);
        private string Jump(string label) => $"jump {label}";
        private string Else(string id) => $"else_{id}";
        private string EndIf(string id) => $"endif_{id}";
        private string BeginLoop(string id) => $"beginloop_{id}";
        private string EndLoop(string id) => $"endloop_{id}";
        private string DeclareLabel(string label) => $".{label}";
    }
}