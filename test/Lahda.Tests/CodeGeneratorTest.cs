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
        [InlineData("float fibo(float n) if(n <= 2) say 1; else say fibo(n - 1) + fibo(n - 2); float start() for(var i = 0; i < 10; i++) print fibo(i + 1);", 1)]
        [InlineData("float start() { var array[10]; for(var i = 0; i < 10; i++) array[i] = i * i; for(var i = 0; i < 10; i++) print array[i]; }", 2)]
        public void CodeGenerator_should_generate_godlike_code(string content, int i)
        {
            var codeSource = CodeSource.FromMemory(content);
            var parser = new LahdaParser(new LahdaLexer(new CompilationConfiguration(codeSource)));
            var output = new StringBuilderOutput();
            var codeGen = new CodeGenerator(output, parser.Root());
            codeGen.Build();
            Console.WriteLine(output.ToString());
            var path = $"test_{i}.s";
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, output.ToString());
        }
    }
}