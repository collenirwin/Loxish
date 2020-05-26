using Lang.Interpreter;
using System;
using System.IO;
using System.Linq;

namespace Lang
{
    class Program
    {
        static readonly Interpreter.Interpreter _interpreter = new Interpreter.Interpreter();

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
            var statements = parser.Parse();

            if (errorState.HasErrors)
            {
                ErrorReporter.ReportSyntaxErrors(errorState);
                return 2;
            }

            var resolver = new Resolver(_interpreter, errorState);
            resolver.Resolve(statements);

            if (errorState.HasErrors)
            {
                ErrorReporter.ReportSyntaxErrors(errorState);
                return 2;
            }

            _interpreter.Interpret(statements);
            return _interpreter.RuntimeExceptionThrown ? 3 : 1;
        }
    }
}
