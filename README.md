# GoClone
A go-like programming language build in c# using llvm

- lexer and parser built from scratch
- powerful type system
- implements high-level features like interfaces/polymorphism

[see code examples](https://github.com/Redninja106/GoClone/tree/master/GoClone/code)

# Syntax

Each file in the GoClone language (`gc`) is a series of declarations

## `module` declaration and imports

The first declaration in a file must be a `module` declaration, which declares which module the file is a part of.

```
module main

import libc
```

After which any of the following declarations can be used in any order:

### `func` Declaration

The `func` declaration declares a function.

```
func add(int a, int b) -> int {
  return a + b;
}
```

functions can also declare receivers, which are used to implement interfaces:

```
interface Addable {
  func add(int i),
}

func (int* a) add(int b) {
  *a = *a + b
}

func main() {
  var x -> int = 0
  x.add(1)
  print(x) // 1

  var addable -> Addable& = x as Addable&
  x.add(1)
  print(x) // 2
}
```

### `type` Declaration

The `type` declaration declares a strong type alias.

```
type MyType as int
```

### `struct` and `interface` Declarations

These are shorthand declarations to simplify large type declarations:

```
interface MyInterface {
}
// is the same as...
type MyInterface as interface {
}
```

## Type syntax
`gc`'s type system consists of concrete and non-concrete types. Concrete types are those with a known size at compile type, such as an primitive type, 
pointer type, or fixed-size array. Non-concrete types are the inverse, e.g. interface types or unknown-length arrays.
Non-concrete types cannot be directly used in code as they do not have a known size. They are instead used indirectly through pointers and references (which do have a known size)

### primitives

The only primitve type is `int`, a 32-bit integer. Primitives are always concrete.

### arrays

An array type is created by adding `[]` to the end of a type, like `int[]`. This is an array of unknown length and is non-concrete.
An array type can be also be given a known length, for example `int[4]` is an array of 4 integer.

### pointers

A pointer type is created by adding a `*` to the end of a type. Pointer types are always concrete and may be constructed from non-concrete types.
For example `int*` is a pointer to an int, and `int[]*` is a pointer to an array of ints (of unknown length).

### references

A reference type is created by adding `&` to the end of a type. Reference types are concrete and may only be constructed from non-concrete types.
A reference is like a pointer that holds additional data about the pointer. For arrays (such as `int[]&`), it holds the length of the array. For 
interfaces, it holds a pointer to the target's virtual table, which is the list of functions that implement the interface for that type. 

### structs

a struct type is created using the `struct` keyword followed by a list of fields:
```
struct { int x, int y }
```

### interfaces
an interface type is created using the `interface` keyword followed by a list of methods:
```
interface {
  func foo() -> int,
}
```










