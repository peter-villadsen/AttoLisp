namespace AttoLisp
{
    public class Environment
    {
        private readonly Dictionary<string, LispValue> _bindings;
        private readonly Environment? _parent;

        public Environment(Environment? parent = null)
        {
            _bindings = new Dictionary<string, LispValue>();
            _parent = parent;
        }

        public void Define(string name, LispValue value)
        {
            _bindings[name] = value;
        }

        public LispValue? Lookup(string name)
        {
            if (_bindings.TryGetValue(name, out var value))
                return value;

            return _parent?.Lookup(name);
        }

        public void Set(string name, LispValue value)
        {
            if (_bindings.ContainsKey(name))
            {
                _bindings[name] = value;
                return;
            }

            if (_parent != null)
            {
                _parent.Set(name, value);
                return;
            }

            throw new Exception($"Undefined variable: {name}");
        }
    }

    public class Evaluator
    {
        private readonly Environment _globalEnv;
        private readonly bool _trace;
        private readonly bool _traceParse;
        private int _traceDepth;

        public Evaluator(bool trace = false, bool traceParse = false)
        {
            _globalEnv = new Environment();
            _trace = trace;
            _traceParse = traceParse;
            _traceDepth = 0;
            InitializeGlobalEnvironment();
        }

        private void Trace(string message)
        {
            if (_trace)
            {
                var indent = new string(' ', _traceDepth * 2);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"{indent}{message}");
                Console.ResetColor();
            }
        }

        public void PrettyPrintForm(LispValue value, int indent = 0)
        {
            var indentStr = new string(' ', indent * 2);

            switch (value)
            {
                case LispList list:
                    if (list.Elements.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(indentStr + "()");
                        Console.ResetColor();
                        return;
                    }

                    // Simple list: all atoms, print on one line
                    if (list.Elements.All(e => e is not LispList))
                    {
                        var parts = list.Elements.Select(e => e.ToString());
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(indentStr + "(" + string.Join(" ", parts) + ")");
                        Console.ResetColor();
                        return;
                    }

                    // Complex list: head + nested lists
                    if (list.Elements[0] is LispSymbol sym)
                    {
                        // Print head and any immediate atomic args on first line
                        var headLine = indentStr + "(" + sym.Name;
                        int i = 1;
                        for (; i < list.Elements.Count && list.Elements[i] is not LispList; i++)
                        {
                            headLine += " " + list.Elements[i];
                        }

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(headLine); // no closing paren yet
                        Console.ResetColor();

                        // Print remaining elements
                        for (; i < list.Elements.Count; i++)
                        {
                            PrettyPrintForm(list.Elements[i], indent + 1);
                        }

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(indentStr + ")");
                        Console.ResetColor();
                    }
                    else
                    {
                        // Generic list
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(indentStr + "(");
                        Console.ResetColor();

                        foreach (var el in list.Elements)
                        {
                            PrettyPrintForm(el, indent + 1);
                        }

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(indentStr + ")");
                        Console.ResetColor();
                    }
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(indentStr + value);
                    Console.ResetColor();
                    break;
            }
        }

        private void InitializeGlobalEnvironment()
        {
            // Boolean literals
            _globalEnv.Define("t", LispBoolean.True);
            _globalEnv.Define("nil", LispBoolean.False);

            // Arithmetic operations supporting integers (BigInteger) and decimals
            _globalEnv.Define("+", new LispFunction("+", args =>
            {
                bool anyDecimal = args.Any(a => a is LispDecimal);
                if (!args.Any()) return anyDecimal ? new LispDecimal(0m) : new LispInteger(System.Numerics.BigInteger.Zero);
                if (anyDecimal)
                {
                    decimal sum = 0m;
                    foreach (var arg in args)
                    {
                        sum += ToDecimal(arg);
                    }
                    return new LispDecimal(sum);
                }
                else
                {
                    System.Numerics.BigInteger sum = System.Numerics.BigInteger.Zero;
                    foreach (var arg in args)
                    {
                        sum += ToBigInt(arg);
                    }
                    return new LispInteger(sum);
                }
            }));

            _globalEnv.Define("-", new LispFunction("-", args =>
            {
                if (args.Count == 0)
                    throw new Exception("- requires at least one argument");

                bool anyDecimal = args.Any(a => a is LispDecimal);
                if (args.Count == 1)
                {
                    return anyDecimal ? new LispDecimal(-ToDecimal(args[0])) : new LispInteger(-ToBigInt(args[0]));
                }

                if (anyDecimal)
                {
                    decimal result = ToDecimal(args[0]);
                    for (int i = 1; i < args.Count; i++) result -= ToDecimal(args[i]);
                    return new LispDecimal(result);
                }
                else
                {
                    var result = ToBigInt(args[0]);
                    for (int i = 1; i < args.Count; i++) result -= ToBigInt(args[i]);
                    return new LispInteger(result);
                }
            }));

            _globalEnv.Define("*", new LispFunction("*", args =>
            {
                bool anyDecimal = args.Any(a => a is LispDecimal);
                if (anyDecimal)
                {
                    decimal product = 1m;
                    foreach (var arg in args) product *= ToDecimal(arg);
                    return new LispDecimal(product);
                }
                else
                {
                    System.Numerics.BigInteger product = System.Numerics.BigInteger.One;
                    if (!args.Any()) return new LispInteger(product);
                    foreach (var arg in args) product *= ToBigInt(arg);
                    return new LispInteger(product);
                }
            }));

            _globalEnv.Define("/", new LispFunction("/", args =>
            {
                if (args.Count == 0)
                    throw new Exception("/ requires at least one argument");

                decimal result = ToDecimal(args[0]);
                if (args.Count == 1)
                {
                    if (result == 0m) throw new Exception("Division by zero");
                    return new LispDecimal(1m / result);
                }
                for (int i = 1; i < args.Count; i++)
                {
                    var d = ToDecimal(args[i]);
                    if (d == 0m) throw new Exception("Division by zero");
                    result /= d;
                }
                return new LispDecimal(result);
            }));

            // Comparison operations
            _globalEnv.Define("=", new LispFunction("=", args =>
            {
                if (args.Count < 2)
                    throw new Exception("= requires at least two arguments");

                for (int i = 1; i < args.Count; i++)
                {
                    if (!AreEqual(args[0], args[i]))
                        return LispBoolean.False;
                }
                return LispBoolean.True;
            }));

            _globalEnv.Define("<", new LispFunction("<", args =>
            {
                if (args.Count < 2)
                    throw new Exception("< requires at least two arguments");

                for (int i = 1; i < args.Count; i++)
                {
                    if (!IsLessThan(args[i - 1], args[i]))
                        return LispBoolean.False;
                }
                return LispBoolean.True;
            }));

            _globalEnv.Define(">", new LispFunction(">", args =>
            {
                if (args.Count < 2)
                    throw new Exception("> requires at least two arguments");

                for (int i = 1; i < args.Count; i++)
                {
                    if (!IsLessThan(args[i], args[i - 1]))
                        return LispBoolean.False;
                }
                return LispBoolean.True;
            }));

            _globalEnv.Define("<=", new LispFunction("<=", args =>
            {
                if (args.Count < 2)
                    throw new Exception("<= requires at least two arguments");

                for (int i = 1; i < args.Count; i++)
                {
                    var a = args[i - 1];
                    var b = args[i];
                    // if a > b then a <= b is false
                    if (IsLessThan(b, a) && !AreEqual(a, b))
                        return LispBoolean.False;
                }
                return LispBoolean.True;
            }));

            _globalEnv.Define(">=", new LispFunction(">=", args =>
            {
                if (args.Count < 2)
                    throw new Exception(">= requires at least two arguments");

                for (int i = 1; i < args.Count; i++)
                {
                    var a = args[i - 1];
                    var b = args[i];
                    // if a < b then a >= b is false
                    if (IsLessThan(a, b) && !AreEqual(a, b))
                        return LispBoolean.False;
                }
                return LispBoolean.True;
            }));

            // String operations
            _globalEnv.Define("concat", new LispFunction("concat", args =>
            {
                var result = string.Concat(args.Select(a =>
                {
                    if (a is LispString str)
                        return str.Value;
                    return a.ToString();
                }));
                return new LispString(result);
            }));

            _globalEnv.Define("str-length", new LispFunction("str-length", args =>
            {
                if (args.Count != 1 || args[0] is not LispString str)
                    throw new Exception("str-length expects a single string argument");
                return new LispInteger(new System.Numerics.BigInteger(str.Value.Length));
            }));

            // Additional string operations
            _globalEnv.Define("substr", new LispFunction("substr", args =>
            {
                if (args.Count != 3 || args[0] is not LispString s)
                    throw new Exception("substr expects (string start length)");
                var i = ToInt32(args[1]);
                var len = ToInt32(args[2]);
                if (i < 0 || len < 0 || i > s.Value.Length) return new LispString("");
                var maxLen = Math.Min(len, s.Value.Length - i);
                return new LispString(s.Value.Substring(i, maxLen));
            }));

            _globalEnv.Define("index-of", new LispFunction("index-of", args =>
            {
                if (args.Count != 2 || args[0] is not LispString s || args[1] is not LispString n)
                    throw new Exception("index-of expects (string needle)");
                var idx = s.Value.IndexOf(n.Value, StringComparison.Ordinal);
                return new LispInteger(new System.Numerics.BigInteger(idx));
            }));

            _globalEnv.Define("to-lower", new LispFunction("to-lower", args =>
            {
                if (args.Count != 1 || args[0] is not LispString s)
                    throw new Exception("to-lower expects a single string argument");
                return new LispString(s.Value.ToLowerInvariant());
            }));


            // Date operations
            _globalEnv.Define("now", new LispFunction("now", args =>
            {
                if (args.Count != 0)
                    throw new Exception("now expects no arguments");
                return new LispDate(DateTime.Now);
            }));

            _globalEnv.Define("date-year", new LispFunction("date-year", args =>
            {
                if (args.Count != 1 || args[0] is not LispDate date)
                    throw new Exception("date-year expects a single date argument");
                return new LispInteger(new System.Numerics.BigInteger(date.Value.Year));
            }));

            _globalEnv.Define("date-month", new LispFunction("date-month", args =>
            {
                if (args.Count != 1 || args[0] is not LispDate date)
                    throw new Exception("date-month expects a single date argument");
                return new LispInteger(new System.Numerics.BigInteger(date.Value.Month));
            }));

            _globalEnv.Define("date-day", new LispFunction("date-day", args =>
            {
                if (args.Count != 1 || args[0] is not LispDate date)
                    throw new Exception("date-day expects a single date argument");
                return new LispInteger(new System.Numerics.BigInteger(date.Value.Day));
            }));

            // List operations
            _globalEnv.Define("list", new LispFunction("list", args =>
            {
                return new LispList(new List<LispValue>(args));
            }));

            _globalEnv.Define("car", new LispFunction("car", args =>
            {
                if (args.Count != 1 || args[0] is not LispList list)
                    throw new Exception("car expects a single list argument");
                if (list.Elements.Count == 0)
                    return LispNil.Instance;
                return list.Elements[0];
            }));

            _globalEnv.Define("cdr", new LispFunction("cdr", args =>
            {
                if (args.Count != 1 || args[0] is not LispList list)
                    throw new Exception("cdr expects a single list argument");
                if (list.Elements.Count == 0)
                    return LispNil.Instance;
                return new LispList(list.Elements.Skip(1).ToList());
            }));

            _globalEnv.Define("cons", new LispFunction("cons", args =>
            {
                if (args.Count != 2)
                    throw new Exception("cons expects exactly two arguments");
                
                if (args[1] is LispList list)
                {
                    var newElements = new List<LispValue> { args[0] };
                    newElements.AddRange(list.Elements);
                    return new LispList(newElements);
                }
                return new LispList(new List<LispValue> { args[0], args[1] });
            }));

            // Empty? predicate
            _globalEnv.Define("empty?", new LispFunction("empty?", args =>
            {
                if (args.Count != 1)
                    throw new Exception("empty? expects exactly one argument");
                var x = args[0];
                if (x is LispNil)
                    return LispBoolean.True;
                if (x is LispList lst)
                    return lst.Elements.Count == 0 ? LispBoolean.True : LispBoolean.False;
                return LispBoolean.False;
            }));

            // Type predicate: list?
            _globalEnv.Define("list?", new LispFunction("list?", args =>
            {
                if (args.Count != 1)
                    throw new Exception("list? expects exactly one argument");
                return args[0] is LispList ? LispBoolean.True : LispBoolean.False;
            }));

            // Type predicate: number?
            _globalEnv.Define("number?", new LispFunction("number?", args =>
            {
                if (args.Count != 1)
                    throw new Exception("number? expects exactly one argument");
                return args[0] is LispInteger or LispDecimal ? LispBoolean.True : LispBoolean.False;
            }));

            // Special forms and utility
            _globalEnv.Define("print", new LispFunction("print", args =>
            {
                foreach (var arg in args)
                {
                    Console.Write(arg.ToString());
                }
                Console.WriteLine();
                return LispNil.Instance;
            }));

            // Logical operations
            _globalEnv.Define("not", new LispFunction("not", args =>
            {
                if (args.Count != 1) throw new Exception("not expects exactly one argument");
                return IsTruthy(args[0]) ? LispBoolean.False : LispBoolean.True;
            }));
            
            _globalEnv.Define("xor", new LispFunction("xor", args =>
            {
                if (args.Count != 2) throw new Exception("xor expects exactly two arguments");
                return (IsTruthy(args[0]) ^ IsTruthy(args[1])) ? LispBoolean.True : LispBoolean.False;
            }));

            // List access function
            _globalEnv.Define("nth", new LispFunction("nth", args =>
            {
                if (args.Count != 2)
                    throw new Exception("nth expects exactly two arguments");
                if (args[0] is not LispList list)
                    throw new Exception("nth expects a list as the first argument");
                var index = ToInt32(args[1]);
                if (index < 0 || index >= list.Elements.Count)
                    return LispNil.Instance;
                return list.Elements[index];
            }));

            // Convert string to number
            _globalEnv.Define("number", new LispFunction("number", args =>
            {
                if (args.Count != 1)
                    throw new Exception("number expects exactly one argument");
                if (args[0] is LispString str)
                {
                    if (str.Value.Contains('.'))
                    {
                        if (decimal.TryParse(str.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                            return new LispDecimal(d);
                    }
                    else
                    {
                        if (System.Numerics.BigInteger.TryParse(str.Value, System.Globalization.CultureInfo.InvariantCulture, out var bi))
                            return new LispInteger(bi);
                    }
                    throw new Exception($"Cannot convert '{str.Value}' to number");
                }
                if (args[0] is LispInteger || args[0] is LispDecimal)
                    return args[0];
                throw new Exception("number expects a string or number argument");
            }));

            // Create symbol from string
            _globalEnv.Define("symbol", new LispFunction("symbol", args =>
            {
                if (args.Count != 1)
                    throw new Exception("symbol expects exactly one argument");
                if (args[0] is LispString str)
                    return new LispSymbol(str.Value);
                if (args[0] is LispSymbol sym)
                    return sym;
                throw new Exception("symbol expects a string argument");
            }));

            // Trigonometric and math functions
            _globalEnv.Define("sin", new LispFunction("sin", args =>
            {
                if (args.Count != 1) throw new Exception("sin expects exactly one argument");
                var x = (double)ToDecimal(args[0]);
                return new LispDecimal((decimal)Math.Sin(x));
            }));

            _globalEnv.Define("cos", new LispFunction("cos", args =>
            {
                if (args.Count != 1) throw new Exception("cos expects exactly one argument");
                var x = (double)ToDecimal(args[0]);
                return new LispDecimal((decimal)Math.Cos(x));
            }));

            _globalEnv.Define("tan", new LispFunction("tan", args =>
            {
                if (args.Count != 1) throw new Exception("tan expects exactly one argument");
                var x = (double)ToDecimal(args[0]);
                return new LispDecimal((decimal)Math.Tan(x));
            }));

            _globalEnv.Define("sqrt", new LispFunction("sqrt", args =>
            {
                if (args.Count != 1) throw new Exception("sqrt expects exactly one argument");
                var x = ToDecimal(args[0]);
                if (x < 0m) throw new Exception("sqrt domain error: negative input");
                return new LispDecimal((decimal)Math.Sqrt((double)x));
            }));

            _globalEnv.Define("exp", new LispFunction("exp", args =>
            {
                if (args.Count != 1) throw new Exception("exp expects exactly one argument");
                var x = (double)ToDecimal(args[0]);
                return new LispDecimal((decimal)Math.Exp(x));
            }));

            _globalEnv.Define("log", new LispFunction("log", args =>
            {
                if (args.Count == 1)
                {
                    var x = ToDecimal(args[0]);
                    if (x <= 0m) throw new Exception("log domain error: non-positive input");
                    return new LispDecimal((decimal)Math.Log((double)x));
                }
                if (args.Count == 2)
                {
                    var x = ToDecimal(args[0]);
                    var @base = ToDecimal(args[1]);
                    if (x <= 0m || @base <= 0m || @base == 1m)
                        throw new Exception("log domain error: invalid base or input");
                    return new LispDecimal((decimal)(Math.Log((double)x, (double)@base)));
                }
                throw new Exception("log expects one or two arguments");
            }));

            _globalEnv.Define("min", new LispFunction("min", args =>
            {
                if (args.Count == 0) throw new Exception("min expects at least one argument");
                bool anyDecimal = args.Any(a => a is LispDecimal);
                if (anyDecimal)
                {
                    decimal m = ToDecimal(args[0]);
                    for (int i = 1; i < args.Count; i++)
                    {
                        var v = ToDecimal(args[i]);
                        if (v < m) m = v;
                    }
                    return new LispDecimal(m);
                }
                else
                {
                    var m = ToBigInt(args[0]);
                    for (int i = 1; i < args.Count; i++)
                    {
                        var v = ToBigInt(args[i]);
                        if (v < m) m = v;
                    }
                    return new LispInteger(m);
                }
            }));

            _globalEnv.Define("max", new LispFunction("max", args =>
            {
                if (args.Count == 0) throw new Exception("max expects at least one argument");
                bool anyDecimal = args.Any(a => a is LispDecimal);
                if (anyDecimal)
                {
                    decimal m = ToDecimal(args[0]);
                    for (int i = 1; i < args.Count; i++)
                    {
                        var v = ToDecimal(args[i]);
                        if (v > m) m = v;
                    }
                    return new LispDecimal(m);
                }
                else
                {
                    var m = ToBigInt(args[0]);
                    for (int i = 1; i < args.Count; i++)
                    {
                        var v = ToBigInt(args[i]);
                        if (v > m) m = v;
                    }
                    return new LispInteger(m);
                }
            }));
        }

        private bool AreEqual(LispValue a, LispValue b)
        {
            if (a is LispInteger ia && b is LispInteger ib)
                return ia.Value == ib.Value;

            if (a is LispDecimal da && b is LispDecimal db)
                return da.Value == db.Value;

            if (a is LispInteger i2 && b is LispDecimal d2) return (decimal)i2.Value == d2.Value;
            if (a is LispDecimal d3 && b is LispInteger i3) return d3.Value == (decimal)i3.Value;
            
            if (a is LispString strA && b is LispString strB)
                return strA.Value == strB.Value;
            
            if (a is LispDate dateA && b is LispDate dateB)
                return dateA.Value == dateB.Value;
            
            if (a is LispSymbol symA && b is LispSymbol symB)
                return symA.Name == symB.Name;

            return a == b;
        }

        private static bool IsTruthy(LispValue v)
        {
            if (v is LispBoolean b) return b.Value;
            if (v is LispNil) return false;
            return true;
        }

        private bool IsLessThan(LispValue a, LispValue b)
        {
            // Treat nil as minimal value to avoid errors in list-processing predicates
            if (a is LispNil && b is LispNil) return false;
            if (a is LispNil)
            {
                // nil < anything else
                return true;
            }
            if (b is LispNil)
            {
                // anything else !< nil
                return false;
            }
            if (a is LispInteger ia && b is LispInteger ib)
                return ia.Value < ib.Value;

            if (a is LispDecimal da && b is LispDecimal db)
                return da.Value < db.Value;

            if (a is LispInteger i2 && b is LispDecimal d2) return (decimal)i2.Value < d2.Value;
            if (a is LispDecimal d3 && b is LispInteger i3) return d3.Value < (decimal)i3.Value;
            
            if (a is LispString strA && b is LispString strB)
                return string.Compare(strA.Value, strB.Value) < 0;
            
            if (a is LispDate dateA && b is LispDate dateB)
                return dateA.Value < dateB.Value;

            throw new Exception($"Cannot compare {a.GetType().Name} and {b.GetType().Name}");
        }

        private static System.Numerics.BigInteger ToBigInt(LispValue v)
        {
            return v switch
            {
                LispNil => System.Numerics.BigInteger.Zero,
                LispInteger li => li.Value,
                LispDecimal ld => new System.Numerics.BigInteger(ld.Value),
                _ => throw new Exception($"Expected number, got {v.GetType().Name}")
            };
        }

        private static decimal ToDecimal(LispValue v)
        {
            return v switch
            {
                LispNil => 0m,
                LispDecimal ld => ld.Value,
                LispInteger li => (decimal)li.Value,
                _ => throw new Exception($"Expected number, got {v.GetType().Name}")
            };
        }

        private static int ToInt32(LispValue v)
        {
            return v switch
            {
                LispInteger li => (int)li.Value,
                LispDecimal ld => (int)ld.Value,
                _ => throw new Exception($"Expected number, got {v.GetType().Name}")
            };
        }

        public LispValue Eval(LispValue expr, Environment? env = null)
        {
            env ??= _globalEnv;

            if (_trace)
            {
                Trace(expr.ToString());
                _traceDepth++;
            }

            LispValue result = LispNil.Instance; 

            try
            {
                result = expr switch
                {
                    LispInteger or LispDecimal or LispString or LispDate or LispBoolean => expr,
                    LispNil => expr,
                    LispSymbol symbol => EvalSymbol(symbol, env),
                    LispList list => EvalList(list, env),
                    _ => throw new Exception($"Cannot evaluate {expr.GetType().Name}")
                };
            }
            finally
            {
                if (_trace)
                {
                    _traceDepth--;
                    Trace($"=> {result}");
                }
            }

            return result;
        }

        private LispValue EvalSymbol(LispSymbol symbol, Environment env)
        {
            var value = env.Lookup(symbol.Name);
            if (value == null)
                throw new Exception($"Undefined symbol: {symbol.Name}");
            return value;
        }

        private LispValue EvalList(LispList list, Environment env)
        {
            if (list.Elements.Count == 0)
                return list;

            var first = list.Elements[0];

            // Handle special forms
            if (first is LispSymbol symbol)
            {
                switch (symbol.Name)
                {
                    case "quote":
                        if (list.Elements.Count != 2)
                            throw new Exception("quote expects exactly one argument");
                        return list.Elements[1];

                    case "if":
                        return EvalIf(list, env);

                    case "and":
                        return EvalAnd(list, env);

                    case "or":
                        return EvalOr(list, env);

                    case "define":
                        return EvalDefine(list, env);

                    case "set!":
                        return EvalSet(list, env);

                    case "lambda":
                        return EvalLambda(list, env);

                    case "cond":
                        return EvalCond(list, env);

                    case "let":
                        return EvalLet(list, env);

                    case "let*":
                        return EvalLetStar(list, env);

                    case "letrec":
                        return EvalLetRec(list, env);
                }
            }

            // Function application
            var func = Eval(first, env);
            if (func is not LispFunction function)
                throw new Exception($"Cannot call {func.GetType().Name} as a function");

            var args = list.Elements.Skip(1).Select(e => Eval(e, env)).ToList();
            return function.Implementation(args);
        }

        private LispValue EvalIf(LispList list, Environment env)
        {
            if (list.Elements.Count < 3 || list.Elements.Count > 4)
                throw new Exception("if expects 2 or 3 arguments: (if condition then-expr [else-expr])");

            Trace($"If condition: {list.Elements[1]}");
            var condition = Eval(list.Elements[1], env);
            bool isTrue = IsTruthy(condition);
            Trace($"If condition value: {condition} => {(isTrue ? "true" : "false")}");

            if (isTrue)
            {
                Trace($"If then-branch: {list.Elements[2]}");
                return Eval(list.Elements[2], env);
            }
            else if (list.Elements.Count == 4)
            {
                Trace($"If else-branch: {list.Elements[3]}");
                return Eval(list.Elements[3], env);
            }
            else
            {
                return LispNil.Instance;
            }
        }

        private LispValue EvalAnd(LispList list, Environment env)
        {
            // (and) => t
            // (and e1 e2 ...) => evaluates left-to-right, short-circuits on first nil/false
            if (list.Elements.Count == 1)
                return LispBoolean.True;

            LispValue result = LispBoolean.True;
            for (int i = 1; i < list.Elements.Count; i++)
            {
                result = Eval(list.Elements[i], env);
                if (!IsTruthy(result))
                    return LispNil.Instance;
            }
            return result;
        }

        private LispValue EvalOr(LispList list, Environment env)
        {
            // (or) => nil
            // (or e1 e2 ...) => evaluates left-to-right, returns first truthy value or nil
            if (list.Elements.Count == 1)
                return LispNil.Instance;

            for (int i = 1; i < list.Elements.Count; i++)
            {
                var result = Eval(list.Elements[i], env);
                if (IsTruthy(result))
                    return result;
            }
            return LispNil.Instance;
        }

        // let: evaluates all binding values in the outer environment, then
        // creates a new environment with all bindings available to the body.
        // (let ((x 1) (y (+ x 1))) body) -- y does NOT see x here.
        private LispValue EvalLet(LispList list, Environment env)
        {
            // (let ((name value) ...) body...)
            if (list.Elements.Count < 3)
                throw new Exception("let expects a bindings list and at least one body expression");

            if (list.Elements[1] is not LispList bindingsList)
                throw new Exception("let expects a list of bindings as its first argument");

            var localEnv = new Environment(env);

            foreach (var bindingVal in bindingsList.Elements)
            {
                if (bindingVal is not LispList binding || binding.Elements.Count != 2)
                    throw new Exception("let bindings must be lists of the form (name value)");

                if (binding.Elements[0] is not LispSymbol nameSym)
                    throw new Exception("let binding name must be a symbol");

                var value = Eval(binding.Elements[1], env); // evaluate in outer env
                localEnv.Define(nameSym.Name, value);
            }

            LispValue result = LispNil.Instance;
            for (int i = 2; i < list.Elements.Count; i++)
            {
                result = Eval(list.Elements[i], localEnv);
            }

            return result;
        }

        // let*: evaluates bindings sequentially; each value expression sees the
        // bindings introduced earlier in the same let*.
        // (let* ((x 1) (y (+ x 1))) body) -- y sees x here.
        private LispValue EvalLetStar(LispList list, Environment env)
        {
            // (let* ((name value) ...) body...)
            if (list.Elements.Count < 3)
                throw new Exception("let* expects a bindings list and at least one body expression");

            if (list.Elements[1] is not LispList bindingsList)
                throw new Exception("let* expects a list of bindings as its first argument");

            var localEnv = new Environment(env);

            foreach (var bindingVal in bindingsList.Elements)
            {
                if (bindingVal is not LispList binding || binding.Elements.Count != 2)
                    throw new Exception("let* bindings must be lists of the form (name value)");

                if (binding.Elements[0] is not LispSymbol nameSym)
                    throw new Exception("let* binding name must be a symbol");

                // In let*, each value is evaluated in the environment that already
                // includes all previous bindings in this let*.
                var value = Eval(binding.Elements[1], localEnv);
                localEnv.Define(nameSym.Name, value);
            }

            LispValue result = LispNil.Instance;
            for (int i = 2; i < list.Elements.Count; i++)
            {
                result = Eval(list.Elements[i], localEnv);
            }

            return result;
        }

        // letrec: all names are visible in all value expressions (mutually
        // recursive bindings). We first create placeholders for all names in a
        // new environment, then evaluate each value in that environment and
        // assign into the existing bindings.
        private LispValue EvalLetRec(LispList list, Environment env)
        {
            // (letrec ((name value) ...) body...)
            if (list.Elements.Count < 3)
                throw new Exception("letrec expects a bindings list and at least one body expression");

            if (list.Elements[1] is not LispList bindingsList)
                throw new Exception("letrec expects a list of bindings as its first argument");

            var localEnv = new Environment(env);

            // First pass: define all names with a temporary value so they are
            // visible (possibly self- or mutually recursive) in all value expressions.
            foreach (var bindingVal in bindingsList.Elements)
            {
                if (bindingVal is not LispList binding || binding.Elements.Count != 2)
                    throw new Exception("letrec bindings must be lists of the form (name value)");

                if (binding.Elements[0] is not LispSymbol nameSym)
                    throw new Exception("letrec binding name must be a symbol");

                // Use nil as an initial placeholder.
                localEnv.Define(nameSym.Name, LispNil.Instance);
            }

            // Second pass: evaluate each value in the localEnv (where all names exist)
            // and assign it into the existing binding using Set.
            foreach (var bindingVal in bindingsList.Elements)
            {
                var binding = (LispList)bindingVal;
                var nameSym = (LispSymbol)binding.Elements[0];
                var value = Eval(binding.Elements[1], localEnv);
                localEnv.Set(nameSym.Name, value);
            }

            LispValue result = LispNil.Instance;
            for (int i = 2; i < list.Elements.Count; i++)
            {
                result = Eval(list.Elements[i], localEnv);
            }

            return result;
        }

        private LispValue EvalDefine(LispList list, Environment env)
        {
            // Support variable define: (define name value)
            // And function define sugar:
            // 1) (define name (arg1 arg2 ...) body...)
            // 2) (define (name arg1 arg2 ...) body...)

            if (list.Elements.Count < 3)
                throw new Exception("define expects at least 2 arguments");

            // Case 2: (define (name args...) body...)
            if (list.Elements[1] is LispList nameAndParams && nameAndParams.Elements.Count > 0 && nameAndParams.Elements[0] is LispSymbol nameSymbol)
            {
                var paramNames = new List<string>();
                for (int i = 1; i < nameAndParams.Elements.Count; i++)
                {
                    if (nameAndParams.Elements[i] is not LispSymbol p)
                        throw new Exception("define function parameters must be symbols");
                    paramNames.Add(p.Name);
                }

                var body = list.Elements.Skip(2).ToList();
                var fn = new LispFunction(nameSymbol.Name, args =>
                {
                    if (args.Count != paramNames.Count)
                        throw new Exception($"{nameSymbol.Name} expects {paramNames.Count} arguments, got {args.Count}");

                    var localEnv = new Environment(env);
                    for (int i = 0; i < paramNames.Count; i++)
                        localEnv.Define(paramNames[i], args[i]);

                    LispValue result = LispNil.Instance;
                    foreach (var expr in body)
                        result = Eval(expr, localEnv);
                    return result;
                });
                env.Define(nameSymbol.Name, fn);
                return fn;
            }

            // Case 1: (define name (args...) body...)
            if (list.Elements[1] is LispSymbol symbol && list.Elements[2] is LispList paramListForSugar && paramListForSugar.Elements.All(e => e is LispSymbol) && list.Elements.Count > 3)
            {
                var paramNames = new List<string>();
                foreach (var p in paramListForSugar.Elements)
                {
                    paramNames.Add(((LispSymbol)p).Name);
                }

                var body = list.Elements.Skip(3).ToList();
                if (body.Count == 0)
                    throw new Exception("define function form requires a body");

                var fn = new LispFunction(symbol.Name, args =>
                {
                    if (args.Count != paramNames.Count)
                        throw new Exception($"{symbol.Name} expects {paramNames.Count} arguments, got {args.Count}");

                    var localEnv = new Environment(env);
                    for (int i = 0; i < paramNames.Count; i++)
                        localEnv.Define(paramNames[i], args[i]);

                    LispValue result = LispNil.Instance;
                    foreach (var expr in body)
                        result = Eval(expr, localEnv);
                    return result;
                });
                env.Define(symbol.Name, fn);
                return fn;
            }

            // Fallback: variable define
            if (list.Elements[1] is not LispSymbol varSymbol)
                throw new Exception("define expects a symbol as the first argument");

            var value = Eval(list.Elements[2], env);
            env.Define(varSymbol.Name, value);
            return value;
        }

        private LispValue EvalSet(LispList list, Environment env)
        {
            if (list.Elements.Count != 3)
                throw new Exception("set! expects exactly 2 arguments: (set! name value)");

            if (list.Elements[1] is not LispSymbol symbol)
                throw new Exception("set! expects a symbol as the first argument");

            var value = Eval(list.Elements[2], env);
            env.Set(symbol.Name, value);
            return value;
        }

        private LispValue EvalLambda(LispList list, Environment env)
        {
            if (list.Elements.Count < 3)
                throw new Exception("lambda expects at least 2 arguments: (lambda (params...) body...)");

            if (list.Elements[1] is not LispList paramList)
                throw new Exception("lambda expects a parameter list");

            var paramNames = new List<string>();
            foreach (var param in paramList.Elements)
            {
                if (param is not LispSymbol paramSymbol)
                    throw new Exception("lambda parameters must be symbols");
                paramNames.Add(paramSymbol.Name);
            }

            var body = list.Elements.Skip(2).ToList();

            return new LispFunction("lambda", args =>
            {
                if (args.Count != paramNames.Count)
                    throw new Exception($"lambda expects {paramNames.Count} arguments, got {args.Count}");

                var localEnv = new Environment(env);
                for (int i = 0; i < paramNames.Count; i++)
                {
                    localEnv.Define(paramNames[i], args[i]);
                }

                LispValue result = LispNil.Instance;
                foreach (var expr in body)
                {
                    result = Eval(expr, localEnv);
                }
                return result;
            });
        }

        private LispValue EvalCond(LispList list, Environment env)
        {
            // (cond (test1 expr1 expr2 ...) (test2 expr...) ... (else expr...))
            if (list.Elements.Count < 2)
                throw new Exception("cond expects at least one clause");

            for (int i = 1; i < list.Elements.Count; i++)
            {
                if (list.Elements[i] is not LispList clause || clause.Elements.Count == 0)
                    throw new Exception("cond clauses must be non-empty lists");

                var first = clause.Elements[0];
                bool selected;

                if (first is LispSymbol sym && sym.Name == "else")
                {
                    selected = true;
                }
                else
                {
                    var testVal = Eval(first, env);
                    selected = IsTruthy(testVal);
                }
                if (selected)
                {
                    LispValue result = LispNil.Instance;
                    if (clause.Elements.Count == 1)
                    {
                        result = Eval(first, env);
                        return result;
                    }

                    for (int j = 1; j < clause.Elements.Count; j++)
                    {
                        result = Eval(clause.Elements[j], env);
                    }
                    return result;
                }
            }

            return LispNil.Instance;
        }
    }
}

