using Xunit;

namespace AttoLisp.Tests
{
    public class ParseExpressionTests
    {
        private readonly Evaluator _evaluator;

        public ParseExpressionTests()
        {
            // Read the stdlib.al and then the ParseExpression.al files
            _evaluator = new Evaluator();
            var stdlibPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "AttoLisp", "stdlib.al");
            var parsePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "AttoLisp", "ParseExpression.al");
            foreach (var path in new[] { stdlibPath, parsePath })
            {
                if (!File.Exists(path)) throw new FileNotFoundException(path);
                var source = File.ReadAllText(path);
                var tokenizer = new Tokenizer(source);
                var tokens = tokenizer.Tokenize();
                var parser = new Parser(tokens);
                var exprs = parser.ParseAll();
                foreach (var expr in exprs)
                {
                    _evaluator.Eval(expr);
                }
            }
        }

        private LispValue Eval(string expression)
        {
            var tokenizer = new Tokenizer(expression);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens);
            var expr = parser.Parse();
            return _evaluator.Eval(expr);
        }

        [Fact]
        public void Parses_Simple_Function_With_Plus()
        {
            var r = Eval("(parseExpression \"Abs(1 + 2)\")");
            var list = (LispList)r;
            Assert.IsType<LispSymbol>(list.Elements[0]);
            Assert.Equal("abs", ((LispSymbol)list.Elements[0]).Name);
            var inner = (LispList)list.Elements[1];
            Assert.Equal("+", ((LispSymbol)inner.Elements[0]).Name);
            Assert.Equal(1, (int)((LispInteger)inner.Elements[1]).Value);
            Assert.Equal(2, (int)((LispInteger)inner.Elements[2]).Value);
        }

        [Fact]
        public void Returns_Nil_On_Syntax_Error()
        {
            var r = Eval("(parseExpression \"Abs(1 +\")");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Parses_Function_With_Multiple_Args()
        {
            var r = Eval("(parseExpression \"sum(1, 2, 3)\")");
            var list = (LispList)r;
            Assert.Equal("sum", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(1, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(2, (int)((LispInteger)list.Elements[2]).Value);
            Assert.Equal(3, (int)((LispInteger)list.Elements[3]).Value);
        }

        [Fact]
        public void Parses_Zero_Arg_Function()
        {
            var r = Eval("(parseExpression \"now()\")");
            var list = (LispList)r;
            Assert.Equal("now", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(1, list.Elements.Count); // only the function symbol
        }

        [Fact]
        public void Parses_Unary_Minus()
        {
            var r = Eval("(parseExpression \"-5\")");
            var list = (LispList)r;
            Assert.Equal("-", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(0, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(-5, (int)((LispInteger)list.Elements[2]).Value);
        }

        [Fact]
        public void Parses_Nested_Parentheses()
        {
            var r = Eval("(parseExpression \"(1 + (2 * 3))\")");
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(1, (int)((LispInteger)list.Elements[1]).Value);
            var mul = (LispList)list.Elements[2];
            Assert.Equal("*", ((LispSymbol)mul.Elements[0]).Name);
            Assert.Equal(2, (int)((LispInteger)mul.Elements[1]).Value);
            Assert.Equal(3, (int)((LispInteger)mul.Elements[2]).Value);
        }

        [Fact]
        public void Parses_Parenthesized_Expression_Only()
        {
            var r = Eval("(parseExpression \"(1 + 2)\")");
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(1, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(2, (int)((LispInteger)list.Elements[2]).Value);
        }

        [Fact]
        public void Parses_Operator_Precedence()
        {
            var r = Eval("(parseExpression \"1 + 2 * 3\")");
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(1, (int)((LispInteger)list.Elements[1]).Value);
            var mul = (LispList)list.Elements[2];
            Assert.Equal("*", ((LispSymbol)mul.Elements[0]).Name);
            Assert.Equal(2, (int)((LispInteger)mul.Elements[1]).Value);
            Assert.Equal(3, (int)((LispInteger)mul.Elements[2]).Value);
        }

        [Fact]
        public void Parses_Function_With_Mixed_Args()
        {
            var r = Eval("(parseExpression \"f(1, x, 2+3)\")");
            var list = (LispList)r;
            Assert.Equal("f", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(4, list.Elements.Count);
            Assert.Equal(1, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal("x", ((LispSymbol)list.Elements[2]).Name);
            var plus = (LispList)list.Elements[3];
            Assert.Equal("+", ((LispSymbol)plus.Elements[0]).Name);
            Assert.Equal(2, (int)((LispInteger)plus.Elements[1]).Value);
            Assert.Equal(3, (int)((LispInteger)plus.Elements[2]).Value);
        }

        [Fact]
        public void Returns_Nil_On_Unbalanced_Parentheses()
        {
            var r = Eval("(parseExpression \"(1 + 2\")");
            Assert.IsType<LispNil>(r);
        }
    }
}
