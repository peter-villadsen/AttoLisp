; AttoLisp symbolic differentiation
; Exposes (differentiate expr var) which returns the derivative of expr with respect to var
; expr: a Lisp expression (quoted)
; var: a symbol representing the variable to differentiate with respect to

; Helper: check if expr is a number (integer or decimal)
; In AttoLisp, we check by trying arithmetic operations
(define is-number? (expr)
  (and (not (= expr nil))
       (not (empty? (list expr)))
       ; Try to add 0 to it - if it works, it's a number
       (not (= (+ expr 0) nil))))
       
; Helper: check if two values are equal as symbols
(define symbol-eq? (a b)
  (= a b))

; Helper: check if expr is a list by checking if car works on it
(define is-list? (expr)
  (and (not (= expr nil))
       (not (= expr (car (list expr))))))

; Helper: get operator from a list expression
(define operator (expr)
  (car expr))

; Helper: get first operand
(define first-operand (expr)
  (car (cdr expr)))

; Helper: get second operand
(define second-operand (expr)
  (car (cdr (cdr expr))))

; Main differentiation function
(define differentiate (expr var)
  (cond
    ; If expr equals var, derivative is 1
    ((= expr var) 1)
    
    ; If expr is not a list, it's either a constant or a different variable
    ; In both cases, derivative is 0
    ((not (list? expr)) 0)
    
    ; Product rule: d/dx(u*v) = u'*v + u*v'
    ((= (operator expr) '*)
     (let ((u (first-operand expr))
           (v (second-operand expr)))
       (let ((du (differentiate u var))
             (dv (differentiate v var)))
         (list '+ (list '* du v) (list '* u dv)))))
    
    ; Quotient rule: d/dx(u/v) = (u'*v - u*v') / v^2
    ((= (operator expr) '/)
     (let ((u (first-operand expr))
           (v (second-operand expr)))
       (let ((du (differentiate u var))
             (dv (differentiate v var)))
         (list '/ 
               (list '- (list '* du v) (list '* u dv))
               (list '* v v)))))
    
    ; Sum rule: d/dx(u+v) = u' + v'
    ((= (operator expr) '+)
     (let ((u (first-operand expr))
           (v (second-operand expr)))
       (let ((du (differentiate u var))
             (dv (differentiate v var)))
         (list '+ du dv))))
    
    ; Difference rule: d/dx(u-v) = u' - v'
    ((= (operator expr) '-)
     (let ((u (first-operand expr))
           (v (second-operand expr)))
       (let ((du (differentiate u var))
             (dv (differentiate v var)))
         (list '- du dv))))
    
    ; Chain rule for sin: d/dx(sin(u)) = cos(u) * u'
    ((= (operator expr) 'sin)
     (let ((u (first-operand expr)))
       (let ((du (differentiate u var)))
         (list '* (list 'cos u) du))))
    
    ; Chain rule for cos: d/dx(cos(u)) = -sin(u) * u'
    ((= (operator expr) 'cos)
     (let ((u (first-operand expr)))
       (let ((du (differentiate u var)))
         (list '* (list '- 0 (list 'sin u)) du))))
    
    ; Chain rule for tan: d/dx(tan(u)) = sec²(u) * u' = (1/cos²(u)) * u'
    ((= (operator expr) 'tan)
     (let ((u (first-operand expr)))
       (let ((du (differentiate u var)))
         (list '* (list '/ 1 (list '* (list 'cos u) (list 'cos u))) du))))
    
    ; Chain rule for exp: d/dx(exp(u)) = exp(u) * u'
    ((= (operator expr) 'exp)
     (let ((u (first-operand expr)))
       (let ((du (differentiate u var)))
         (list '* (list 'exp u) du))))
    
    ; Chain rule for log: d/dx(log(u)) = (1/u) * u'
    ((= (operator expr) 'log)
     (let ((u (first-operand expr)))
       (let ((du (differentiate u var)))
         (list '* (list '/ 1 u) du))))
    
    ; Chain rule for sqrt: d/dx(sqrt(u)) = (1/(2*sqrt(u))) * u'
    ((= (operator expr) 'sqrt)
     (let ((u (first-operand expr)))
       (let ((du (differentiate u var)))
         (list '* (list '/ 1 (list '* 2 (list 'sqrt u))) du))))
    
    ; Unknown expression - treat as constant
    (else 0)))

; Examples:
; (differentiate '(* 2 x) 'x)  => (+ (* 0 x) (* 2 1))
; (differentiate '(* x x) 'x)  => (+ (* 1 x) (* x 1))
; (differentiate '(+ x 5) 'x)  => (+ 1 0)
; (differentiate '(/ x 2) 'x)  => (/ (- (* 1 2) (* x 0)) (* 2 2))
; (differentiate '(/ (* x x) x) 'x)  => (/ (- (* (+ (* 1 x) (* x 1)) x) (* (* x x) 1)) (* x x))
; (differentiate '(sin x) 'x)  => (* (cos x) 1)
; (differentiate '(cos x) 'x)  => (* (- 0 (sin x)) 1)
; (differentiate '(tan x) 'x)  => (* (/ 1 (* (cos x) (cos x))) 1)
; (differentiate '(exp x) 'x)  => (* (exp x) 1)
; (differentiate '(log x) 'x)  => (* (/ 1 x) 1)
; (differentiate '(sqrt x) 'x) => (* (/ 1 (* 2 (sqrt x))) 1)
; (differentiate '(sin (* 2 x)) 'x) => (* (cos (* 2 x)) (+ (* 0 x) (* 2 1)))
