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
