# Microsoft.CodeQuality.Analyzers

## [CA1000](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1000): Do not declare static members on generic types

When a static member of a generic type is called, the type argument must be specified for the type. When a generic instance member that does not support inference is called, the type argument must be specified for the member. In these two cases, the syntax for specifying the type argument is different and easily confused.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1001](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1001): Types that own disposable fields should be disposable

A class declares and implements an instance field that is a System.IDisposable type, and the class does not implement IDisposable. A class that declares an IDisposable field indirectly owns an unmanaged resource and should implement the IDisposable interface.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1002](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1002): Do not expose generic lists

System.Collections.Generic.List\<T> is a generic collection that's designed for performance and not inheritance. List\<T> does not contain virtual members that make it easier to change the behavior of an inherited class.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1003](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1003): Use generic event handler instances

A type contains an event that declares an EventHandler delegate that returns void, whose signature contains two parameters (the first an object and the second a type that is assignable to EventArgs), and the containing assembly targets Microsoft .NET Framework?2.0.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1005](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1005): Avoid excessive parameters on generic types

The more type parameters a generic type contains, the more difficult it is to know and remember what each type parameter represents.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1008](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1008): Enums should have zero value

The default value of an uninitialized enumeration, just as other value types, is zero. A nonflags-attributed enumeration should define a member by using the value of zero so that the default value is a valid value of the enumeration. If an enumeration that has the FlagsAttribute attribute applied defines a zero-valued member, its name should be ""None"" to indicate that no values have been set in the enumeration.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1010](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1010): Generic interface should also be implemented

To broaden the usability of a type, implement one of the generic interfaces. This is especially true for collections as they can then be used to populate generic collection types.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1012](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1012): Abstract types should not have public constructors

Constructors on abstract types can be called only by derived types. Because public constructors create instances of a type, and you cannot create instances of an abstract type, an abstract type that has a public constructor is incorrectly designed.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1014](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1014): Mark assemblies with CLSCompliant

The Common Language Specification (CLS) defines naming restrictions, data types, and rules to which assemblies must conform if they will be used across programming languages. Good design dictates that all assemblies explicitly indicate CLS compliance by using CLSCompliantAttribute . If this attribute is not present on an assembly, the assembly is not compliant.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1016](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1016): Mark assemblies with assembly version

The .NET Framework uses the version number to uniquely identify an assembly, and to bind to types in strongly named assemblies. The version number is used together with version and publisher policy. By default, applications run only with the assembly version with which they were built.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1017](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1017): Mark assemblies with ComVisible

ComVisibleAttribute determines how COM clients access managed code. Good design dictates that assemblies explicitly indicate COM visibility. COM visibility can be set for the whole assembly and then overridden for individual types and type members. If this attribute is not present, the contents of the assembly are visible to COM clients.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1018](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1018): Mark attributes with AttributeUsageAttribute

Specify AttributeUsage on {0}

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1019](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1019): Define accessors for attribute arguments

Remove the property setter from {0} or reduce its accessibility because it corresponds to positional argument {1}

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1021](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1021): Avoid out parameters

Passing types by reference (using 'out' or 'ref') requires experience with pointers, understanding how value types and reference types differ, and handling methods with multiple return values. Also, the difference between 'out' and 'ref' parameters is not widely understood.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1024](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1024): Use properties where appropriate

A public or protected method has a name that starts with ""Get"", takes no parameters, and returns a value that is not an array. The method might be a good candidate to become a property.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1027](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1027): Mark enums with FlagsAttribute

An enumeration is a value type that defines a set of related named constants. Apply FlagsAttribute to an enumeration when its named constants can be meaningfully combined.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1028](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1028): Enum Storage should be Int32

An enumeration is a value type that defines a set of related named constants. By default, the System.Int32 data type is used to store the constant value. Although you can change this underlying type, it is not required or recommended for most scenarios.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1030](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1030): Use events where appropriate

This rule detects methods that have names that ordinarily would be used for events. If a method is called in response to a clearly defined state change, the method should be invoked by an event handler. Objects that call the method should raise events instead of calling the method directly.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1031](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1031): Do not catch general exception types

A general exception such as System.Exception or System.SystemException or a disallowed exception type is caught in a catch statement, or a general catch clause is used. General and disallowed exceptions should not be caught.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1032](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1032): Implement standard exception constructors

Failure to provide the full set of constructors can make it difficult to correctly handle exceptions.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1033](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1033): Interface methods should be callable by child types

