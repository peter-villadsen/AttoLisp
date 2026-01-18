namespace AttoLisp.Tests;

public class ArithmeticTests
{
    private readonly Evaluator _evaluator;

    public ArithmeticTests()
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
    public void Addition_MultipleNumbers_ReturnsSum()
    {
        var result = Eval("(+ 1 2 3)");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(6, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Subtraction_TwoNumbers_ReturnsDifference()
    {
        var result = Eval("(- 10 3)");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(7, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Multiplication_TwoNumbers_ReturnsProduct()
    {
        var result = Eval("(* 4 5)");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(20, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Division_TwoNumbers_ReturnsQuotient()
    {
        var result = Eval("(/ 20 4)");
        
        Assert.IsType<LispDecimal>(result);
        Assert.Equal(5m, ((LispDecimal)result).Value);
    }

    [Fact]
    public void NestedArithmetic_ReturnsCorrectResult()
    {
        var result = Eval("(* 2 (+ 3 4))");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(14, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Addition_EmptyArgs_ReturnsZero()
    {
        var result = Eval("(+)");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(0, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Multiplication_EmptyArgs_ReturnsOne()
    {
        var result = Eval("(*)");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(1, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Subtraction_SingleNumber_ReturnsNegation()
    {
        var result = Eval("(- 5)");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(-5, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Division_SingleNumber_ReturnsReciprocal()
    {
        var result = Eval("(/ 4)");
        
        Assert.IsType<LispDecimal>(result);
        Assert.Equal(0.25m, ((LispDecimal)result).Value);
    }

    [Fact]
    public void BigInteger_Addition_VeryLarge_ReturnsInteger()
    {
        // 10^50 + 1
        var result = Eval("(+ 100000000000000000000000000000000000000000000000000 1)");
        Assert.IsType<LispInteger>(result);
        var bi = ((LispInteger)result).Value;
        Assert.Equal(System.Numerics.BigInteger.Parse("100000000000000000000000000000000000000000000000001"), bi);
    }

    [Fact]
    public void BigInteger_Multiplication_VeryLarge_ReturnsInteger()
    {
        // (10^25) * (10^25) = 10^50
        var result = Eval("(* 10000000000000000000000000 10000000000000000000000000)");
        Assert.IsType<LispInteger>(result);
        var bi = ((LispInteger)result).Value;
        Assert.Equal(System.Numerics.BigInteger.Pow(10, 50), bi);
    }

    [Fact]
    public void BigInteger_Subtraction_VeryLargeNegative_ReturnsInteger()
    {
        var result = Eval("(- 0 1000000000000000000000000000000000000000000000000)");
        Assert.IsType<LispInteger>(result);
        var bi = ((LispInteger)result).Value;
        Assert.Equal(System.Numerics.BigInteger.Parse("-1000000000000000000000000000000000000000000000000"), bi);
    }

    [Fact]
    public void Mixed_IntegerAndDecimal_PromotesToDecimal_WithinRange()
    {
        // Use an integer within decimal range to avoid overflow during promotion
        var result = Eval("(+ 10000000000000000000000000 1.5)");
        Assert.IsType<LispDecimal>(result);
        Assert.Equal(10000000000000000000000001.5m, ((LispDecimal)result).Value);
    }
}
