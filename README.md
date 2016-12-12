[![Build status](https://ci.appveyor.com/api/projects/status/wjbpltekjfpvjgqs?svg=true)](https://ci.appveyor.com/project/hussein-aitlahcen/lahda-compiler)

# Lahda

Lahda is a custom programming language, we aim to provide a compiler for the MSM (mini stack machine) from the LIMSI.

Its syntax looks like the JavaScript, but it is statically typed.

MSC project (compilation course) at **Polytech Paris-Sud**.

*Authors : Maxime Recuerda, Hussein Ait-Lahcen*

## Introduction

### Getting started

* Generate the MSM (mini stack machine) : 
  * `cd MSM/`
  * `make`

* Restore nugets packages :
  * `dotnet restore`

* Build the project :
  * `dotnet build`

### Supported types

* Floating

* Comming soon
  * String
  * Struct
  * Array

### Arithmetic Expressions

* **Floating** = `(((([0-9]+\\.[0-9]*)|([0-9]*\\.[0-9]+))([Ee][+-]?[0-9]+)?)|([0-9]+([Ee][+-]?[0-9]+)))`
* **Bool** = *`true`* | *`false`*
* **Primitive** = Floating | Bool | Identifier | `-` Primitive | *`!`* Primitive | *`(`* Expression *`)`*
* **Divisible** = Primitive (*`/`* Divisible)?
* **Multiplicative** = Divisible (`*` Multiplicative)?
* **Additive** = Multiplicative ((*`+`* | *`-`*) Additive)?
* **BitwiseAnd** = Additive (*`&`* BitwiseAnd)?
* **BitwiseOr** = BitwiseAnd (*`|`* BitwiseOr)?
* **Comparative** = BitwiseOr ((*`==`* | *`!=`* | *`>`* | *`<`* | *`>=`* | *`<=`*) Comparative)?
* **LogicalAnd** = Comparative (*`&&`* LogicalAnd)?
* **LogicalOr** = LogicalAnd (*`||`* LogicalOr)?
* **Expression** = LogicalOr

### Statements

* **StatementEnd** = *`;`*
* **Identifier** = `([A-Za-z][A-Za-z0-9_]*)`
* **BinaryOperator** = *`+=`* | *`-=`* | *`/=`* | *`%=`* | *`^=`* | *`=`* | `*=`
* **UnaryOperator** = *`++`* | *`--`*
* **Assignation** = Identifier (BinaryOperator Expression | UnaryOperator)
* **Declaration** = *`var`* Identifier *`=`* Expression
* **LoopControl** = `continue` | `break`
* **Inline** = (Assignation | Declaration | LoopControl) StatementEnd
* **Block** = *`{`* Statement\* *`}`*
* **Conditional** = *`if`* *`(`* Expression *`)`* Statement (*`else`* Statement)?
* **WhileLoop** = *`while`* *`(`* Expression *`)`* Statement
* **DoLoop** = *`do`* Statement ((*`while`* | *`until`*) *`(`* Expression *`)`*  | *`forever`* ) StatementEnd
* **ForLoop** = *`for`* *`(`* Declaration StatemendEnd Expression StatemendEnd Assignation *`)`* Statement
* **Loop** = WhileLoop | DoLoop | ForLoop
* **Statement** = Inline | Loop | Block | StatementEnd

### Example

```javascript
float fibo(float n) 
  if(n <= 2) 
    say 1; 
  else 
    say fibo(n - 1) + fibo(n - 2); 

float start() 
  for(var i = 0; i < 10; i++) 
    print fibo(i + 1);
```

#### Generated code

```assembly
; ROOT
; FUN
.fibo
push.f 0
; IF (n NotGreater 2) THEN
; 	RET 1
; ELSE
; 	RET (CALL fibo((n Sub 1)) Add CALL fibo((n Sub 2)))
; ENDIF
; 
get 0
push.f 2
cmple.f
jumpf else_0_1
; RET 1
push.f 1
ret
jump endif_0_1
.else_0_1
; RET (CALL fibo((n Sub 1)) Add CALL fibo((n Sub 2)))
; CALL fibo((n Sub 1))
prep fibo
get 0
push.f 1
sub.f
call 1
; CALL fibo((n Sub 2))
prep fibo
get 0
push.f 2
sub.f
call 1
add.f
ret
.endif_0_1
push.f 0
ret
; FUN
.start
push.f 0
; DECL i = 0
push.f 0
set 0
.beginloop_0_1
; IF (i Less 10) THEN
; 	PRINT CALL fibo((i Add 1))
; ELSE
; 	BREAK
; 
; ENDIF
; 
get 0
push.f 10
cmplt.f
jumpf else_0_2
; PRINT CALL fibo((i Add 1))
; CALL fibo((i Add 1))
prep fibo
get 0
push.f 1
add.f
call 1
out.f
push.i 10
out.c
jump endif_0_2
.else_0_2
; BREAK
jump endloop_0_1
.endif_0_2
.iterloop_0_1
; ASSIGN i = (i Add 1)
get 0
push.f 1
add.f
set 0
jump beginloop_0_1
.endloop_0_1
halt
```
