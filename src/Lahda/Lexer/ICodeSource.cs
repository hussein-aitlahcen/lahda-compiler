namespace Lahda.Lexer
{
    public interface ICodeSource 
    {
        string Path { get; }
        string Content { get; }
    }
}