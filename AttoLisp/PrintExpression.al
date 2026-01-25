; AttoLisp expression pretty-printing to LaTeX
; Exposes (print-expr expr) which converts a Lisp expression to LaTeX math notation

; Helper: get operator from a list expression
(define operator (expr)
  (if (list? expr)
      (car expr)
      nil))

; Helper: get first operand
(define first-operand (expr)
  (car (cdr expr)))

; Helper: get second operand
(define second-operand (expr)
  (car (cdr (cdr expr))))

; Helper: get all operands
(define operands (expr)
  (if (list? expr)
      (cdr expr)
      nil))

; Helper: join list of strings with separator
(define join-with (sep strings)
  (cond
    ((empty? strings) "")
    ((= (length strings) 1) (car strings))
    (else (concat (car strings) sep (join-with sep (cdr strings))))))

; Helper: map print-expr over a list
(define print-list (exprs)
  (if (empty? exprs)
      (list)
      (cons (print-expr (car exprs))
            (print-list (cdr exprs)))))

; Helper: operator precedence (higher = tighter binding)
(define precedence (op)
  (cond
    ((or (= op '+) (= op '-)) 1)
    ((or (= op '*) (= op '/)) 2)
    ((= op '^) 3)
    (else 0)))

; Helper: check if subexpression needs parentheses
(define needs-parens? (expr parent-op)
  (if (not (list? expr))
      nil
      (let ((expr-op (operator expr)))
        (and (not (= expr-op nil))
             (< (precedence expr-op) (precedence parent-op))))))

; Helper: print with conditional parentheses
(define print-with-parens (expr parent-op)
  (let ((printed (print-expr expr)))
    (if (needs-parens? expr parent-op)
        (concat "(" printed ")")
        printed)))

; Print addition: a + b + c
(define print-sum (expr)
  (let ((terms (print-list (operands expr))))
    (join-with " + " terms)))

; Print subtraction: handle unary and binary
(define print-difference (expr)
  (let ((ops (operands expr)))
    (cond
      ; Unary: (- x) -> "-x"
      ((= (length ops) 1)
       (concat "-" (print-with-parens (car ops) '-)))
      ; Binary: (- a b) -> "a - b"
      (else
       (concat (print-with-parens (first-operand expr) '-)
               " - "
               (print-with-parens (second-operand expr) '-))))))

; Print multiplication: smart formatting
(define print-product (expr)
  (let ((left (first-operand expr))
        (right (second-operand expr)))
    (cond
      ; Number × anything -> implicit multiplication "2x"
      ((number? left)
       (concat (print-expr left) (print-with-parens right '*)))
      ; Otherwise use \cdot for clarity
      (else
       (concat (print-with-parens left '*)
               " \\cdot "
               (print-with-parens right '*))))))

; Print division: use \frac
(define print-fraction (expr)
  (concat "\\frac{"
          (print-expr (first-operand expr))
          "}{"
          (print-expr (second-operand expr))
          "}"))

; Print power: superscript
(define print-power (expr)
  (concat (print-with-parens (first-operand expr) '^)
          "^{"
          (print-expr (second-operand expr))
          "}"))

; Print square root
(define print-sqrt (expr)
  (concat "\\sqrt{" (print-expr (first-operand expr)) "}"))

; Print exponential (e^x)
(define print-exp (expr)
  (concat "e^{" (print-expr (first-operand expr)) "}"))

; Print natural logarithm
(define print-log (expr)
  (concat "\\ln(" (print-expr (first-operand expr)) ")"))

; Print trigonometric function
(define print-trig (name expr)
  (concat "\\" name "(" (print-expr (first-operand expr)) ")"))

; Main print function
(define print-expr (expr)
  (cond
    ; Atoms: numbers and symbols
    ((not (list? expr))
     (concat "" expr))  ; concat auto-converts to string
    
    ; Arithmetic operators
    ((= (operator expr) '+) (print-sum expr))
    ((= (operator expr) '-) (print-difference expr))
    ((= (operator expr) '*) (print-product expr))
    ((= (operator expr) '/) (print-fraction expr))
    ((= (operator expr) '^) (print-power expr))
    
    ; Trigonometric functions
    ((= (operator expr) 'sin) (print-trig "sin" expr))
    ((= (operator expr) 'cos) (print-trig "cos" expr))
    ((= (operator expr) 'tan) (print-trig "tan" expr))
    
    ; Mathematical functions
    ((= (operator expr) 'sqrt) (print-sqrt expr))
    ((= (operator expr) 'exp) (print-exp expr))
    ((= (operator expr) 'log) (print-log expr))
    
    ; Unknown: generic function-style printing
    (else
     (concat (concat "" (operator expr))
             "("
             (join-with ", " (print-list (operands expr)))
             ")"))))

; Examples:
; (print-expr 'x)                => "x"
; (print-expr 42)                => "42"
; (print-expr '(+ x 2))          => "x + 2"
; (print-expr '(* 2 x))          => "2x"
; (print-expr '(/ x 2))          => "\frac{x}{2}"
; (print-expr '(sqrt x))         => "\sqrt{x}"
; (print-expr '(sin (* 2 x)))    => "\sin(2x)"
; (print-expr '(exp x))          => "e^{x}"
; (print-expr '(log x))          => "\ln(x)"
; (print-expr '(^ x 2))          => "x^{2}"
; (print-expr '(/ (+ x 1) (- x 1))) => "\frac{x + 1}{x - 1}"
; (print-expr '(* (+ a b) c))    => "(a + b) \cdot c"
