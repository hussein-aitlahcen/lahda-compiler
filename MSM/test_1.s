.start
;----------
; var i = 0
;----------
push.f 0
push.f 0
set 0
;--------
; loop
;--------
.beginloop_0_1
;----------
; if (0 Equals (i NotLess 10))
;----------
push.f 0
get 0
push.f 10
cmpge.f
cmpeq.f
jumpf else_0_1
;--------
; print(i)
;--------
get 0
out.f
push.i 10
out.c
;----------
; i = (i Add 1)
;----------
get 0
push.f 1
add.f
set 0
jump endif_0_1
.else_0_1
jump endloop_0_1
.endif_0_1
jump beginloop_0_1
.endloop_0_1
halt
