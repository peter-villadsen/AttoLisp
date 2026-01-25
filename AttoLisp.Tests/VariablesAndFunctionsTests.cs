using Xunit;

namespace AttoLisp.Tests
{
    public class VariablesAndFunctionsTests : EvaluatorTestsBase
    {
        [Fact]
        public void Define_CreatesVariable()
        {
            var result = Eval("(define x 10)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(10, (int)((LispInteger)result).Value);
            
            // Verify variable can be retrieved
            var lookupResult = Eval("x");
            Assert.Equal(10, (int)((LispInteger)lookupResult).Value);
        }

        [Fact]
        public void SetBang_UpdatesVariable()
        {
            Eval("(define x 10)");
            var result = Eval("(set! x 20)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(20, (int)((LispInteger)result).Value);
            
            // Verify variable was updated
            var lookupResult = Eval("x");
            Assert.Equal(20, (int)((LispInteger)lookupResult).Value);
        }

        [Fact]
        public void If_TrueCondition_EvaluatesThenBranch()
        {
            var result = Eval("(if (> 10 5) \"big\" \"small\")");
            
            Assert.IsType<LispString>(result);
            Assert.Equal("big", ((LispString)result).Value);
        }

        [Fact]
        public void If_FalseCondition_EvaluatesElseBranch()
        {
            var result = Eval("(if (< 10 5) \"big\" \"small\")");
            
            Assert.IsType<LispString>(result);
            Assert.Equal("small", ((LispString)result).Value);
        }

        [Fact]
        public void Lambda_CreatesAnonymousFunction()
        {
            var result = Eval("(lambda (x) (* x x))");
            
            Assert.IsType<LispFunction>(result);
        }

        [Fact]
        public void Lambda_CanBeInvokedDirectly()
        {
            var result = Eval("((lambda (x) (* x x)) 5)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(25, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void DefineSquare_CreatesSquareFunction()
        {
            Eval("(define square (lambda (x) (* x x)))");
            var result = Eval("(square 5)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(25, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void DefineFunctionSugar_NameThenParams_Works()
        {
            Eval("(define add (x y) (+ x y))");
            var result = Eval("(add 2 3)");
            Assert.IsType<LispInteger>(result);
            Assert.Equal(5, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void DefineFunctionSugar_NameInParamList_Works()
        {
            Eval("(define (mul x y) (* x y))");
            var result = Eval("(mul 3 4)");
            Assert.IsType<LispInteger>(result);
            Assert.Equal(12, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void Lambda_MultipleParameters_Works()
        {
            var result = Eval("((lambda (x y) (+ x y)) 3 4)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(7, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void Lambda_ClosesOverVariables()
        {
            Eval("(define x 10)");
            Eval("(define add-x (lambda (y) (+ x y)))");
            var result = Eval("(add-x 5)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(15, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void If_WithVariables_Works()
        {
            Eval("(define x 10)");
            var result = Eval("(if (> x 5) \"big\" \"small\")");
            
            Assert.IsType<LispString>(result);
            Assert.Equal("big", ((LispString)result).Value);
        }

        [Fact]
        public void Let_Evaluates_Bindings_In_Outer_Env()
        {
            // y does not see x in the same let binding group
            var r = Eval("(let ((x 1) (y (+ x 1))) y)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void LetStar_Evaluates_Bindings_Sequentially()
        {
            var r = Eval("(let* ((x 1) (y (+ x 1))) y)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(2, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void LetRec_Allows_Mutually_Recursive_Local_Functions()
        {
            // Simple even/odd using letrec
            var expr = "(letrec ((even (lambda (n) (if (= n 0) t (odd (- n 1)))))" +
                       "        (odd  (lambda (n) (if (= n 0) nil (even (- n 1))))))" +
                       "  (and (even 4) (odd 3)))";

            var r = Eval(expr);
            Assert.IsType<LispBoolean>(r);
            Assert.True(((LispBoolean)r).Value);
        }
    }
}
