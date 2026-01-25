using System.Collections.Generic;
using System.IO;

namespace AttoLisp.Tests
{
    public static class TestHelper
    {
        public static List<LispValue> ParseFile(string path)
        {
            var source = File.ReadAllText(path);
            return ParseSource(source);
        }

        public static List<LispValue> ParseSource(string source)
        {
            var tokenizer = new Tokenizer(source);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens);
            return parser.ParseAll();
        }
    }
}
