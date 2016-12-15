using Lahda.Parser;

namespace Lahda.Common
{
    public sealed class ArrayVariableSymbol : AbstractAddressableSymbol
    {
        public ArrayVariableSymbol(string name) : base(ObjectType.Pointer, name, 0)
        {
        }

        public override AbstractStatementNode ReleaseInstruction()
        {
            return BuiltinFunctions.RecoverMemory(new AddressableIdentifierNode(this));
        }
    }
}