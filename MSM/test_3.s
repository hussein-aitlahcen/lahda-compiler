.start
; @i = 0
push.f 0
; DECL i = 0
push.f 0
set 0
.beginloop_0_1
; IF ((i Equals 10) Equals 0) THEN
; 	PRINT i
; 	ASSIGN i = (i Add 1)
; 
; ELSE
; 	BREAK
; 
; 
get 0
push.f 10
cmpeq.f
push.f 0
cmpeq.f
jumpf else_0_1
; PRINT i
get 0
out.f
push.i 10
out.c
; ASSIGN i = (i Add 1)
get 0
push.f 1
add.f
set 0
jump endif_0_1
.else_0_1
; BREAK
jump endloop_0_1
.endif_0_1
.iterloop_0_1
jump beginloop_0_1
.endloop_0_1
halt
