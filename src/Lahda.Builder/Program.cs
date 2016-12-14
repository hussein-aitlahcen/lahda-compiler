using System;

using System.Linq;
using Lahda.Lexer.Impl;
using Lahda.Parser;
using Lahda.Codegen;
using Lahda;
using System.IO;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
                Console.WriteLine("wtf dude give some files: <output> <file.lah...>");
            var outname = args[0];
            var sources = new string[]{
                "stdlib/stdlib.lah"
            }.Concat(args.Skip(1));

            foreach (var file in args.Skip(1))
                Console.WriteLine($"Compiling {file}");
            var multiSource = new MultiCodeSource(sources.Select(x => CodeSource.FromFile(x)).ToArray());
            Console.WriteLine(multiSource.Content);
            var parser = new LahdaParser(new LahdaLexer(new CompilationConfiguration(multiSource)));
            var output = new StringBuilderOutput();
            var codeGen = new CodeGenerator(output, parser.Root());
            codeGen.Build();
            if (File.Exists(outname))
                File.Delete(outname);
            File.WriteAllText(outname, output.ToString());
        }
    }
}
