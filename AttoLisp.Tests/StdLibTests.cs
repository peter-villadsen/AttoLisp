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
            Assert.True(((LispBoolean)r1).Value);
            Assert.False(((LispBoolean)r2).Value);
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
    }
}
