.start
    
    ;----------
    ; var i = 5
    ;----------
    push.f 0
    push.f 5
    set 0
    
    ;----------
    ; var s = 1
    ;----------
    push.f 0
    push.f 1
    set 1
      
      ;----------
      ; var x = 0
      ;----------
      push.f 0
      push.f 1
      set 2
      
      ;----------
      ; LOOP
      ;----------
      .begin_loop0
        get 2
        get 0
        cmple.f
        jumpf ifnot1
              
              ;----------
              ; s = (s * x)
              ;----------
              get 1
              get 2
              mul.f
              set 1
            
            ;----------
            ; x = (x + 1)
            ;----------
            get 2
            push.f 1
            add.f
            set 2
          jump begin_loop0
        jump endif1
        .ifnot1
          jump end_loop0
        .endif1
      .end_loop0
halt
