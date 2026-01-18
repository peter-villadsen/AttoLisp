namespace AttoLisp.Tests;

public class StringTests
{
    private readonly Evaluator _evaluator;

    public StringTests()
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
    public void StringLiteral_ReturnsString()
    {
        var result = Eval("\"hello\"");
        
        Assert.IsType<LispString>(result);
        Assert.Equal("hello", ((LispString)result).Value);
    }

    [Fact]
    public void Concat_MultipleStrings_ReturnsConcatenated()
    {
        var result = Eval("(concat \"hello\" \" \" \"world\")");
        
        Assert.IsType<LispString>(result);
        Assert.Equal("hello world", ((LispString)result).Value);
    }

    [Fact]
    public void StrLength_ReturnsCorrectLength()
    {
        var result = Eval("(str-length \"hello\")");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(5, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void StrLength_EmptyString_ReturnsZero()
    {
        var result = Eval("(str-length \"\")");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(0, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Concat_EmptyArgs_ReturnsEmptyString()
    {
        var result = Eval("(concat)");
        
        Assert.IsType<LispString>(result);
        Assert.Equal("", ((LispString)result).Value);
    }

    [Fact]
    public void StringComparison_LessThan_ReturnsTrue()
    {
        var result = Eval("(< \"apple\" \"banana\")");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }

    [Fact]
    public void StringComparison_Equality_ReturnsTrue()
    {
        var result = Eval("(= \"test\" \"test\")");
        
        Assert.IsType<LispBoolean>(result);
        Assert.True(((LispBoolean)result).Value);
    }
}
