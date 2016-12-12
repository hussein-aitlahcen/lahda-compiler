namespace Lahda.Common
{
    public sealed class PrimitiveVariableSymbol : AbstractAddressableSymbol
    {
        private const int FloatSize = 1;

        public PrimitiveVariableSymbol(string name, int ptr = 0) : base(ObjectType.Floating, name, ptr, FloatSize)
        {
        }
    }
}