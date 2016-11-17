using Lahda.Parser;
using Lahda.Lexer;
using Lahda.Parser.Impl;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Lahda.Codegen
{
    public sealed class CodeGenerator
    {
        private enum ScopeType
        {
            Loop,
            Conditional
        }

        public AbstractNode RootNode { get; }

        private Dictionary<ScopeType, Stack<int>> ScopeLabel { get; set; }

        private string CurrentLabel(ScopeType type) => string.Join("_", ScopeLabel[type]);

        public CodeGenerator(AbstractNode rootNode)
        {
            RootNode = rootNode;
            ScopeLabel = new Dictionary<ScopeType, Stack<int>>()
            {
                { ScopeType.Loop, new Stack<int>(new []{ 0 }) },
                { ScopeType.Conditional, new Stack<int>(new []{ 0 }) },
            };
        }

        public string Build()
        {
            Optimize();
            var sb = new StringBuilder();
            sb.AppendLine(".start");
            foreach (var line in Generate(RootNode))
                sb.AppendLine(line);
            sb.AppendLine("halt");
            return sb.ToString();
        }

        private void Optimize()
        {
            RootNode.OptimizeChilds();
        }

        private void PushScopeLabel(ScopeType type) => ScopeLabel[type].Push(0);
        private void IncrementScopeLabel(ScopeType type) => ScopeLabel[type].Push(ScopeLabel[type].Pop() + 1);
        private void PopScopeLabel(ScopeType type) => ScopeLabel[type].Pop();

        private IEnumerable<string> Generate(AbstractNode node)
        {
            switch (node.Type)
            {
                case NodeType.Block:
                    foreach (var statement in ((BlockNode)node).Statements)
                        foreach (var line in Generate(statement))
                            yield return line;
                    break;

                case NodeType.Operation:
                    var operation = (OperationNode)node;
                    foreach (var line in Generate(operation.Left))
                        yield return line;
                    foreach (var line in Generate(operation.Right))
                        yield return line;
                    Generate(operation.Right);
                    switch (operation.Operator)
                    {
                        case Operators.ADD:
                            yield return "add.f";
                            break;
                        case Operators.SUB:
                            yield return "subb.f";
                            break;
                        case Operators.MUL:
                            yield return "mul.f";
                            break;
                        case Operators.DIV:
                            yield return "div.f";
                            break;
                        case Operators.AND:
                        case Operators.ANDALSO:
                            yield return "and";
                            break;
                        case Operators.OR:
                        case Operators.ORELSE:
                            yield return "or";
                            break;
                        case Operators.EQUALS:
                            yield return "cmpeq.f";
                            break;
                        case Operators.NOT_EQUALS:
                            yield return "cmpne.f";
                            break;
                        case Operators.NOT_GREATER:
                            yield return "cmple.f";
                            break;
                        case Operators.LESS:
                            yield return "cmplt.f";
                            break;
                        case Operators.NOT_LESS:
                            yield return "cmpge.f";
                            break;
                        case Operators.GREATER:
                            yield return "cmpgt.f";
                            break;
                        case Operators.MOD:

                            break;
                    }
                    break;

                case NodeType.Declaration:
                    var decl = (DeclarationNode)node;
                    yield return ";----------";
                    yield return $"; var {decl.Identifier.Symbol.Name} = {decl.Expression}";
                    yield return ";----------";
                    yield return "push.f 0";
                    foreach (var line in Generate(decl.Expression))
                        yield return line;
                    yield return $"set {decl.Identifier.Symbol.Pointer}";
                    break;

                case NodeType.Assignation:
                    var assign = (AssignationNode)node;
                    yield return ";----------";
                    yield return $"; {assign.Identifier.Symbol.Name} = {assign.Expression}";
                    yield return ";----------";
                    foreach (var line in Generate(assign.Expression))
                        yield return line;
                    yield return $"set {assign.Identifier.Symbol.Pointer}";
                    break;

                case NodeType.Literal:
                    var lit = (LiteralNode)node;
                    yield return $"push.f {lit.Value}";
                    break;

                case NodeType.Identifier:
                    var ident = (IdentifierNode)node;
                    yield return $"get {ident.Symbol.Pointer}";
                    break;

                case NodeType.Print:
                    var print = (PrintNode)node;
                    yield return ";--------";
                    yield return $"; print({print.Expression})";
                    yield return ";--------";
                    foreach (var line in Generate(print.Expression))
                        yield return line;
                    yield return "out.f";
                    break;

                case NodeType.Loop:
                    var loop = (LoopNode)node;
                    yield return ";--------";
                    yield return $"; loop";
                    yield return ";--------";
                    IncrementScopeLabel(ScopeType.Loop);
                    var loopId = CurrentLabel(ScopeType.Loop);
                    PushScopeLabel(ScopeType.Loop);
                    yield return DeclareLabel(BeginLoop(loopId));
                    foreach (var line in Generate(loop.Cond))
                        yield return line;
                    yield return DeclareLabel(EndLoop(loopId));
                    PopScopeLabel(ScopeType.Loop);
                    break;

                case NodeType.Break:
                    BreakLoop(CurrentLabel(ScopeType.Loop));
                    break;

                case NodeType.Conditional:
                    var cond = (ConditionalNode)node;
                    yield return ";----------";
                    yield return $"; if {cond.Expression}";
                    yield return ";----------";
                    IncrementScopeLabel(ScopeType.Conditional);
                    var condId = CurrentLabel(ScopeType.Conditional);
                    PushScopeLabel(ScopeType.Conditional);
                    foreach (var line in Generate(cond.Expression))
                        yield return line;
                    yield return JumpFalse(IfNot(condId));
                    foreach (var line in Generate(cond.TrueStatements))
                        yield return line;
                    yield return Jump(BeginLoop(condId));
                    yield return DeclareLabel(IfNot(condId));
                    foreach (var line in Generate(cond.FalseStatements))
                        yield return line;
                    yield return DeclareLabel(EndIf(condId));
                    PopScopeLabel(ScopeType.Conditional);
                    break;
            }
        }


        private string JumpSpec(char type, string label) => $"jump{type} {label}";
        private string JumpFalse(string label) => JumpSpec('f', label);
        private string Jump(string label) => $"jump {label}";
        private string IfNot(string id) => $"ifnot_{id}";
        private string EndIf(string id) => $"endif_{id}";
        private string BeginLoop(string id) => $"begin_loop_{id}";
        private string EndLoop(string id) => $"end_loop_{id}";
        private string BreakLoop(string id) => Jump(EndLoop(id));
        private string DeclareLabel(string label) => $".{label}";
    }
}