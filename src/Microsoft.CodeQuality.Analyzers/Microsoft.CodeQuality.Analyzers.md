# Microsoft.CodeQuality.Analyzers

## [CA1000](https://docs.microsoft.com/visualstudio/code-quality/ca1000): Do not declare static members on generic types

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

When a static member of a generic type is called, the type argument must be specified for the type. When a generic instance member that does not support inference is called, the type argument must be specified for the member. In these two cases, the syntax for specifying the type argument is different and easily confused.

## [CA1001](https://docs.microsoft.com/visualstudio/code-quality/ca1001): Types that own disposable fields should be disposable

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

A class declares and implements an instance field that is a System.IDisposable type, and the class does not implement IDisposable. A class that declares an IDisposable field indirectly owns an unmanaged resource and should implement the IDisposable interface.

## [CA1002](https://docs.microsoft.com/visualstudio/code-quality/ca1002): Do not expose generic lists

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

System.Collections.Generic.List<T> is a generic collection that's designed for performance and not inheritance. List<T> does not contain virtual members that make it easier to change the behavior of an inherited class.

## [CA1003](https://docs.microsoft.com/visualstudio/code-quality/ca1003): Use generic event handler instances

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

A type contains an event that declares an EventHandler delegate that returns void, whose signature contains two parameters (the first an object and the second a type that is assignable to EventArgs), and the containing assembly targets Microsoft .NET Framework?2.0.

## [CA1005](https://docs.microsoft.com/visualstudio/code-quality/ca1005): Avoid excessive parameters on generic types

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

The more type parameters a generic type contains, the more difficult it is to know and remember what each type parameter represents.

## [CA1008](https://docs.microsoft.com/visualstudio/code-quality/ca1008): Enums should have zero value

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

The default value of an uninitialized enumeration, just as other value types, is zero. A nonflags-attributed enumeration should define a member by using the value of zero so that the default value is a valid value of the enumeration. If an enumeration that has the FlagsAttribute attribute applied defines a zero-valued member, its name should be ""None"" to indicate that no values have been set in the enumeration.

## [CA1010](https://docs.microsoft.com/visualstudio/code-quality/ca1010): Generic interface should also be implemented

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

To broaden the usability of a type, implement one of the generic interfaces. This is especially true for collections as they can then be used to populate generic collection types.

## [CA1012](https://docs.microsoft.com/visualstudio/code-quality/ca1012): Abstract types should not have public constructors

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

Constructors on abstract types can be called only by derived types. Because public constructors create instances of a type, and you cannot create instances of an abstract type, an abstract type that has a public constructor is incorrectly designed.

## [CA1014](https://docs.microsoft.com/visualstudio/code-quality/ca1014): Mark assemblies with CLSCompliant

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

The Common Language Specification (CLS) defines naming restrictions, data types, and rules to which assemblies must conform if they will be used across programming languages. Good design dictates that all assemblies explicitly indicate CLS compliance by using CLSCompliantAttribute . If this attribute is not present on an assembly, the assembly is not compliant.

## [CA1016](https://docs.microsoft.com/visualstudio/code-quality/ca1016): Mark assemblies with assembly version

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

The .NET Framework uses the version number to uniquely identify an assembly, and to bind to types in strongly named assemblies. The version number is used together with version and publisher policy. By default, applications run only with the assembly version with which they were built.

## [CA1017](https://docs.microsoft.com/visualstudio/code-quality/ca1017): Mark assemblies with ComVisible

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

ComVisibleAttribute determines how COM clients access managed code. Good design dictates that assemblies explicitly indicate COM visibility. COM visibility can be set for the whole assembly and then overridden for individual types and type members. If this attribute is not present, the contents of the assembly are visible to COM clients.

## [CA1018](https://docs.microsoft.com/visualstudio/code-quality/ca1018): Mark attributes with AttributeUsageAttribute

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Specify AttributeUsage on {0}

## [CA1019](https://docs.microsoft.com/visualstudio/code-quality/ca1019): Define accessors for attribute arguments

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

Remove the property setter from {0} or reduce its accessibility because it corresponds to positional argument {1}

