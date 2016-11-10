using System;
using Lahda.Lexer;
using Lahda.Lexer.Impl;
using Lahda.Parser;
using Xunit;

namespace Lahda.Tests
{
    public sealed class ParserTest
    {
        private LahdaParser GetParser(string content)
        {
            return new LahdaParser(new LahdaLexer(CodeSource.FromMemory(content)));
        }

        [Theory]
        [InlineData("2+5")]
        [InlineData("3*(2+5)/2.e+8")]
        [InlineData("3*((2+5)/2.e+8)/.5")]
        public void Parser_should_parse_expression(string content)
        {
            GetParser(content).ArithmeticExpression();
        }

        [Theory]
        [InlineData("2+")]
        [InlineData("3**(2+5)/2.e+8")]
        [InlineData("3*((2+5)/2.e+8/.5")]
        public void Parser_sould_not_parse_invalid_expression(string content)
        {
            Assert.Throws(typeof(InvalidOperationException), GetParser(content).ArithmeticExpression);
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
            var parser = GetParser(content);
            var node = parser.ArithmeticExpression();
            Console.WriteLine(node);
        }

        [Theory]
        [InlineData("y = 2;")]
        [InlineData("m_currentIndex = 8*y;")]
        [InlineData("i = x + y + z * 2;")]
        public void Parser_should_parse_assignations(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextExpression();
            Console.WriteLine(node);
        }

        [Theory]
        [InlineData("var m_index = 6;")]
        [InlineData("var i = j * 9;")]
        [InlineData("var z = 9 * i + 2;")]
        public void Parser_should_parse_declarations(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextExpression();
            Console.WriteLine(node);
        }

        [Theory]
        [InlineData("{ var x = 2; x = x + 2; var m_index = x * 8; }")]
        public void Parser_should_parse_statements_block(string content)
        {
            var parser = GetParser(content);
            var node = parser.StatementsBlock();
            Console.WriteLine(node);
        }
    }
}