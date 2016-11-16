using System;
using Lahda.Lexer.Impl;
using Lahda.Parser;
using Lahda.Codegen;
using Xunit;

namespace Lahda.Tests
{
    public sealed class CodeGeneratorTest
    {
        [Theory]
        [InlineData("{ var i = 5; var s = 1; for(var x = 1; x <= i; x = x + 1) { s = s * x; } }")]
        [InlineData("{ var x = 50; var y = 0; if(y <= 0) { y = 1; } while(y < x) { y = y * (y + 1); } }")]
        public void CodeGenerator_should_generate_godlike_code(string content)
        {
            var codeSource = CodeSource.FromMemory(content);
            var parser = new LahdaParser(new LahdaLexer(codeSource));
            var codeGen = new CodeGenerator(parser.NextExpression());
            var built = codeGen.Build();
            Console.WriteLine(built);
        }
    }
}