![Icon](https://raw.github.com/NickStrupat/NonNullable/master/Icons/package_icon.png)

# NonNullable
NonNullable&lt;T&gt; struct with IL-weaving to prevent all possible null assignment (via default(T) and the parameter-less constructor)

# Cases to consider

- Uses of NonNullable&lt;T&gt; within an assembly are checked with a compile-time IL-weaver such as Fody, with the assembly marked as verified with an attribute
	- Check locals and fields of methods and types for possibility of null
- Generic methods and methods/properties of generic types must be checked for returning `default(T)` and marked as verified with an attribute if they don't
	- Check if Activator calls the default constructor of a struct
- Uses of NonNullable&lt;T&gt; returned from another assembly which doesn't have the verified attribute must have IL weaved in to check for `null` at each point of return
- Uses of NonNullable&lt;T&gt; returned from a generic method or a method of a generic type which doesn't have the verified attribute must have IL weaved in to check for `null`