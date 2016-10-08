using System.IO;

namespace Lahda.Lexer.Impl
{
    public sealed class CodeSource : ICodeSource
    {
        public string Path { get; }
        public string Content { get; }

        public CodeSource(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public static CodeSource FromMemory(string content) => new CodeSource("[memory]", content);
        public static CodeSource FromFile(string path) => new CodeSource(path, File.ReadAllText(path));
    }
}