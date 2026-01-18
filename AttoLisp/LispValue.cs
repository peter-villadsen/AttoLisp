using System.Numerics;

namespace AttoLisp
{
    /// <summary>
    /// Represents a value in the Lisp interpreter
    /// </summary>
    public abstract class LispValue
    {
        public abstract override string ToString();
    }

    public class LispInteger : LispValue
    {
        public BigInteger Value { get; }

        public LispInteger(BigInteger value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }

    public class LispDecimal : LispValue
    {
        public decimal Value { get; }

        public LispDecimal(decimal value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public class LispString : LispValue
    {
        public string Value { get; }

        public LispString(string value)
        {
            Value = value;
        }

        public override string ToString() => $"\"{Value}\"";
    }

    public class LispDate : LispValue
    {
        public DateTime Value { get; }

        public LispDate(DateTime value)
        {
            Value = value;
        }

        public override string ToString() => $"#date\"{Value:yyyy-MM-dd HH:mm:ss}\"";
    }

    public class LispSymbol : LispValue
    {
        public string Name { get; }

        public LispSymbol(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }

    public class LispList : LispValue
    {
        public List<LispValue> Elements { get; }

        public LispList(List<LispValue> elements)
        {
            Elements = elements;
        }

        public override string ToString()
        {
            return $"({string.Join(" ", Elements.Select(e => e.ToString()))})";
        }
    }

    public class LispNil : LispValue
    {
        public static readonly LispNil Instance = new LispNil();

        private LispNil() { }

        public override string ToString() => "nil";
    }

    public class LispBoolean : LispValue
    {
        public bool Value { get; }

        public static readonly LispBoolean True = new LispBoolean(true);
        public static readonly LispBoolean False = new LispBoolean(false);

        private LispBoolean(bool value)
        {
            Value = value;
        }

        public override string ToString() => Value ? "t" : "nil";
    }

    public class LispFunction : LispValue
    {
        public Func<List<LispValue>, LispValue> Implementation { get; }
        public string Name { get; }

        public LispFunction(string name, Func<List<LispValue>, LispValue> implementation)
        {
            Name = name;
            Implementation = implementation;
        }

        public override string ToString() => $"#<function:{Name}>";
    }
}
