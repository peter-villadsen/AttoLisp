; AttoLisp symbolic expression simplification
; Exposes (simplify-expr expr) which simplifies a Lisp expression
; Removes identity elements and zero factors from arithmetic expressions

; Helper: check if expr is a list
(define is-list? (expr)
  (and (not (= expr nil))
       (not (= expr (car (list expr))))))

; Helper: get operator from a list expression
(define operator (expr)
  (if (list? expr)
      (car expr)
      nil))

; Helper: get all operands from a list expression
(define operands (expr)
  (if (list? expr)
      (cdr expr)
      nil))

; Helper: recursively simplify a list of expressions
(define simplify-list (exprs)
  (if (empty? exprs)
      (list)
      (cons (simplify-expr (car exprs))
            (simplify-list (cdr exprs)))))

; Helper: filter out identity elements from a list
; For addition: remove 0s
; For multiplication: remove 1s
(define filter-identity (op args)
  (cond
    ((= op '+)
     (filter (lambda (x) (not (= x 0))) args))
    ((= op '*)
     (filter (lambda (x) (not (= x 1))) args))
    (else args)))

; Helper: check if any element is a zero factor (for multiplication)
(define has-zero? (args)
  (cond
    ((empty? args) nil)
    ((= (car args) 0) t)
    (else (has-zero? (cdr args)))))

; Helper: get the first operand safely (returns nil if not available)
(define first-op (expr)
  (let ((ops (operands expr)))
    (if (empty? ops)
        nil
        (car ops))))

; Main simplification function with recursive descent
(define simplify-expr (expr)
  (cond
    ; If not a list, return as-is (atoms don't simplify)
    ((not (list? expr)) expr)
    
    ; Math function identity values
    ; sin(0) => 0
    ((and (= (operator expr) 'sin)
          (= (first-op expr) 0))
     0)
    
    ; cos(0) => 1
    ((and (= (operator expr) 'cos)
          (= (first-op expr) 0))
     1)
    
    ; tan(0) => 0
    ((and (= (operator expr) 'tan)
          (= (first-op expr) 0))
     0)
    
    ; exp(0) => 1
    ((and (= (operator expr) 'exp)
          (= (first-op expr) 0))
     1)
    
    ; log(1) => 0
    ((and (= (operator expr) 'log)
          (= (first-op expr) 1))
     0)
    
    ; sqrt(0) => 0
    ((and (= (operator expr) 'sqrt)
          (= (first-op expr) 0))
     0)
    
    ; sqrt(1) => 1
    ((and (= (operator expr) 'sqrt)
          (= (first-op expr) 1))
     1)
    
    ; Inverse operations: log(exp(x)) => x
    ((and (= (operator expr) 'log)
          (list? (first-op expr))
          (= (operator (first-op expr)) 'exp))
     (first-op (first-op expr)))
    
    ; Inverse operations: exp(log(x)) => x
    ((and (= (operator expr) 'exp)
          (list? (first-op expr))
          (= (operator (first-op expr)) 'log))
     (first-op (first-op expr)))
    
    ; Handle addition: (+ e1 0 e2) -> (+ e1 e2)
    ; First recursively simplify operands
    ((= (operator expr) '+)
     (let ((simplified-ops (simplify-list (operands expr))))
       (let ((filtered (filter-identity '+ simplified-ops)))
         (cond
           ; No operands left -> 0
           ((empty? filtered) 0)
           ; One operand left -> just that operand
           ((= (length filtered) 1) (car filtered))
           ; Multiple operands -> reconstruct addition
           (else (cons '+ filtered))))))
    
    ; Handle multiplication: (* e1 1 e2) -> (* e1 e2)
    ; Also: (* e1 0 e2) -> 0
    ; First recursively simplify operands
    ((= (operator expr) '*)
     (let ((simplified-ops (simplify-list (operands expr))))
       (cond
         ; If any operand is 0, entire product is 0
         ((has-zero? simplified-ops) 0)
         ; Otherwise filter out 1s
         (else
          (let ((filtered (filter-identity '* simplified-ops)))
            (cond
              ; No operands left -> 1
              ((empty? filtered) 1)
              ; One operand left -> just that operand
              ((= (length filtered) 1) (car filtered))
              ; Multiple operands -> reconstruct multiplication
              (else (cons '* filtered))))))))
    
    ; Handle subtraction: (- e 0) -> e, (- 0 e) -> (- e), (- e e) -> 0
    ((= (operator expr) '-)
     (let ((simplified-ops (simplify-list (operands expr))))
       (cond
         ; Unary minus: keep as-is
         ((= (length simplified-ops) 1) (cons '- simplified-ops))
         ; Binary subtraction
         ((= (length simplified-ops) 2)
          (let ((a (car simplified-ops))
                (b (car (cdr simplified-ops))))
            (cond
              ; (- e 0) => e
              ((= b 0) a)
              ; (- 0 e) => (- e)
              ((= a 0) (list '- b))
              ; (- e e) => 0
              ((= a b) 0)
              ; Otherwise keep as-is
              (else (cons '- simplified-ops)))))
         ; Otherwise keep as-is
         (else (cons '- simplified-ops)))))
    
    ; Handle division: (/ e 1) -> e
    ((= (operator expr) '/)
     (let ((simplified-ops (simplify-list (operands expr))))
       (cond
         ; Reciprocal: keep as-is
         ((= (length simplified-ops) 1) (cons '/ simplified-ops))
         ; Binary: if second arg is 1, return first arg
         ((and (= (length simplified-ops) 2)
               (= (car (cdr simplified-ops)) 1))
          (car simplified-ops))
         ; Otherwise keep as-is
         (else (cons '/ simplified-ops)))))
    
    ; For other functions (sin, cos, tan, exp, log, sqrt without special cases)
    ; recursively simplify their arguments
    ((or (= (operator expr) 'sin)
         (= (operator expr) 'cos)
         (= (operator expr) 'tan)
         (= (operator expr) 'exp)
         (= (operator expr) 'log)
         (= (operator expr) 'sqrt))
     (cons (operator expr) (simplify-list (operands expr))))
    
    ; Unknown operator: recursively simplify operands
    (else (cons (operator expr) (simplify-list (operands expr))))))

; Examples:
; (simplify-expr '(+ x 0))        => x
; (simplify-expr '(+ 0 x 0 y))    => (+ x y)
; (simplify-expr '(* x 1))        => x
; (simplify-expr '(* x 0 y))      => 0
; (simplify-expr '(* 1 x 1 y))    => (* x y)
; (simplify-expr '(- x 0))        => x
; (simplify-expr '(/ x 1))        => x
; (simplify-expr '(+ (* 1 x) 0))  => x  ; recursive
; (simplify-expr '(sin 0))        => 0
; (simplify-expr '(cos 0))        => 1
; (simplify-expr '(exp 0))        => 1
; (simplify-expr '(log 1))        => 0
; (simplify-expr '(sqrt 1))       => 1
; (simplify-expr '(log (exp x)))  => x
; (simplify-expr '(exp (log x)))  => x
; (simplify-expr '(- x x))        => 0
; (simplify-expr '(- 0 x))        => (- x)
