; AttoLisp version of a simple mathematical expression parser.
; Exposes (parseExpression "...") which returns a Lisp list AST or nil on error.
; Grammar (roughly):
;   expr   := term (('+' | '-') term)*
;   term   := factor (('*' | '/') factor)*
;   factor := unary ('^' factor)?      ; right-associative exponent
;   unary  := ('+' | '-') unary | primary
;   primary:= number
;           | symbol [ '(' arg-list ')' ]
;           | '(' expr ')'
;   arg-list := expr (',' expr)*

; A token is represented as a list (type value):
;   type  = 'num, 'id, 'op, 'lparen, 'rparen, 'comma, 'eof
;   value = number or symbol depending on type

(define isDigit (ch)
  (and (>= ch "0") (<= ch "9")))

(define isLetter (ch)
  (or (and (>= ch "a") (<= ch "z"))
      (and (>= ch "A") (<= ch "Z"))))

(define isWhitespace (ch)
  (or (= ch " ") (= ch "\t")))

(define substring (s start end)
  ; naive substring using built-ins: assume 0 <= start < end <= (str-length s)
  (let ((len (str-length s)))
    (if (or (< start 0) (> end len) (> start end))
        ""
        (substr s start (- end start)))))

(define make-token (kind value)
  (list kind value))

; Scan a run of digits starting at index i. Returns (list endIndex)
(define scan-number (s i len)
  (if (>= i len)
      (list i)
      (let ((ch (substr s i 1)))
        (if (isDigit ch)
            (scan-number s (+ i 1) len)
            (list i)))))

; Scan a run of letters starting at index i. Returns (list endIndex)
(define scan-ident (s i len)
  (if (>= i len)
      (list i)
      (let ((ch (substr s i 1)))
        (if (isLetter ch)
            (scan-ident s (+ i 1) len)
            (list i)))))