An unsealed externally visible type provides an explicit method implementation of a public interface and does not provide an alternative externally visible method that has the same name.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1034](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1034): Nested types should not be visible

A nested type is a type that is declared in the scope of another type. Nested types are useful to encapsulate private implementation details of the containing type. Used for this purpose, nested types should not be externally visible.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1036](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1036): Override methods on comparable types

A public or protected type implements the System.IComparable interface. It does not override Object.Equals nor does it overload the language-specific operator for equality, inequality, less than, less than or equal, greater than or greater than or equal.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1040](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1040): Avoid empty interfaces

Interfaces define members that provide a behavior or usage contract. The functionality that is described by the interface can be adopted by any type, regardless of where the type appears in the inheritance hierarchy. A type implements an interface by providing implementations for the members of the interface. An empty interface does not define any members; therefore, it does not define a contract that can be implemented.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1041](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1041): Provide ObsoleteAttribute message

A type or member is marked by using a System.ObsoleteAttribute attribute that does not have its ObsoleteAttribute.Message property specified. When a type or member that is marked by using ObsoleteAttribute is compiled, the Message property of the attribute is displayed. This gives the user information about the obsolete type or member.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1043](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1043): Use Integral Or String Argument For Indexers

Indexers, that is, indexed properties, should use integer or string types for the index. These types are typically used for indexing data structures and increase the usability of the library. Use of the Object type should be restricted to those cases where the specific integer or string type cannot be specified at design time. If the design requires other types for the index, reconsider whether the type represents a logical data store. If it does not represent a logical data store, use a method.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1044](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1044): Properties should not be write only

Although it is acceptable and often necessary to have a read-only property, the design guidelines prohibit the use of write-only properties. This is because letting a user set a value, and then preventing the user from viewing that value, does not provide any security. Also, without read access, the state of shared objects cannot be viewed, which limits their usefulness.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1045](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1045): Do not pass types by reference

Passing types by reference (using out or ref) requires experience with pointers, understanding how value types and reference types differ, and handling methods that have multiple return values. Also, the difference between out and ref parameters is not widely understood.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1046](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1046): Do not overload equality operator on reference types

For reference types, the default implementation of the equality operator is almost always correct. By default, two references are equal only if they point to the same object. If the operator is providing meaningful value equality, the type should implement the generic 'System.IEquatable' interface.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1047](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1047): Do not declare protected member in sealed type

Types declare protected members so that inheriting types can access or override the member. By definition, you cannot inherit from a sealed type, which means that protected methods on sealed types cannot be called.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1050](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1050): Declare types in namespaces

Types are declared in namespaces to prevent name collisions and as a way to organize related types in an object hierarchy.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1051](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1051): Do not declare visible instance fields

The primary use of a field should be as an implementation detail. Fields should be private or internal and should be exposed by using properties.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1052](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1052): Static holder types should be Static or NotInheritable

Type '{0}' is a static holder type but is neither static nor NotInheritable

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1054](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1054): URI-like parameters should not be strings

This rule assumes that the parameter represents a Uniform Resource Identifier (URI). A string representation or a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. 'System.Uri' class provides these services in a safe and secure manner.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1055](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1055): URI-like return values should not be strings

This rule assumes that the method returns a URI. A string representation of a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. The System.Uri class provides these services in a safe and secure manner.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1056](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1056): URI-like properties should not be strings

This rule assumes that the property represents a Uniform Resource Identifier (URI). A string representation of a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. The System.Uri class provides these services in a safe and secure manner.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1060](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1060): Move pinvokes to native methods class

Platform Invocation methods, such as those that are marked by using the System.Runtime.InteropServices.DllImportAttribute attribute, or methods that are defined by using the Declare keyword in Visual Basic, access unmanaged code. These methods should be of the NativeMethods, SafeNativeMethods, or UnsafeNativeMethods class.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1061](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1061): Do not hide base class methods

A method in a base type is hidden by an identically named method in a derived type when the parameter signature of the derived method differs only by types that are more weakly derived than the corresponding types in the parameter signature of the base method.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1062](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1062): Validate arguments of public methods

An externally visible method dereferences one of its reference arguments without verifying whether that argument is null (Nothing in Visual Basic). All reference arguments that are passed to externally visible methods should be checked against null. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument. If the method is designed to be called only by known assemblies, you should make the method internal.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1063](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1063): Implement IDisposable Correctly

