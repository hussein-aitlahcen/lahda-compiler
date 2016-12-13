namespace Lahda.Common
{
    public sealed class PrimitiveVariableSymbol : AbstractAddressableSymbol
    {
        private const int FloatSize = 1;

        public PrimitiveVariableSymbol(string name) : base(ObjectType.Floating, name, 0, 0, 1)
        {
        }
    }
}