using Xunit;

namespace AttoLisp.Tests
{
    public class StdLibTests : EvaluatorTestsBase
    {
        [Fact]
        public void StdLib_Functions_Are_Available()
        {
            var r = Eval("(+ 1 2)");
            Assert.IsType<LispInteger>(r);
            Assert.Equal(3, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Abs_Works()
        {
            var r1 = Eval("(abs -5)");
            var r2 = Eval("(abs 7)");
            Assert.Equal(5, (int)((LispInteger)r1).Value);
            Assert.Equal(7, (int)((LispInteger)r2).Value);
        }

        [Fact]
        public void Length_Works_ForLists()
        {
            var r = Eval("(length (list 1 2 3 4))");
            Assert.Equal(4, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Map_Works()
        {
            var r = Eval("(map (lambda (x) (* x 2)) (list 1 2 3))");
            var list = (LispList)r;
            Assert.Equal(3, list.Elements.Count);
            Assert.Equal(2, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(4, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(6, (int)((LispInteger)list.Elements[2]).Value);
        }

        [Fact]
        public void Filter_Works()
        {
            var r = Eval("(filter (lambda (x) (> x 1)) (list 1 2 3))");
            var list = (LispList)r;
            Assert.Equal(2, list.Elements.Count);
            Assert.Equal(2, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(3, (int)((LispInteger)list.Elements[1]).Value);
        }

        [Fact]
        public void Reduce_Works()
        {
            var r = Eval("(reduce (lambda (acc x) (+ acc x)) 0 (list 1 2 3))");
            Assert.Equal(6, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Range_Works()
        {
            var r = Eval("(range 3 6)");
            var list = (LispList)r;
            Assert.Equal(4, list.Elements.Count);
            Assert.Equal(3, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(4, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(5, (int)((LispInteger)list.Elements[2]).Value);
            Assert.Equal(6, (int)((LispInteger)list.Elements[3]).Value);
        }

        [Fact]
        public void Compose_Works()
        {
            Eval("(define inc (lambda (x) (+ x 1)))");
            Eval("(define double (lambda (x) (* x 2)))");
            Eval("(define h (compose inc double))");
            var r = Eval("(h 5)");
            Assert.Equal(11, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void EmptyQ_Works()
        {
            var r1 = Eval("(empty? (list))");
            var r2 = Eval("(empty? (list 1))");
            Assert.True(IsTruthy(r1));
            Assert.False(IsTruthy(r2));
        }

        [Fact]
        public void Reverse_Works()
        {
            var r = Eval("(reverse (list 1 2 3))");
            var list = (LispList)r;
            Assert.Equal(3, list.Elements.Count);
            Assert.Equal(3, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(2, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(1, (int)((LispInteger)list.Elements[2]).Value);
        }

        [Fact]
        public void Merge_Works()
        {
            var r = Eval("(merge (list 1 2) (list 3 4))");
            var list = (LispList)r;
            Assert.Equal(4, list.Elements.Count);
            Assert.Equal(1, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(2, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(3, (int)((LispInteger)list.Elements[2]).Value);
            Assert.Equal(4, (int)((LispInteger)list.Elements[3]).Value);
        }

        [Fact]
        public void First_Works()
        {
            var r = Eval("(first (list 10 20 30))");
            Assert.Equal(10, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void Tail_Works()
        {
            var r = Eval("(tail (list 10 20 30))");
            var list = (LispList)r;
            Assert.Equal(2, list.Elements.Count);
            Assert.Equal(20, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(30, (int)((LispInteger)list.Elements[1]).Value);
        }

        [Fact]
        public void Head_Works()
        {
            var r = Eval("(head (list 7 8 9))");
            Assert.Equal(7, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void String_Substr_Works()
        {
            var r = Eval("(substr \"hello\" 1 3)");
            Assert.Equal("ell", ((LispString)r).Value);
        }

        [Fact]
        public void String_IndexOf_Works()
        {
            var r = Eval("(index-of \"hello world\" \"world\")");
            Assert.Equal(6, (int)((LispInteger)r).Value);
        }

        [Fact]
        public void String_ToLower_Works()
        {
            var r = Eval("(to-lower \"HeLLo\")");
            Assert.Equal("hello", ((LispString)r).Value);
        }

        [Fact]
        public void Xor_TrueFalse_IsTrue()
        {
            var result = Eval("(xor t nil)");
            Assert.True(((LispBoolean)result).Value);
        }

        [Fact]
        public void Xor_TrueTrue_IsFalse()
        {
            var result = Eval("(xor t t)");
            Assert.False(((LispBoolean)result).Value);
        }

        [Fact]
        public void Xor_FalseFalse_IsFalse()
        {
            var result = Eval("(xor nil nil)");
            Assert.False(((LispBoolean)result).Value);
        }

        [Fact]
        public void Sqrt_Works()
        {
            var r = Eval("(sqrt 9)");
            Assert.IsType<LispDecimal>(r);
            Assert.Equal(3m, ((LispDecimal)r).Value);
        }

        [Fact]
        public void Exp_And_Log_Work()
        {
            // exp(0) = 1
            var e0 = Eval("(exp 0)");
            Assert.IsType<LispDecimal>(e0);
            Assert.Equal(1m, ((LispDecimal)e0).Value);

            // log(e^1) ~= 1 using base-e and base-10 variants not strictly tested here,
            // but we verify log and exp are at least callable.
            var e1 = Eval("(exp 1)");
            Assert.IsType<LispDecimal>(e1);

            var l10 = Eval("(log 10)");
            Assert.IsType<LispDecimal>(l10);
        }

        [Fact]
        public void Trig_Functions_Work()
        {
            var s0 = Eval("(sin 0)");
            Assert.IsType<LispDecimal>(s0);
            Assert.Equal(0m, ((LispDecimal)s0).Value);

            var c0 = Eval("(cos 0)");
            Assert.IsType<LispDecimal>(c0);
            Assert.Equal(1m, ((LispDecimal)c0).Value);

            var t0 = Eval("(tan 0)");
            Assert.IsType<LispDecimal>(t0);
            Assert.Equal(0m, ((LispDecimal)t0).Value);
        }

        [Fact]
        public void Min_Max_Work()
        {
            var mn = Eval("(min 3 1 4 2)");
            Assert.IsType<LispInteger>(mn);
            Assert.Equal(1, (int)((LispInteger)mn).Value);

            var mx = Eval("(max 3 1 4 2)");
            Assert.IsType<LispInteger>(mx);
            Assert.Equal(4, (int)((LispInteger)mx).Value);
        }
    }
}
