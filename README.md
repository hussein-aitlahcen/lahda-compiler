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

### Example

The Alloc/Free builtin functions.

You can notice that the *`:`* is the equivalent of *`*`* in C, dereferencing a pointer.

```javascript
float _mem_is_init()
    say :(:0);

float _mem_set_init()
    :(:0) = 1;

float _block_number()
    say :((:0) + 1);

float _block_number_set(float value)
    :((:0) + 1) = value;

float _first_block_addr()
    say (:0) + 2;

float _mem_size()
    say 50000 - _first_block_addr();

float _block_size(float block_addr)
    say :block_addr;

float _block_size_set(float block_addr, float size)
    :block_addr = size;

float _block_free(float block_addr)
    say :block_addr + 1;

float _block_free_set(float block_addr, float free)
    :block_addr + 1 = free;

float _next_block_addr(float block_addr) 
{
    var block_size = _block_size(block_addr);
    var block_size_end = block_addr + block_size + 1;
    if(block_size_end  == _mem_size())
    {
        say -1;
    }

    say block_size_end + 1;
}

float _previous_block_addr(float block_addr) 
{
    var first_block_addr = _first_block_addr();
    if(block_addr <= first_block_addr)
    {
        say -1;
    }

    var previous_block_addr = first_block_addr;
    var current_block_addr = _next_block_addr(previous_block_addr);
    while(current_block_addr < block_addr)
    {
        previous_block_addr = current_block_addr;
        current_block_addr = _next_block_addr(previous_block_addr);
    }
    say previous_block_addr;
}

float _next_block_create(float block_addr, float initial_size)
{
    var next_block_addr = _next_block_addr(block_addr);
    if(next_block_addr != -1)
    {
        var block_size = _block_size(block_addr);
        var block_total_size = block_size + 2;
        var next_block_size = initial_size - block_total_size;
        _block_size_set(next_block_addr, next_block_size);
        _block_free_set(next_block_addr, true);
    }
}

float _block_fusion_right(float block_addr)
{
    if(block_addr < _first_block_addr())
    {
        say -1;
    }

    var block_free = _block_free(block_addr);
    var block_size = _block_size(block_addr);
    if(block_free)
    {
        var next_block_addr = _next_block_addr(block_addr);
        if(next_block_addr != -1)
        {
            var next_block_free = _block_free(next_block_addr);
            var next_block_size = _block_size(next_block_addr);
            if(next_block_free)
            {
                var block_fusion_size = block_size + next_block_size + 2;
                _block_size_set(block_addr, block_fusion_size);
            }
        }
    }
}

float _init_mem()
{
    if(!_mem_is_init())
    {
        _mem_set_init();
        var block_addr = _first_block_addr();
        _block_size_set(block_addr, _mem_size() - 2);
        _block_free_set(block_addr, true);
    }
}

float bmem(float required_size)
{
    _init_mem();

    var block_addr = _first_block_addr();
    var free_addr = -1;
    while(free_addr == -1) 
    {
        var block_size = _block_size(block_addr);
        var block_free = _block_free(block_addr);
        if(block_size >= required_size && block_free)
        {
            free_addr = block_addr;
            _block_size_set(block_addr, required_size);
            _block_free_set(block_addr, false);
            if(block_size > required_size)
            {
                _next_block_create(block_addr, block_size);
            }
        }
        else 
        {
            block_addr = _next_block_addr(block_addr);
            if(block_addr == -1)
            {
                say -1;
            }
        }
    }
    _block_number_set(_block_number() + 1);
    say free_addr + 2;
}

float rmem(float pointer)
{
    var block_addr = pointer - 2;
    _block_free_set(block_addr, true);
    _block_fusion_right(block_addr);
    _block_fusion_right(_previous_block_addr(block_addr));
    _block_number_set(_block_number() - 1);
}
```