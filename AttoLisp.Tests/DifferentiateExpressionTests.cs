using Xunit;

namespace AttoLisp.Tests
{
    public class DifferentiateExpressionTests : EvaluatorTestsBase
    {
        public DifferentiateExpressionTests()
        {
            var diffPath = Path.Combine(AppContext.BaseDirectory, "DifferentiateExpression.al");
            if (!File.Exists(diffPath)) throw new FileNotFoundException(diffPath);
            var exprs = TestHelper.ParseFile(diffPath);
            foreach (var expr in exprs)
            {
                Evaluator.Eval(expr);
            }
            
            // Also load SimplifyExpression for tests that need it
            var simplifyPath = Path.Combine(AppContext.BaseDirectory, "SimplifyExpression.al");
            if (File.Exists(simplifyPath))
            {
                var simplifyExprs = TestHelper.ParseFile(simplifyPath);
                foreach (var expr in simplifyExprs)
                {
                    Evaluator.Eval(expr);
                }
            }
        }

        [Fact]
        public void Differentiates_Product_2x_WrtX_Unsimplified()
        {
            // d/dx(2*x) = (+ (* 0 x) (* 2 1)) before simplification
            var r = Eval("(differentiate '(* 2 x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Product_2x_WrtX_Simplified()
        {
            // d/dx(2*x) simplified = 2
            var r = Eval("(simplify-expr (differentiate '(* 2 x) 'x))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(2, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Differentiates_Constant_WrtX()
        {
            // d/dx(5) = 0
            var r = Eval("(differentiate '5 'x)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Differentiates_Variable_WrtItself()
        {
            // d/dx(x) = 1
            var r = Eval("(differentiate 'x 'x)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Differentiates_Variable_WrtDifferentVariable()
        {
            // d/dx(y) = 0
            var r = Eval("(differentiate 'y 'x)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Differentiates_Product_x_x_WrtX_Unsimplified()
        {
            // d/dx(x*x) = (+ (* 1 x) (* x 1)) before simplification
            var r = Eval("(differentiate '(* x x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Product_x_x_WrtX_Simplified()
        {
            // d/dx(x*x) = (+ x x) after simplification
            var r = Eval("(simplify-expr (differentiate '(* x x) 'x))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[2]).Name);
        }

        [Fact]
        public void Differentiates_Sum_x_plus_5_WrtX_Unsimplified()
        {
            // d/dx(x + 5) = (+ 1 0) before simplification
            var r = Eval("(differentiate '(+ x 5) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Sum_x_plus_5_WrtX_Simplified()
        {
            // d/dx(x + 5) = 1 after simplification
            var r = Eval("(simplify-expr (differentiate '(+ x 5) 'x))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Differentiates_Quotient_x_over_2_WrtX_Unsimplified()
        {
            // d/dx(x/2) = (- (* 1 2) (* x 0)) / (* 2 2) before simplification
            var r = Eval("(differentiate '(/ x 2) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("/", ((LispSymbol)list.Elements[0]).Name);
            // Check structure: (/ numerator denominator)
            Assert.IsType<LispList>(list.Elements[1]); // numerator is (- ...)
            Assert.IsType<LispList>(list.Elements[2]); // denominator is (* 2 2)
        }

        [Fact]
        public void Differentiates_Quotient_x_over_2_WrtX_Simplified()
        {
            // d/dx(x/2) = 2 / 4 = (/ 2 4) after one simplification pass
            // Note: full simplification to 1/2 would require numeric evaluation
            var r = Eval("(simplify-expr (differentiate '(/ x 2) 'x))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("/", ((LispSymbol)list.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Quotient_x_squared_over_x_WrtX()
        {
            // d/dx(x²/x) using quotient rule
            var r = Eval("(differentiate '(/ (* x x) x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("/", ((LispSymbol)list.Elements[0]).Name);
            // Result should be a quotient with numerator and denominator
            Assert.Equal(3, list.Elements.Count); // operator + 2 operands
        }

        [Fact]
        public void Differentiates_Sin_x_WrtX()
        {
            // d/dx(sin(x)) = cos(x) * 1
            var r = Eval("(differentiate '(sin x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            var cosExpr = (LispList)list.Elements[1];
            Assert.Equal("cos", ((LispSymbol)cosExpr.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Cos_x_WrtX()
        {
            // d/dx(cos(x)) = -sin(x) * 1
            var r = Eval("(differentiate '(cos x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            // First element should be (- 0 (sin x))
            var negSin = (LispList)list.Elements[1];
            Assert.Equal("-", ((LispSymbol)negSin.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Tan_x_WrtX()
        {
            // d/dx(tan(x)) = (1 / cos²(x)) * 1
            var r = Eval("(differentiate '(tan x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            var secSquared = (LispList)list.Elements[1];
            Assert.Equal("/", ((LispSymbol)secSquared.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Exp_x_WrtX()
        {
            // d/dx(exp(x)) = exp(x) * 1
            var r = Eval("(differentiate '(exp x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            var expExpr = (LispList)list.Elements[1];
            Assert.Equal("exp", ((LispSymbol)expExpr.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Log_x_WrtX()
        {
            // d/dx(log(x)) = (1/x) * 1
            var r = Eval("(differentiate '(log x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            var oneOverX = (LispList)list.Elements[1];
            Assert.Equal("/", ((LispSymbol)oneOverX.Elements[0]).Name);
            Assert.Equal(1, (int)((LispInteger)oneOverX.Elements[1]).Value);
        }

        [Fact]
        public void Differentiates_Sqrt_x_WrtX()
        {
            // d/dx(sqrt(x)) = (1/(2*sqrt(x))) * 1
            var r = Eval("(differentiate '(sqrt x) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            var derivative = (LispList)list.Elements[1];
            Assert.Equal("/", ((LispSymbol)derivative.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Sin_2x_WrtX_ChainRule()
        {
            // d/dx(sin(2*x)) = cos(2*x) * d/dx(2*x)
            // This tests the chain rule application
            var r = Eval("(differentiate '(sin (* 2 x)) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            // Should have cos(2*x) as first factor
            var cosExpr = (LispList)list.Elements[1];
            Assert.Equal("cos", ((LispSymbol)cosExpr.Elements[0]).Name);
            // Second factor should be the derivative of 2*x
            var innerDerivative = (LispList)list.Elements[2];
            Assert.Equal("+", ((LispSymbol)innerDerivative.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Exp_x_squared_WrtX_ChainRule()
        {
            // d/dx(exp(x*x)) = exp(x*x) * d/dx(x*x)
            var r = Eval("(differentiate '(exp (* x x)) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
            var expExpr = (LispList)list.Elements[1];
            Assert.Equal("exp", ((LispSymbol)expExpr.Elements[0]).Name);
        }

        [Fact]
        public void Differentiates_Log_x_plus_1_WrtX_ChainRule()
        {
            // d/dx(log(x+1)) = (1/(x+1)) * 1
            var r = Eval("(differentiate '(log (+ x 1)) 'x)");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("*", ((LispSymbol)list.Elements[0]).Name);
        }

        [Fact]
        public void Full_Pipeline_Differentiate_And_Simplify_Sin_x()
        {
            // d/dx(sin(x)) = (* (cos x) 1) => simplified to (cos x)
            var r = Eval("(simplify-expr (differentiate '(sin x) 'x))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("cos", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
            Assert.Equal(2, list.Elements.Count); // Just (cos x)
        }

        [Fact]
        public void Full_Pipeline_Differentiate_And_Simplify_2x()
        {
            // d/dx(2*x) = (+ (* 0 x) (* 2 1)) => simplified to 2
            var r = Eval("(simplify-expr (differentiate '(* 2 x) 'x))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(2, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Full_Pipeline_Differentiate_And_Simplify_x_plus_5()
        {
            // d/dx(x+5) = (+ 1 0) => simplified to 1
            var r = Eval("(simplify-expr (differentiate '(+ x 5) 'x))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(1, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Full_Pipeline_Exp_x_Simplified()
        {
            // d/dx(exp(x)) = (* (exp x) 1) => simplified to (exp x)
            var r = Eval("(simplify-expr (differentiate '(exp x) 'x))");
            Assert.IsType<LispList>(r);
            var list = (LispList)r;
            Assert.Equal("exp", ((LispSymbol)list.Elements[0]).Name);
            Assert.Equal("x", ((LispSymbol)list.Elements[1]).Name);
        }
    }
}
