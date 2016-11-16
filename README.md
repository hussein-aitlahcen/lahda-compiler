[![Build status](https://ci.appveyor.com/api/projects/status/wjbpltekjfpvjgqs?svg=true)](https://ci.appveyor.com/project/hussein-aitlahcen/lahda-compiler)

#Lahda compiler prototype.

Lahda is a custom programming language, we aim to provide a compiler for the MSM (mini stack machine) from the LIMSI.

Its syntax looks like the JavaScript, but it is statically typed.

No optimizations are yet used.

## Example
```javascript
var x = 50;
var y = 0; 
if(y <= 0) { 
    y = 1; 
} 
while(y < x) {
    y = y * (y + 1); 
} 
```

Will be compiled into

```assembly
.start
    ;----------
    ; var x = 50
    ;----------
    push.f 0
    push.f 50
    set 0
    ;----------
    ; var y = 0
    ;----------
    push.f 0
    push.f 0
    set 1
    get 1
    push.f 0
    cmple.f
    jumpf ifnot_0
      ;----------
      ; y = 1
      ;----------
      push.f 1
      set 1
    jump endif_0
    .ifnot_0
    .endif_0
    ;----------
    ; loop_1
    ;----------
    .begin_loop_1
      get 1
      get 0
      cmplt.f
      jumpf ifnot_2
          ;----------
          ; y = (y * (y + 1))
          ;----------
          get 1
          get 1
          push.f 1
          add.f
          mul.f
          set 1
        jump begin_loop_1
      jump endif_2
      .ifnot_2
        jump end_loop_1
      .endif_2
    .end_loop_1
halt
```

MSC project (compilation course) at Polytech Paris-Sud.

Creators : Maxime Recuerda, Hussein Ait-Lahcen
