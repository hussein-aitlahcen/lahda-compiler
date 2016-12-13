namespace Lahda.Common
{
    public abstract class AbstractAddressableSymbol : AbstractSymbol
    {
        public int StackPointer { get; set; }
        public int HeapPointer { get; set; }
        public int Size { get; set; }

        public AbstractAddressableSymbol(ObjectType type, string name, int ptr, int heapPtr, int size) : base(type, name)
        {
            StackPointer = ptr;
            HeapPointer = heapPtr;
            Size = size;
        }
    }
}