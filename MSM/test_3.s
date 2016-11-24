.start
;----------
; @i = 0
;----------
push.f 0
;----------
; @j = 1
;----------
push.f 0
;----------
; @k = 2
;----------
push.f 0
;----------
; @l = 3
;----------
push.f 0
;----------
; @m = 2
;----------
;----------
; @n = 3
;----------
;----------
; DECL i = 0
;----------
push.f 0
set 0
;----------
; DECL j = 1
;----------
push.f 1
set 1
;----------
; DECL k = 2
;----------
push.f 2
set 2
;----------
; DECL l = 3
;----------
push.f 3
set 3
;----------
; PRINT k
;----------
get 2
out.f
push.i 10
out.c
;----------
; PRINT l
;----------
get 3
out.f
push.i 10
out.c
;----------
; DECL m = 4
;----------
push.f 4
set 2
;----------
; DECL n = 5
;----------
push.f 5
set 3
;----------
; PRINT i
;----------
get 0
out.f
push.i 10
out.c
;----------
; PRINT j
;----------
get 1
out.f
push.i 10
out.c
;----------
; PRINT m
;----------
get 2
out.f
push.i 10
out.c
;----------
; PRINT n
;----------
get 3
out.f
push.i 10
out.c
halt
