# AttoLisp - A Small Lisp Interpreter

A minimal Lisp interpreter implementation in C# with support for arithmetic, strings, dates, and basic functional programming constructs.

## Architecture

### Components

1. **LispValue.cs** - Core data types
   - `LispNumber` - Numeric values (double precision)
   - `LispString` - String values
   - `LispDate` - DateTime values
   - `LispSymbol` - Variable/function names
   - `LispList` - List structures
   - `LispBoolean` - Boolean values (t/nil)
   - `LispNil` - Null/empty value
   - `LispFunction` - Function values

2. **Tokenizer.cs** - Lexical analysis
   - Breaks input into tokens
   - Handles parentheses, numbers, strings, symbols, and dates
   - Supports comments (;) and escape sequences

3. **Parser.cs** - Syntax analysis
   - Converts tokens into AST
   - Creates LispValue objects

4. **Evaluator.cs** - Interpretation engine
   - Evaluates expressions
   - Manages environment/scope
   - Implements built-in functions and special forms

5. **Program.cs** - REPL
   - Read-Eval-Print Loop
   - User interaction

## Built-in Functions

### Arithmetic
- `(+ args...)` - Addition
- `(- args...)` - Subtraction
- `(* args...)` - Multiplication
- `(/ args...)` - Division

### Comparison
- `(= a b ...)` - Equality
- `(< a b ...)` - Less than
- `(> a b ...)` - Greater than

### Strings
- `(concat args...)` - Concatenate strings
- `(str-length str)` - Get string length

### Dates
- `(now)` - Get current date/time
- `#d"YYYY-MM-DD"` - Date literal
- `(date-year date)` - Extract year
- `(date-month date)` - Extract month
- `(date-day date)` - Extract day

### Lists
- `(list args...)` - Create list
- `(car list)` - First element
- `(cdr list)` - Rest of list
- `(cons item list)` - Prepend item

### Special Forms
- `(quote expr)` - Return unevaluated expression
- `(if cond then [else])` - Conditional
- `(define name value)` - Define variable
- `(set! name value)` - Update variable
- `(lambda (params...) body...)` - Create function

### I/O
- `(print args...)` - Print to console

## Examples

```lisp
; Basic arithmetic
(+ 1 2 3)  ; => 6
(* 2 (+ 3 4))  ; => 14

; String operations
(concat "Hello" " " "World")  ; => "Hello World"

; Date operations
(date-year (now))  ; => current year
(date-month #d"2024-12-25")  ; => 12

; Variables and functions
(define x 10)
(define square (lambda (x) (* x x)))
(square 5)  ; => 25

; Conditionals
(if (> 10 5) "big" "small")  ; => "big"

; Lists
(define nums (list 1 2 3 4 5))
(car nums)  ; => 1
(cdr nums)  ; => (2 3 4 5)
```

## Running the REPL

```bash
cd AttoLisp
dotnet run
```

Type `help` for more examples or `exit`/`quit` to exit.

## Future Enhancements (for compiler version)

- Tail call optimization
- More data types (vectors, hash maps)
- Module system
- Macro support
- Bytecode compilation
- Native code generation
