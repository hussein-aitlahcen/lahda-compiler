.start
;----------
; DECL i = 0
;----------
push.f 0
set 0
.beginloop_0_1
;----------
; IF (i Less 10) THEN
; 	PRINT i
; ELSE
; 	BREAK
; 
; 
;----------
get 0
push.f 10
cmplt.f
jumpf else_0_1
;----------
; PRINT i
;----------
get 0
out.f
push.i 10
out.c
jump endif_0_1
.else_0_1
;----------
; BREAK
;----------
jump endloop_0_1
.endif_0_1
.iterloop_0_1
;----------
; ASSIGN i = (i Add 1)
;----------
get 0
push.f 1
add.f
set 0
jump beginloop_0_1
.endloop_0_1
;----------
; DECL i = 0
;----------
push.f 0
set 0
.beginloop_0_2
;----------
; IF (i Less 10) THEN
; 	PRINT i
; ELSE
; 	BREAK
; 
; 
;----------
get 0
push.f 10
cmplt.f
jumpf else_0_2
;----------
; PRINT i
;----------
get 0
out.f
push.i 10
out.c
jump endif_0_2
.else_0_2
;----------
; BREAK
;----------
jump endloop_0_2
.endif_0_2
.iterloop_0_2
;----------
; ASSIGN i = (i Add 1)
;----------
get 0
push.f 1
add.f
set 0
jump beginloop_0_2
.endloop_0_2
halt