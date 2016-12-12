namespace Lahda.Common
{
    public abstract class AbstractAddressableSymbol : AbstractSymbol
    {
        public int Pointer { get; set; }
        public int Size { get; set; }

        public AbstractAddressableSymbol(ObjectType type, string name, int size) : base(type, name) { }
    }
}