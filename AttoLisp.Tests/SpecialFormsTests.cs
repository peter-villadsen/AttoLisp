using Xunit;

namespace AttoLisp.Tests
{
    public class SpecialFormsTests : EvaluatorTestsBase
    {
        [Fact]
        public void Quote_ReturnsUnevaluatedList()
        {
            var result = Eval("(quote (1 2 3))");
            
            Assert.IsType<LispList>(result);
            var list = (LispList)result;
            Assert.Equal(3, list.Elements.Count);
            Assert.IsType<LispInteger>(list.Elements[0]);
            Assert.Equal(1, (int)((LispInteger)list.Elements[0]).Value);
        }

        [Fact]
        public void Quote_DoesNotEvaluateSymbols()
        {
            var result = Eval("(quote x)");
            
            Assert.IsType<LispSymbol>(result);
            Assert.Equal("x", ((LispSymbol)result).Name);
        }

        [Fact]
        public void Quote_PreservesNestedLists()
        {
            var result = Eval("(quote (+ 1 2))");
            
            Assert.IsType<LispList>(result);
            var list = (LispList)result;
            Assert.Equal(3, list.Elements.Count);
            Assert.IsType<LispSymbol>(list.Elements[0]);
            Assert.Equal("+", ((LispSymbol)list.Elements[0]).Name);
        }

        [Fact]
        public void If_WithoutElse_ReturnsThenOrNil()
        {
            var result = Eval("(if (> 10 5) \"yes\")");
            
            Assert.IsType<LispString>(result);
            Assert.Equal("yes", ((LispString)result).Value);
        }

        [Fact]
        public void If_FalseConditionWithoutElse_ReturnsNil()
        {
            var result = Eval("(if (< 10 5) \"yes\")");
            
            Assert.IsType<LispNil>(result);
        }

        [Fact]
        public void Lambda_MultipleExpressions_ReturnsLastValue()
        {
            var result = Eval("((lambda (x) (+ x 1) (* x 2)) 5)");
            
            Assert.IsType<LispInteger>(result);
            Assert.Equal(10, (int)((LispInteger)result).Value);
        }

        [Fact]
        public void QuoteSugar_List_ReturnsUnevaluatedList()
        {
            var result = Eval("'(1 2 3)");
            
            Assert.IsType<LispList>(result);
            var list = (LispList)result;
            Assert.Equal(3, list.Elements.Count);
            Assert.Equal(1, (int)((LispInteger)list.Elements[0]).Value);
            Assert.Equal(2, (int)((LispInteger)list.Elements[1]).Value);
            Assert.Equal(3, (int)((LispInteger)list.Elements[2]).Value);
        }

        [Fact]
        public void QuoteSugar_Symbol_ReturnsSymbol()
        {
            var result = Eval("'x");
            
            Assert.IsType<LispSymbol>(result);
            Assert.Equal("x", ((LispSymbol)result).Name);
        }
    }
}
