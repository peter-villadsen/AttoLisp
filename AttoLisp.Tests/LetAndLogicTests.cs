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
    }
}