## [CA1021](https://docs.microsoft.com/visualstudio/code-quality/ca1021): Avoid out parameters

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Passing types by reference (using 'out' or 'ref') requires experience with pointers, understanding how value types and reference types differ, and handling methods with multiple return values. Also, the difference between 'out' and 'ref' parameters is not widely understood.

## [CA1024](https://docs.microsoft.com/visualstudio/code-quality/ca1024): Use properties where appropriate

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

A public or protected method has a name that starts with ""Get"", takes no parameters, and returns a value that is not an array. The method might be a good candidate to become a property.

## [CA1027](https://docs.microsoft.com/visualstudio/code-quality/ca1027): Mark enums with FlagsAttribute

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

An enumeration is a value type that defines a set of related named constants. Apply FlagsAttribute to an enumeration when its named constants can be meaningfully combined.

## [CA1028](https://docs.microsoft.com/visualstudio/code-quality/ca1028): Enum Storage should be Int32

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

An enumeration is a value type that defines a set of related named constants. By default, the System.Int32 data type is used to store the constant value. Although you can change this underlying type, it is not required or recommended for most scenarios.

## [CA1030](https://docs.microsoft.com/visualstudio/code-quality/ca1030): Use events where appropriate

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

This rule detects methods that have names that ordinarily would be used for events. If a method is called in response to a clearly defined state change, the method should be invoked by an event handler. Objects that call the method should raise events instead of calling the method directly.

## [CA1031](https://docs.microsoft.com/visualstudio/code-quality/ca1031): Do not catch general exception types

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A general exception such as System.Exception or System.SystemException or a disallowed exception type is caught in a catch statement, or a general catch clause is used. General and disallowed exceptions should not be caught.

## [CA1032](https://docs.microsoft.com/visualstudio/code-quality/ca1032): Implement standard exception constructors

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Failure to provide the full set of constructors can make it difficult to correctly handle exceptions.

## [CA1033](https://docs.microsoft.com/visualstudio/code-quality/ca1033): Interface methods should be callable by child types

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

An unsealed externally visible type provides an explicit method implementation of a public interface and does not provide an alternative externally visible method that has the same name.

## [CA1034](https://docs.microsoft.com/visualstudio/code-quality/ca1034): Nested types should not be visible

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A nested type is a type that is declared in the scope of another type. Nested types are useful to encapsulate private implementation details of the containing type. Used for this purpose, nested types should not be externally visible.

## [CA1036](https://docs.microsoft.com/visualstudio/code-quality/ca1036): Override methods on comparable types

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

A public or protected type implements the System.IComparable interface. It does not override Object.Equals nor does it overload the language-specific operator for equality, inequality, less than, less than or equal, greater than or greater than or equal.

## [CA1040](https://docs.microsoft.com/visualstudio/code-quality/ca1040): Avoid empty interfaces

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Interfaces define members that provide a behavior or usage contract. The functionality that is described by the interface can be adopted by any type, regardless of where the type appears in the inheritance hierarchy. A type implements an interface by providing implementations for the members of the interface. An empty interface does not define any members; therefore, it does not define a contract that can be implemented.

## [CA1041](https://docs.microsoft.com/visualstudio/code-quality/ca1041): Provide ObsoleteAttribute message

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A type or member is marked by using a System.ObsoleteAttribute attribute that does not have its ObsoleteAttribute.Message property specified. When a type or member that is marked by using ObsoleteAttribute is compiled, the Message property of the attribute is displayed. This gives the user information about the obsolete type or member.

## [CA1043](https://docs.microsoft.com/visualstudio/code-quality/ca1043): Use Integral Or String Argument For Indexers

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Indexers, that is, indexed properties, should use integer or string types for the index. These types are typically used for indexing data structures and increase the usability of the library. Use of the Object type should be restricted to those cases where the specific integer or string type cannot be specified at design time. If the design requires other types for the index, reconsider whether the type represents a logical data store. If it does not represent a logical data store, use a method.

## [CA1044](https://docs.microsoft.com/visualstudio/code-quality/ca1044): Properties should not be write only

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Although it is acceptable and often necessary to have a read-only property, the design guidelines prohibit the use of write-only properties. This is because letting a user set a value, and then preventing the user from viewing that value, does not provide any security. Also, without read access, the state of shared objects cannot be viewed, which limits their usefulness.

