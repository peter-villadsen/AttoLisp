namespace AttoLisp.Tests;

public class ListTests
{
    private readonly Evaluator _evaluator;

    public ListTests()
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
    public void List_CreatesListWithElements()
    {
        var result = Eval("(list 1 2 3)");
        
        Assert.IsType<LispList>(result);
        var list = (LispList)result;
        Assert.Equal(3, list.Elements.Count);
        Assert.Equal(1, (int)((LispInteger)list.Elements[0]).Value);
        Assert.Equal(2, (int)((LispInteger)list.Elements[1]).Value);
        Assert.Equal(3, (int)((LispInteger)list.Elements[2]).Value);
    }

    [Fact]
    public void Car_ReturnsFirstElement()
    {
        var result = Eval("(car (list 1 2 3))");
        
        Assert.IsType<LispInteger>(result);
        Assert.Equal(1, (int)((LispInteger)result).Value);
    }

    [Fact]
    public void Cdr_ReturnsRestOfList()
    {
        var result = Eval("(cdr (list 1 2 3))");
        
        Assert.IsType<LispList>(result);
        var list = (LispList)result;
        Assert.Equal(2, list.Elements.Count);
        Assert.Equal(2, (int)((LispInteger)list.Elements[0]).Value);
        Assert.Equal(3, (int)((LispInteger)list.Elements[1]).Value);
    }

    [Fact]
    public void Cons_PrependsElementToList()
    {
        var result = Eval("(cons 0 (list 1 2))");
        
        Assert.IsType<LispList>(result);
        var list = (LispList)result;
        Assert.Equal(3, list.Elements.Count);
        Assert.Equal(0, (int)((LispInteger)list.Elements[0]).Value);
        Assert.Equal(1, (int)((LispInteger)list.Elements[1]).Value);
        Assert.Equal(2, (int)((LispInteger)list.Elements[2]).Value);
    }

    [Fact]
    public void Car_EmptyList_ReturnsNil()
    {
        var result = Eval("(car (list))");
        
        Assert.IsType<LispNil>(result);
    }

    [Fact]
    public void Cdr_EmptyList_ReturnsNil()
    {
        var result = Eval("(cdr (list))");
        
        Assert.IsType<LispNil>(result);
    }

    [Fact]
    public void List_MixedTypes_CreatesHeterogeneousList()
    {
        var result = Eval("(list 1 \"hello\" #d\"2024-01-01\")");
        
        Assert.IsType<LispList>(result);
        var list = (LispList)result;
        Assert.Equal(3, list.Elements.Count);
        Assert.IsType<LispInteger>(list.Elements[0]);
        Assert.IsType<LispString>(list.Elements[1]);
        Assert.IsType<LispDate>(list.Elements[2]);
    }
}
