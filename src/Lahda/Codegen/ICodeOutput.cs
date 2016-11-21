using System.Text;

namespace Lahda.Codegen
{
    public sealed class StringBuilderOutput : ICodeOutput
    {
        private StringBuilder m_sb = new StringBuilder();

        public void Clear() => m_sb.Clear();

        public void Write(string command) => m_sb.AppendLine(command);

        public override string ToString() => m_sb.ToString();
    }

    public interface ICodeOutput
    {
        void Write(string command);
    }
}