## [CA1045](https://docs.microsoft.com/visualstudio/code-quality/ca1045): Do not pass types by reference

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Passing types by reference (using out or ref) requires experience with pointers, understanding how value types and reference types differ, and handling methods that have multiple return values. Also, the difference between out and ref parameters is not widely understood.

## [CA1046](https://docs.microsoft.com/visualstudio/code-quality/ca1046): Do not overload equality operator on reference types

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

For reference types, the default implementation of the equality operator is almost always correct. By default, two references are equal only if they point to the same object. If the operator is providing meaningful value equality, the type should implement the generic 'System.IEquatable' interface.

## [CA1047](https://docs.microsoft.com/visualstudio/code-quality/ca1047): Do not declare protected member in sealed type

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Types declare protected members so that inheriting types can access or override the member. By definition, you cannot inherit from a sealed type, which means that protected methods on sealed types cannot be called.

## [CA1050](https://docs.microsoft.com/visualstudio/code-quality/ca1050): Declare types in namespaces

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Types are declared in namespaces to prevent name collisions and as a way to organize related types in an object hierarchy.

## [CA1051](https://docs.microsoft.com/visualstudio/code-quality/ca1051): Do not declare visible instance fields

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

The primary use of a field should be as an implementation detail. Fields should be private or internal and should be exposed by using properties.

## [CA1052](https://docs.microsoft.com/visualstudio/code-quality/ca1052): Static holder types should be Static or NotInheritable

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Type '{0}' is a static holder type but is neither static nor NotInheritable

## [CA1054](https://docs.microsoft.com/visualstudio/code-quality/ca1054): URI-like parameters should not be strings

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

This rule assumes that the parameter represents a Uniform Resource Identifier (URI). A string representation or a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. 'System.Uri' class provides these services in a safe and secure manner.

## [CA1055](https://docs.microsoft.com/visualstudio/code-quality/ca1055): URI-like return values should not be strings

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

This rule assumes that the method returns a URI. A string representation of a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. The System.Uri class provides these services in a safe and secure manner.

## [CA1056](https://docs.microsoft.com/visualstudio/code-quality/ca1056): URI-like properties should not be strings

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

This rule assumes that the property represents a Uniform Resource Identifier (URI). A string representation of a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. The System.Uri class provides these services in a safe and secure manner.

## [CA1060](https://docs.microsoft.com/visualstudio/code-quality/ca1060): Move pinvokes to native methods class

|Item|Value|
|-|-|
|Category|Design|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Platform Invocation methods, such as those that are marked by using the System.Runtime.InteropServices.DllImportAttribute attribute, or methods that are defined by using the Declare keyword in Visual Basic, access unmanaged code. These methods should be of the NativeMethods, SafeNativeMethods, or UnsafeNativeMethods class.

## [CA1061](https://docs.microsoft.com/visualstudio/code-quality/ca1061): Do not hide base class methods

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A method in a base type is hidden by an identically named method in a derived type when the parameter signature of the derived method differs only by types that are more weakly derived than the corresponding types in the parameter signature of the base method.

## [CA1062](https://docs.microsoft.com/visualstudio/code-quality/ca1062): Validate arguments of public methods

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

An externally visible method dereferences one of its reference arguments without verifying whether that argument is null (Nothing in Visual Basic). All reference arguments that are passed to externally visible methods should be checked against null. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument. If the method is designed to be called only by known assemblies, you should make the method internal.

## [CA1063](https://docs.microsoft.com/visualstudio/code-quality/ca1063): Implement IDisposable Correctly

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

All IDisposable types should implement the Dispose pattern correctly.

## [CA1064](https://docs.microsoft.com/visualstudio/code-quality/ca1064): Exceptions should be public

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

An internal exception is visible only inside its own internal scope. After the exception falls outside the internal scope, only the base exception can be used to catch the exception. If the internal exception is inherited from T:System.Exception, T:System.SystemException, or T:System.ApplicationException, the external code will not have sufficient information to know what to do with the exception.

## [CA1065](https://docs.microsoft.com/visualstudio/code-quality/ca1065): Do not raise exceptions in unexpected locations

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A method that is not expected to throw exceptions throws an exception.

## [CA1066](https://docs.microsoft.com/visualstudio/code-quality/ca1066): Implement IEquatable when overriding Object.Equals

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

