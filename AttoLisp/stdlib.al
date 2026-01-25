; AttoLisp standard library (.al)
; Commonly used functions: abs, length (lists), map, filter, reduce, range, compose
; Note: For string length use built-in (str-length s)

; Absolute value
(define abs (x)
  (if (< x 0)
      (- x)
      x))

; Length of a list (recursive)
(define length (xs)
  (cond
    ((= xs nil) 0)
    ((= (car xs) nil) 0) ; empty list: car yields nil
    (else (+ 1 (length (cdr xs))))))

; Map higher-order function
(define map (f xs)
  (if (empty? xs)
      (list)
      (cons (f (car xs)) (map f (cdr xs)))))

; Filter higher-order function
(define filter (pred xs)
  (if (empty? xs)
      (list)
      (if (pred (car xs))
          (cons (car xs) (filter pred (cdr xs)))
          (filter pred (cdr xs)))))

; Reduce/fold-left
(define reduce (f acc xs)
  (if (empty? xs)
      acc
      (reduce f (f acc (car xs)) (cdr xs))))

; Create a list of integers from start to end inclusive
(define range (start end)
  (if (> start end)
      (list)
      (cons start (range (+ start 1) end))))

; Function composition
(define compose (f g)
  (lambda (x)
    (f (g x))))

; Boolean ops are provided by built-ins: and, or, not, xor

; Is list empty?
(define empty? (xs)
  (if (or (= xs nil) (= (car xs) nil))
      t
      nil))

; Reverse a list
(define reverse (xs)
  (define rev (lambda (acc ys)
    (if (or (= ys nil) (= (car ys) nil))
        acc
        (rev (cons (car ys) acc) (cdr ys)))))
  (rev (list) xs))

; Merge/append two lists
(define merge (a b)
  (if (or (= a nil) (= (car a) nil))
      b
      (cons (car a) (merge (cdr a) b))))

; Standard aliases for list head and tail
(define first (xs)
  (car xs))

(define tail (xs)
  (cdr xs))

; Alias 'head' for 'car'
(define head (xs)
  (car xs))

; Examples:
; (abs -5)                ; => 5
; (length (list 1 2 3))   ; => 3
; (map (lambda (x) (* x 2)) (list 1 2 3))   ; => (2 4 6)
; (filter (lambda (x) (> x 1)) (list 1 2 3)) ; => (2 3)
; (reduce (lambda (acc x) (+ acc x)) 0 (list 1 2 3)) ; => 6
; (range 3 6)            ; => (3 4 5 6)
; ((compose (lambda (x) (+ x 1)) (lambda (x) (* x 2))) 5) ; => 11
; (xor true false)      ; => true
; (xor true true)       ; => false
; (xor false false)     ; => false
