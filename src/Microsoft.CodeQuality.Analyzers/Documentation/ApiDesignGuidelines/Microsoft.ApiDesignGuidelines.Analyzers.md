### CA1000: Do not declare static members on generic types ###

When a static member of a generic type is called, the type argument must be specified for the type. When a generic instance member that does not support inference is called, the type argument must be specified for the member. In these two cases, the syntax for specifying the type argument is different and easily confused.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182139.aspx](https://msdn.microsoft.com/en-us/library/ms182139.aspx)

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

### CA1010: Collections should implement generic interface ###

To broaden the usability of a collection, implement one of the generic collection interfaces. Then the collection can be used to populate generic collection types.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182132.aspx](https://msdn.microsoft.com/en-us/library/ms182132.aspx)

### CA1012: Abstract types should not have constructors ###

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182126.aspx](http://msdn.microsoft.com/library/ms182126.aspx)

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

### CA1024: Use properties where appropriate ###

A public or protected method has a name that starts with ""Get"", takes no parameters, and returns a value that is not an array. The method might be a good candidate to become a property.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182181.aspx](http://msdn.microsoft.com/library/ms182181.aspx)

### CA1027: Mark enums with FlagsAttribute ###

An enumeration is a value type that defines a set of related named constants. Apply FlagsAttribute to an enumeration when its named constants can be meaningfully combined.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182159.aspx](http://msdn.microsoft.com/library/ms182159.aspx)

### CA1028: Enum Storage should be Int32 ###

An enumeration is a value type that defines a set of related named constants. By default, the System.Int32 data type is used to store the constant value. Although you can change this underlying type, it is not required or recommended for most scenarios.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182147.aspx](https://msdn.microsoft.com/en-us/library/ms182147.aspx)

### CA1032: Implement standard exception constructors ###

Failure to provide the full set of constructors can make it difficult to correctly handle exceptions.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182151.aspx](https://msdn.microsoft.com/en-us/library/ms182151.aspx)

### CA1033: Interface methods should be callable by child types ###

An unsealed externally visible type provides an explicit method implementation of a public interface and does not provide an alternative externally visible method that has the same name.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/library/ms182153.aspx](https://msdn.microsoft.com/library/ms182153.aspx)

### CA1034: Nested types should not be visible ###

A nested type is a type that is declared in the scope of another type. Nested types are useful to encapsulate private implementation details of the containing type. Used for this purpose, nested types should not be externally visible.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182162.aspx](https://msdn.microsoft.com/en-us/library/ms182162.aspx)

### CA1036: Override methods on comparable types ###

A public or protected type implements the System.IComparable interface. It does not override Object.Equals nor does it overload the language-specific operator for equality, inequality, less than, or greater than.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182163.aspx](http://msdn.microsoft.com/library/ms182163.aspx)

### CA1040: Avoid empty interfaces ###

Interfaces define members that provide a behavior or usage contract. The functionality that is described by the interface can be adopted by any type, regardless of where the type appears in the inheritance hierarchy. A type implements an interface by providing implementations for the members of the interface. An empty interface does not define any members; therefore, it does not define a contract that can be implemented.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182128.aspx](https://msdn.microsoft.com/en-us/library/ms182128.aspx)

### CA1041: Provide ObsoleteAttribute message ###

A type or member is marked by using a System.ObsoleteAttribute attribute that does not have its ObsoleteAttribute.Message property specified. When a type or member that is marked by using ObsoleteAttribute is compiled, the Message property of the attribute is displayed. This gives the user information about the obsolete type or member.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182166.aspx](https://msdn.microsoft.com/en-us/library/ms182166.aspx)

### CA1043: Use Integral Or String Argument For Indexers ###

Indexers, that is, indexed properties, should use integer or string types for the index. These types are typically used for indexing data structures and increase the usability of the library. Use of the Object type should be restricted to those cases where the specific integer or string type cannot be specified at design time. If the design requires other types for the index, reconsider whether the type represents a logical data store. If it does not represent a logical data store, use a method.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182180.aspx](https://msdn.microsoft.com/en-us/library/ms182180.aspx)

### CA1050: Declare types in namespaces ###

Types are declared in namespaces to prevent name collisions and as a way to organize related types in an object hierarchy.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182134.aspx](https://msdn.microsoft.com/en-us/library/ms182134.aspx)

### CA1052: Static holder types should be Static or NotInheritable ###

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182168.aspx](http://msdn.microsoft.com/library/ms182168.aspx)

### CA1060: Move pinvokes to native methods class ###

Platform Invocation methods, such as those that are marked by using the System.Runtime.InteropServices.DllImportAttribute attribute, or methods that are defined by using the Declare keyword in Visual Basic, access unmanaged code. These methods should be of the NativeMethods, SafeNativeMethods, or UnsafeNativeMethods class.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182161.aspx](http://msdn.microsoft.com/library/ms182161.aspx)

### CA1064: Exceptions should be public ###

An internal exception is visible only inside its own internal scope. After the exception falls outside the internal scope, only the base exception can be used to catch the exception. If the internal exception is inherited from T:System.Exception, T:System.SystemException, or T:System.ApplicationException, the external code will not have sufficient information to know what to do with the exception.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb264484.aspx](https://msdn.microsoft.com/en-us/library/bb264484.aspx)

### CA1065: Do not raise exceptions in unexpected locations ###

A method that is not expected to throw exceptions throws an exception.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb386039.aspx](https://msdn.microsoft.com/en-us/library/bb386039.aspx)

### CA1066: Type {0} should implement IEquatable<T> because it overrides Equals ###

Category: Design

Severity: Warning

### CA1067: Override Object.Equals(object) when implementing IEquatable<T> ###

Category: Design

Severity: Warning

### CA1068: CancellationToken parameters must come last ###

Category: Design

Severity: Warning

### CA1708: Identifiers should differ by more than case ###

Identifiers for namespaces, types, members, and parameters cannot differ only by case because languages that target the common language runtime are not required to be case-sensitive.

Category: Naming

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182242.aspx](http://msdn.microsoft.com/library/ms182242.aspx)

### CA1714: Flags enums should have plural names ###

A public enumeration has the System.FlagsAttribute attribute, and its name does not end in ""s"". Types that are marked by using FlagsAttribute have names that are plural because the attribute indicates that more than one value can be specified.

Category: Naming

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb264486.aspx](https://msdn.microsoft.com/en-us/library/bb264486.aspx)

### CA1715: Identifiers should have correct prefix ###

Identifiers should have correct prefix

Category: Naming

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182243.aspx](http://msdn.microsoft.com/library/ms182243.aspx)

### CA1716: Identifiers should not match keywords ###

A namespace name or a type name matches a reserved keyword in a programming language. Identifiers for namespaces and types should not match keywords that are defined by languages that target the common language runtime.

Category: Naming

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182248.aspx](https://msdn.microsoft.com/en-us/library/ms182248.aspx)

### CA1717: Only FlagsAttribute enums should have plural names ###

Naming conventions dictate that a plural name for an enumeration indicates that more than one value of the enumeration can be specified at the same time.

Category: Naming

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb264487.aspx](https://msdn.microsoft.com/en-us/library/bb264487.aspx)

### CA1720: Identifier contains type name ###


Names of parameters and members are better used to communicate their meaning than to describe their type, which is expected to be provided by development tools. For names of members, if a data type name must be used, use a language-independent name instead of a language-specific one.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb531486.aspx](https://msdn.microsoft.com/en-us/library/bb531486.aspx)

### CA1721: Property names should not match get methods ###

The name of a public or protected member starts with ""Get"" and otherwise matches the name of a public or protected property. ""Get"" methods and properties should have names that clearly distinguish their function.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182253.aspx](https://msdn.microsoft.com/en-us/library/ms182253.aspx)

### CA1724: Type names should not match namespaces ###

Type names should not match the names of namespaces that are defined in the .NET Framework class library. Violating this rule can reduce the usability of the library.

Category: Naming

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182257.aspx](https://msdn.microsoft.com/en-us/library/ms182257.aspx)

### CA1815: Override equals and operator equals on value types ###

For value types, the inherited implementation of Equals uses the Reflection library and compares the contents of all fields. Reflection is computationally expensive, and comparing every field for equality might be unnecessary. If you expect users to compare or sort instances, or to use instances as hash table keys, your value type should implement Equals.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182276.aspx](https://msdn.microsoft.com/en-us/library/ms182276.aspx)

### CA1819: Properties should not return arrays ###

Arrays that are returned by properties are not write-protected, even when the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/0fss9skc.aspx](https://msdn.microsoft.com/en-us/library/0fss9skc.aspx)

### CA2007: Do not directly await a Task ###

While authoring a library where the consumer is not known and when there is no need for a SynchronizationContext,  consider using ConfigureAwait(false). Otherwise, the consumers of the library may face deadlocks by consuming the asynchronous methods in a blocking fashion.

Category: Reliability

Severity: Warning

### CA2217: Do not mark enums with FlagsAttribute ###

An externally visible enumeration is marked by using FlagsAttribute, and it has one or more values that are not powers of two or a combination of the other defined values on the enumeration.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182335.aspx](http://msdn.microsoft.com/library/ms182335.aspx)

### CA2218: Override GetHashCode on overriding Equals ###

GetHashCode returns a value, based on the current instance, that is suited for hashing algorithms and data structures such as a hash table. Two objects that are the same type and are equal must return the same hash code.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182358.aspx](https://msdn.microsoft.com/en-us/library/ms182358.aspx)

### CA2224: Override Equals on overloading operator equals ###

A public type implements the equality operator but does not override Object.Equals.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182357.aspx](https://msdn.microsoft.com/en-us/library/ms182357.aspx)

### CA2225: Operator overloads have named alternates ###

An operator overload was detected, and the expected named alternative method was not found. The named alternative member provides access to the same functionality as the operator and is provided for developers who program in languages that do not support overloaded operators.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182355.aspx](https://msdn.microsoft.com/en-us/library/ms182355.aspx)

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

### CA2234: Pass system uri objects instead of strings ###

A call is made to a method that has a string parameter whose name contains "uri", "URI", "urn", "URN", "url", or "URL". The declaring type of the method contains a corresponding method overload that has a System.Uri parameter.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182360.aspx](https://msdn.microsoft.com/en-us/library/ms182360.aspx)