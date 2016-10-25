namespace Lahda.Common
{
    public sealed class Symbol 
    {
        public static Symbol Unknow = new Symbol("0000", 0);

        public string Identifier { get; }        
        public ulong Pointer { get; }

        public Symbol(string identifier, ulong pointer) 
        {
            Identifier = identifier;
            Pointer = pointer;
        }

        public bool IsUnknow => Identifier == Unknow.Identifier;
    }
}