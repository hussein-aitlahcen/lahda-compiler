namespace Lahda.Lexer
{
    public interface ILexer 
    {
        IToken NextToken();
        IToken PeekToken();
    }
}