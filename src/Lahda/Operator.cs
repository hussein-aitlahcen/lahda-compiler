namespace Lahda
{
    public sealed class Operator
    {
        public OperatorType Type { get; }
        public string Value { get; }
        public Operator(OperatorType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}