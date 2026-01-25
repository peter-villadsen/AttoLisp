using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AttoLisp.Tests
{
    public abstract class EvaluatorTestsBase
    {
        protected readonly Evaluator Evaluator;

        protected EvaluatorTestsBase()
        {
            Evaluator = new Evaluator();

            var stdlibPath = Path.Combine(AppContext.BaseDirectory, "stdlib.al");
            if (File.Exists(stdlibPath))
            {
                var exprs = TestHelper.ParseFile(stdlibPath);
                foreach (var expr in exprs)
                {
                    Evaluator.Eval(expr);
                }
            }
        }

        protected LispValue Eval(string expression)
        {
            var exprs = TestHelper.ParseSource(expression);
            return Evaluator.Eval(exprs.Single());
        }

        protected bool IsTruthy(LispValue value)
        {
            if (value is LispBoolean b)
                return b.Value;
            return value is not LispNil; // only nil is falsey by convention
        }
    }
}
