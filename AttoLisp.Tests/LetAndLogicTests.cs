using Xunit;

namespace AttoLisp.Tests
{
    public class LetAndLogicTests : EvaluatorTestsBase
    {
        [Fact]
        public void Let_Binds_Local_Variables()
        {
            var r = Eval("(let ((x 1) (y 2)) (+ x y))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(3, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Let_Shadowing_Uses_Inner_Binding()
        {
            Eval("(define x 10)");
            var r = Eval("(let ((x 5)) x)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void And_Zero_Args_Returns_T()
        {
            var r = Eval("(and)");
            Assert.IsType<LispBoolean>(r);
            Assert.True(((LispBoolean)r).Value);
        }

        [Fact]
        public void And_Multi_Args_ShortCircuits_On_Nil()
        {
            var r = Eval("(and 1 nil 2)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void And_Multi_Args_Returns_Last_Value()
        {
            var r = Eval("(and 1 2 3)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(3, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Or_Zero_Args_Returns_Nil()
        {
            var r = Eval("(or)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Or_Multi_Args_ShortCircuits_And_Returns_First_Truthy()
        {
            var r = Eval("(or nil 0 2)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Or_Multi_Args_All_Nil_Returns_Nil()
        {
            var r = Eval("(or nil nil)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Relational_Operators_Work_With_Integers()
        {
            Assert.IsType<LispBoolean>(Eval("(< 1 2 3)"));
            Assert.True(((LispBoolean)Eval("(< 1 2 3)")).Value);
            Assert.False(((LispBoolean)Eval("(< 1 2 2)")).Value);

            Assert.True(((LispBoolean)Eval("(> 3 2 1)")).Value);
            Assert.False(((LispBoolean)Eval("(> 3 3 1)")).Value);

            Assert.True(((LispBoolean)Eval("(<= 1 2 2)")).Value);
            Assert.False(((LispBoolean)Eval("(<= 2 1)")).Value);

            Assert.True(((LispBoolean)Eval("(>= 3 3 1)")).Value);
            Assert.False(((LispBoolean)Eval("(>= 1 2)")).Value);
        }

        [Fact]
        public void And_ShortCircuits_Does_Not_Evaluate_After_Nil()
        {
            // If and didn't short-circuit, this would throw an error
            // because undefined-symbol would be evaluated
            var r = Eval("(and nil undefined-symbol)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Or_ShortCircuits_Does_Not_Evaluate_After_Truthy()
        {
            // If or didn't short-circuit, this would throw an error
            // because undefined-symbol would be evaluated
            var r = Eval("(or 5 undefined-symbol)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void And_ShortCircuits_On_First_Nil()
        {
            // Define a variable to track if second expression was evaluated
            Eval("(define side-effect-happened nil)");
            var r = Eval("(and nil (set! side-effect-happened t))");
            
            Assert.IsType<LispNil>(r);
            // Verify side effect didn't happen (short-circuited before set!)
            var sideEffect = Eval("side-effect-happened");
            Assert.IsType<LispNil>(sideEffect);
        }

        [Fact]
        public void And_ShortCircuits_On_Second_Nil()
        {
            Eval("(define side-effect-happened nil)");
            var r = Eval("(and t nil (set! side-effect-happened t))");
            
            Assert.IsType<LispNil>(r);
            // Verify third expression wasn't evaluated
            var sideEffect = Eval("side-effect-happened");
            Assert.IsType<LispNil>(sideEffect);
        }

        [Fact]
        public void And_Evaluates_All_When_All_Truthy()
        {
            Eval("(define counter 0)");
            Eval("(and (set! counter (+ counter 1)) (set! counter (+ counter 1)) (set! counter (+ counter 1)))");
            
            var counter = Eval("counter");
            Assert.IsType<LispInteger>(counter);
            Assert.Equal(3, (int)((LispInteger)counter).Value);
        }

        [Fact]
        public void Or_ShortCircuits_On_First_Truthy()
        {
            Eval("(define side-effect-happened nil)");
            var r = Eval("(or 5 (set! side-effect-happened t))");
            
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
            // Verify side effect didn't happen
            var sideEffect = Eval("side-effect-happened");
            Assert.IsType<LispNil>(sideEffect);
        }

        [Fact]
        public void Or_ShortCircuits_On_Second_Truthy()
        {
            Eval("(define side-effect-happened nil)");
            var r = Eval("(or nil 5 (set! side-effect-happened t))");
            
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
            // Verify third expression wasn't evaluated
            var sideEffect = Eval("side-effect-happened");
            Assert.IsType<LispNil>(sideEffect);
        }

        [Fact]
        public void Or_Evaluates_All_When_All_Nil()
        {
            // To test that or evaluates all expressions when they're all nil,
            // we need expressions that have side effects but return nil
            Eval("(define counter 0)");
            Eval("(define increment-and-return-nil (lambda () (set! counter (+ counter 1)) nil))");
            
            var result = Eval("(or (increment-and-return-nil) (increment-and-return-nil) (increment-and-return-nil))");
            
            // or should return nil (no truthy value found)
            Assert.IsType<LispNil>(result);
            
            // All three lambdas should have been called, incrementing counter to 3
            var counter = Eval("counter");
            Assert.IsType<LispInteger>(counter);
            Assert.Equal(3, (int)((LispInteger)counter).Value);
        }

        [Fact]
        public void And_Nested_With_Or_ShortCircuits_Properly()
        {
            // (and t (or nil 5)) should return 5 without evaluating anything after
            var r = Eval("(and t (or nil 5 undefined-symbol))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Or_Nested_With_And_ShortCircuits_Properly()
        {
            // (or nil (and t 5)) should return 5
            var r = Eval("(or nil (and t 5))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void And_With_List_Check_ShortCircuits()
        {
            // This is the actual use case from SimplifyExpression.al
            // If first-op returns a non-list, should not call operator on it
            Eval("(define first-op (lambda (x) x))");
            Eval("(define operator (lambda (x) (car x)))");
            
            // This should not throw because 'and' short-circuits when (list? 5) is false
            var r = Eval("(and (list? (first-op 5)) (= (operator (first-op 5)) 'exp))");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void And_Returns_Last_Value_When_All_True()
        {
            var r = Eval("(and 1 2 \"hello\")");
            Assert.IsType<LispString>(r);
            Assert.Equal("hello", ((LispString)r).Value);
        }

        [Fact]
        public void Or_Returns_First_Truthy_Value()
        {
            var r = Eval("(or nil nil \"first\" 42)");
            Assert.IsType<LispString>(r);
            Assert.Equal("first", ((LispString)r).Value);
        }

        [Fact]
        public void And_With_Zero_Is_Truthy()
        {
            // In Lisp, 0 is truthy (only nil and false are falsy)
            var r = Eval("(and 0 5)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(5, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Or_With_Zero_Returns_Zero()
        {
            // 0 is truthy, so or should return it
            var r = Eval("(or nil 0)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(0, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void And_Single_Arg_Returns_That_Arg()
        {
            var r = Eval("(and 42)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(42, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Or_Single_Arg_Returns_That_Arg()
        {
            var r = Eval("(or 42)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(42, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void And_Single_Nil_Returns_Nil()
        {
            var r = Eval("(and nil)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Or_Single_Nil_Returns_Nil()
        {
            var r = Eval("(or nil)");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Complex_Nested_And_Or_Expression()
        {
            // ((and (or nil t) (or t nil)) should evaluate to t
            var r = Eval("(and (or nil t) (or t nil))");
            Assert.IsType<LispBoolean>(r);
            Assert.True(((LispBoolean)r).Value);
        }

        [Fact]
        public void And_Or_With_If_Expressions()
        {
            // (and (if t 5 0) (if nil 0 10)) should return 10
            var r = Eval("(and (if t 5 0) (if nil 0 10))");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(10, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void ShortCircuit_Prevents_Division_By_Zero()
        {
            // This should not throw because and short-circuits
            var r = Eval("(and nil (/ 1 0))");
            Assert.IsType<LispNil>(r);
        }

        [Fact]
        public void Or_ShortCircuit_Prevents_Division_By_Zero()
        {
            // This should not throw because or short-circuits
            var r = Eval("(or t (/ 1 0))");
            Assert.IsType<LispBoolean>(r);
            Assert.True(((LispBoolean)r).Value);
        }
    }
}
