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
* **Primitive** = Floating | Bool | Identifier | `-` Primitive | *`!`* Primitive | *`(`* Expression *`)`* | *`:`* Expression
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
* **Assignation** = (Identifier | *`:`* Expression ) (BinaryOperator Expression | UnaryOperator)
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
* **Function** = *`float`* Identifier *`(`* (*`float`* Identifier *`,`*)* *`)`*
* **Root** = Function*

### Built-in functions

```javascript
// mem.lah
float bmem(float size);
float rmem(float ptr);
float size(float ptr);
float display(float ptr);

// math.lah
float pow(float a, float b);
float sqrt(float a);
float abs(float a);

// collections.lah
float linked_list_new(float firstValue);
float linked_list_add(float list, float value);
float linked_list_remove(float list, float index);
float linked_list_size(float list);
float linked_list_display_values(float list);
```

### Example

You can notice that the *`:`* is the equivalent of *`*`* in C, dereferencing a pointer.

```javascript
float player_struct_size()
    say 4;

float player_new(float id, float level, float x, float y)
{
    var player = bmem(player_struct_size());
    :player = id;
    :player + 1 = level;
    :player + 2 = x;
    :player + 3 = y;
    say player;
}

float player_destroy(float player)
    rmem(player);

float player_id(float player)
    say :player;

float player_level(float player)
    say :player + 1;

float player_level_set(float player, float level)
    :player + 1 = level;

float player_x(float player)
    say :player + 2;

float player_x_set(float player, float x)
    :player + 2 = x;

float player_y(float player)
    say :player + 3;

float player_y_set(float player, float y)
    :player + 3 = y;

float player_distance(float playerOne, float playerTwo) 
{
    var xa = player_x(playerOne);
    var xb = player_x(playerTwo);
    var ya = player_y(playerOne);
    var yb = player_y(playerTwo);
    say sqrt((xb - xa) ^ 2 + (yb - ya) ^ 2);
}

float player_display(float player)
{
    print "===========";
    print "player id";
    print player_id(player);
    print "player level";
    print player_level(player);
    print "player x";
    print player_x(player);
    print "player y";
    print player_y(player);
}

float start() 
{    
    var players = linked_list_new(player_new(1, 100, 10, 15));
    linked_list_add(players, player_new(2, 30, 15, 2));
    linked_list_add(players, player_new(3, 45, 5, 9));
    linked_list_add(players, player_new(4, 10, 6, 7));

    var current = players;
    var list_size = linked_list_size(current);
    for(var i = 0; i < list_size; i++)
    {
        player_display(linked_list_value(current));
        current = linked_list_next(current);
    }
}
```