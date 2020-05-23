using Lang.Interpreter;
using System;
using System.IO;
using System.Linq;

namespace Lang
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Too many arguments passed.");
                return 1;
            }

            if (args.Any())
            {
                return RunFile(args.First());
            }

            RunREPL();
            return 0;
        }

        static int RunFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at '{path}'.");
            }

            return Run(File.ReadAllText(path));
        }

        static void RunREPL()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
            }
        }

        static int Run(string source)
        {
            var errorState = new ErrorState();
            var lexer = new Lexer(source, errorState);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens, errorState);
            var expression = parser.Parse();

            if (errorState.HasErrors)
            {
                ErrorReporter.ReportSyntaxErrors(errorState);
                return 2;
            }

            Console.WriteLine(new AstPrinter().Print(expression));
            return 0;
        }
    }
}
