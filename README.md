![Icon](https://raw.github.com/NickStrupat/NonNullable/master/Icons/package_icon.png)

# NonNullable
NonNullable&lt;T&gt; struct with IL-weaving to prevent all possible null assignment (via default(T) and the parameter-less constructor)

# Cases to consider

- Uses of NonNullable&lt;T&gt; within an assembly are checked with a compile-time IL-weaver such as Fody, with the assembly marked as verified with an attribute
	- Check locals and fields of methods and types for possibility of null
- Generic methods and methods/properties of generic types must be checked for returning `new T()` or `default(T)` and marked as verified with an attribute if they don't
	- This is because a generic method/property could return `default(NonNullable<T>)`, which doesn't call the default constructor (we cannot catch this without IL-weaving the call site with a check)
	- Check if Activator calls the default constructor of a struct (IT DOES!)
- Uses of NonNullable&lt;T&gt; returned from another assembly which doesn't have the verified attribute must have IL weaved in to check for `null` at each point of return
- Uses of NonNullable&lt;T&gt; returned from a generic method or a method of a generic type which doesn't have the verified attribute must have IL weaved in to check for `null`