using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lahda.Lexer
{
    public abstract class AbstractLexer : ILexer
    {
        public const char StringDelimiter = '"';
        public const char EscapeChar = '\\';
        public const char NewLine = '\n';
        public static char[] EscapableCharacters = { '\r', '\t', '\f', ' ' };

        public CompilationConfiguration Configuration { get; }

        private int m_position;
        private int m_line;
        private int m_column;
        private IToken m_currentToken;

        public AbstractLexer(CompilationConfiguration configuration)
        {
            Configuration = configuration;
        }

        private void InitializeIfNecessary()
        {
            if (m_currentToken == null)
                m_currentToken = ParseAndDetermineNextToken();
        }

        public IToken NextToken()
        {
            InitializeIfNecessary();
            var current = m_currentToken;
            m_currentToken = ParseAndDetermineNextToken();
            return current;
        }

        public IToken PeekToken()
        {
            InitializeIfNecessary();
            return m_currentToken;
        }

        private char CurrentCharacter => Configuration.CodeSource.Content[m_position];

        private bool Escapable(char character) => EscapableCharacters.Contains(character);

        private bool IsEOF() => IsEOF(m_position);

        private bool IsEOF(int position) => position >= Configuration.CodeSource.Content.Length;

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
            return new TokenPosition(Configuration.CodeSource, m_line, m_column);
        }

        private IToken DetermineToken(string tokenContent)
        {
            var position = BuildTokenPosition();

            var keywords = Configuration.BuildKeywordRegex();
            var operators = Configuration.BuildOperatorRegex();
            var identifiers = Configuration.BuildIdentifierRegex();
            var integer = Configuration.BuildIntegerRegex();
            var floating = Configuration.BuildFloatingRegex();

            if (keywords.IsMatch(tokenContent))
            {
                return new ValueToken<string>(TokenType.Keyword, position, tokenContent);
            }
            else if (operators.IsMatch(tokenContent))
            {
                return new ValueToken<string>(TokenType.Operator, position, tokenContent);
            }
            else if (integer.IsMatch(tokenContent))
            {
                return new ValueToken<float>(TokenType.Floating, position, float.Parse(tokenContent, CultureInfo.InvariantCulture));
            }
            else if (floating.IsMatch(tokenContent))
            {
                return new ValueToken<float>(TokenType.Floating, position, float.Parse(tokenContent, CultureInfo.InvariantCulture));
            }
            else if (identifiers.IsMatch(tokenContent))
            {
                return new ValueToken<string>(TokenType.Identifier, position, tokenContent);
            }
            else if (tokenContent.First() == StringDelimiter && tokenContent.Last() == StringDelimiter)
            {
                return new ValueToken<string>(TokenType.String, position, tokenContent.Substring(1, tokenContent.Length - 2));
            }
            else if (tokenContent == Configuration.EndOfStatement())
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