All IDisposable types should implement the Dispose pattern correctly.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1064](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1064): Exceptions should be public

An internal exception is visible only inside its own internal scope. After the exception falls outside the internal scope, only the base exception can be used to catch the exception. If the internal exception is inherited from T:System.Exception, T:System.SystemException, or T:System.ApplicationException, the external code will not have sufficient information to know what to do with the exception.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1065](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1065): Do not raise exceptions in unexpected locations

A method that is not expected to throw exceptions throws an exception.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1066](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1066): Implement IEquatable when overriding Object.Equals

When a type T overrides Object.Equals(object), the implementation must cast the object argument to the correct type T before performing the comparison. If the type implements IEquatable\<T>, and therefore offers the method T.Equals(T), and if the argument is known at compile time to be of type T, then the compiler can call IEquatable\<T>.Equals(T) instead of Object.Equals(object), and no cast is necessary, improving performance.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1067](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1067): Override Object.Equals(object) when implementing IEquatable\<T>

When a type T implements the interface IEquatable\<T>, it suggests to a user who sees a call to the Equals method in source code that an instance of the type can be equated with an instance of any other type. The user might be confused if their attempt to equate the type with an instance of another type fails to compile. This violates the "principle of least surprise".

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1068](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1068): CancellationToken parameters must come last

Method '{0}' should take CancellationToken as the last parameter

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1069](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1069): Enums values should not be duplicated

The field reference '{0}' is duplicated in this bitwise initialization

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1070](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1070): Do not declare event fields as virtual

Do not declare virtual events in a base class. Overridden events in a derived class have undefined behavior. The C# compiler does not handle this correctly and it is unpredictable whether a subscriber to the derived event will actually be subscribing to the base class event.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1200](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1200): Avoid using cref tags with a prefix

Use of cref tags with prefixes should be avoided, since it prevents the compiler from verifying references and the IDE from updating references during refactorings. It is permissible to suppress this error at a single documentation site if the cref must use a prefix because the type being mentioned is not findable by the compiler. For example, if a cref is mentioning a special attribute in the full framework but you're in a file that compiles against the portable framework, or if you want to reference a type at higher layer of Roslyn, you should suppress the error. You should not suppress the error just because you want to take a shortcut and avoid using the full syntax.

|Item|Value|
|-|-|
|Category|Documentation|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1501](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1501): Avoid excessive inheritance

Deeply nested type hierarchies can be difficult to follow, understand, and maintain. This rule limits analysis to hierarchies in the same module. To fix a violation of this rule, derive the type from a base type that is less deep in the inheritance hierarchy or eliminate some of the intermediate base types.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1502](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1502): Avoid excessive complexity

Cyclomatic complexity measures the number of linearly independent paths through the method, which is determined by the number and complexity of conditional branches. A low cyclomatic complexity generally indicates a method that is easy to understand, test, and maintain. The cyclomatic complexity is calculated from a control flow graph of the method and is given as follows: `cyclomatic complexity = the number of edges - the number of nodes + 1`, where a node represents a logic branch point and an edge represents a line between nodes.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1505](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1505): Avoid unmaintainable code

The maintainability index is calculated by using the following metrics: lines of code, program volume, and cyclomatic complexity. Program volume is a measure of the difficulty of understanding of a symbol that is based on the number of operators and operands in the code. Cyclomatic complexity is a measure of the structural complexity of the type or method. A low maintainability index indicates that code is probably difficult to maintain and would be a good candidate to redesign.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1506](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1506): Avoid excessive class coupling

This rule measures class coupling by counting the number of unique type references that a symbol contains. Symbols that have a high degree of class coupling can be difficult to maintain. It is a good practice to have types and methods that exhibit low coupling and high cohesion. To fix this violation, try to redesign the code to reduce the number of types to which it is coupled.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1507](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1507): Use nameof to express symbol names

Using nameof helps keep your code valid when refactoring.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1508](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1508): Avoid dead conditional code

'{0}' is never '{1}'. Remove or refactor the condition(s) to avoid dead code.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1509](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1509): Invalid entry in code metrics rule specification file

Invalid entry in code metrics rule specification file.

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1700](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1700): Do not name enum values 'Reserved'

This rule assumes that an enumeration member that has a name that contains "reserved" is not currently used but is a placeholder to be renamed or removed in a future version. Renaming or removing a member is a breaking change.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1707](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1707): Identifiers should not contain underscores

