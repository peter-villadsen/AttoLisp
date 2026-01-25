namespace AttoLisp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Parse command line switches and positional script files
            bool traceEval = false;
            bool traceParse = false;
            bool enterRepl = false;
            var scriptPaths = new List<string>();

            if (args != null)
            {
                foreach (var arg in args)
                {
                    var argLower = arg.ToLowerInvariant();
                    switch (argLower)
                    {
                        case "--trace":
                        case "-t":
                            traceEval = true;
                            break;
                        case "--traceparse":
                        case "-tp":
                            traceParse = true;
                            break;
                        case "--repl":
                        case "-r":
                            enterRepl = true;
                            break;
                        default:
                            // treat anything else as a script path
                            scriptPaths.Add(arg);
                            break;
                    }
                }
            }

            // If no scripts and no --repl flag, default to REPL mode
            if (scriptPaths.Count == 0 && !enterRepl)
            {
                enterRepl = true;
            }

            Console.WriteLine("AttoLisp Interpreter v0.2");
            if (traceEval)
            {
                Console.WriteLine("[trace] Evaluation tracing enabled");
            }
            if (traceParse)
            {
                Console.WriteLine("[trace] Parse tracing enabled");
            }

            var evaluator = new Evaluator(traceEval, traceParse);

            // Load stdlib if present in working directory
            var stdlibPath = Path.Combine(AppContext.BaseDirectory, "stdlib.al");
            if (File.Exists(stdlibPath))
            {
                try
                {
                    var exprs = ParseFile(stdlibPath);
                    foreach (var expr in exprs)
                    {
                        // Evaluate for side-effects (defines), do not print results
                        evaluator.Eval(expr);
                    }
                }
                catch (Exception ex)
                {
                    WriteFriendlyParseError("standard library", ex);
                }
            }

            if (scriptPaths.Count > 0)
            {
                foreach (var path in scriptPaths)
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

                        var exprs = ParseFile(path);

                        foreach (var expr in exprs)
                        {
                            if (traceParse)
                            {
                                // When parse tracing is enabled, pretty-print
                                // each top-level form from script files.
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine($"[parse {Path.GetFileName(path)}]");
                                Console.ResetColor();
                                evaluator.PrettyPrintForm(expr);
                            }

                            // Evaluate script forms for side-effects; don't echo like REPL
                            var _ = evaluator.Eval(expr);
                        }
                    }
                    catch (Exception ex)
                    {
                        var displayName = Path.GetFileName(path);
                        WriteFriendlyParseError(displayName, ex);
                    }
                }
                
                if (!enterRepl)
                {
                    // Scripts executed, no REPL requested, exit
                    return;
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

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Goodbye!");
                        break;
                    }

                    var tokenizer = new Tokenizer(input);
                    var tokens = tokenizer.Tokenize();

                    var parser = new Parser(tokens);
                    var expr = parser.Parse();

                    if (traceParse && expr is LispList list)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("[parse repl]");
                        Console.ResetColor();
                        evaluator.PrettyPrintForm(expr);
                    }

                    var result = evaluator.Eval(expr);

                    // Always show the result in the interactive REPL, even if it is nil
                    Console.WriteLine($"=> {result}");
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

        internal static List<LispValue> ParseFile(string path)
        {
            var source = File.ReadAllText(path);
            return ParseSource(source);
        }

        internal static List<LispValue> ParseSource(string source)
        {
            var tokenizer = new Tokenizer(source);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens);
            return parser.ParseAll();
        }

        private static void WriteFriendlyParseError(string sourceLabel, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            // Try to extract line/column information from the message, if present
            // Expected pattern from Parser: "Expected ')' at end of list (line X, col Y)"
            var msg = ex.Message ?? string.Empty;
            var lineInfo = "";
            var idx = msg.IndexOf("(line ", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                lineInfo = msg.Substring(idx).TrimEnd('.');
                msg = msg.Substring(0, idx).TrimEnd();
            }

            Console.WriteLine($"Syntax error while reading {sourceLabel}: {msg}.");
            if (!string.IsNullOrEmpty(lineInfo))
            {
                Console.WriteLine($"Location: {lineInfo}.");
                Console.WriteLine("Hint: The list that ends here most likely started earlier in the file; check for a missing ')' before this position.");
            }

            Console.ResetColor();
        }
    }
}

