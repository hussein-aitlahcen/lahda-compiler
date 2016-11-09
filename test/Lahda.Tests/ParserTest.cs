using System;
using Lahda.Lexer;
using Lahda.Lexer.Impl;
using Lahda.Parser;
using Xunit;

namespace Lahda.Tests
{
    public sealed class ParserTest
    {
        [Theory]
        [InlineData("2+5")]
        [InlineData("3*(2+5)/2.e+8")]
        [InlineData("3*((2+5)/2.e+8)/.5")]
        public void Parser_should_parse_expression(string content)
        {
            var codeSource = CodeSource.FromMemory(content);
            var lexer = new LahdaLexer(codeSource);
            var parser = new LahdaParser(lexer);

            parser.ArithmeticExpression();
        }

        [Theory]
        [InlineData("2+")]
        [InlineData("3**(2+5)/2.e+8")]
        [InlineData("3*((2+5)/2.e+8/.5")]
        public void Parser_sould_not_parse_invalid_expression(string content)
        {
            var codeSource = CodeSource.FromMemory(content);
            var lexer = new LahdaLexer(codeSource);
            var parser = new LahdaParser(lexer);

            Assert.Throws(typeof(InvalidOperationException), parser.ArithmeticExpression);
        }

        [Theory]
        [InlineData("true || false")]
        [InlineData("false || false")]
        [InlineData("false || true")]
        [InlineData("true || true")]
        [InlineData("true && true")]
        [InlineData("true && false")]
        [InlineData("false && true")]
        [InlineData("false && false")]
        [InlineData("(1+2) == 3 && false")]
        [InlineData("(1+2) > 2 && (1/5 == 2)")]
        [InlineData("(1+2) >= 2 && true && (1/5 <= 2)")]
        [InlineData("2 + 5 != 7 && x > 2")]
        [InlineData("x == y && z <= x / 2")]
        public void Parser_should_parse_boolean_expression(string content)
        {
            var codeSource = CodeSource.FromMemory(content);
            var lexer = new LahdaLexer(codeSource);
            var parser = new LahdaParser(lexer);

            var expression = parser.ArithmeticExpression();
            Console.WriteLine(expression.ToString());
        }
    }
}