By convention, identifier names do not contain the underscore (_) character. This rule checks namespaces, types, members, and parameters.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1708](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1708): Identifiers should differ by more than case

Identifiers for namespaces, types, members, and parameters cannot differ only by case because languages that target the common language runtime are not required to be case-sensitive.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1710](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1710): Identifiers should have correct suffix

By convention, the names of types that extend certain base types or that implement certain interfaces, or types that are derived from these types, have a suffix that is associated with the base type or interface.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1711](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1711): Identifiers should not have incorrect suffix

By convention, only the names of types that extend certain base types or that implement certain interfaces, or types that are derived from these types, should end with specific reserved suffixes. Other type names should not use these reserved suffixes.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1712](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1712): Do not prefix enum values with type name

An enumeration's values should not start with the type name of the enumeration.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1713](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1713): Events should not have 'Before' or 'After' prefix

Event names should describe the action that raises the event. To name related events that are raised in a specific sequence, use the present or past tense to indicate the relative position in the sequence of actions. For example, when naming a pair of events that is raised when closing a resource, you might name it 'Closing' and 'Closed', instead of 'BeforeClose' and 'AfterClose'.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1714](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1714): Flags enums should have plural names

A public enumeration has the System.FlagsAttribute attribute, and its name does not end in ""s"". Types that are marked by using FlagsAttribute have names that are plural because the attribute indicates that more than one value can be specified.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1715](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1715): Identifiers should have correct prefix

The name of an externally visible interface does not start with an uppercase ""I"". The name of a generic type parameter on an externally visible type or method does not start with an uppercase ""T"".

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1716](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1716): Identifiers should not match keywords

A namespace name or a type name matches a reserved keyword in a programming language. Identifiers for namespaces and types should not match keywords that are defined by languages that target the common language runtime.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1717](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1717): Only FlagsAttribute enums should have plural names

Naming conventions dictate that a plural name for an enumeration indicates that more than one value of the enumeration can be specified at the same time.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1720](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1720): Identifier contains type name

Names of parameters and members are better used to communicate their meaning than to describe their type, which is expected to be provided by development tools. For names of members, if a data type name must be used, use a language-independent name instead of a language-specific one.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1721](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1721): Property names should not match get methods

The name of a public or protected member starts with ""Get"" and otherwise matches the name of a public or protected property. ""Get"" methods and properties should have names that clearly distinguish their function.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1724](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1724): Type names should not match namespaces

Type names should not match the names of namespaces that are defined in the .NET Framework class library. Violating this rule can reduce the usability of the library.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1725](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1725): Parameter names should match base declaration

Consistent naming of parameters in an override hierarchy increases the usability of the method overrides. A parameter name in a derived method that differs from the name in the base declaration can cause confusion about whether the method is an override of the base method or a new overload of the method.

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1801](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1801): Review unused parameters

Avoid unused paramereters in your code. If the parameter cannot be removed, then change its name so it starts with an underscore and is optionally followed by an integer, such as '_', '_1', '_2', etc. These are treated as special discard symbol names.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1802](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1802): Use literals where appropriate

A field is declared static and read-only (Shared and ReadOnly in Visual Basic), and is initialized by using a value that is computable at compile time. Because the value that is assigned to the targeted field is computable at compile time, change the declaration to a const (Const in Visual Basic) field so that the value is computed at compile time instead of at run?time.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1805](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1805): Do not initialize unnecessarily

The .NET runtime initializes all fields of reference types to their default values before running the constructor. In most cases, explicitly initializing a field to its default value in a constructor is redundant, adding maintenance costs and potentially degrading performance (such as with increased assembly size), and the explicit initialization can be removed.  In some cases, such as with static readonly fields that permanently retain their default value, consider instead changing them to be constants or properties.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1806](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1806): Do not ignore method results

A new object is created but never used; or a method that creates and returns a new string is called and the new string is never used; or a COM or P/Invoke method returns an HRESULT or error code that is never used.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1812](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1812): Avoid uninstantiated internal classes

An instance of an assembly-level type is not created by code in the assembly.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1814](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1814): Prefer jagged arrays over multidimensional

A jagged array is an array whose elements are arrays. The arrays that make up the elements can be of different sizes, leading to less wasted space for some sets of data.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1815](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1815): Override equals and operator equals on value types

For value types, the inherited implementation of Equals uses the Reflection library and compares the contents of all fields. Reflection is computationally expensive, and comparing every field for equality might be unnecessary. If you expect users to compare or sort instances, or to use instances as hash table keys, your value type should implement Equals.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1819](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1819): Properties should not return arrays