When a type T overrides Object.Equals(object), the implementation must cast the object argument to the correct type T before performing the comparison. If the type implements IEquatable<T>, and therefore offers the method T.Equals(T), and if the argument is known at compile time to be of type T, then the compiler can call IEquatable<T>.Equals(T) instead of Object.Equals(object), and no cast is necessary, improving performance.

## [CA1067](https://docs.microsoft.com/visualstudio/code-quality/ca1067): Override Object.Equals(object) when implementing IEquatable<T>

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

When a type T implements the interface IEquatable<T>, it suggests to a user who sees a call to the Equals method in source code that an instance of the type can be equated with an instance of any other type. The user might be confused if their attempt to equate the type with an instance of another type fails to compile. This violates the "principle of least surprise".

## [CA1068](https://docs.microsoft.com/visualstudio/code-quality/ca1068): CancellationToken parameters must come last

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Method '{0}' should take CancellationToken as the last parameter

## [CA1069](https://docs.microsoft.com/visualstudio/code-quality/ca1069): Enums values should not be duplicated

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

The field reference '{0}' is duplicated in this bitwise initialization

## [CA1070](https://docs.microsoft.com/visualstudio/code-quality/ca1070): Do not declare event fields as virtual

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Do not declare virtual events in a base class. Overridden events in a derived class have undefined behavior. The C# compiler does not handle this correctly and it is unpredictable whether a subscriber to the derived event will actually be subscribing to the base class event.

## [CA1200](https://docs.microsoft.com/visualstudio/code-quality/ca1200): Avoid using cref tags with a prefix

|Item|Value|
|-|-|
|Category|Documentation|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Use of cref tags with prefixes should be avoided, since it prevents the compiler from verifying references and the IDE from updating references during refactorings. It is permissible to suppress this error at a single documentation site if the cref must use a prefix because the type being mentioned is not findable by the compiler. For example, if a cref is mentioning a special attribute in the full framework but you're in a file that compiles against the portable framework, or if you want to reference a type at higher layer of Roslyn, you should suppress the error. You should not suppress the error just because you want to take a shortcut and avoid using the full syntax.

## [CA1501](https://docs.microsoft.com/visualstudio/code-quality/ca1501): Avoid excessive inheritance

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Deeply nested type hierarchies can be difficult to follow, understand, and maintain. This rule limits analysis to hierarchies in the same module. To fix a violation of this rule, derive the type from a base type that is less deep in the inheritance hierarchy or eliminate some of the intermediate base types.

## [CA1502](https://docs.microsoft.com/visualstudio/code-quality/ca1502): Avoid excessive complexity

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Cyclomatic complexity measures the number of linearly independent paths through the method, which is determined by the number and complexity of conditional branches. A low cyclomatic complexity generally indicates a method that is easy to understand, test, and maintain. The cyclomatic complexity is calculated from a control flow graph of the method and is given as follows: `cyclomatic complexity = the number of edges - the number of nodes + 1`, where a node represents a logic branch point and an edge represents a line between nodes.

## [CA1505](https://docs.microsoft.com/visualstudio/code-quality/ca1505): Avoid unmaintainable code

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

The maintainability index is calculated by using the following metrics: lines of code, program volume, and cyclomatic complexity. Program volume is a measure of the difficulty of understanding of a symbol that is based on the number of operators and operands in the code. Cyclomatic complexity is a measure of the structural complexity of the type or method. A low maintainability index indicates that code is probably difficult to maintain and would be a good candidate to redesign.

## [CA1506](https://docs.microsoft.com/visualstudio/code-quality/ca1506): Avoid excessive class coupling

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

This rule measures class coupling by counting the number of unique type references that a symbol contains. Symbols that have a high degree of class coupling can be difficult to maintain. It is a good practice to have types and methods that exhibit low coupling and high cohesion. To fix this violation, try to redesign the code to reduce the number of types to which it is coupled.

## [CA1507](https://docs.microsoft.com/visualstudio/code-quality/ca1507): Use nameof to express symbol names

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Using nameof helps keep your code valid when refactoring.

## [CA1508](https://docs.microsoft.com/visualstudio/code-quality/ca1508): Avoid dead conditional code

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

