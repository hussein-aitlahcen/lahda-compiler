.start
;----------
; var x = 1
;----------
push.f 0
push.f 1
set 0
;----------
; var s = 1
;----------
push.f 0
push.f 1
set 1
;--------
; loop
;--------
.begin_loop_0_1
;----------
; if (s < 5)
;----------
get 1
push.f 5
cmplt.f
jumpf ifnot_0_1
;----------
; s = (s + 1)
;----------
get 1
push.f 1
add.f
set 1
;----------
; x = (x * s)
;----------
get 0
get 1
mul.f
set 0
jump endif_0_1
.ifnot_0_1
jump end_loop_0_1
.endif_0_1
jump begin_loop_0_1
.end_loop_0_1
;--------
; print(x)
;--------
get 0
out.f
halt