(define tokenize-internal (s i len tokens)
  (if (>= i len)
      (reverse (cons (make-token 'eof 'eof) tokens))
      (let ((ch (substr s i 1)))
        (cond
          ((isWhitespace ch)
           (tokenize-internal s (+ i 1) len tokens))

          ; number (only integers for tests)
          ((isDigit ch)
           (let ((start i)
                 (end (car (scan-number s i len))))
             (let ((numStr (substring s start end)))
               (tokenize-internal s end len
                 (cons (make-token 'num (number numStr)) tokens)))) )

          ; identifier (function name or variable)
          ((isLetter ch)
           (let ((start i)
                 (end (car (scan-ident s i len))))
             (let ((name (substring s start end)))
               (tokenize-internal s end len
                 (cons (make-token 'id (symbol (to-lower name))) tokens)))) )

          ; parentheses, comma, operators
          ((= ch "(")
           (tokenize-internal s (+ i 1) len (cons (make-token 'lparen 'lparen) tokens)))
          ((= ch ")")
           (tokenize-internal s (+ i 1) len (cons (make-token 'rparen 'rparen) tokens)))
          ((= ch ",")
           (tokenize-internal s (+ i 1) len (cons (make-token 'comma 'comma) tokens)))
          ((or (= ch "+") (= ch "-") (= ch "*") (= ch "/") (= ch "^"))
           (tokenize-internal s (+ i 1) len (cons (make-token 'op (symbol ch)) tokens)))

          (else
           ; Unknown character, skip it so parser will likely fail
           (tokenize-internal s (+ i 1) len tokens))))))

(define tokenize (s)
  (tokenize-internal s 0 (str-length s) (list)))

; Helpers to work with token stream represented as (list tokens index)

(define ts-current (ts)
  (let ((tokens (car ts))
        (idx (car (cdr ts))))
    (nth tokens idx)))

(define ts-advance (ts)
  (let ((tokens (car ts))
        (idx (car (cdr ts))))
    (list tokens (+ idx 1))))

(define token-kind (tok)
  (car tok))

(define token-value (tok)
  (car (cdr tok)))

; parseExpression entrypoint used by tests
(define parseExpression (s)
  (let ((tokens (tokenize s)))
    (let ((ts (list tokens 0)))
      (let ((result (parse-expr ts)))
        (if (= result nil)
            nil
            (let ((node (car result))
                  (restTs (car (cdr result))))
              (let ((tok (ts-current restTs)))
                (if (= (token-kind tok) 'eof)
                    node
                    nil))))))))

; Each parse-* function returns either nil on failure, or (list node new-ts)

(define parse-expr (ts)
  (let ((termRes (parse-term ts)))
    (if (= termRes nil)
        nil
        (let ((left (car termRes))
              (ts1 (car (cdr termRes))))
          (parse-expr-rest left ts1)))))


(define parse-expr-rest (left ts)
  (let ((tok (ts-current ts)))
    (if (and (= (token-kind tok) 'op)
             (or (= (token-value tok) '+) (= (token-value tok) '-)))
        (let ((op (token-value tok))
              (ts1 (ts-advance ts)))
          (let ((rightRes (parse-term ts1)))
            (if (= rightRes nil)
                nil
                (let ((right (car rightRes))
                      (ts2 (car (cdr rightRes))))
                  (parse-expr-rest (list op left right) ts2)))))
        (list left ts))))

(define parse-term (ts)
  (let ((facRes (parse-factor ts)))
    (if (= facRes nil)
        nil
        (let ((left (car facRes))
              (ts1 (car (cdr facRes))))
          (parse-term-rest left ts1)))))

(define parse-term-rest (left ts)
  (let ((tok (ts-current ts)))
    (if (and (= (token-kind tok) 'op)
             (or (= (token-value tok) '*) (= (token-value tok) '/)))
        (let ((op (token-value tok))
              (ts1 (ts-advance ts)))
          (let ((rightRes (parse-factor ts1)))
            (if (= rightRes nil)
                nil
                (let ((right (car rightRes))
                      (ts2 (car (cdr rightRes))))
                  (parse-term-rest (list op left right) ts2)))))
        (list left ts))))

(define parse-factor (ts)
  (let ((unRes (parse-unary ts)))
    (if (= unRes nil)
        nil
        (let ((left (car unRes))
              (ts1 (car (cdr unRes))))
          (let ((tok (ts-current ts1)))
            (if (and (= (token-kind tok) 'op)
                     (= (token-value tok) '^))
                (let ((ts2 (ts-advance ts1)))
                  (let ((rightRes (parse-factor ts2))) ; right-assoc
                    (if (= rightRes nil)
                        nil
                        (let ((right (car rightRes))
                              (ts3 (car (cdr rightRes))))
                          (list (list '^ left right) ts3)))))
                (list left ts1)))))) ) ; <-- make sure this is the else

(define parse-unary (ts)
  (let ((tok (ts-current ts)))
    (if (and (= (token-kind tok) 'op)
             (or (= (token-value tok) '+) (= (token-value tok) '-)))
        (let ((op (token-value tok))
              (ts1 (ts-advance ts)))
          (let ((unRes (parse-unary ts1)))
            (if (= unRes nil)
                nil
                (let ((expr (car unRes))
                      (ts2 (car (cdr unRes))))
                  (if (= op '-)
                      ; Our tests expect unary minus to become (- 0 expr)
                      (list (list '- 0 expr) ts2)
                      (list expr ts2))))))
        (parse-primary ts))))

(define parse-primary (ts)
  (let ((tok (ts-current ts)))
    (cond
      ((= (token-kind tok) 'num)
       (list (token-value tok) (ts-advance ts)))
      ((= (token-kind tok) 'id)
       (parse-id-or-call ts))
      ((= (token-kind tok) 'lparen)
       (let ((ts1 (ts-advance ts)))
         (let ((exprRes (parse-expr ts1)))
           (if (= exprRes nil)
               nil
               (let ((node (car exprRes))
                     (ts2 (car (cdr exprRes))))
                 (let ((tok2 (ts-current ts2)))
                   (if (= (token-kind tok2) 'rparen)
                       (list node (ts-advance ts2))
                       nil)))))))
      (else nil))))

(define parse-id-or-call (ts)
  (let ((tok (ts-current ts)))
    (let ((name (token-value tok))
          (ts1 (ts-advance ts)))
      (let ((tok1 (ts-current ts1)))
        (if (= (token-kind tok1) 'lparen)
            ; function call
            (let ((argsRes (parse-arg-list (ts-advance ts1))))
              (if (= argsRes nil)
                  nil
                  (let ((args (car argsRes))
                        (ts2 (car (cdr argsRes))))
                    (let ((tok2 (ts-current ts2)))
                      (if (= (token-kind tok2) 'rparen)
                          (list (cons name args) (ts-advance ts2))
                          nil)))))
            ; variable
            (list name ts1))))))

(define parse-arg-list (ts)
  ; parse zero or more comma-separated expr; used only after '('
  (let ((tok (ts-current ts)))
    (if (= (token-kind tok) 'rparen)
        (list (list) ts) ; zero-arg function
        (let ((firstRes (parse-expr ts)))
          (if (= firstRes nil)
              nil
              (let ((arg1 (car firstRes))
                    (ts1 (car (cdr firstRes))))
                (parse-more-args (list arg1) ts1)))))))

(define parse-more-args (acc ts)
  (let ((tok (ts-current ts)))
    (if (= (token-kind tok) 'comma)
        (let ((ts1 (ts-advance ts)))
          (let ((nextRes (parse-expr ts1)))
            (if (= nextRes nil)
                nil
                (let ((arg (car nextRes))
                      (ts2 (car (cdr nextRes))))
                  (parse-more-args (merge acc (list arg)) ts2)))))
        (list acc ts))))