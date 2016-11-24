.start
; @i = 0
push.f 0
; @x = 1
push.f 0
; DECL i = 0
push.f 0
set 0
; DECL x = 0
push.f 0
set 1
.beginloop_0_1
; IF (x Equals 0) THEN
; 	IF (i Greater 5) THEN
; 		ASSIGN x = 1
; 	ELSE
; 
; 	ENDIF
; 
; 	ASSIGN i = (i Add 1)
; 
; ELSE
; 	BREAK
; 
; ENDIF
; 
get 1
push.f 0
cmpeq.f
jumpf else_0_1
; IF (i Greater 5) THEN
; 	ASSIGN x = 1
; ELSE
; 
; ENDIF
; 
get 0
push.f 5
cmpgt.f
jumpf else_0_1_1
; ASSIGN x = 1
push.f 1
set 1
jump endif_0_1_1
.else_0_1_1
.endif_0_1_1
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
; PRINT i
get 0
out.f
push.i 10
out.c
halt
