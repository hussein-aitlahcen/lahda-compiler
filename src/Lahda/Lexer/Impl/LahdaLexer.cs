namespace Lahda.Lexer.Impl
{
    public sealed class LahdaLexer : AbstractLexer
    {
        public LahdaLexer(CompilationConfiguration configuration) : base(configuration)
        {
            configuration.AddKeyword(KeywordType.If, "if");
            configuration.AddKeyword(KeywordType.Else, "else");
            configuration.AddKeyword(KeywordType.Break, "break");
            configuration.AddKeyword(KeywordType.Continue, "continue");
            configuration.AddKeyword(KeywordType.While, "while");
            configuration.AddKeyword(KeywordType.Do, "do");
            configuration.AddKeyword(KeywordType.For, "for");
            configuration.AddKeyword(KeywordType.Float, "float");
            configuration.AddKeyword(KeywordType.Forever, "forever");
            configuration.AddKeyword(KeywordType.Reset, "reset");
            configuration.AddKeyword(KeywordType.Var, "var");
            configuration.AddKeyword(KeywordType.Until, "until");
            configuration.AddKeyword(KeywordType.False, "false");
            configuration.AddKeyword(KeywordType.True, "true");
            configuration.AddKeyword(KeywordType.String, "string");
            configuration.AddKeyword(KeywordType.Print, "print");
            configuration.AddKeyword(KeywordType.Return, "say");

            configuration.AddOperator(OperatorType.Reference, "@");
            configuration.AddOperator(OperatorType.Dereference, ":");
            configuration.AddOperator(OperatorType.Comma, ",");
            configuration.AddOperator(OperatorType.Increment, "++");
            configuration.AddOperator(OperatorType.Decrement, "--");
            configuration.AddOperator(OperatorType.Negate, "!");
            configuration.AddOperator(OperatorType.Add, "+");
            configuration.AddOperator(OperatorType.Sub, "-");
            configuration.AddOperator(OperatorType.Mul, "*");
            configuration.AddOperator(OperatorType.Div, "/");
            configuration.AddOperator(OperatorType.Mod, "%");
            configuration.AddOperator(OperatorType.AddAssign, "+=");
            configuration.AddOperator(OperatorType.SubAssign, "-=");
            configuration.AddOperator(OperatorType.MulAssign, "*=");
            configuration.AddOperator(OperatorType.DivAssign, "/=");
            configuration.AddOperator(OperatorType.ModAssign, "%=");
            configuration.AddOperator(OperatorType.Assign, "=");
            configuration.AddOperator(OperatorType.BitwiseAnd, "&");
            configuration.AddOperator(OperatorType.AndAlso, "&&");
            configuration.AddOperator(OperatorType.BitwiseOr, "|");
            configuration.AddOperator(OperatorType.OrElse, "||");
            configuration.AddOperator(OperatorType.BraceOpen, "{");
            configuration.AddOperator(OperatorType.BraceClose, "}");
            configuration.AddOperator(OperatorType.ParentheseOpen, "(");
            configuration.AddOperator(OperatorType.ParentheseClose, ")");
            configuration.AddOperator(OperatorType.Equals, "==");
            configuration.AddOperator(OperatorType.NotEquals, "!=");
            configuration.AddOperator(OperatorType.Greater, ">");
            configuration.AddOperator(OperatorType.NotGreater, "<=");
            configuration.AddOperator(OperatorType.Less, "<");
            configuration.AddOperator(OperatorType.NotLess, ">=");
            configuration.AddOperator(OperatorType.Pow, "^");
            configuration.AddOperator(OperatorType.BracketOpen, "[");
            configuration.AddOperator(OperatorType.BracketClose, "]");
        }
    }
}