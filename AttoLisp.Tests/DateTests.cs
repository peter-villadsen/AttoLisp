namespace AttoLisp.Tests;

public class DateTests
{
    private readonly Evaluator _evaluator;

    public DateTests()
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
    public void Now_ReturnsCurrentDate()
    {
        var result = Eval("(now)");
        
        Assert.IsType<LispDate>(result);
        var date = ((LispDate)result).Value;
        Assert.True((DateTime.Now - date).TotalSeconds < 1);
    }

    [Fact]
    public void DateLiteral_ParsesCorrectly()
    {
        var result = Eval("#d\"2024-01-15\"");
        
        Assert.IsType<LispDate>(result);
        var date = ((LispDate)result).Value;
        Assert.Equal(2024, date.Year);
        Assert.Equal(1, date.Month);
        Assert.Equal(15, date.Day);
    }

    [Fact]
    public void DateYear_ReturnsCurrentYear()
    {
        var result = Eval("(date-year (now))");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(DateTime.Now.Year, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void DateMonth_ReturnsCurrentMonth()
    {
        var result = Eval("(date-month (now))");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(DateTime.Now.Month, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void DateDay_ReturnsCurrentDay()
    {
        var result = Eval("(date-day (now))");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(DateTime.Now.Day, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void DateMonth_FromLiteral_ReturnsCorrectMonth()
    {
        var result = Eval("(date-month #d\"2024-12-25\")");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(12, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void DateComparison_LessThan_ReturnsTrue()
    {
        var result = Eval("(< #d\"2024-01-01\" #d\"2024-12-31\")");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }

    [Fact]
    public void DateComparison_Equality_ReturnsTrue()
    {
        var result = Eval("(= #d\"2024-01-15\" #d\"2024-01-15\")");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }
}
