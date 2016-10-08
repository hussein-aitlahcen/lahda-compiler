namespace Lahda.Lexer
{
    public interface IToken 
    {
        TokenType Type { get; }
        TokenPosition Position { get; }
    }
}