using System.Linq;

namespace Lahda.Lexer.Impl
{
    public sealed class MultiCodeSource : ICodeSource
    {
        public string Path { get; }
        public string Content { get; }
        public CodeSource[] Sources { get; }

        public MultiCodeSource(CodeSource[] sources)
        {
            Sources = sources;
            Path = string.Join("|", Sources.Select(s => s.Path));
            Content = string.Join("\n\n", Sources.Select(s => s.Content));
        }
    }
}