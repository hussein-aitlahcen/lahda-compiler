namespace Lahda.Common
{
    public sealed class FunctionSymbol : AbstractSymbol
    {
        public int ParameterCount { get; set; }

        public FunctionSymbol(string name) : base(ObjectType.Function, name)
        {
        }
    }
}