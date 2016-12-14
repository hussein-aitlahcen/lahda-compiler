namespace Lahda.Common
{
    public sealed class ArrayVariableSymbol : AbstractAddressableSymbol
    {
        public ArrayVariableSymbol(string name) : base(ObjectType.Pointer, name, 0)
        {
        }
    }
}