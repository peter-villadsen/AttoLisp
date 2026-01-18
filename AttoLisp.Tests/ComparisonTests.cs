namespace AttoLisp.Tests;

public class ComparisonTests
{
    private readonly Evaluator _evaluator;

    public ComparisonTests()
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
    public void Equality_SameNumbers_ReturnsTrue()
    {
        var result = Eval("(= 5 5)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }

    [Fact]
    public void Equality_DifferentNumbers_ReturnsFalse()
    {
        var result = Eval("(= 5 6)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.False(((LispBoolean)result).Value);
    }

    [Fact]
    public void LessThan_TrueCondition_ReturnsTrue()
    {
        var result = Eval("(< 3 5)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }

    [Fact]
    public void LessThan_FalseCondition_ReturnsFalse()
    {
        var result = Eval("(< 5 3)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.False(((LispBoolean)result).Value);
    }

    [Fact]
    public void GreaterThan_TrueCondition_ReturnsTrue()
    {
        var result = Eval("(> 10 5)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }

    [Fact]
    public void GreaterThan_FalseCondition_ReturnsFalse()
    {
        var result = Eval("(> 5 10)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.False(((LispBoolean)result).Value);
    }

    [Fact]
    public void Equality_MultipleValues_AllSame_ReturnsTrue()
    {
        var result = Eval("(= 5 5 5 5)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }

    [Fact]
    public void LessThan_ChainedComparison_ReturnsTrue()
    {
        var result = Eval("(< 1 2 3 4)");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }
}
