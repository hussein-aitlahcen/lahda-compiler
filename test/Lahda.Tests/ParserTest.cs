using System;
using Lahda.Lexer.Impl;
using Lahda.Parser;
using Xunit;

namespace Lahda.Tests
{
    public sealed class ParserTest
    {
        private LahdaParser GetParser(string content)
        {
            return new LahdaParser(new LahdaLexer(new CompilationConfiguration(CodeSource.FromMemory(content))));
        }

        [Theory]
        [InlineData("2+5")]
        [InlineData("3*(2+5)/2.e+8")]
        [InlineData("3*(2+5)%2.e+8")]
        [InlineData("3*((2+5)/2.e+8)/.5")]
        [InlineData("3*2%5/5")]
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
        [InlineData("true || false || !true")]
        [InlineData("false || false")]
        [InlineData("false || true")]
        [InlineData("true || true")]
        [InlineData("true && true")]
        [InlineData("true && false")]
        [InlineData("false && true")]
        [InlineData("false && false")]
        [InlineData("(1+2) == 3 && false")]
        [InlineData("(1+2) > 2 && (1/5 == 2) && !(5 * 2 > 5)")]
        [InlineData("(1+2) >= 2 && true && (1/5 <= 2)")]
        [InlineData("2 + 5 != 7 && 1 > 2")]
        public void Parser_should_parse_boolean_expression(string content)
        {
            var parser = GetParser(content);
            var node = parser.ArithmeticExpression();
        }

        [Theory]
        [InlineData("{ var y = 2; y = 5; }")]
        [InlineData("{ var m_currentIndex = 8; m_currentIndex = m_currentIndex + 8; }")]
        [InlineData("{ var i = 1; i = 0; }")]
        public void Parser_should_parse_assignations(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextStatement();
        }

        [Theory]
        [InlineData("var m_index = 6;")]
        [InlineData("var i = 9;")]
        [InlineData("var z = 9 * 2;")]
        public void Parser_should_parse_declarations(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextStatement();
        }

        [Theory]
        [InlineData("{ var x = 2; x = x + 2; var m_index = x * 8; }")]
        [InlineData("{ {} }")]
        public void Parser_should_parse_statements_block(string content)
        {
            var parser = GetParser(content);
            var node = parser.StatementsBlock();
        }

        [Theory]
        [InlineData("{ var x = 1; var m_index = 0; var z = 2; if(x > 2 && 3 && m_index <= z) { var yo = 2; yo = yo / 4; } else { z = 211111; } }")]
        [InlineData("{ var x = 1; if(x) x = 5; }")]
        [InlineData("{ var x = 1; if(x) while(5) { } else x = 1; }")]
        [InlineData("{ var x = 1; if(x) { } else x = 1; }")]
        [InlineData("{ var x = 1; if(x) x = 0; else { } }")]
        public void Parser_should_parse_conditional(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextStatement();
        }

        [Theory]
        [InlineData("{ for(var i = 0; i < 5; i = i + 1) print i; }")]
        [InlineData("{ for(var i = 0; i < 5; i = i + 1) { var x = 2; x = x + 2; } }")]
        [InlineData("{ var y = 2; var x = 1; while(y < 5) { x = x * 5; y = y + 1; } }")]
        [InlineData("{ do { var x = 1 + 2; } while(3); }")]
        [InlineData("{ do { var yo = 2; } until(5 == 5); }")]
        [InlineData("{ do { var mdr = 2 / 2 % 5; } forever; }")]
        [InlineData("{ var i = 0; do i = i + 1; forever; }")]
        [InlineData("{ var i = 0; do i = i + 1; while(i < 5); }")]
        [InlineData("{ var i = 0; var a = 5; while(i) a = 2; }")]
        public void Parser_should_parse_loops(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextStatement();
        }

        [Theory]
        [InlineData("for(var i = 0; i < 5; i += 1) break;")]
        [InlineData("{ var x = 1; do { if(x) break; else x += 1; } forever; }")]
        [InlineData("{ var x = 1; do { if(x) break; else x += 1; } forever; }")]
        public void Parser_should_parse_loop_controls(string content)
        {
            var parser = GetParser(content);
            var node = parser.NextStatement();
        }

        [Theory]
        [InlineData("{ var x = 5; x = y + 2; }")]
        [InlineData("{ x = 5; }")]
        [InlineData("{ var x = 5; y = x * 5; }")]
        [InlineData("{ var x = 5; { x = 8; var y = 2; y = y + 2; } y = 8; }")]
        public void Parser_should_fire_unknow_symbol(string content)
        {
            var parser = GetParser(content);
            Assert.Throws(typeof(InvalidOperationException), parser.StatementsBlock);
        }

        [Theory]
        [InlineData("float a(float x) say x + 1; float b() say a(a(a(5))) + 1;")]
        public void Parser_should_parse_function(string content)
        {
            var parser = GetParser(content);
            parser.FunctionExpression();
            parser.FunctionExpression();
        }

        [Theory]
        [InlineData("float start() { var array[10]; }")]
        [InlineData("float start() { var array[15]; array[10]++; }")]
        public void Parser_should_parse_arrays(string content)
        {
            var parser = GetParser(content);
            parser.FunctionExpression();
        }
    }
}