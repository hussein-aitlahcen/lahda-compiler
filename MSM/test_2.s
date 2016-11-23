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
; if (0 Equals (i Greater 10))
;----------
push.f 0
get 0
push.f 10
cmpgt.f
cmpeq.f
jumpf else_0_1
;----------
; i = (i Add 5)
;----------
get 0
push.f 5
add.f
set 0
jump endif_0_1
.else_0_1
jump endloop_0_1
.endif_0_1
jump beginloop_0_1
.endloop_0_1
halt
