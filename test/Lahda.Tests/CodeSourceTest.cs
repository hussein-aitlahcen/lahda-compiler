using System;
using Lahda.Lexer;
using Lahda.Lexer.Impl;
using Xunit;

namespace Lahda.Tests
{
    public sealed class CodeSourceTest 
    {
        [Theory]
        [InlineData("Hello World !")]
        [InlineData("Bitch, i'm fabulous")]
        [InlineData("No dude allowed")]
        public void CodeSource_should_be_ok_lol(string content)
        {
            var codeSource = CodeSource.FromMemory(content);
            Assert.Equal(content, codeSource.Content);
        }
    }
}