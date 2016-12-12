namespace Lahda.Common
{
    public abstract class AbstractSymbol
    {
        public static AbstractSymbol Unknow = new PrimitiveVariableSymbol("0000");

        public ObjectType Type { get; }
        public string Name { get; }

        public AbstractSymbol(ObjectType type, string name)
        {
            Type = type;
            Name = name;
        }

        public bool IsUnknow => Name == Unknow.Name;
    }
}