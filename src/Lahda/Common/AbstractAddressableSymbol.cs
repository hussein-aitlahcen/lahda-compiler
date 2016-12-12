namespace Lahda.Common
{
    public abstract class AbstractAddressableSymbol : AbstractSymbol
    {
        public int StackPointer { get; set; }
        public int HeapPointer { get; set; }
        public int Size { get; set; }

        public AbstractAddressableSymbol(ObjectType type, string name, int ptr = 0, int heapPtr = 0, int size = 0) : base(type, name)
        {
            StackPointer = ptr;
            HeapPointer = heapPtr;
            Size = size;
        }
    }
}