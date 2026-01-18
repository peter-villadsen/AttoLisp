namespace AttoLisp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AttoLisp Interpreter v0.2");
            var evaluator = new Evaluator();

            // Load stdlib if present in working directory
            var stdlibPath = Path.Combine(AppContext.BaseDirectory, "stdlib.al");
            if (File.Exists(stdlibPath))
            {
                try
                {
                    var source = File.ReadAllText(stdlibPath);
                    var tokenizer = new Tokenizer(source);
                    var tokens = tokenizer.Tokenize();
                    var parser = new Parser(tokens);
                    var exprs = parser.ParseAll();
                    foreach (var expr in exprs)
                    {
                        evaluator.Eval(expr);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to load stdlib: {ex.Message}");
                    Console.ResetColor();
                }
            }

            if (args != null && args.Length > 0)
            {
                foreach (var path in args)
                {
                    try
                    {
                        if (!File.Exists(path))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Warning: File not found: {path}");
                            Console.ResetColor();
                            continue;
                        }

                        var source = File.ReadAllText(path);
                        var tokenizer = new Tokenizer(source);
                        var tokens = tokenizer.Tokenize();
                        var parser = new Parser(tokens);
                        var exprs = parser.ParseAll();

                        foreach (var expr in exprs)
                        {
                            var result = evaluator.Eval(expr);
                            if (result is not LispNil)
                            {
                                Console.WriteLine($"=> {result}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error in {path}: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Entering REPL...");
            }

            Console.WriteLine("Type 'exit' or 'quit' to exit");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    Console.Write("lisp> ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    input = input.Trim();

                    if (input.ToLower() == "exit" || input.ToLower() == "quit")
                    {
                        Console.WriteLine("Goodbye!");
                        break;
                    }

                    var tokenizer = new Tokenizer(input);
                    var tokens = tokenizer.Tokenize();

                    var parser = new Parser(tokens);
                    var expr = parser.Parse();

                    var result = evaluator.Eval(expr);

                    if (result is not LispNil)
                    {
                        Console.WriteLine($"=> {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }
        }
    }
}
