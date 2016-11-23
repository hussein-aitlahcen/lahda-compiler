using System;
using Lahda.Lexer;
using Lahda.Lexer.Impl;
using Xunit;

namespace Lahda.Tests
{
    public sealed class LexerTest
    {
        private ILexer BuildLexer(string content) =>
            new LahdaLexer(new CompilationConfiguration(CodeSource.FromMemory(content)));

        private IToken SingleToken(string content) =>
            BuildLexer(content).NextToken();

        private void AssertTokenType(TokenType type, IToken token) =>
            Assert.Equal(token.Type, type);

        private void AssertSingleTokenType(TokenType type, string content) =>
            AssertTokenType(type, SingleToken(content));

        private void AssertOperatorSupported(OperatorType type)
        {
            var lexer = BuildLexer("");
            var op = lexer.Configuration.GetOperator(type);
            Assert.NotEmpty(op);
            AssertSingleTokenType(TokenType.Operator, op);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\n")]
        public void Lexer_should_returns_EOF(string content) =>
            AssertSingleTokenType(TokenType.EOF, content);

        [Theory]
        [InlineData("hello_2")]
        [InlineData("m_x_var")]
        [InlineData("m22map")]
        [InlineData("value2")]
        [InlineData("p")]
        [InlineData("e")]
        [InlineData("a5_")]
        public void Lexer_should_parse_identifier(string identifier) =>
            AssertSingleTokenType(TokenType.Identifier, identifier);

        [Theory]
        [InlineData("if")]
        [InlineData("for")]
        [InlineData("while")]
        [InlineData("var")]
        [InlineData("float")]
        [InlineData("string")]
        public void Lexer_should_parse_keyword(string keyword) =>
            AssertSingleTokenType(TokenType.Keyword, keyword);

        [Theory]
        [InlineData("1")]
        [InlineData("700000000")]
        [InlineData("92")]
        [InlineData("6")]
        [InlineData("5632")]
        public void Lexer_should_parse_integer(string value) =>
            AssertSingleTokenType(TokenType.Floating, value);


        [Theory]
        [InlineData(".55")]
        [InlineData(".2e+4")]
        [InlineData(".0e-24")]
        [InlineData("0.2")]
        [InlineData("0.5e+5")]
        [InlineData("1e+9")]
        [InlineData("1.2e-56")]
        public void Lexer_should_parse_floating(string value) =>
            AssertSingleTokenType(TokenType.Floating, value);

        [Theory]
        [InlineData(OperatorType.AddAssign)]
        [InlineData(OperatorType.SubAssign)]
        [InlineData(OperatorType.DivAssign)]
        [InlineData(OperatorType.MulAssign)]
        [InlineData(OperatorType.ModAssign)]
        [InlineData(OperatorType.Add)]
        [InlineData(OperatorType.Sub)]
        [InlineData(OperatorType.Mul)]
        [InlineData(OperatorType.Div)]
        [InlineData(OperatorType.Mod)]
        [InlineData(OperatorType.ParentheseOpen)]
        [InlineData(OperatorType.ParentheseClose)]
        [InlineData(OperatorType.BraceClose)]
        [InlineData(OperatorType.BraceOpen)]
        [InlineData(OperatorType.BitwiseAnd)]
        [InlineData(OperatorType.BitwiseOr)]
        [InlineData(OperatorType.AndAlso)]
        [InlineData(OperatorType.OrElse)]
        [InlineData(OperatorType.Pow)]
        [InlineData(OperatorType.Equals)]
        [InlineData(OperatorType.NotEquals)]
        [InlineData(OperatorType.Less)]
        [InlineData(OperatorType.Greater)]
        [InlineData(OperatorType.NotGreater)]
        [InlineData(OperatorType.NotLess)]
        [InlineData(OperatorType.Assign)]
        [InlineData(OperatorType.Negate)]

        public void Lexer_should_parse_operator(OperatorType type) =>
            AssertOperatorSupported(type);

        [Theory]
        [InlineData("(2+5*2)", new[]
        {
            TokenType.Operator,
            TokenType.Floating,
            TokenType.Operator,
            TokenType.Floating,
            TokenType.Operator,
            TokenType.Floating,
            TokenType.Operator
        })]
        [InlineData(".5e+5*2/0.2e-5", new[]
        {
            TokenType.Floating,
            TokenType.Operator,
            TokenType.Floating,
            TokenType.Operator,
            TokenType.Floating
        })]
        [InlineData("var x = .2e+5", new[]
        {
            TokenType.Keyword,
            TokenType.Identifier,
            TokenType.Operator,
            TokenType.Floating
        })]
        [InlineData("var y = \"Hello World /**/!\"", new[]
        {
            TokenType.Keyword,
            TokenType.Identifier,
            TokenType.Operator,
            TokenType.String
        })]
        [InlineData("var empty = \"\"", new[]
        {
            TokenType.Keyword,
            TokenType.Identifier,
            TokenType.Operator,
            TokenType.String
        })]
        public void Lexer_should_parse_expression(string expression, TokenType[] expectedTypes)
        {
            var lexer = BuildLexer(expression);
            foreach (var expectedType in expectedTypes)
            {
                var token = lexer.NextToken();
                Assert.Equal(token.Type, expectedType);
            }
            Assert.Equal(lexer.NextToken().Type, TokenType.EOF);
        }
    }
}