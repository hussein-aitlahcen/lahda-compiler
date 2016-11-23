namespace Lahda
{
    public sealed class Keyword
    {
        public KeywordType Type { get; }
        public string Value { get; }
        public Keyword(KeywordType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}