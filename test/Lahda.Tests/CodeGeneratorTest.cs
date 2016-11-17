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
        [InlineData("{ var a = 1; var b = a + 2 + 3 || 2 % 5 < 2 % 4 + 2 - 1; { var c = 3; var d = 4; } var f = 6; { var e = 5; } }")]
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