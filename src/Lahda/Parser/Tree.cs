using System.Collections.Generic;
using System.Text;
using Lahda.Lexer;

namespace Lahda.Parser
{
    public sealed class Tree
    {
        public IToken Token { get; }

        public IEnumerable<Tree> Childs { get; }

        public Tree(IToken token, params Tree[] args) 
        {
            Token = token;
            Childs = new List<Tree>(args);
        }

        public override string ToString() => ToString(0);

        private string ToString(int index) 
        {
            var sb = new StringBuilder();
            for (var i = 0; i < index; i++)
                sb.Append("\t|");
            sb.Append($"- {Token}\n");
            foreach(var child in Childs)
                sb.Append(child.ToString(index + 1));
            return sb.ToString();
        }
    }
}