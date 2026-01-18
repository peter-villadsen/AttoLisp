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

; Helper: trim whitespace from both ends of a string
(define str-trim (s)
  (let ((len (str-length s)))
    (define left (lambda (i)
      (if (>= i len) i
          (if (or (= (substr s i 1) " ") (= (substr s i 1) "\t"))
              (left (+ i 1))
              i))))
    (define right (lambda (i)
      (if (< i 0) i
          (if (or (= (substr s i 1) " ") (= (substr s i 1) "\t"))
              (right (- i 1))
              i))))
    (let ((l (left 0)) (r (right (- len 1))))
      (if (> l r) "" (substr s l (+ 1 (- r l)))))))

; Helper: lowercase via built-in to-lower from Evaluator

; Simple substring, char-code, char-from-code, and string-to-list are assumed available.
; If not, parseExpression will bail out to nil for unsupported environment.

; Scanner utilities
(define is-digit (ch)
  (and (>= ch "0") (<= ch "9")))

(define is-letter (ch)
  ; Generalized: a letter is any char that is not whitespace, not a digit, and not a reserved symbol
  (and (not (= ch " ")) (not (= ch "\t"))
       (not (or (= ch "(") (= ch ")") (= ch "+") (= ch "-") (= ch "*") (= ch "/") (= ch ",")))
       (not (and (>= ch "0") (<= ch "9")))) )

(define is-alnum (ch)
  (or (is-letter ch) (is-digit ch)))

