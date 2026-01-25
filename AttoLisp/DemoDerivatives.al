; Demo: Full symbolic differentiation pipeline with LaTeX output
; Shows: Parse ? Differentiate ? Simplify ? Print to LaTeX

; Example expressions to differentiate
(define demo-expr-1 '(sin x))
(define demo-expr-2 '(* 2 x))
(define demo-expr-3 '(* x x))
(define demo-expr-4 '(exp (* 2 x)))
(define demo-expr-5 '(/ (+ x 1) (- x 1)))

; Function to show full pipeline
(define show-derivative (expr var)
  (print "Original: " (print-expr expr))
  (print "Derivative (raw): " (differentiate expr var))
  (print "Derivative (simplified): " (simplify-expr (differentiate expr var)))
  (print "LaTeX output: " (print-expr (simplify-expr (differentiate expr var))))
  (print ""))

; Run demos
(print "=== Symbolic Differentiation with LaTeX Output ===")
(print "")

(show-derivative demo-expr-1 'x)
; d/dx(sin(x)) = \cos(x)

(show-derivative demo-expr-2 'x)
; d/dx(2x) = 2

(show-derivative demo-expr-3 'x)
; d/dx(x²) = 2x

(show-derivative demo-expr-4 'x)
; d/dx(e^(2x)) = 2e^{2x}

(show-derivative demo-expr-5 'x)
; d/dx((x+1)/(x-1)) = complicated quotient rule result

(print "=== Demo Complete ===")
