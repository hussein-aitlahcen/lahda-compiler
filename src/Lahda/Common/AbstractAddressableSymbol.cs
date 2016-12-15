using Lahda.Parser;
using Lahda.Parser.Impl;

namespace Lahda.Common
{
    public abstract class AbstractAddressableSymbol : AbstractSymbol
    {
        public int Pointer { get; set; }
        public AbstractAddressableSymbol(ObjectType type, string name, int ptr) : base(type, name)
        {
            Pointer = ptr;
        }

        public virtual AbstractStatementNode ReleaseInstruction()
        {
            return new BlockNode();
        }
    }
}