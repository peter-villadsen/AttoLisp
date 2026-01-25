using Xunit;

namespace AttoLisp.Tests
{
    public class CondTests : EvaluatorTestsBase
    {
        [Fact]
        public void Cond_SelectsFirstTrueClause()
        {
            var r = Eval("(cond ((> 2 3) 10) ((< 1 2) 20) (else 30))");
            Assert.Equal(20, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Cond_ElseClauseUsedWhenNoMatch()
        {
            var r = Eval("(cond ((> 2 3) 10) ((= 1 2) 20) (else 30))");
            Assert.Equal(30, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Cond_ReturnsTestValueIfNoExpressions()
        {
            var r = Eval("(cond ((= 1 1)))");
            Assert.True(((LispBoolean)r).Value);
        }

        [Fact]
        public void Cond_ReturnsNilIfNoClauseMatches()
        {
            var r = Eval("(cond ((= 1 2)) ((> 5 6)))");
            Assert.IsType<LispNil>(r);
        }
    }
}
