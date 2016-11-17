namespace Lahda.Common
{
    public sealed class Symbol
    {
        public static Symbol Unknow = new Symbol(SymbolType.Floating, "0000");

        public SymbolType Type { get; }
        public string Name { get; }
        public int Pointer { get; set; }

        public Symbol(SymbolType type, string name)
        {
            Type = type;
            Name = name;
        }

        public bool IsUnknow => Name == Unknow.Name;
    }
}