namespace Lahda.Lexer
{
    public class Token : IToken
    {
        public TokenType Type { get; }
        public TokenPosition Position { get; }
        public Token(TokenType type, TokenPosition position) 
        {
            Type = type;
            Position = position;
        }

        public override string ToString() => $"tok<{Type}, {Position}>";
    }
}