'{0}' is never '{1}'. Remove or refactor the condition(s) to avoid dead code.

## [CA1509](https://docs.microsoft.com/visualstudio/code-quality/ca1509): Invalid entry in code metrics rule specification file

|Item|Value|
|-|-|
|Category|Maintainability|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Invalid entry in code metrics rule specification file.

## [CA1700](https://docs.microsoft.com/visualstudio/code-quality/ca1700): Do not name enum values 'Reserved'

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

This rule assumes that an enumeration member that has a name that contains "reserved" is not currently used but is a placeholder to be renamed or removed in a future version. Renaming or removing a member is a breaking change.

## [CA1707](https://docs.microsoft.com/visualstudio/code-quality/ca1707): Identifiers should not contain underscores

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

By convention, identifier names do not contain the underscore (_) character. This rule checks namespaces, types, members, and parameters.

## [CA1708](https://docs.microsoft.com/visualstudio/code-quality/ca1708): Identifiers should differ by more than case

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

Identifiers for namespaces, types, members, and parameters cannot differ only by case because languages that target the common language runtime are not required to be case-sensitive.

## [CA1710](https://docs.microsoft.com/visualstudio/code-quality/ca1710): Identifiers should have correct suffix

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

By convention, the names of types that extend certain base types or that implement certain interfaces, or types that are derived from these types, have a suffix that is associated with the base type or interface.

## [CA1711](https://docs.microsoft.com/visualstudio/code-quality/ca1711): Identifiers should not have incorrect suffix

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

By convention, only the names of types that extend certain base types or that implement certain interfaces, or types that are derived from these types, should end with specific reserved suffixes. Other type names should not use these reserved suffixes.

## [CA1712](https://docs.microsoft.com/visualstudio/code-quality/ca1712): Do not prefix enum values with type name

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

An enumeration's values should not start with the type name of the enumeration.

## [CA1713](https://docs.microsoft.com/visualstudio/code-quality/ca1713): Events should not have 'Before' or 'After' prefix

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Event names should describe the action that raises the event. To name related events that are raised in a specific sequence, use the present or past tense to indicate the relative position in the sequence of actions. For example, when naming a pair of events that is raised when closing a resource, you might name it 'Closing' and 'Closed', instead of 'BeforeClose' and 'AfterClose'.

## [CA1714](https://docs.microsoft.com/visualstudio/code-quality/ca1714): Flags enums should have plural names

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A public enumeration has the System.FlagsAttribute attribute, and its name does not end in ""s"". Types that are marked by using FlagsAttribute have names that are plural because the attribute indicates that more than one value can be specified.

## [CA1715](https://docs.microsoft.com/visualstudio/code-quality/ca1715): Identifiers should have correct prefix

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

The name of an externally visible interface does not start with an uppercase ""I"". The name of a generic type parameter on an externally visible type or method does not start with an uppercase ""T"".

## [CA1716](https://docs.microsoft.com/visualstudio/code-quality/ca1716): Identifiers should not match keywords

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A namespace name or a type name matches a reserved keyword in a programming language. Identifiers for namespaces and types should not match keywords that are defined by languages that target the common language runtime.

## [CA1717](https://docs.microsoft.com/visualstudio/code-quality/ca1717): Only FlagsAttribute enums should have plural names

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Naming conventions dictate that a plural name for an enumeration indicates that more than one value of the enumeration can be specified at the same time.

## [CA1720](https://docs.microsoft.com/visualstudio/code-quality/ca1720): Identifier contains type name

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Names of parameters and members are better used to communicate their meaning than to describe their type, which is expected to be provided by development tools. For names of members, if a data type name must be used, use a language-independent name instead of a language-specific one.

## [CA1721](https://docs.microsoft.com/visualstudio/code-quality/ca1721): Property names should not match get methods

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

The name of a public or protected member starts with ""Get"" and otherwise matches the name of a public or protected property. ""Get"" methods and properties should have names that clearly distinguish their function.

## [CA1724](https://docs.microsoft.com/visualstudio/code-quality/ca1724): Type names should not match namespaces

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Type names should not match the names of namespaces that are defined in the .NET Framework class library. Violating this rule can reduce the usability of the library.

## [CA1725](https://docs.microsoft.com/visualstudio/code-quality/ca1725): Parameter names should match base declaration

