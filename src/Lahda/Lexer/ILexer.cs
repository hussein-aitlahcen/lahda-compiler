namespace Lahda.Lexer
{
    public interface ILexer
    {
        CompilationConfiguration Configuration { get; }
        IToken NextToken();
        IToken PeekToken();
    }
}