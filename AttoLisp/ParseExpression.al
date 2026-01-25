; ParseExpression.al
; Recursive-descent expression parser in AttoLisp.
; Grammar (informal):
;   expr   := term (("+"|"-") term)*
;   term   := factor (("*"|"/") factor)*
;   factor := ("+"|"-") factor | primary
;   primary:= number | identifier "(" arglist? ")" | identifier | "(" expr ")"
;   arglist:= expr ("," expr)*
; Identifiers and operators use Unicode strings; tokenizer treats identifiers as any non-whitespace
; sequence that doesn't start with a digit and doesn't include reserved characters: ()+-*/,
; Returns nil on syntax error.

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Helpers
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

; trim whitespace from both ends of a string (spaces and tabs only)
(define str-trim (s)
  (let ((len (str-length s)))
    (define left
      (lambda (i)
        (if (>= i len)
            i
            (if (or (= (substr s i 1) " ")
                    (= (substr s i 1) "\t"))
                (left (+ i 1))
                i))))
    (define right
      (lambda (i)
        (if (< i 0)
            i
            (if (or (= (substr s i 1) " ")
                    (= (substr s i 1) "\t"))
                (right (- i 1))
                i))))
    (let ((l (left 0))
          (r (right (- len 1))))
      (if (> l r)
          ""
          (substr s l (+ 1 (- r l)))))))

; char classification
(define is-digit (ch)
  (and (>= ch "0") (<= ch "9")))

(define is-letter (ch)
  ; A letter is any non-whitespace, non-digit, non-reserved char.
  (and (not (= ch " "))
       (not (= ch "\t"))
       (not (or (= ch "(")
                (= ch ")")
                (= ch "+")
                (= ch "-")
                (= ch "*")
                (= ch "/")
                (= ch ",")))
       (not (and (>= ch "0") (<= ch "9")))))

(define is-alnum (ch)
  (or (is-letter ch) (is-digit ch)))

; span over characters while pred holds, returning end index
(define span
  (lambda (s len i pred)
    (if (and (< i len)
             (pred (substr s i 1)))
        (span s len (+ i 1) pred)
        i)))

; consume whitespace starting at i, return new index
(define skip-whitespace
  (lambda (s len i)
    (if (and (< i len)
             (or (= (substr s i 1) " ")
                 (= (substr s i 1) "\t")))
        (skip-whitespace s len (+ i 1))
        i)))

; tokenize a single-character symbol at i
(define tokenize-symbol
  (lambda (s len i acc)
    (let ((c (substr s i 1)))
      (list (+ i 1)
            (cons (list (symbol "sym") c) acc))))) 

; tokenize a number starting at i
(define tokenize-number
  (lambda (s len i acc)
    (let* ((end  (span s len i
                        (lambda (ch)
                          (or (is-digit ch)
                              (= ch ".")))))
           (text (substr s i (- end i))))
      (list end
            (cons (list (symbol "num") text) acc))))) 

; tokenize an identifier starting at i
(define tokenize-identifier
  (lambda (s len i acc)
    (let* ((end  (span s len i
                        (lambda (ch) (is-alnum ch))))
           (text (substr s i (- end i))))
      (list end
            (cons (list (symbol "id") (to-lower text))
                  acc)))) )

; handle unknown character at i: return error token list directly
(define tokenize-error
  (lambda (s len i)
    (let ((c (substr s i 1)))
      (list (list (symbol "error") c)))) )

; core tokenizer loop used by tokenize
(define tokenize-loop
  (lambda (s len i acc)
    (if (>= i len)
        (reverse acc)
        (let ((i2 (skip-whitespace s len i)))
          (if (>= i2 len)
              (reverse acc)
              (let ((c (substr s i2 1)))
                (cond
                  ; single-character symbols
                  (((or (= c "(") (= c ")")
                        (= c "+") (= c "-")
                        (= c "*") (= c "/")
                        (= c ",")))
                   (let ((r (tokenize-symbol s len i2 acc)))
                     (tokenize-loop s len (car r) (car (cdr r)))))

                  ; number: digits with optional '.'
                  ((is-digit c)
                   (let ((r (tokenize-number s len i2 acc)))
                     (tokenize-loop s len (car r) (car (cdr r)))))

                  ; identifier
                  ((is-letter c)
                   (let ((r (tokenize-identifier s len i2 acc)))
                     (tokenize-loop s len (car r) (car (cdr r)))))

                  ; unknown char – treat as error token, parser will fail
                  (else
                   (tokenize-error s len i2)))))))))


(define tokenize (s)
  (let ((len (str-length s)))
    (tokenize-loop s len 0 (list))
  )
)



