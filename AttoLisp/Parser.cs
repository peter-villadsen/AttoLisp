namespace AttoLisp
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        public LispValue Parse()
        {
            return ParseExpression();
        }

        public List<LispValue> ParseAll()
        {
            var expressions = new List<LispValue>();
            while (true)
            {
                var expr = ParseExpression();
                if (expr is LispNil && CurrentToken.Type == TokenType.EOF)
                {
                    break;
                }
                expressions.Add(expr);
                if (CurrentToken.Type == TokenType.EOF)
                {
                    break;
                }
            }
            return expressions;
        }

        private Token CurrentToken => _tokens[_position];

        private void Advance()
        {
            if (_position < _tokens.Count - 1)
                _position++;
        }

        private LispValue ParseExpression()
        {
            Token token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.Number:
                    Advance();
                    if (token.Value.Contains('.'))
                    {
                        if (decimal.TryParse(token.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var dec))
                            return new LispDecimal(dec);
                        throw new Exception($"Invalid decimal number: {token.Value}");
                    }
                    else
                    {
                        try
                        {
                            var bi = System.Numerics.BigInteger.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture);
                            return new LispInteger(bi);
                        }
                        catch
                        {
                            throw new Exception($"Invalid integer number: {token.Value}");
                        }
                    }

                case TokenType.String:
                    Advance();
                    return new LispString(token.Value);

                case TokenType.Date:
                    Advance();
                    if (DateTime.TryParse(token.Value, out DateTime date))
                    {
                        return new LispDate(date);
                    }
                    throw new Exception($"Invalid date format: {token.Value}");

                case TokenType.Symbol:
                    Advance();
                    if (token.Value.ToLower() == "nil")
                        return LispNil.Instance;
                    if (token.Value.ToLower() == "t")
                        return LispBoolean.True;
                    return new LispSymbol(token.Value);

                case TokenType.LeftParen:
                    return ParseList();

                case TokenType.Quote:
                    Advance();
                    // 'expr => (quote expr)
                    var quoted = ParseExpression();
                    return new LispList(new List<LispValue>
                    {
                        new LispSymbol("quote"),
                        quoted
                    });

                case TokenType.EOF:
                    return LispNil.Instance;

                default:
                    throw new Exception($"Unexpected token: {token.Type} at position {token.Position}");
            }
        }

        private LispValue ParseList()
        {
            Advance(); // Skip '('
            var elements = new List<LispValue>();

            while (CurrentToken.Type != TokenType.RightParen && CurrentToken.Type != TokenType.EOF)
            {
                elements.Add(ParseExpression());
            }

            if (CurrentToken.Type != TokenType.RightParen)
                throw new Exception($"Expected ')' at end of list (line {CurrentToken.Line}, col {CurrentToken.Column})");

            Advance(); // Skip ')'
            return new LispList(elements);
        }
    }
}
