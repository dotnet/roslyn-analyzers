### CA1001: Types that own disposable fields should be disposable ###

A class declares and implements an instance field that is a System.IDisposable type, and the class does not implement IDisposable. A class that declares an IDisposable field indirectly owns an unmanaged resource and should implement the IDisposable interface.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182172.aspx](http://msdn.microsoft.com/library/ms182172.aspx)

### CA1003: Use generic event handler instances ###

A type contains a delegate that returns void, whose signature contains two parameters (the first an object and the second a type that is assignable to EventArgs), and the containing assembly targets Microsoft .NET Framework?2.0.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182178.aspx](http://msdn.microsoft.com/library/ms182178.aspx)

### CA1008: Enums should have zero value ###

The default value of an uninitialized enumeration, just as other value types, is zero. A nonflags-attributed enumeration should define a member by using the value of zero so that the default value is a valid value of the enumeration. If an enumeration that has the FlagsAttribute attribute applied defines a zero-valued member, its name should be ""None"" to indicate that no values have been set in the enumeration.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182149.aspx](http://msdn.microsoft.com/library/ms182149.aspx)

### CA1014: Mark assemblies with CLSCompliant ###

The Common Language Specification (CLS) defines naming restrictions, data types, and rules to which assemblies must conform if they will be used across programming languages. Good design dictates that all assemblies explicitly indicate CLS compliance by using CLSCompliantAttribute . If this attribute is not present on an assembly, the assembly is not compliant.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182156.aspx](http://msdn.microsoft.com/library/ms182156.aspx)

### CA1016: Mark assemblies with assembly version ###

The .NET Framework uses the version number to uniquely identify an assembly, and to bind to types in strongly named assemblies. The version number is used together with version and publisher policy. By default, applications run only with the assembly version with which they were built.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182155.aspx](http://msdn.microsoft.com/library/ms182155.aspx)

### CA1017: Mark assemblies with ComVisible ###

ComVisibleAttribute determines how COM clients access managed code. Good design dictates that assemblies explicitly indicate COM visibility. COM visibility can be set for the whole assembly and then overridden for individual types and type members. If this attribute is not present, the contents of the assembly are visible to COM clients.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182157.aspx](http://msdn.microsoft.com/library/ms182157.aspx)

### CA1018: Mark attributes with AttributeUsageAttribute ###

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182158.aspx](http://msdn.microsoft.com/library/ms182158.aspx)

### CA1019: Define accessors for attribute arguments ###

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182136.aspx](http://msdn.microsoft.com/library/ms182136.aspx)

### CA1027: Mark enums with FlagsAttribute ###

An enumeration is a value type that defines a set of related named constants. Apply FlagsAttribute to an enumeration when its named constants can be meaningfully combined.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182159.aspx](http://msdn.microsoft.com/library/ms182159.aspx)

### CA1036: Override methods on comparable types ###

A public or protected type implements the System.IComparable interface. It does not override Object.Equals nor does it overload the language-specific operator for equality, inequality, less than, or greater than.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182163.aspx](http://msdn.microsoft.com/library/ms182163.aspx)

### CA1060: Move pinvokes to native methods class ###

Platform Invocation methods, such as those that are marked by using the System.Runtime.InteropServices.DllImportAttribute attribute, or methods that are defined by using the Declare keyword in Visual Basic, access unmanaged code. These methods should be of the NativeMethods, SafeNativeMethods, or UnsafeNativeMethods class.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182161.aspx](http://msdn.microsoft.com/library/ms182161.aspx)

### CA1720: Identifier contains type name ###


Names of parameters and members are better used to communicate their meaning than to describe their type, which is expected to be provided by development tools. For names of members, if a data type name must be used, use a language-independent name instead of a language-specific one.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb531486.aspx](https://msdn.microsoft.com/en-us/library/bb531486.aspx)

### CA2217: Do not mark enums with FlagsAttribute ###

An externally visible enumeration is marked by using FlagsAttribute, and it has one or more values that are not powers of two or a combination of the other defined values on the enumeration.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182335.aspx](http://msdn.microsoft.com/library/ms182335.aspx)

### CA2227: Collection properties should be read only ###

A writable collection property allows a user to replace the collection with a different collection. A read-only property stops the collection from being replaced but still allows the individual members to be set.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/library/ms182327.aspx](https://msdn.microsoft.com/library/ms182327.aspx)

### CA2231: Overload operator equals on overriding value type Equals ###

In most programming languages there is no default implementation of the equality operator (==) for value types. If your programming language supports operator overloads, you should consider implementing the equality operator. Its behavior should be identical to that of Equals

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182359.aspx](http://msdn.microsoft.com/library/ms182359.aspx)