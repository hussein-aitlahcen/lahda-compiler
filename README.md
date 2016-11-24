[![Build status](https://ci.appveyor.com/api/projects/status/wjbpltekjfpvjgqs?svg=true)](https://ci.appveyor.com/project/hussein-aitlahcen/lahda-compiler)

# Lahda

Lahda is a custom programming language, we aim to provide a compiler for the MSM (mini stack machine) from the LIMSI.

Its syntax looks like the JavaScript, but it is statically typed.

MSC project (compilation course) at **Polytech Paris-Sud**.

*Authors : Maxime Recuerda, Hussein Ait-Lahcen*

## Introduction

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
* **ForLoop** = *`for`* *`(`* Declaration ; Expression ; Assignation *`)`* Statement
* **Loop** = WhileLoop | DoLoop | ForLoop
* **Statement** = Inline | Loop | Block | StatementEnd

### Example

Compute factorial(5)
```javascript
var x = 1; 
var s = 1; 
while(s < 5) 
{ 
    s = s + 1; 
    x = x * s; 
} 
print x;
```

Generated code

```assembly
----------
 var x = 1
----------
push.f 0
push.f 1
set 0
----------
 var s = 1
----------
push.f 0
push.f 1
set 1
--------
 loop
--------
.beginloop_0_1
----------
 if (s < 5)
----------
get 1
push.f 5
cmplt.f
jumpf else_0_1
----------
 s = (s + 1)
----------
get 1
push.f 1
add.f
set 1
----------
 x = (x * s)
----------
get 0
get 1
mul.f
set 0
jump endif_0_1
.else_0_1
jump endloop_0_1
.endif_0_1
jump beginloop_0_1
.endloop_0_1
--------
 print(x)
--------
get 0
out.f
halt
```