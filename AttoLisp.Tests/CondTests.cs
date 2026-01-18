using Xunit;

namespace AttoLisp.Tests
{
    public class CondTests
    {
        private readonly Evaluator _evaluator;

        public CondTests()
        {
            _evaluator = new Evaluator();
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