|Item|Value|
|-|-|
|Category|Naming|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

Consistent naming of parameters in an override hierarchy increases the usability of the method overrides. A parameter name in a derived method that differs from the name in the base declaration can cause confusion about whether the method is an override of the base method or a new overload of the method.

## [CA1801](https://docs.microsoft.com/visualstudio/code-quality/ca1801): Review unused parameters

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Avoid unused paramereters in your code. If the parameter cannot be removed, then change its name so it starts with an underscore and is optionally followed by an integer, such as '_', '_1', '_2', etc. These are treated as special discard symbol names.

## [CA1802](https://docs.microsoft.com/visualstudio/code-quality/ca1802): Use literals where appropriate

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

A field is declared static and read-only (Shared and ReadOnly in Visual Basic), and is initialized by using a value that is computable at compile time. Because the value that is assigned to the targeted field is computable at compile time, change the declaration to a const (Const in Visual Basic) field so that the value is computed at compile time instead of at run?time.

## [CA1805](https://docs.microsoft.com/visualstudio/code-quality/ca1805): Do not initialize unnecessarily

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

The .NET runtime initializes all fields of reference types to their default values before running the constructor. In most cases, explicitly initializing a field to its default value in a constructor is redundant, adding maintenance costs and potentially degrading performance (such as with increased assembly size), and the explicit initialization can be removed.  In some cases, such as with static readonly fields that permanently retain their default value, consider instead changing them to be constants or properties.

## [CA1806](https://docs.microsoft.com/visualstudio/code-quality/ca1806): Do not ignore method results

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A new object is created but never used; or a method that creates and returns a new string is called and the new string is never used; or a COM or P/Invoke method returns an HRESULT or error code that is never used.

## [CA1812](https://docs.microsoft.com/visualstudio/code-quality/ca1812): Avoid uninstantiated internal classes

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

An instance of an assembly-level type is not created by code in the assembly.

## [CA1814](https://docs.microsoft.com/visualstudio/code-quality/ca1814): Prefer jagged arrays over multidimensional

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A jagged array is an array whose elements are arrays. The arrays that make up the elements can be of different sizes, leading to less wasted space for some sets of data.

## [CA1815](https://docs.microsoft.com/visualstudio/code-quality/ca1815): Override equals and operator equals on value types

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

For value types, the inherited implementation of Equals uses the Reflection library and compares the contents of all fields. Reflection is computationally expensive, and comparing every field for equality might be unnecessary. If you expect users to compare or sort instances, or to use instances as hash table keys, your value type should implement Equals.

## [CA1819](https://docs.microsoft.com/visualstudio/code-quality/ca1819): Properties should not return arrays

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Arrays that are returned by properties are not write-protected, even when the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property.

## [CA1821](https://docs.microsoft.com/visualstudio/code-quality/ca1821): Remove empty Finalizers

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Finalizers should be avoided where possible, to avoid the additional performance overhead involved in tracking object lifetime.

## [CA1822](https://docs.microsoft.com/visualstudio/code-quality/ca1822): Mark members as static

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Members that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.

## [CA1823](https://docs.microsoft.com/visualstudio/code-quality/ca1823): Avoid unused private fields

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Private fields were detected that do not appear to be accessed in the assembly.

## [CA2007](https://docs.microsoft.com/visualstudio/code-quality/ca2007): Consider calling ConfigureAwait on the awaited task

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

When an asynchronous method awaits a Task directly, continuation occurs in the same thread that created the task. Consider calling Task.ConfigureAwait(Boolean) to signal your intention for continuation. Call ConfigureAwait(false) on the task to schedule continuations to the thread pool, thereby avoiding a deadlock on the UI thread. Passing false is a good option for app-independent libraries. Calling ConfigureAwait(true) on the task has the same behavior as not explicitly calling ConfigureAwait. By explicitly calling this method, you're letting readers know you intentionally want to perform the continuation on the original synchronization context.

## [CA2011](https://docs.microsoft.com/visualstudio/code-quality/ca2011): Avoid infinite recursion

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Do not assign the property within its setter. This call might result in an infinite recursion.

## [CA2109](https://docs.microsoft.com/visualstudio/code-quality/ca2109): Review visible event handlers

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|

