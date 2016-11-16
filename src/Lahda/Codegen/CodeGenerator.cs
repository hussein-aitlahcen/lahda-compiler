using Lahda.Parser;
using Lahda.Common;
using Lahda.Lexer;
using Lahda.Parser.Impl;
using System.Text;

namespace Lahda.Codegen
{
    public sealed class CodeGenerator
    {
        private sealed class GenerationState
        {
            public AbstractNode ParentNode { get; set; }
            public AbstractNode Node { get; set; }
            public StringBuilder Output { get; set; }
            public int IndentationLevel { get; set; }

            public GenerationState Copy(AbstractNode nextNode)
            {
                return Copy(nextNode, IndentationLevel);
            }

            public GenerationState Copy(AbstractNode nextNode, int indentLevel)
            {
                return new GenerationState()
                {
                    ParentNode = Node,
                    Node = nextNode,
                    IndentationLevel = indentLevel,
                    Output = Output
                };
            }
        }

        public AbstractNode RootNode { get; }

        public ulong VarId { get; private set; }
        public uint LabelId { get; private set; }

        public CodeGenerator(AbstractNode rootNode)
        {
            RootNode = rootNode;
        }

        public string Build()
        {
            VarId = 0;
            LabelId = 0;
            var sb = new StringBuilder();
            sb.AppendLine(".start");
            Generate(new GenerationState()
            {
                Node = RootNode,
                Output = sb,
                IndentationLevel = 1,
            });
            sb.AppendLine("halt");
            return sb.ToString();
        }

        private void Generate(GenerationState state)
        {
            switch (state.Node.Type)
            {
                case NodeType.Block:
                    foreach (var statement in ((BlockNode)state.Node).Statements)
                        Generate(state.Copy(statement, state.IndentationLevel + 1));
                    break;

                case NodeType.Operation:
                    var operation = (OperationNode)state.Node;
                    Generate(state.Copy(operation.Left));
                    Generate(state.Copy(operation.Right));
                    switch (operation.Operator)
                    {
                        case Operators.ADD:
                            Append(state, "add.f");
                            break;
                        case Operators.SUB:
                            Append(state, "subb.f");
                            break;
                        case Operators.MUL:
                            Append(state, "mul.f");
                            break;
                        case Operators.DIV:
                            Append(state, "div.f");
                            break;
                        case Operators.AND:
                        case Operators.ANDALSO:
                            Append(state, "and");
                            break;
                        case Operators.OR:
                        case Operators.ORELSE:
                            Append(state, "or");
                            break;
                        case Operators.EQUALS:
                            Append(state, "cmpeq.f");
                            break;
                        case Operators.NOT_EQUALS:
                            Append(state, "cmpne.f");
                            break;
                        case Operators.NOT_GREATER:
                            Append(state, "cmple.f");
                            break;
                        case Operators.LESS:
                            Append(state, "cmplt.f");
                            break;
                        case Operators.NOT_LESS:
                            Append(state, "cmpge.f");
                            break;
                        case Operators.GREATER:
                            Append(state, "cmpgt.f");
                            break;
                        case Operators.MOD:

                            break;
                    }
                    break;

                case NodeType.Declaration:
                    var decl = (DeclarationNode)state.Node;
                    decl.Identifier.Symbol.Pointer = VarId++;
                    Append(state, ";----------");
                    Append(state, $"; var {decl.Identifier.Symbol.Name} = {decl.Expression}");
                    Append(state, ";----------");
                    Append(state, "push.f 0");
                    Generate(state.Copy(decl.Expression));
                    Append(state, $"set {decl.Identifier.Symbol.Pointer}");
                    break;

                case NodeType.Assignation:
                    var assign = (AssignationNode)state.Node;
                    Append(state, ";----------");
                    Append(state, $"; {assign.Identifier.Symbol.Name} = {assign.Expression}");
                    Append(state, ";----------");
                    Generate(state.Copy(assign.Expression));
                    Append(state, $"set {assign.Identifier.Symbol.Pointer}");
                    break;

                case NodeType.Literal:
                    var lit = (LiteralNode)state.Node;
                    Append(state, $"push.f {lit.Value}");
                    break;

                case NodeType.Identifier:
                    var ident = (IdentifierNode)state.Node;
                    Append(state, $"get {ident.Symbol.Pointer}");
                    break;

                case NodeType.Loop:
                    var loop = (LoopNode)state.Node;
                    loop.Id = LabelId++;
                    Append(state, ";----------");
                    Append(state, $"; loop_{loop.Id}");
                    Append(state, ";----------");
                    BeginLoop(state, loop.Id);
                    Generate(state.Copy(loop.StmtsBlock, state.IndentationLevel + 1));
                    EndLoop(state, loop.Id);
                    break;

                case NodeType.Break:
                    BreakLoop(state, state.Node.Id);
                    break;

                case NodeType.Continue:
                    ContinueLoop(state, state.Node.Id);
                    break;

                case NodeType.Conditional:
                    var cond = (ConditionalNode)state.Node;
                    cond.Id = LabelId++;
                    Generate(state.Copy(cond.Expression));
                    Append(state, $"jumpf ifnot_{cond.Id}");
                    Generate(state.Copy(cond.TrueStatements));
                    Append(state, $"jump endif_{cond.Id}");
                    Append(state, $".ifnot_{cond.Id}");
                    Generate(state.Copy(cond.FalseStatements));
                    Append(state, $".endif_{cond.Id}");
                    break;
            }
        }

        private void BreakLoop(GenerationState state, uint level)
        {
            Append(state, $"jump end_loop_{level}");
        }

        private void ContinueLoop(GenerationState state, uint level)
        {
            Append(state, $"jump begin_loop_{level}");
        }

        private void BeginLoop(GenerationState state, uint level)
        {
            Append(state, $".begin_loop_{level}");
        }

        private void EndLoop(GenerationState state, uint level)
        {
            Append(state, $".end_loop_{level}");
        }

        private void Append(GenerationState state, string v)
        {
            for (int i = 0; i < state.IndentationLevel; i++)
                state.Output.Append("  ");
            state.Output.AppendLine(v);
        }
    }
}