using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lahda.Lexer;

namespace Lahda
{
    public sealed class CompilationConfiguration
    {
        public List<Operator> Operators { get; }
        public List<Keyword> Keywords { get; }
        public ICodeSource CodeSource { get; }

        public CompilationConfiguration(ICodeSource codeSource)
        {
            CodeSource = codeSource;
            Keywords = new List<Keyword>();
            Operators = new List<Operator>();
        }

        public string EndOfStatement() => ";";

        public Regex BuildIdentifierRegex() => R("^([A-Za-z_][A-Za-z0-9_]*)$");

        public Regex BuildFloatingRegex() => R("^(((([0-9]+\\.[0-9]*)|([0-9]*\\.[0-9]+))([Ee][+-]?[0-9]+)?)|([0-9]+([Ee][+-]?[0-9]+)))$");

        public Regex BuildIntegerRegex() => R("^([0-9]+)$");

        public Regex BuildKeywordRegex()
        {
            var sb = new StringBuilder();
            sb.Append("^(");
            sb.Append(string.Join("|", Keywords.Select(key => Regex.Escape(key.Value))));
            sb.Append(")$");
            return R(sb.ToString());
        }

        public Regex BuildOperatorRegex()
        {
            var sb = new StringBuilder();
            sb.Append("^(");
            sb.Append(string.Join("|", Operators.Select(op => Regex.Escape(op.Value))));
            sb.Append(")$");
            return R(sb.ToString());
        }

        private Regex R(string regex) => new Regex(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void AddOperator(OperatorType type, string value) => Operators.Add(new Operator(type, value));

        public void AddKeyword(KeywordType type, string value) => Keywords.Add(new Keyword(type, value));

        public OperatorType GetOperatorType(string value)
        {
            foreach (var op in Operators)
                if (op.Value == value)
                    return op.Type;
            return OperatorType.None;
        }

        public KeywordType GetKeywordType(string value)
        {
            foreach (var key in Keywords)
                if (key.Value == value)
                    return key.Type;
            return KeywordType.None;
        }

        public string GetOperator(OperatorType type) => Operators.First(op => op.Type == type).Value;

        public string GetKeyword(KeywordType type) => Keywords.First(key => key.Type == type).Value;

        public bool IsOperator(OperatorType type, string value) => Operators.Any(op => op.Type == type && op.Value == value);

        public bool IsKeyword(KeywordType type, string value) => Keywords.Any(key => key.Type == type && key.Value == value);
    }
}