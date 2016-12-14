namespace Lahda.Common
{
    public sealed class PrimitiveVariableSymbol : AbstractAddressableSymbol
    {
        public PrimitiveVariableSymbol(string name, int ptr = 0) : base(ObjectType.Floating, name, ptr)
        {
        }
    }
}