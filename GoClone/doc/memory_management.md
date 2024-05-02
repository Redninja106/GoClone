# Memory Management

## stack vs heap allocation

By default, values are allocated on the stack.
```
func main() {
	var a = 0 // on the stack
}
```

If a value has its address taken, it's upgraded to the heap, unless the compiler can prove it won't live longer then the function it the value exists in.

```
[external]
func someNativeFunc(int* ptr)

[pure]
func somePureFunc(int* ptr) -> int {
	// ...
}

func main() {
	var x = 0
	someNativeFunc(&x) // x is heap allocated

	var y = 0
	somePureFunc(&y) // y is still stack allocated
}
```

## cleanup

When a value is allocated on the heap, it's reference counted
```
[external]
func someNativeFunc(int* ptr)

func main() {

	var y = 0 // y is allocated on the heap
	// _incRefCnt(y); _getRefCnt(y)=1
	someNativeFunc(&y)
	// _decRefCnt(y); _getRefCnt(y)=0; y is deallocated 
}
```

### circular refs and weak refs

if two heap allocated values reference each other, they can keep each other alive. This is a memory leak. 
the compiler should emit a warning or error for these cases.
to resolve this, one or both of the references should be marked as [weak] meaning it references and 
