namespace Lahda.Lexer
{
    public enum TokenType
    {
        EOF = -2,
        Unknow = -1,
        Identifier = 0,
        Integer = 1,
        Floating = 2,
        Keyword = 3,
        Operator = 4,
        String = 5,
        SpecialCharacter = 6
    }
}