Arrays that are returned by properties are not write-protected, even when the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1821](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1821): Remove empty Finalizers

Finalizers should be avoided where possible, to avoid the additional performance overhead involved in tracking object lifetime.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1822](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1822): Mark members as static

Members that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1823](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1823): Avoid unused private fields

Private fields were detected that do not appear to be accessed in the assembly.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2007](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2007): Consider calling ConfigureAwait on the awaited task

When an asynchronous method awaits a Task directly, continuation occurs in the same thread that created the task. Consider calling Task.ConfigureAwait(Boolean) to signal your intention for continuation. Call ConfigureAwait(false) on the task to schedule continuations to the thread pool, thereby avoiding a deadlock on the UI thread. Passing false is a good option for app-independent libraries. Calling ConfigureAwait(true) on the task has the same behavior as not explicitly calling ConfigureAwait. By explicitly calling this method, you're letting readers know you intentionally want to perform the continuation on the original synchronization context.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2011](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2011): Avoid infinite recursion

Do not assign the property within its setter. This call might result in an infinite recursion.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2109](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2109): Review visible event handlers

A public or protected event-handling method was detected. Event-handling methods should not be exposed unless absolutely necessary.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2119](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2119): Seal methods that satisfy private interfaces

An inheritable public type provides an overridable method implementation of an internal (Friend in Visual Basic) interface. To fix a violation of this rule, prevent the method from being overridden outside the assembly.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2200](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2200): Rethrow to preserve stack details

Re-throwing caught exception changes stack information

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2211](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2211): Non-constant fields should not be visible

Static fields that are neither constants nor read-only are not thread-safe. Access to such a field must be carefully controlled and requires advanced programming techniques to synchronize access to the class object.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2214](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2214): Do not call overridable methods in constructors

Virtual methods defined on the class should not be called from constructors. If a derived class has overridden the method, the derived class version will be called (before the derived class constructor is called).

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2217](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2217): Do not mark enums with FlagsAttribute

An externally visible enumeration is marked by using FlagsAttribute, and it has one or more values that are not powers of two or a combination of the other defined values on the enumeration.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA2218](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2218): Override GetHashCode on overriding Equals

GetHashCode returns a value, based on the current instance, that is suited for hashing algorithms and data structures such as a hash table. Two objects that are the same type and are equal must return the same hash code.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2219](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2219): Do not raise exceptions in finally clauses

When an exception is raised in a finally clause, the new exception hides the active exception. This makes the original error difficult to detect and debug.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2224](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2224): Override Equals on overloading operator equals

A public type implements the equality operator but does not override Object.Equals.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2225](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2225): Operator overloads have named alternates

An operator overload was detected, and the expected named alternative method was not found. The named alternative member provides access to the same functionality as the operator and is provided for developers who program in languages that do not support overloaded operators.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2226](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2226): Operators should have symmetrical overloads

A type implements the equality or inequality operator and does not implement the opposite operator.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2227](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2227): Collection properties should be read only

A writable collection property allows a user to replace the collection with a different collection. A read-only property stops the collection from being replaced but still allows the individual members to be set.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2231](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2231): Overload operator equals on overriding value type Equals

In most programming languages there is no default implementation of the equality operator (==) for value types. If your programming language supports operator overloads, you should consider implementing the equality operator. Its behavior should be identical to that of Equals.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2234](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2234): Pass system uri objects instead of strings

A call is made to a method that has a string parameter whose name contains "uri", "URI", "urn", "URN", "url", or "URL". The declaring type of the method contains a corresponding method overload that has a System.Uri parameter.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2244](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2244): Do not duplicate indexed element initializations

Indexed elements in objects initializers must initialize unique elements. A duplicate index might overwrite a previous element initialization.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2245](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2245): Do not assign a property to itself

The property {0} should not be assigned to itself

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2246](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2246): Assigning symbol and its member in the same statement

Assigning to a symbol and its member (field/property) in the same statement is not recommended. It is not clear if the member access was intended to use symbol's old value prior to the assignment or new value from the assignment in this statement. For clarity, consider splitting the assignments into separate statements.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA9998](https://docs.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers): Analyzer package has been deprecated

FxCopAnalyzers package has been deprecated in favor of 'Microsoft.CodeAnalysis.NetAnalyzers', that ships with the .NET SDK. Please refer to https://docs.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers to migrate to .NET analyzers.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---
