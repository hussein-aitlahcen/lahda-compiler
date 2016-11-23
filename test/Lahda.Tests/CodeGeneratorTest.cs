using System;
using Lahda.Lexer.Impl;
using Lahda.Parser;
using Lahda.Codegen;
using Xunit;
using System.IO;

namespace Lahda.Tests
{
    public sealed class CodeGeneratorTest
    {
        [Theory]
        [InlineData("for(var i = 0; i < -(-(-(-10))); i += 1) print i;", 1)]
        [InlineData("{ var i = 0; while(!(i > 10)) i += 5; }", 2)]
        public void CodeGenerator_should_generate_godlike_code(string content, int i)
        {
            var codeSource = CodeSource.FromMemory(content);
            var parser = new LahdaParser(new LahdaLexer(new CompilationConfiguration(codeSource)));
            var output = new StringBuilderOutput();
            var codeGen = new CodeGenerator(output, parser.NextStatement());
            codeGen.Build();
            Console.WriteLine(output.ToString());
            var path = $"test_{i}.s";
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, output.ToString());
        }
    }
}