; Tokenization: returns list of (type value) pairs, types: num, id, sym
(define tokenize (s)
  (define len (str-length s))
  (define loop (lambda (i acc)
    (if (>= i len)
        (reverse acc)
        (let ((c (substr s i 1)))
          (cond
            (((or (= c " ") (= c "\t"))) (loop (+ i 1) acc))
            (((or (= c "(") (= c ")") (= c "+") (= c "-") (= c "*") (= c "/") (= c ",")))
              (loop (+ i 1) (cons (list (symbol "sym") c) acc)))
            ((is-digit c)
              (let ((j i))
                (define collect (lambda (k)
                  (if (and (< k len) (or (is-digit (substr s k 1)) (= (substr s k 1) ".")))
                      (collect (+ k 1)) k)))
                (let ((end (collect j)) (text (substr s i (- end i))))
                  (loop end (cons (list (symbol "num") text) acc))))
            ((is-letter c)
              (let ((j i))
                (define collect (lambda (k)
                  (if (and (< k len) (is-alnum (substr s k 1)))
                      (collect (+ k 1)) k)))
                (let ((end (collect j)) (text (substr s i (- end i))))
                  (loop end (cons (list (symbol "id") (to-lower text)) acc))))
            (else
              ; unknown char
              (list (symbol "error"))))))))
  (loop 0 (list)))

; Parser state represented as (tokens position)
(define make-state (tokens) (list tokens 0))
(define state-tokens (st) (car st))
(define state-pos (st) (car (cdr st)))
(define state-set-pos (st p) (list (state-tokens st) p))
(define state-current (st)
  (let ((p (state-pos st)) (toks (state-tokens st)))
    (if (or (= toks nil) (>= p (length toks))) nil (nth toks p))))
(define advance (st) (state-set-pos st (+ (state-pos st) 1)))

; Helpers for tokens
(define tok-type (tok) (car tok))
(define tok-val (tok) (car (cdr tok)))

; Parse functions return (result newState) or (nil st) on error
(define parse-number (st)
  (let ((tok (state-current st)))
    (if (and tok (= (tok-type tok) (symbol "num")))
        (list (number (tok-val tok)) (advance st))
        (list nil st))))

(define parse-identifier (st)
  (let ((tok (state-current st)))
    (if (and tok (= (tok-type tok) (symbol "id")))
        (list (symbol (tok-val tok)) (advance st))
        (list nil st))))

(define match-sym (st sym)
  (let ((tok (state-current st)))
    (if (and tok (= (tok-type tok) (symbol "sym")) (= (tok-val tok) sym))
        (list true (advance st))
        (list false st))))

; factor := ("+"|"-") factor | primary
(define parse-factor (st)
  (let ((m (match-sym st "+")))
    (if (car m)
        (let ((res (parse-factor (car (cdr m)))))
          (if (= (car res) nil) res (list (list (symbol "+") (number 0) (car res)) (car (cdr res)))))
        (let ((m2 (match-sym st "-")))
          (if (car m2)
              (let ((res2 (parse-factor (car (cdr m2)))))
                (if (= (car res2) nil) res2 (list (list (symbol "-") (number 0) (car res2)) (car (cdr res2)))))
              (parse-primary st))))))

; primary := number | id "(" arglist? ")" | id | "(" expr ")"
(define parse-primary (st)
  (let ((num (parse-number st)))
    (if (not (= (car num) nil)) num
        (let ((id (parse-identifier st)))
          (if (not (= (car id) nil))
              ; maybe function call with 0..N args
              (let ((m (match-sym (car (cdr id)) "(")))
                (if (car m)
                    (let ((state (car (cdr m))))
                      ; parse optional arglist: expr ("," expr)*
                      (define parse-args (lambda (st args)
                        (let ((close (match-sym st ")")))
                          (if (car close)
                              (list args (car (cdr close)))
                              (let ((ex (parse-expr st)))
                                (if (= (car ex) nil)
                                    (list nil st)
                                    (let ((after (car (cdr ex))) (comma (match-sym (car (cdr ex)) ",")))
                                      (if (car comma)
                                          (parse-args (car (cdr comma)) (merge args (list (car ex))))
                                          (let ((cl (match-sym after ")")))
                                            (if (car cl)
                                                (list (merge args (list (car ex))) (car (cdr cl)))
                                                (list nil st))))))))))
                      (let ((res (parse-args state (list))))
                        (if (= (car res) nil)
                            (list nil st)
                            (list (merge (list (car id)) (car res)) (car (cdr res))))))
                    id))
              ; parentheses
              (let ((m2 (match-sym st "(")))
                (if (car m2)
                    (let ((ex2 (parse-expr (car (cdr m2)))) (mc (match-sym (car (cdr ex2)) ")")))
                      (if (or (= (car ex2) nil) (not (car mc)))
                          (list nil st)
                          (list (car ex2) (car (cdr mc)))))
                    (list nil st))))))))

; term := factor (("*"|"/") factor)*
(define parse-term (st)
  (let ((f (parse-factor st)))
    (if (= (car f) nil) f
        (define loop (lambda (acc state)
          (let ((m1 (match-sym state "*")))
            (if (car m1)
                (let ((f2 (parse-factor (car (cdr m1)))))
                  (if (= (car f2) nil)
                      (list acc state)
                      (loop (list (symbol "*") acc (car f2)) (car (cdr f2)))))
                (let ((m2 (match-sym state "/")))
                  (if (car m2)
                      (let ((f3 (parse-factor (car (cdr m2)))))
                        (if (= (car f3) nil)
                            (list acc state)
                            (loop (list (symbol "/") acc (car f3)) (car (cdr f3)))))
                      (list acc state)))))))
        (let ((r (loop (car f) (car (cdr f)))))
          (list (car r) (car (cdr r)))))))

; expr := term (("+"|"-") term)*
(define parse-expr (st)
  (let ((t (parse-term st)))
    (if (= (car t) nil) t
        (define loop (lambda (acc state)
          (let ((m1 (match-sym state "+")))
            (if (car m1)
                (let ((t2 (parse-term (car (cdr m1)))))
                  (if (= (car t2) nil)
                      (list acc state)
                      (loop (list (symbol "+") acc (car t2)) (car (cdr t2)))))
                (let ((m2 (match-sym state "-")))
                  (if (car m2)
                      (let ((t3 (parse-term (car (cdr m2)))))
                        (if (= (car t3) nil)
                            (list acc state)
                            (loop (list (symbol "-") acc (car t3)) (car (cdr t3)))))
                      (list acc state)))))))
        (let ((r (loop (car t) (car (cdr t)))))
          (list (car r) (car (cdr r)))))))

; Parse expression recursively; returns lisp value or nil on error
(define parseExpression (input)
  (let ((s (str-trim input)))
    (if (= (str-length s) 0)
        nil
        (let ((tokens (tokenize s)))
          (let ((st (make-state tokens)) (res (parse-expr st)))
            (if (= (car res) nil)
                nil
                (car res)))))))
