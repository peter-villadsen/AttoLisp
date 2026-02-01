using Xunit;

namespace AttoLisp.Tests
{
    public class SimplifyExpressionTests : EvaluatorTestsBase
    {
        public SimplifyExpressionTests()
        {
            var simplifyPath = Path.Combine(AppContext.BaseDirectory, "SimplifyExpression.al");
            if (!File.Exists(simplifyPath)) throw new FileNotFoundException(simplifyPath);
            var exprs = TestHelper.ParseFile(simplifyPath);
            foreach (var expr in exprs)
            {
                Evaluator.Eval(expr);
            }
        }

        [Fact]
        public void Simplify_Addition_With_Zero_Right()
        {
            // (+ x 0) => x
            var r = Eval("(simplify-expr '(+ x 0))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Addition_With_Zero_Left()
        {
            // (+ 0 x) => x
            var r = Eval("(simplify-expr '(+ 0 x))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Addition_With_Multiple_Zeros()
        {
            // (+ 0 x 0 y) => (+ x y)
            var r = Eval("(simplify-expr '(+ 0 x 0 y))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(2, list.Elements.Count - 1); // operator + 2 operands
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal("y", ((LispSymbol)list.Elements[2]).Name);
        }

        [Fact]
        public void Simplify_Addition_All_Zeros()
        {
            // (+ 0 0) => 0
            var r = Eval("(simplify-expr '(+ 0 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Multiplication_With_One_Right()
        {
            // (* x 1) => x
            var r = Eval("(simplify-expr '(* x 1))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Multiplication_With_One_Left()
        {
            // (* 1 x) => x
            var r = Eval("(simplify-expr '(* 1 x))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Multiplication_With_Multiple_Ones()
        {
            // (* 1 x 1 y) => (* x y)
            var r = Eval("(simplify-expr '(* 1 x 1 y))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal(2, list.Elements.Count - 1); // operator + 2 operands
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal("y", ((LispSymbol)list.Elements[2]).Name);
        }

        [Fact]
        public void Simplify_Multiplication_With_Zero()
        {
            // (* x 0 y) => 0
            var r = Eval("(simplify-expr '(* x 0 y))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Multiplication_All_Ones()
        {
            // (* 1 1) => 1
            var r = Eval("(simplify-expr '(* 1 1))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Subtraction_With_Zero_Right()
        {
            // (- x 0) => x
            var r = Eval("(simplify-expr '(- x 0))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Division_By_One()
        {
            // (/ x 1) => x
            var r = Eval("(simplify-expr '(/ x 1))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Atom_Returns_Unchanged()
        {
            // Atoms don't simplify
            var r = Eval("(simplify-expr 'x)");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Number_Returns_Unchanged()
        {
            // Numbers don't simplify
            var r = Eval("(simplify-expr '42)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(42, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Derivative_Pattern_Cos_Times_One()
        {
            // (* (cos x) 1) => (cos x)
            var r = Eval("(simplify-expr '(* (cos x) 1))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("cos", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal(2, list.Elements.Count); // Just (cos x), no multiplication
        }

        [Fact]
        public void Simplify_Derivative_Pattern_One_Times_Sin()
        {
            // (* 1 (sin x)) => (sin x)
            var r = Eval("(simplify-expr '(* 1 (sin x)))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("sin", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
        }

        [Fact]
        public void Simplify_Recursive_Multiplication_And_Addition()
        {
            // (+ (* 1 x) 0) => x (needs recursive simplification)
            var r = Eval("(simplify-expr '(+ (* 1 x) 0))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Sin_Zero()
        {
            // (sin 0) => 0
            var r = Eval("(simplify-expr '(sin 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Cos_Zero()
        {
            // (cos 0) => 1
            var r = Eval("(simplify-expr '(cos 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Tan_Zero()
        {
            // (tan 0) => 0
            var r = Eval("(simplify-expr '(tan 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Exp_Zero()
        {
            // (exp 0) => 1
            var r = Eval("(simplify-expr '(exp 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Log_One()
        {
            // (log 1) => 0
            var r = Eval("(simplify-expr '(log 1))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Sqrt_Zero()
        {
            // (sqrt 0) => 0
            var r = Eval("(simplify-expr '(sqrt 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Sqrt_One()
        {
            // (sqrt 1) => 1
            var r = Eval("(simplify-expr '(sqrt 1))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Log_Exp_Inverse()
        {
            // (log (exp x)) => x
            var r = Eval("(simplify-expr '(log (exp x)))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Exp_Log_Inverse()
        {
            // (exp (log x)) => x
            var r = Eval("(simplify-expr '(exp (log x)))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Subtraction_Same_Operands()
        {
            // (- x x) => 0
            var r = Eval("(simplify-expr '(- x x))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Subtraction_Zero_Left()
        {
            // (- 0 x) => (- x)
            var r = Eval("(simplify-expr '(- 0 x))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("-", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal(2, list.Elements.Count); // unary minus
        }

        [Fact]
        public void Simplify_Nested_Math_Functions()
        {
            // (sin (+ x 0)) => (sin x) (recursive on arguments)
            var r = Eval("(simplify-expr '(sin (+ x 0)))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("sin", ((LispSymbol)list.Elements[0]).Name);
            Assert.IsType<LispSymbol>(list.Elements[1]);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
        }

        [Fact]
        public void Simplify_Complex_Derivative_Expression()
        {
            // Simulate a derivative result: (+ (* 0 x) (* 2 1))
            // Should simplify to: 2
            var r = Eval("(simplify-expr '(+ (* 0 x) (* 2 1)))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(2, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Addition_Constant_Folding()
        {
            // (+ x 1 y 2) => (+ 3 x y)
            var r = Eval("(simplify-expr '(+ x 1 y 2))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            
            // Should have 3 operands: the constant 3, x, and y
            Assert.Equal(4, list.Elements.Count); // operator + 3 operands
            
            // First operand should be 3
            Assert.IsType<LispInteger>(list.Elements[1]);
            Assert.Equal(3, (int)((LispInteger)list.Elements[1]).Value);
        }

        [Fact]
        public void Simplify_Addition_All_Constants()
        {
            // (+ 1 2 3) => 6
            var r = Eval("(simplify-expr '(+ 1 2 3))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(6, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Multiplication_Constant_Folding()
        {
            // (* x 4 y 2 (sin x)) => (* 8 x y (sin x))
            var r = Eval("(simplify-expr '(* x 4 y 2 (sin x)))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            
            // Should have 4 operands: the constant 8, x, y, and (sin x)
            Assert.Equal(5, list.Elements.Count); // operator + 4 operands
            
            // First operand should be 8
            Assert.IsType<LispInteger>(list.Elements[1]);
            Assert.Equal(8, (int)((LispInteger)list.Elements[1]).Value);
        }

        [Fact]
        public void Simplify_Multiplication_All_Constants()
        {
            // (* 2 3 4) => 24
            var r = Eval("(simplify-expr '(* 2 3 4))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(24, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Subtraction_Two_Constants()
        {
            // (- 5 3) => 2
            var r = Eval("(simplify-expr '(- 5 3))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(2, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Division_Two_Constants()
        {
            // (/ 10 2) => 5
            var r = Eval("(simplify-expr '(/ 10 2))");
            Assert.IsType<LispDecimal>(r);
            Assert.Equal(5m, ((LispDecimal)r).Value);
        }

        [Fact]
        public void Simplify_Addition_Constants_With_Zeros()
        {
            // (+ x 1 0 y 2 0) => (+ 3 x y)
            var r = Eval("(simplify-expr '(+ x 1 0 y 2 0))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            
            // First operand should be 3
            Assert.IsType<LispInteger>(list.Elements[1]);
            Assert.Equal(3, (int)((LispInteger)list.Elements[1]).Value);
        }

        [Fact]
        public void Simplify_Multiplication_Constants_With_Ones()
        {
            // (* x 2 1 y 3 1) => (* 6 x y)
            var r = Eval("(simplify-expr '(* x 2 1 y 3 1))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            
            // First operand should be 6
            Assert.IsType<LispInteger>(list.Elements[1]);
            Assert.Equal(6, (int)((LispInteger)list.Elements[1]).Value);
        }

        [Fact]
        public void Simplify_Nested_Constant_Folding()
        {
            // (+ (* 2 3) (* 4 5)) => (+ 6 20) => 26
            var r = Eval("(simplify-expr '(+ (* 2 3) (* 4 5)))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(26, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Power_x_to_0()
        {
            // (^ x 0) => 1
            var r = Eval("(simplify-expr '(^ x 0))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Power_x_to_1()
        {
            // (^ x 1) => x
            var r = Eval("(simplify-expr '(^ x 1))");
            Assert.IsType<LispSymbol>(r);
            Assert.Equal("x", ((LispSymbol)r).Name);
        }

        [Fact]
        public void Simplify_Power_0_to_n()
        {
            // (^ 0 5) => 0
            var r = Eval("(simplify-expr '(^ 0 5))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Power_1_to_n()
        {
            // (^ 1 100) => 1
            var r = Eval("(simplify-expr '(^ 1 100))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Simplify_Power_Nested()
        {
            // (^ (+ x 0) 2) => (^ x 2) after simplifying the base
            var r = Eval("(simplify-expr '(^ (+ x 0) 2))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("^", ((LispSymbol)list.Elements[0]).Name);
            Assert.IsType<LispSymbol>(list.Elements[1]);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal(2, (int)((LispInteger)list.Elements[2]).Value);
        }
    }
}
