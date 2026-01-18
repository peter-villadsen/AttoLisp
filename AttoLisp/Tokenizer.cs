namespace AttoLisp
{
    public enum TokenType
    {
        LeftParen,
        RightParen,
        Number,
        String,
        Symbol,
        Date,
        Quote,
        EOF
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string value, int position, int line, int column)
        {
            Type = type;
            Value = value;
            Position = position;
            Line = line;
            Column = column;
        }
    }

    public class Tokenizer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;

        public Tokenizer(string input)
        {
            _input = input;
            _position = 0;
            _line = 1;
            _column = 1;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                SkipWhitespace();

                if (_position >= _input.Length)
                    break;

                char current = _input[_position];

                if (current == '(')
                {
                    tokens.Add(new Token(TokenType.LeftParen, "(", _position, _line, _column));
                    _position++;
                    _column++;
                }
                else if (current == ')')
                {
                    tokens.Add(new Token(TokenType.RightParen, ")", _position, _line, _column));
                    _position++;
                    _column++;
                }
                else if (current == '\'')
                {
                    tokens.Add(new Token(TokenType.Quote, "'", _position, _line, _column));
                    _position++;
                    _column++;
                }
                else if (current == '"')
                {
                    tokens.Add(ReadString());
                }
                else if (current == '#' && _position + 1 < _input.Length && _input[_position + 1] == 'd')
                {
                    tokens.Add(ReadDate());
                }
                else if (current == ';')
                {
                    SkipComment();
                }
                else if (char.IsDigit(current) || (current == '-' && _position + 1 < _input.Length && char.IsDigit(_input[_position + 1])))
                {
                    tokens.Add(ReadNumber());
                }
                else
                {
                    tokens.Add(ReadSymbol());
                }
            }

            tokens.Add(new Token(TokenType.EOF, "", _position, _line, _column));
            return tokens;
        }

        private void SkipWhitespace()
        {
            while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
            {
                if (_input[_position] == '\n') { _line++; _column = 1; }
                else { _column++; }
                _position++;
            }
        }

        private void SkipComment()
        {
            while (_position < _input.Length && _input[_position] != '\n')
            {
                _position++;
            }
            if (_position < _input.Length && _input[_position] == '\n')
            {
                _position++;
                _line++;
                _column = 1;
            }
        }

        private Token ReadString()
        {
            int start = _position;
            int startCol = _column;
            _position++; // Skip opening quote
            _column++;

            var value = new System.Text.StringBuilder();

            while (_position < _input.Length && _input[_position] != '"')
            {
                if (_input[_position] == '\\' && _position + 1 < _input.Length)
                {
                    _position++;
                    _column++;
                    switch (_input[_position])
                    {
                        case 'n': value.Append('\n'); break;
                        case 't': value.Append('\t'); break;
                        case 'r': value.Append('\r'); break;
                        case '\\': value.Append('\\'); break;
                        case '"': value.Append('"'); break;
                        default: value.Append(_input[_position]); break;
                    }
                    _position++;
                    _column++;
                }
                else
                {
                    value.Append(_input[_position]);
                    _position++;
                    _column++;
                }
            }

            if (_position < _input.Length)
            {
                _position++; // Skip closing quote
                _column++;
            }

            return new Token(TokenType.String, value.ToString(), start, _line, startCol);
        }

        private Token ReadDate()
        {
            int start = _position;
            int startCol = _column;
            _position += 2; // Skip #d
            _column += 2;

            if (_position < _input.Length && _input[_position] == '"')
            {
                _position++; // Skip opening quote
                _column++;
                var value = new System.Text.StringBuilder();

                while (_position < _input.Length && _input[_position] != '"')
                {
                    value.Append(_input[_position]);
                    _position++;
                    _column++;
                }

                if (_position < _input.Length)
            {
                _position++; // Skip closing quote
                _column++;
            }

                return new Token(TokenType.Date, value.ToString(), start, _line, startCol);
            }

            throw new Exception($"Invalid date format at position {start}");
        }

        private Token ReadNumber()
        {
            int start = _position;
            int startCol = _column;
            var value = new System.Text.StringBuilder();

            // Consume optional leading minus
            if (_position < _input.Length && _input[_position] == '-')
            {
                value.Append(_input[_position]);
                _position++;
                _column++;
            }

            while (_position < _input.Length && (char.IsDigit(_input[_position]) || _input[_position] == '.'))
            {
                value.Append(_input[_position]);
                _position++;
                _column++;
            }

            return new Token(TokenType.Number, value.ToString(), start, _line, startCol);
        }

        private Token ReadSymbol()
        {
            int start = _position;
            int startCol = _column;
            var value = new System.Text.StringBuilder();

            while (_position < _input.Length &&
                   !char.IsWhiteSpace(_input[_position]) &&
                   _input[_position] != '(' &&
                   _input[_position] != ')' &&
                   _input[_position] != ';')
            {
                value.Append(_input[_position]);
                _position++;
                _column++;
            }

            return new Token(TokenType.Symbol, value.ToString(), start, _line, startCol);
        }
    }
}
