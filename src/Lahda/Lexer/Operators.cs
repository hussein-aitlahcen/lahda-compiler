namespace Lahda.Lexer
{
    //^(\\+|-|\\*|\\/|%|\\(|\\)|\\^|==|!=|<|>|<=|>=|=)$
    public static class Operators 
    {
        public const string ADD = "+";
        public const string SUB = "-";
        public const string MUL = "*";
        public const string DIV = "/";
        public const string MOD = "%";
        public const string BRACKET_OPEN = "(";
        public const string BRACKET_CLOSE = ")";
        public const string POW = "^";
        public const string EQUALS = "==";
        public const string NOT_EQUALS = "!=";
        public const string GREATER = ">";
        public const string LESS = "<";
        public const string NOT_LESS = ">=";
        public const string NOT_GREATER = "<=";
        public const string ASSIGN = "=";
    }
}