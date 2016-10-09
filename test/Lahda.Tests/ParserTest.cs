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

            parser.Expression();
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

            Assert.Throws(typeof(InvalidOperationException), parser.Expression);
        }
    }
}