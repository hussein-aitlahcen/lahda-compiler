namespace Lahda.Lexer
{
    public sealed class TokenPosition
    {
        public ICodeSource CodeSource { get; }
        public int Line { get; }
        public int Column { get; }
        public TokenPosition(ICodeSource codeSource, int line, int column) 
        {
            CodeSource = codeSource;
            Line = line;
            Column = column;
        }

        public override string ToString() => $"pos<{Line}, {Column}>";
    }
}