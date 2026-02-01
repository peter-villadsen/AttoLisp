; AttoLisp symbolic expression simplification
; Exposes (simplify-expr expr) which simplifies a Lisp expression
; Removes identity elements and zero factors from arithmetic expressions
; Performs constant folding on numeric operands

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

; Helper: partition a list into numbers and non-numbers
; Returns a list of two lists: (numbers non-numbers)
(define partition-numbers (exprs)
  (if (empty? exprs)
      (list (list) (list))
      (let ((first (car exprs))
            (rest-partition (partition-numbers (cdr exprs))))
        (let ((nums (car rest-partition))
              (non-nums (car (cdr rest-partition))))
          (if (number? first)
              (list (cons first nums) non-nums)
              (list nums (cons first non-nums)))))))

; Helper: sum a list of numbers
(define sum-numbers (nums)
  (if (empty? nums)
      0
      (+ (car nums) (sum-numbers (cdr nums)))))

; Helper: multiply a list of numbers
(define product-numbers (nums)
  (if (empty? nums)
      1
      (* (car nums) (product-numbers (cdr nums)))))

; Helper: fold constants in addition
; Takes a list of operands and returns a simplified list with constants combined
(define fold-add-constants (args)
  (let ((parts (partition-numbers args)))
    (let ((nums (car parts))
          (non-nums (car (cdr parts))))
      (cond
        ; No numbers: return as-is
        ((empty? nums) non-nums)
        ; Only numbers: compute sum and return as single element
        ((empty? non-nums) (list (sum-numbers nums)))
        ; Mixed: compute sum and append to non-numbers
        (else
         (let ((sum (sum-numbers nums)))
           (if (= sum 0)
               non-nums
               (cons sum non-nums))))))))

; Helper: fold constants in multiplication
; Takes a list of operands and returns a simplified list with constants combined
(define fold-mul-constants (args)
  (let ((parts (partition-numbers args)))
    (let ((nums (car parts))
          (non-nums (car (cdr parts))))
      (cond
        ; No numbers: return as-is
        ((empty? nums) non-nums)
        ; Only numbers: compute product and return as single element
        ((empty? non-nums) (list (product-numbers nums)))
        ; Mixed: compute product and append to non-numbers
        (else
         (let ((product (product-numbers nums)))
           (cond
             ; Product is 0: entire expression is 0
             ((= product 0) (list 0))
             ; Product is 1: omit it (identity)
             ((= product 1) non-nums)
             ; Otherwise: prepend product to non-numbers
             (else (cons product non-nums)))))))))

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
    
    ; Handle addition with constant folding
    ((= (operator expr) '+)
     (let ((simplified-ops (simplify-list (operands expr))))
       (let ((folded (fold-add-constants simplified-ops)))
         (cond
           ; No operands left -> 0
           ((empty? folded) 0)
           ; One operand left -> just that operand
           ((= (length folded) 1) (car folded))
           ; Multiple operands -> reconstruct addition
           (else (cons '+ folded))))))
    
    ; Handle multiplication with constant folding
    ((= (operator expr) '*)
     (let ((simplified-ops (simplify-list (operands expr))))
       (cond
         ; If any operand is 0, entire product is 0
         ((has-zero? simplified-ops) 0)
         ; Otherwise fold constants
         (else
          (let ((folded (fold-mul-constants simplified-ops)))
            (cond
              ; Result is just zero
              ((and (= (length folded) 1) (= (car folded) 0)) 0)
              ; No operands left -> 1
              ((empty? folded) 1)
              ; One operand left -> just that operand
              ((= (length folded) 1) (car folded))
              ; Multiple operands -> reconstruct multiplication
              (else (cons '* folded))))))))
    
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
              ; Both are numbers: compute the result
              ((and (number? a) (number? b)) (- a b))
              ; Otherwise keep as-is
              (else (cons '- simplified-ops)))))
         ; Otherwise keep as-is
         (else (cons '- simplified-ops)))))
    
    ; Handle division: (/ e 1) -> e
    ((= (operator expr) '/)
     (let ((simplified-ops (simplify-list (operands expr))))
       (cond
         ; Reciprocal: keep as-is (or compute if number)
         ((= (length simplified-ops) 1)
          (if (number? (car simplified-ops))
              (/ 1 (car simplified-ops))
              (cons '/ simplified-ops)))
         ; Binary division
         ((= (length simplified-ops) 2)
          (let ((a (car simplified-ops))
                (b (car (cdr simplified-ops))))
            (cond
              ; Division by 1: return numerator
              ((= b 1) a)
              ; Both are numbers: compute the result
              ((and (number? a) (number? b)) (/ a b))
              ; Otherwise keep as-is
              (else (cons '/ simplified-ops)))))
         ; Otherwise keep as-is
         (else (cons '/ simplified-ops)))))
    
    ; Handle power: (^ e 0) -> 1, (^ e 1) -> e, (^ 0 n) -> 0 (for n > 0)
    ((= (operator expr) '^)
     (let ((simplified-ops (simplify-list (operands expr))))
       (let ((base (car simplified-ops))
             (exp (car (cdr simplified-ops))))
         (cond
           ; x^0 => 1 (except 0^0 which we'll leave as-is for now)
           ((and (= exp 0) (not (= base 0))) 1)
           ; x^1 => x
           ((= exp 1) base)
           ; 0^n => 0 (for positive n)
           ((and (= base 0) (number? exp) (> exp 0)) 0)
           ; 1^n => 1
           ((= base 1) 1)
           ; Both numbers: compute
           ((and (number? base) (number? exp))
            ; For integer exponents, use multiplication; otherwise leave as-is for now
            (if (and (number? exp) (>= exp 0) (= exp (/ (* exp 1) 1)))
                ; Integer exponent >= 0: could compute, but for simplicity just rebuild
                (cons '^ simplified-ops)
                (cons '^ simplified-ops)))
           ; Otherwise keep as-is
           (else (cons '^ simplified-ops))))))

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
; (simplify-expr '(+ x 1 y 2))    => (+ 3 x y)
; (simplify-expr '(* x 1))        => x
; (simplify-expr '(* x 0 y))      => 0
; (simplify-expr '(* 1 x 1 y))    => (* x y)
; (simplify-expr '(* x 4 y 2 (sin x)))  => (* 8 x y (sin x))
; (simplify-expr '(- x 0))        => x
; (simplify-expr '(/ x 1))        => x
; (simplify-expr '(+ (* 1 x) 0))  => x  ; recursive
; (simplify-expr '(^ x 0))        => 1
; (simplify-expr '(^ x 1))        => x
; (simplify-expr '(^ 0 5))        => 0
; (simplify-expr '(^ 1 100))      => 1
; (simplify-expr '(sin 0))        => 0
; (simplify-expr '(cos 0))        => 1
; (simplify-expr '(exp 0))        => 1
; (simplify-expr '(log 1))        => 0
; (simplify-expr '(sqrt 1))       => 1
; (simplify-expr '(log (exp x)))  => x
; (simplify-expr '(exp (log x)))  => x
; (simplify-expr '(- x x))        => 0
; (simplify-expr '(- 0 x))        => (- x)
; (simplify-expr '(- 5 3))        => 2
; (simplify-expr '(/ 10 2))       => 5
