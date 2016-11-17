using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Lahda.Lexer.Impl
{
    public sealed class LahdaLexer : ILexer
    {
        private static Regex RegexIdentifier = new Regex("^([A-Za-z][A-Za-z0-9_]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RegexInteger = new Regex("^([0-9]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RegexFloating = new Regex("^(((([0-9]+\\.[0-9]*)|([0-9]*\\.[0-9]+))([Ee][+-]?[0-9]+)?)|([0-9]+([Ee][+-]?[0-9]+)))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RegexKeyword = new Regex("^(print|if|else|for|while|do|until|forever|break|reset|continue|poor|var|int|float|string|true|false)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RegexOperator = new Regex("^(\\+|-|\\*|\\/|%|\\(|\\)|\\^|==|!=|<|>|<=|>=|=|\\|\\||\\||&&|&|{|})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const char StringDelimiter = '"';
        private const char EscapeChar = '\\';
        private const char NewLine = '\n';
        private static char[] EscapableCharacters = { '\r', '\t', '\f', ' ' };

        public ICodeSource CodeSource { get; }

        private int m_position;
        private int m_line;
        private int m_column;
        private IToken m_currentToken;

        public LahdaLexer(ICodeSource codeSource)
        {
            CodeSource = codeSource;

            m_currentToken = ParseAndDetermineNextToken();
        }

        public IToken NextToken()
        {
            var current = m_currentToken;
            m_currentToken = ParseAndDetermineNextToken();
            return current;
        }

        public IToken PeekToken()
        {
            return m_currentToken;
        }

        private char CurrentCharacter => CodeSource.Content[m_position];

        private bool Escapable(char character) => EscapableCharacters.Contains(character);

        private bool IsEOF() => IsEOF(m_position);

        private bool IsEOF(int position) => position >= CodeSource.Content.Length;

        private void EscapeToNextToken()
        {
            // escape spaces etc..
            while (!IsEOF() && Escapable(CurrentCharacter))
            {
                m_position++;
            }
        }

        private string ParseNextTokenContent()
        {
            var builder = new StringBuilder();
            while (!IsEOF() && !Escapable(CurrentCharacter))
            {
                switch (CurrentCharacter)
                {
                    case EscapeChar:
                        m_position++;
                        builder.Append(CurrentCharacter);
                        break;
                    case NewLine:
                        m_line++;
                        m_column = 0;
                        break;
                    case StringDelimiter:
                        builder.Append(CurrentCharacter);
                        m_position++;
                        var stringEnd = false;
                        while (!IsEOF() && !stringEnd)
                        {
                            switch (CurrentCharacter)
                            {
                                case EscapeChar:
                                    m_position++;
                                    builder.Append(CurrentCharacter);
                                    break;
                                case StringDelimiter:
                                    builder.Append(CurrentCharacter);
                                    stringEnd = true;
                                    m_position--;
                                    break;
                                default:
                                    builder.Append(CurrentCharacter);
                                    break;
                            }
                            m_position++;
                        }
                        break;
                    default:
                        builder.Append(CurrentCharacter);
                        break;
                }
                m_position++;
                m_column++;
            }
            return builder.ToString();
        }

        private TokenPosition BuildTokenPosition()
        {
            return new TokenPosition(CodeSource, m_line, m_column);
        }

        private IToken DetermineToken(string tokenContent)
        {
            var position = BuildTokenPosition();
            if (RegexKeyword.IsMatch(tokenContent))
            {
                return new ValueToken<string>(TokenType.Keyword, position, tokenContent);
            }
            else if (RegexOperator.IsMatch(tokenContent))
            {
                return new ValueToken<string>(TokenType.Operator, position, tokenContent);
            }
            else if (RegexInteger.IsMatch(tokenContent))
            {
                return new ValueToken<float>(TokenType.Floating, position, float.Parse(tokenContent, CultureInfo.InvariantCulture));
            }
            else if (RegexFloating.IsMatch(tokenContent))
            {
                return new ValueToken<float>(TokenType.Floating, position, float.Parse(tokenContent, CultureInfo.InvariantCulture));
            }
            else if (RegexIdentifier.IsMatch(tokenContent))
            {
                return new ValueToken<string>(TokenType.Identifier, position, tokenContent);
            }
            else if (tokenContent.First() == StringDelimiter && tokenContent.Last() == StringDelimiter)
            {
                return new ValueToken<string>(TokenType.String, position, tokenContent.Substring(1, tokenContent.Length - 2));
            }
            else if (tokenContent == ";")
            {
                return new ValueToken<string>(TokenType.SpecialCharacter, position, tokenContent);
            }
            return new Token(TokenType.Unknow, position);
        }

        private IToken ParseAndDetermineNextToken()
        {
            EscapeToNextToken();

            if (IsEOF())
            {
                return new Token(TokenType.EOF, BuildTokenPosition());
            }

            var tokenContent = ParseNextTokenContent();
            var tokenSize = tokenContent.Length;
            IToken currentToken = new Token(TokenType.Unknow, BuildTokenPosition());
            while (tokenSize > 0 && currentToken.Type == TokenType.Unknow)
            {
                currentToken = DetermineToken(tokenContent.Substring(0, tokenSize));
                tokenSize--;
            }

            m_position -= tokenContent.Length - tokenSize - 1;

            if (currentToken.Type == TokenType.Unknow)
                return ParseAndDetermineNextToken();

            return currentToken;
        }
    }
}