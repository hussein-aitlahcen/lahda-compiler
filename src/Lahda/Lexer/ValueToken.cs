namespace Lahda.Lexer
{
    public sealed class ValueToken<T> : Token
    {
        public T Value { get; }

        public ValueToken(TokenType type, TokenPosition position, T value) 
            : base(type, position)
        {
            Value = value;
        }

        public override string ToString() => $"val<{base.ToString()}, {Value}>";
    }
}