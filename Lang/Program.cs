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
            var lexer = new Lexer(source, new ErrorState());
            var tokens = lexer.Tokenize();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            if (lexer.ErrorState.HasErrors)
            {
                foreach (var error in lexer.ErrorState.Errors)
                {
                    WriteError(error.Line, "", error.Message);
                }

                return 2;
            }

            return 0;
        }

        static void WriteError(int line, string where, string message)
        {
            Console.WriteLine($"[Line {line}] Error: {message}");
        }
    }
}
