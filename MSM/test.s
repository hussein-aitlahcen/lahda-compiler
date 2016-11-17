.start
;----------
; var a = 1
;----------
push.f 0
push.f 1
set 0
;----------
; var b = ((a + 5) || 1)
;----------
push.f 0
get 0
push.f 5
add.f
push.f 1
or
set 1
;--------
; print((b + 1))
;--------
get 1
push.f 1
add.f
out.f
;----------
; var c = 3
;----------
push.f 0
push.f 3
set 2
;----------
; var d = 4
;----------
push.f 0
push.f 4
set 3
;----------
; var f = 6
;----------
push.f 0
push.f 6
set 2
;----------
; var e = 5
;----------
push.f 0
push.f 5
set 3
halt