### Rule description

A public or protected event-handling method was detected. Event-handling methods should not be exposed unless absolutely necessary.

## [CA2119](https://docs.microsoft.com/visualstudio/code-quality/ca2119): Seal methods that satisfy private interfaces

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

An inheritable public type provides an overridable method implementation of an internal (Friend in Visual Basic) interface. To fix a violation of this rule, prevent the method from being overridden outside the assembly.

## [CA2200](https://docs.microsoft.com/visualstudio/code-quality/ca2200): Rethrow to preserve stack details

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Re-throwing caught exception changes stack information

## [CA2211](https://docs.microsoft.com/visualstudio/code-quality/ca2211): Non-constant fields should not be visible

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Static fields that are neither constants nor read-only are not thread-safe. Access to such a field must be carefully controlled and requires advanced programming techniques to synchronize access to the class object.

## [CA2214](https://docs.microsoft.com/visualstudio/code-quality/ca2214): Do not call overridable methods in constructors

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Virtual methods defined on the class should not be called from constructors. If a derived class has overridden the method, the derived class version will be called (before the derived class constructor is called).

## [CA2217](https://docs.microsoft.com/visualstudio/code-quality/ca2217): Do not mark enums with FlagsAttribute

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|

### Rule description

An externally visible enumeration is marked by using FlagsAttribute, and it has one or more values that are not powers of two or a combination of the other defined values on the enumeration.

## [CA2218](https://docs.microsoft.com/visualstudio/code-quality/ca2218): Override GetHashCode on overriding Equals

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

GetHashCode returns a value, based on the current instance, that is suited for hashing algorithms and data structures such as a hash table. Two objects that are the same type and are equal must return the same hash code.

## [CA2219](https://docs.microsoft.com/visualstudio/code-quality/ca2219): Do not raise exceptions in finally clauses

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

When an exception is raised in a finally clause, the new exception hides the active exception. This makes the original error difficult to detect and debug.

## [CA2224](https://docs.microsoft.com/visualstudio/code-quality/ca2224): Override Equals on overloading operator equals

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

A public type implements the equality operator but does not override Object.Equals.

## [CA2225](https://docs.microsoft.com/visualstudio/code-quality/ca2225): Operator overloads have named alternates

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

An operator overload was detected, and the expected named alternative method was not found. The named alternative member provides access to the same functionality as the operator and is provided for developers who program in languages that do not support overloaded operators.

## [CA2226](https://docs.microsoft.com/visualstudio/code-quality/ca2226): Operators should have symmetrical overloads

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

A type implements the equality or inequality operator and does not implement the opposite operator.

## [CA2227](https://docs.microsoft.com/visualstudio/code-quality/ca2227): Collection properties should be read only

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A writable collection property allows a user to replace the collection with a different collection. A read-only property stops the collection from being replaced but still allows the individual members to be set.

## [CA2231](https://docs.microsoft.com/visualstudio/code-quality/ca2231): Overload operator equals on overriding value type Equals

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

In most programming languages there is no default implementation of the equality operator (==) for value types. If your programming language supports operator overloads, you should consider implementing the equality operator. Its behavior should be identical to that of Equals.

## [CA2234](https://docs.microsoft.com/visualstudio/code-quality/ca2234): Pass system uri objects instead of strings

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

A call is made to a method that has a string parameter whose name contains "uri", "URI", "urn", "URN", "url", or "URL". The declaring type of the method contains a corresponding method overload that has a System.Uri parameter.

## [CA2244](https://docs.microsoft.com/visualstudio/code-quality/ca2244): Do not duplicate indexed element initializations

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|

### Rule description

Indexed elements in objects initializers must initialize unique elements. A duplicate index might overwrite a previous element initialization.

## [CA2245](https://docs.microsoft.com/visualstudio/code-quality/ca2245): Do not assign a property to itself

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

The property {0} should not be assigned to itself

## [CA2246](https://docs.microsoft.com/visualstudio/code-quality/ca2246): Assigning symbol and its member in the same statement

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|

### Rule description

Assigning to a symbol and its member (field/property) in the same statement is not recommended. It is not clear if the member access was intended to use symbol's old value prior to the assignment or new value from the assignment in this statement. For clarity, consider splitting the assignments into separate statements.

