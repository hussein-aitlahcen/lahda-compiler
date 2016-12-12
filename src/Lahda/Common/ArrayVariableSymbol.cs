namespace Lahda.Common
{
    public sealed class ArrayVariableSymbol : AbstractAddressableSymbol
    {
        public ArrayVariableSymbol(string name, int size) : base(ObjectType.Array, name, size)
        {
        }
    }
}