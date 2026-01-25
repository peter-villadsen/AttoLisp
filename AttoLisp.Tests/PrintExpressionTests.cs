using Xunit;

namespace AttoLisp.Tests
{
    public class PrintExpressionTests : EvaluatorTestsBase
    {
        public PrintExpressionTests()
        {
            var printPath = Path.Combine(AppContext.BaseDirectory, "PrintExpression.al");
            if (!File.Exists(printPath)) throw new FileNotFoundException(printPath);
            var exprs = TestHelper.ParseFile(printPath);
            foreach (var expr in exprs)
            {
                Evaluator.Eval(expr);
            }
        }

        [Fact]
        public void PrintExpr_Simple_Variable()
        {
            var r = Eval("(print-expr 'x)");
            Assert.IsType<LispString>(r);
            Assert.Equal("x", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Number()
        {
            var r = Eval("(print-expr '42)");
            Assert.IsType<LispString>(r);
            Assert.Equal("42", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Addition()
        {
            var r = Eval("(print-expr '(+ x 2))");
            Assert.IsType<LispString>(r);
            Assert.Equal("x + 2", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Multiple_Addition()
        {
            var r = Eval("(print-expr '(+ x y z))");
            Assert.IsType<LispString>(r);
            Assert.Equal("x + y + z", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Subtraction()
        {
            var r = Eval("(print-expr '(- x 2))");
            Assert.IsType<LispString>(r);
            Assert.Equal("x - 2", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Unary_Minus()
        {
            var r = Eval("(print-expr '(- x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("-x", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Product_Number_Variable()
        {
            var r = Eval("(print-expr '(* 2 x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("2x", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Product_Variables()
        {
            var r = Eval("(print-expr '(* x y))");
            Assert.IsType<LispString>(r);
            Assert.Equal("x \\cdot y", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Product_With_Parens()
        {
            var r = Eval("(print-expr '(* (+ a b) c))");
            Assert.IsType<LispString>(r);
            Assert.Equal("(a + b) \\cdot c", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Fraction()
        {
            var r = Eval("(print-expr '(/ x 2))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\frac{x}{2}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Nested_Fraction()
        {
            var r = Eval("(print-expr '(/ (+ x 1) (- x 1)))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\frac{x + 1}{x - 1}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Power()
        {
            var r = Eval("(print-expr '(^ x 2))");
            Assert.IsType<LispString>(r);
            Assert.Equal("x^{2}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Power_With_Parens()
        {
            var r = Eval("(print-expr '(^ (+ x 1) 2))");
            Assert.IsType<LispString>(r);
            Assert.Equal("(x + 1)^{2}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Sqrt()
        {
            var r = Eval("(print-expr '(sqrt x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\sqrt{x}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Sqrt_Complex()
        {
            var r = Eval("(print-expr '(sqrt (+ x 1)))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\sqrt{x + 1}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Exponential()
        {
            var r = Eval("(print-expr '(exp x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("e^{x}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Exponential_Complex()
        {
            var r = Eval("(print-expr '(exp (+ x 1)))");
            Assert.IsType<LispString>(r);
            Assert.Equal("e^{x + 1}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Log()
        {
            var r = Eval("(print-expr '(log x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\ln(x)", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Sin()
        {
            var r = Eval("(print-expr '(sin x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\sin(x)", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Cos()
        {
            var r = Eval("(print-expr '(cos x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\cos(x)", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Tan()
        {
            var r = Eval("(print-expr '(tan x))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\tan(x)", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Sin_Product()
        {
            var r = Eval("(print-expr '(sin (* 2 x)))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\sin(2x)", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Complex_Expression()
        {
            // (x^2 + 1) / (x - 1)
            var r = Eval("(print-expr '(/ (+ (^ x 2) 1) (- x 1)))");
            Assert.IsType<LispString>(r);
            Assert.Equal("\\frac{x^{2} + 1}{x - 1}", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Precedence_No_Extra_Parens()
        {
            // x + y * z should be "x + y \cdot z" not "(x) + (y \cdot z)"
            var r = Eval("(print-expr '(+ x (* y z)))");
            Assert.IsType<LispString>(r);
            Assert.Equal("x + y \\cdot z", ((LispString)r).Value);
        }

        [Fact]
        public void PrintExpr_Full_Derivative_Pipeline()
        {
            // Load differentiate and simplify
            var diffPath = Path.Combine(AppContext.BaseDirectory, "DifferentiateExpression.al");
            var simplifyPath = Path.Combine(AppContext.BaseDirectory, "SimplifyExpression.al");
            
            if (File.Exists(diffPath) && File.Exists(simplifyPath))
            {
                foreach (var expr in TestHelper.ParseFile(diffPath))
                    Evaluator.Eval(expr);
                foreach (var expr in TestHelper.ParseFile(simplifyPath))
                    Evaluator.Eval(expr);

                // d/dx(sin(x)) simplified and printed
                var r = Eval("(print-expr (simplify-expr (differentiate '(sin x) 'x)))");
                Assert.IsType<LispString>(r);
                Assert.Equal("\\cos(x)", ((LispString)r).Value);
            }
        }
    }
}
