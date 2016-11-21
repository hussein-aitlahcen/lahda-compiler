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
        [InlineData("{ var x = 1; var s = 1; while(s < 5) { s = s + 1; x = x * s; } print x; }")]
        public void CodeGenerator_should_generate_godlike_code(string content)
        {
            var codeSource = CodeSource.FromMemory(content);
            var parser = new LahdaParser(new LahdaLexer(codeSource));
            var output = new StringBuilderOutput();
            var codeGen = new CodeGenerator(output, parser.NextStatement());
            codeGen.Build();
            Console.WriteLine(output.ToString());
        }
    }
}