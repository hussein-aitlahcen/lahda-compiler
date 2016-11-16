namespace Lahda.Common
{
    public sealed class Symbol
    {
        public static Symbol Unknow = new Symbol("0000");

        public string Name { get; }
        public ulong Pointer { get; set; }

        public Symbol(string name)
        {
            Name = name;
        }

        public bool IsUnknow => Name == Unknow.Name;
    }
}