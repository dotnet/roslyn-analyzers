### CA1000: Do not declare static members on generic types ###

When a static member of a generic type is called, the type argument must be specified for the type. When a generic instance member that does not support inference is called, the type argument must be specified for the member. In these two cases, the syntax for specifying the type argument is different and easily confused.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1000-do-not-declare-static-members-on-generic-types](https://docs.microsoft.com/visualstudio/code-quality/ca1000-do-not-declare-static-members-on-generic-types)

### CA1001: Types that own disposable fields should be disposable ###

A class declares and implements an instance field that is a System.IDisposable type, and the class does not implement IDisposable. A class that declares an IDisposable field indirectly owns an unmanaged resource and should implement the IDisposable interface.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1001-types-that-own-disposable-fields-should-be-disposable](https://docs.microsoft.com/visualstudio/code-quality/ca1001-types-that-own-disposable-fields-should-be-disposable)

### CA1003: Use generic event handler instances ###

A type contains an event that declares an EventHandler delegate that returns void, whose signature contains two parameters (the first an object and the second a type that is assignable to EventArgs), and the containing assembly targets Microsoft .NET Framework?2.0.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1003-use-generic-event-handler-instances](https://docs.microsoft.com/visualstudio/code-quality/ca1003-use-generic-event-handler-instances)

### CA1008: Enums should have zero value ###

The default value of an uninitialized enumeration, just as other value types, is zero. A nonflags-attributed enumeration should define a member by using the value of zero so that the default value is a valid value of the enumeration. If an enumeration that has the FlagsAttribute attribute applied defines a zero-valued member, its name should be ""None"" to indicate that no values have been set in the enumeration.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1008-enums-should-have-zero-value](https://docs.microsoft.com/visualstudio/code-quality/ca1008-enums-should-have-zero-value)

### CA1010: Collections should implement generic interface ###

To broaden the usability of a collection, implement one of the generic collection interfaces. Then the collection can be used to populate generic collection types.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1010-collections-should-implement-generic-interface](https://docs.microsoft.com/visualstudio/code-quality/ca1010-collections-should-implement-generic-interface)

### CA1012: Abstract types should not have constructors ###

Constructors on abstract types can be called only by derived types. Because public constructors create instances of a type, and you cannot create instances of an abstract type, an abstract type that has a public constructor is incorrectly designed.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1012-abstract-types-should-not-have-constructors](https://docs.microsoft.com/visualstudio/code-quality/ca1012-abstract-types-should-not-have-constructors)

### CA1014: Mark assemblies with CLSCompliant ###

The Common Language Specification (CLS) defines naming restrictions, data types, and rules to which assemblies must conform if they will be used across programming languages. Good design dictates that all assemblies explicitly indicate CLS compliance by using CLSCompliantAttribute . If this attribute is not present on an assembly, the assembly is not compliant.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1014-mark-assemblies-with-clscompliantattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1014-mark-assemblies-with-clscompliantattribute)

### CA1016: Mark assemblies with assembly version ###

The .NET Framework uses the version number to uniquely identify an assembly, and to bind to types in strongly named assemblies. The version number is used together with version and publisher policy. By default, applications run only with the assembly version with which they were built.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1016-mark-assemblies-with-assemblyversionattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1016-mark-assemblies-with-assemblyversionattribute)

### CA1017: Mark assemblies with ComVisible ###

ComVisibleAttribute determines how COM clients access managed code. Good design dictates that assemblies explicitly indicate COM visibility. COM visibility can be set for the whole assembly and then overridden for individual types and type members. If this attribute is not present, the contents of the assembly are visible to COM clients.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1017-mark-assemblies-with-comvisibleattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1017-mark-assemblies-with-comvisibleattribute)

### CA1018: Mark attributes with AttributeUsageAttribute ###

Specify AttributeUsage on {0}.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1018-mark-attributes-with-attributeusageattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1018-mark-attributes-with-attributeusageattribute)

### CA1019: Define accessors for attribute arguments ###

Remove the property setter from {0} or reduce its accessibility because it corresponds to positional argument {1}.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1019-define-accessors-for-attribute-arguments](https://docs.microsoft.com/visualstudio/code-quality/ca1019-define-accessors-for-attribute-arguments)

### CA1024: Use properties where appropriate ###

A public or protected method has a name that starts with ""Get"", takes no parameters, and returns a value that is not an array. The method might be a good candidate to become a property.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1024-use-properties-where-appropriate](https://docs.microsoft.com/visualstudio/code-quality/ca1024-use-properties-where-appropriate)

### CA1027: Mark enums with FlagsAttribute ###

An enumeration is a value type that defines a set of related named constants. Apply FlagsAttribute to an enumeration when its named constants can be meaningfully combined.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1027-mark-enums-with-flagsattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1027-mark-enums-with-flagsattribute)

### CA1028: Enum Storage should be Int32 ###

An enumeration is a value type that defines a set of related named constants. By default, the System.Int32 data type is used to store the constant value. Although you can change this underlying type, it is not required or recommended for most scenarios.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1028-enum-storage-should-be-int32](https://docs.microsoft.com/visualstudio/code-quality/ca1028-enum-storage-should-be-int32)

### CA1030: Use events where appropriate ###

This rule detects methods that have names that ordinarily would be used for events. If a method is called in response to a clearly defined state change, the method should be invoked by an event handler. Objects that call the method should raise events instead of calling the method directly.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1030-use-events-where-appropriate](https://docs.microsoft.com/visualstudio/code-quality/ca1030-use-events-where-appropriate)

### CA1031: Do not catch general exception types ###

A general exception such as System.Exception or System.SystemException is caught in a catch statement, or a general catch clause is used. General exceptions should not be caught.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://msdn.microsoft.com/en-us/library/ms182137.aspx](https://msdn.microsoft.com/en-us/library/ms182137.aspx)

### CA1032: Implement standard exception constructors ###

Failure to provide the full set of constructors can make it difficult to correctly handle exceptions.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1032-implement-standard-exception-constructors](https://docs.microsoft.com/visualstudio/code-quality/ca1032-implement-standard-exception-constructors)

### CA1033: Interface methods should be callable by child types ###

An unsealed externally visible type provides an explicit method implementation of a public interface and does not provide an alternative externally visible method that has the same name.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1033-interface-methods-should-be-callable-by-child-types](https://docs.microsoft.com/visualstudio/code-quality/ca1033-interface-methods-should-be-callable-by-child-types)

### CA1034: Nested types should not be visible ###

A nested type is a type that is declared in the scope of another type. Nested types are useful to encapsulate private implementation details of the containing type. Used for this purpose, nested types should not be externally visible.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1034-nested-types-should-not-be-visible](https://docs.microsoft.com/visualstudio/code-quality/ca1034-nested-types-should-not-be-visible)

### CA1036: Override methods on comparable types ###

A public or protected type implements the System.IComparable interface. It does not override Object.Equals nor does it overload the language-specific operator for equality, inequality, less than, less than or equal, greater than or greater than or equal.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1036-override-methods-on-comparable-types](https://docs.microsoft.com/visualstudio/code-quality/ca1036-override-methods-on-comparable-types)

### CA1040: Avoid empty interfaces ###

Interfaces define members that provide a behavior or usage contract. The functionality that is described by the interface can be adopted by any type, regardless of where the type appears in the inheritance hierarchy. A type implements an interface by providing implementations for the members of the interface. An empty interface does not define any members; therefore, it does not define a contract that can be implemented.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1040-avoid-empty-interfaces](https://docs.microsoft.com/visualstudio/code-quality/ca1040-avoid-empty-interfaces)

### CA1041: Provide ObsoleteAttribute message ###

A type or member is marked by using a System.ObsoleteAttribute attribute that does not have its ObsoleteAttribute.Message property specified. When a type or member that is marked by using ObsoleteAttribute is compiled, the Message property of the attribute is displayed. This gives the user information about the obsolete type or member.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1041-provide-obsoleteattribute-message](https://docs.microsoft.com/visualstudio/code-quality/ca1041-provide-obsoleteattribute-message)

### CA1043: Use Integral Or String Argument For Indexers ###

Indexers, that is, indexed properties, should use integer or string types for the index. These types are typically used for indexing data structures and increase the usability of the library. Use of the Object type should be restricted to those cases where the specific integer or string type cannot be specified at design time. If the design requires other types for the index, reconsider whether the type represents a logical data store. If it does not represent a logical data store, use a method.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1043-use-integral-or-string-argument-for-indexers](https://docs.microsoft.com/visualstudio/code-quality/ca1043-use-integral-or-string-argument-for-indexers)

### CA1044: Properties should not be write only ###

Although it is acceptable and often necessary to have a read-only property, the design guidelines prohibit the use of write-only properties. This is because letting a user set a value, and then preventing the user from viewing that value, does not provide any security. Also, without read access, the state of shared objects cannot be viewed, which limits their usefulness.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1044-properties-should-not-be-write-only](https://docs.microsoft.com/visualstudio/code-quality/ca1044-properties-should-not-be-write-only)

### CA1050: Declare types in namespaces ###

Types are declared in namespaces to prevent name collisions and as a way to organize related types in an object hierarchy.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1050-declare-types-in-namespaces](https://docs.microsoft.com/visualstudio/code-quality/ca1050-declare-types-in-namespaces)

### CA1051: Do not declare visible instance fields ###

The primary use of a field should be as an implementation detail. Fields should be private or internal and should be exposed by using properties.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1051-do-not-declare-visible-instance-fields](https://docs.microsoft.com/visualstudio/code-quality/ca1051-do-not-declare-visible-instance-fields)

### CA1052: Static holder types should be Static or NotInheritable ###

Type '{0}' is a static holder type but is neither static nor NotInheritable

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1052-static-holder-types-should-be-sealed](https://docs.microsoft.com/visualstudio/code-quality/ca1052-static-holder-types-should-be-sealed)

### CA1054: Uri parameters should not be strings ###

If a method takes a string representation of a URI, a corresponding overload should be provided that takes an instance of the URI class, which provides these services in a safe and secure manner.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1054-uri-parameters-should-not-be-strings](https://docs.microsoft.com/visualstudio/code-quality/ca1054-uri-parameters-should-not-be-strings)

### CA1055: Uri return values should not be strings ###

This rule assumes that the method returns a URI. A string representation of a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. The System.Uri class provides these services in a safe and secure manner.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1055-uri-return-values-should-not-be-strings](https://docs.microsoft.com/visualstudio/code-quality/ca1055-uri-return-values-should-not-be-strings)

### CA1056: Uri properties should not be strings ###

This rule assumes that the property represents a Uniform Resource Identifier (URI). A string representation of a URI is prone to parsing and encoding errors, and can lead to security vulnerabilities. The System.Uri class provides these services in a safe and secure manner.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1056-uri-properties-should-not-be-strings](https://docs.microsoft.com/visualstudio/code-quality/ca1056-uri-properties-should-not-be-strings)

### CA1058: Types should not extend certain base types ###

An externally visible type extends certain base types. Use one of the alternatives.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1058-types-should-not-extend-certain-base-types](https://docs.microsoft.com/visualstudio/code-quality/ca1058-types-should-not-extend-certain-base-types)

### CA1060: Move pinvokes to native methods class ###

Platform Invocation methods, such as those that are marked by using the System.Runtime.InteropServices.DllImportAttribute attribute, or methods that are defined by using the Declare keyword in Visual Basic, access unmanaged code. These methods should be of the NativeMethods, SafeNativeMethods, or UnsafeNativeMethods class.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1060-move-p-invokes-to-nativemethods-class](https://docs.microsoft.com/visualstudio/code-quality/ca1060-move-p-invokes-to-nativemethods-class)

### CA1061: Do not hide base class methods ###

A method in a base type is hidden by an identically named method in a derived type when the parameter signature of the derived method differs only by types that are more weakly derived than the corresponding types in the parameter signature of the base method.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1061-do-not-hide-base-class-methods](https://docs.microsoft.com/visualstudio/code-quality/ca1061-do-not-hide-base-class-methods)

### CA1062: Validate arguments of public methods ###

An externally visible method dereferences one of its reference arguments without verifying whether that argument is null (Nothing in Visual Basic). All reference arguments that are passed to externally visible methods should be checked against null. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument. If the method is designed to be called only by known assemblies, you should make the method internal.

Category: Design

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1062-validate-arguments-of-public-methods](https://docs.microsoft.com/visualstudio/code-quality/ca1062-validate-arguments-of-public-methods)

### CA1063: Implement IDisposable Correctly ###

All IDisposable types should implement the Dispose pattern correctly.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1063-implement-idisposable-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca1063-implement-idisposable-correctly)

### CA1064: Exceptions should be public ###

An internal exception is visible only inside its own internal scope. After the exception falls outside the internal scope, only the base exception can be used to catch the exception. If the internal exception is inherited from T:System.Exception, T:System.SystemException, or T:System.ApplicationException, the external code will not have sufficient information to know what to do with the exception.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1064-exceptions-should-be-public](https://docs.microsoft.com/visualstudio/code-quality/ca1064-exceptions-should-be-public)

### CA1065: Do not raise exceptions in unexpected locations ###

A method that is not expected to throw exceptions throws an exception.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1065-do-not-raise-exceptions-in-unexpected-locations](https://docs.microsoft.com/visualstudio/code-quality/ca1065-do-not-raise-exceptions-in-unexpected-locations)

### CA1066: Type {0} should implement IEquatable<T> because it overrides Equals ###

When a type T overrides Object.Equals(object), the implementation must cast the object argument to the correct type T before performing the comparison. If the type implements IEquatable<T>, and therefore offers the method T.Equals(T), and if the argument is known at compile time to be of type T, then the compiler can call IEquatable<T>.Equals(T) instead of Object.Equals(object), and no cast is necessary, improving performance.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [http://go.microsoft.com/fwlink/?LinkId=734907](http://go.microsoft.com/fwlink/?LinkId=734907)

### CA1067: Override Object.Equals(object) when implementing IEquatable<T> ###

When a type T implements the interface IEquatable<T>, it suggests to a user who sees a call to the Equals method in source code that an instance of the type can be equated with an instance of any other type. The user might be confused if their attempt to equate the type with an instance of another type fails to compile. This violates the "principle of least surprise".

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [http://go.microsoft.com/fwlink/?LinkId=734909](http://go.microsoft.com/fwlink/?LinkId=734909)

### CA1068: CancellationToken parameters must come last ###

Method '{0}' should take CancellationToken as the last parameter

Category: Design

Severity: Warning

IsEnabledByDefault: True

### CA1200: Avoid using cref tags with a prefix ###

Use of cref tags with prefixes should be avoided, since it prevents the compiler from verifying references and the IDE from updating references during refactorings. It is permissible to suppress this error at a single documentation site if the cref must use a prefix because the type being mentioned is not findable by the compiler. For example, if a cref is mentioning a special attribute in the full framework but you're in a file that compiles against the portable framework, or if you want to reference a type at higher layer of Roslyn, you should suppress the error. You should not suppress the error just because you want to take a shortcut and avoid using the full syntax.

Category: Documentation

Severity: Warning

IsEnabledByDefault: True

### CA1303: Do not pass literals as localized parameters ###

A method passes a string literal as a parameter to a constructor or method in the .NET Framework class library and that string should be localizable. To fix a violation of this rule, replace the string literal with a string retrieved through an instance of the ResourceManager class.

Category: Globalization

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1303-do-not-pass-literals-as-localized-parameters](https://docs.microsoft.com/visualstudio/code-quality/ca1303-do-not-pass-literals-as-localized-parameters)

### CA1304: Specify CultureInfo ###

A method or constructor calls a member that has an overload that accepts a System.Globalization.CultureInfo parameter, and the method or constructor does not call the overload that takes the CultureInfo parameter. When a CultureInfo or System.IFormatProvider object is not supplied, the default value that is supplied by the overloaded member might not have the effect that you want in all locales. If the result will be displayed to the user, specify 'CultureInfo.CurrentCulture' as the 'CultureInfo' parameter. Otherwise, if the result will be stored and accessed by software, such as when it is persisted to disk or to a database, specify 'CultureInfo.InvariantCulture'.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1304-specify-cultureinfo](https://docs.microsoft.com/visualstudio/code-quality/ca1304-specify-cultureinfo)

### CA1305: Specify IFormatProvider ###

A method or constructor calls one or more members that have overloads that accept a System.IFormatProvider parameter, and the method or constructor does not call the overload that takes the IFormatProvider parameter. When a System.Globalization.CultureInfo or IFormatProvider object is not supplied, the default value that is supplied by the overloaded member might not have the effect that you want in all locales. If the result will be based on the input from/output displayed to the user, specify 'CultureInfo.CurrentCulture' as the 'IFormatProvider'. Otherwise, if the result will be stored and accessed by software, such as when it is loaded from disk/database and when it is persisted to disk/database, specify 'CultureInfo.InvariantCulture'

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1305-specify-iformatprovider](https://docs.microsoft.com/visualstudio/code-quality/ca1305-specify-iformatprovider)

### CA1307: Specify StringComparison ###

A string comparison operation uses a method overload that does not set a StringComparison parameter. If the result will be displayed to the user, such as when sorting a list of items for display in a list box, specify 'StringComparison.CurrentCulture' or 'StringComparison.CurrentCultureIgnoreCase' as the 'StringComparison' parameter. If comparing case-insensitive identifiers, such as file paths, environment variables, or registry keys and values, specify 'StringComparison.OrdinalIgnoreCase'. Otherwise, if comparing case-sensitive identifiers, specify 'StringComparison.Ordinal'.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1307-specify-stringcomparison](https://docs.microsoft.com/visualstudio/code-quality/ca1307-specify-stringcomparison)

### CA1308: Normalize strings to uppercase ###

Strings should be normalized to uppercase. A small group of characters cannot make a round trip when they are converted to lowercase. To make a round trip means to convert the characters from one locale to another locale that represents character data differently, and then to accurately retrieve the original characters from the converted characters.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1308-normalize-strings-to-uppercase](https://docs.microsoft.com/visualstudio/code-quality/ca1308-normalize-strings-to-uppercase)

### CA1309: Use ordinal stringcomparison ###

A string comparison operation that is nonlinguistic does not set the StringComparison parameter to either Ordinal or OrdinalIgnoreCase. By explicitly setting the parameter to either StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase, your code often gains speed, becomes more correct, and becomes more reliable.

Category: Globalization

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison](https://docs.microsoft.com/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison)

### CA1401: P/Invokes should not be visible ###

A public or protected method in a public type has the System.Runtime.InteropServices.DllImportAttribute attribute (also implemented by the Declare keyword in Visual Basic). Such methods should not be exposed.

Category: Interoperability

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1401-p-invokes-should-not-be-visible](https://docs.microsoft.com/visualstudio/code-quality/ca1401-p-invokes-should-not-be-visible)

### CA1501: Avoid excessive inheritance ###

Deeply nested type hierarchies can be difficult to follow, understand, and maintain. This rule limits analysis to hierarchies in the same module. To fix a violation of this rule, derive the type from a base type that is less deep in the inheritance hierarchy or eliminate some of the intermediate base types.

Category: Maintainability

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1501-avoid-excessive-inheritance](https://docs.microsoft.com/visualstudio/code-quality/ca1501-avoid-excessive-inheritance)

### CA1502: Avoid excessive complexity ###

Cyclomatic complexity measures the number of linearly independent paths through the method, which is determined by the number and complexity of conditional branches. A low cyclomatic complexity generally indicates a method that is easy to understand, test, and maintain. The cyclomatic complexity is calculated from a control flow graph of the method and is given as follows:

cyclomatic complexity = the number of edges - the number of nodes + 1

where a node represents a logic branch point and an edge represents a line between nodes.

Category: Maintainability

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1502-avoid-excessive-complexity](https://docs.microsoft.com/visualstudio/code-quality/ca1502-avoid-excessive-complexity)

### CA1505: Avoid unmaintainable code ###

The maintainability index is calculated by using the following metrics: lines of code, program volume, and cyclomatic complexity. Program volume is a measure of the difficulty of understanding of a symbol that is based on the number of operators and operands in the code. Cyclomatic complexity is a measure of the structural complexity of the type or method.
A low maintainability index indicates that code is probably difficult to maintain and would be a good candidate to redesign.

Category: Maintainability

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1505-avoid-unmaintainable-code](https://docs.microsoft.com/visualstudio/code-quality/ca1505-avoid-unmaintainable-code)

### CA1506: Avoid excessive class coupling ###

This rule measures class coupling by counting the number of unique type references that a symbol contains. Symbols that have a high degree of class coupling can be difficult to maintain. It is a good practice to have types and methods that exhibit low coupling and high cohesion. To fix this violation, try to redesign the code to reduce the number of types to which it is coupled.

Category: Maintainability

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1506-avoid-excessive-class-coupling](https://docs.microsoft.com/visualstudio/code-quality/ca1506-avoid-excessive-class-coupling)

### CA1507: Use nameof to express symbol names ###

Using nameof helps keep your code valid when refactoring.

Category: Maintainability

Severity: Warning

IsEnabledByDefault: True

Help: [https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeQuality.Analyzers/Microsoft.CodeQuality.Analyzers.md#maintainability](https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeQuality.Analyzers/Microsoft.CodeQuality.Analyzers.md#maintainability)

### CA1508: Avoid dead conditional code ###

'{0}' is always '{1}'. Remove or refactor the condition(s) to avoid dead code.

Category: Maintainability

Severity: Warning

IsEnabledByDefault: False

### CA1509: Invalid entry in code metrics rule specification file ###

Invalid entry in code metrics rule specification file

Category: Maintainability

Severity: Warning

IsEnabledByDefault: False

### CA1707: Identifiers should not contain underscores ###

By convention, identifier names do not contain the underscore (_) character. This rule checks namespaces, types, members, and parameters.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1707-identifiers-should-not-contain-underscores](https://docs.microsoft.com/visualstudio/code-quality/ca1707-identifiers-should-not-contain-underscores)

### CA1708: Identifiers should differ by more than case ###

Identifiers for namespaces, types, members, and parameters cannot differ only by case because languages that target the common language runtime are not required to be case-sensitive.

Category: Naming

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1708-identifiers-should-differ-by-more-than-case](https://docs.microsoft.com/visualstudio/code-quality/ca1708-identifiers-should-differ-by-more-than-case)

### CA1710: Identifiers should have correct suffix ###

By convention, the names of types that extend certain base types or that implement certain interfaces, or types that are derived from these types, have a suffix that is associated with the base type or interface.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1710-identifiers-should-have-correct-suffix](https://docs.microsoft.com/visualstudio/code-quality/ca1710-identifiers-should-have-correct-suffix)

### CA1711: Identifiers should not have incorrect suffix ###

By convention, only the names of types that extend certain base types or that implement certain interfaces, or types that are derived from these types, should end with specific reserved suffixes. Other type names should not use these reserved suffixes.

Category: Naming

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1711-identifiers-should-not-have-incorrect-suffix](https://docs.microsoft.com/visualstudio/code-quality/ca1711-identifiers-should-not-have-incorrect-suffix)

### CA1712: Do not prefix enum values with type name ###

An enumeration's values should not start with the type name of the enumeration.

Category: Naming

Severity: Error

IsEnabledByDefault: True

Help: [https://msdn.microsoft.com/en-us/library/ms182237.aspx](https://msdn.microsoft.com/en-us/library/ms182237.aspx)

### CA1714: Flags enums should have plural names ###

A public enumeration has the System.FlagsAttribute attribute, and its name does not end in ""s"". Types that are marked by using FlagsAttribute have names that are plural because the attribute indicates that more than one value can be specified.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1714-flags-enums-should-have-plural-names](https://docs.microsoft.com/visualstudio/code-quality/ca1714-flags-enums-should-have-plural-names)

### CA1715: Identifiers should have correct prefix ###

Identifiers should have correct prefix

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1715-identifiers-should-have-correct-prefix](https://docs.microsoft.com/visualstudio/code-quality/ca1715-identifiers-should-have-correct-prefix)

### CA1716: Identifiers should not match keywords ###

A namespace name or a type name matches a reserved keyword in a programming language. Identifiers for namespaces and types should not match keywords that are defined by languages that target the common language runtime.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1716-identifiers-should-not-match-keywords](https://docs.microsoft.com/visualstudio/code-quality/ca1716-identifiers-should-not-match-keywords)

### CA1717: Only FlagsAttribute enums should have plural names ###

Naming conventions dictate that a plural name for an enumeration indicates that more than one value of the enumeration can be specified at the same time.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1717-only-flagsattribute-enums-should-have-plural-names](https://docs.microsoft.com/visualstudio/code-quality/ca1717-only-flagsattribute-enums-should-have-plural-names)

### CA1720: Identifier contains type name ###

Names of parameters and members are better used to communicate their meaning than to describe their type, which is expected to be provided by development tools. For names of members, if a data type name must be used, use a language-independent name instead of a language-specific one.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1720-identifiers-should-not-contain-type-names](https://docs.microsoft.com/visualstudio/code-quality/ca1720-identifiers-should-not-contain-type-names)

### CA1721: Property names should not match get methods ###

The name of a public or protected member starts with ""Get"" and otherwise matches the name of a public or protected property. ""Get"" methods and properties should have names that clearly distinguish their function.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1721-property-names-should-not-match-get-methods](https://docs.microsoft.com/visualstudio/code-quality/ca1721-property-names-should-not-match-get-methods)

### CA1724: Type names should not match namespaces ###

Type names should not match the names of namespaces that are defined in the .NET Framework class library. Violating this rule can reduce the usability of the library.

Category: Naming

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1724-type-names-should-not-match-namespaces](https://docs.microsoft.com/visualstudio/code-quality/ca1724-type-names-should-not-match-namespaces)

### CA1725: Parameter names should match base declaration ###

Consistent naming of parameters in an override hierarchy increases the usability of the method overrides. A parameter name in a derived method that differs from the name in the base declaration can cause confusion about whether the method is an override of the base method or a new overload of the method.

Category: Naming

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1725-parameter-names-should-match-base-declaration](https://docs.microsoft.com/visualstudio/code-quality/ca1725-parameter-names-should-match-base-declaration)

### CA1801: Review unused parameters ###

A method signature includes a parameter that is not used in the method body.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1801-review-unused-parameters](https://docs.microsoft.com/visualstudio/code-quality/ca1801-review-unused-parameters)

### CA1802: Use literals where appropriate ###

A field is declared static and read-only (Shared and ReadOnly in Visual Basic), and is initialized by using a value that is computable at compile time. Because the value that is assigned to the targeted field is computable at compile time, change the declaration to a const (Const in Visual Basic) field so that the value is computed at compile time instead of at run?time.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1802-use-literals-where-appropriate](https://docs.microsoft.com/visualstudio/code-quality/ca1802-use-literals-where-appropriate)

### CA1806: Do not ignore method results ###

A new object is created but never used; or a method that creates and returns a new string is called and the new string is never used; or a COM or P/Invoke method returns an HRESULT or error code that is never used.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1806-do-not-ignore-method-results](https://docs.microsoft.com/visualstudio/code-quality/ca1806-do-not-ignore-method-results)

### CA1810: Initialize reference type static fields inline ###

A reference type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1810-initialize-reference-type-static-fields-inline](https://docs.microsoft.com/visualstudio/code-quality/ca1810-initialize-reference-type-static-fields-inline)

### CA1812: Avoid uninstantiated internal classes ###

An instance of an assembly-level type is not created by code in the assembly.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1812-avoid-uninstantiated-internal-classes](https://docs.microsoft.com/visualstudio/code-quality/ca1812-avoid-uninstantiated-internal-classes)

### CA1813: Avoid unsealed attributes ###

The .NET Framework class library provides methods for retrieving custom attributes. By default, these methods search the attribute inheritance hierarchy. Sealing the attribute eliminates the search through the inheritance hierarchy and can improve performance.

Category: Performance

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1813-avoid-unsealed-attributes](https://docs.microsoft.com/visualstudio/code-quality/ca1813-avoid-unsealed-attributes)

### CA1814: Prefer jagged arrays over multidimensional ###

A jagged array is an array whose elements are arrays. The arrays that make up the elements can be of different sizes, leading to less wasted space for some sets of data.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1814-prefer-jagged-arrays-over-multidimensional](https://docs.microsoft.com/visualstudio/code-quality/ca1814-prefer-jagged-arrays-over-multidimensional)

### CA1815: Override equals and operator equals on value types ###

For value types, the inherited implementation of Equals uses the Reflection library and compares the contents of all fields. Reflection is computationally expensive, and comparing every field for equality might be unnecessary. If you expect users to compare or sort instances, or to use instances as hash table keys, your value type should implement Equals.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1815-override-equals-and-operator-equals-on-value-types](https://docs.microsoft.com/visualstudio/code-quality/ca1815-override-equals-and-operator-equals-on-value-types)

### CA1816: Dispose methods should call SuppressFinalize ###

A method that is an implementation of Dispose does not call GC.SuppressFinalize; or a method that is not an implementation of Dispose calls GC.SuppressFinalize; or a method calls GC.SuppressFinalize and passes something other than this (Me in Visual?Basic).

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1816-call-gc-suppressfinalize-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca1816-call-gc-suppressfinalize-correctly)

### CA1819: Properties should not return arrays ###

Arrays that are returned by properties are not write-protected, even when the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1819-properties-should-not-return-arrays](https://docs.microsoft.com/visualstudio/code-quality/ca1819-properties-should-not-return-arrays)

### CA1820: Test for empty strings using string length ###

Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1820-test-for-empty-strings-using-string-length](https://docs.microsoft.com/visualstudio/code-quality/ca1820-test-for-empty-strings-using-string-length)

### CA1821: Remove empty Finalizers ###

Finalizers should be avoided where possible, to avoid the additional performance overhead involved in tracking object lifetime.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1821-remove-empty-finalizers](https://docs.microsoft.com/visualstudio/code-quality/ca1821-remove-empty-finalizers)

### CA1822: Mark members as static ###

Members that do not access instance data or call instance methods can be marked as static (Shared in Visual Basic). After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1822-mark-members-as-static](https://docs.microsoft.com/visualstudio/code-quality/ca1822-mark-members-as-static)

### CA1823: Avoid unused private fields ###

Private fields were detected that do not appear to be accessed in the assembly.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1823-avoid-unused-private-fields](https://docs.microsoft.com/visualstudio/code-quality/ca1823-avoid-unused-private-fields)

### CA1824: Mark assemblies with NeutralResourcesLanguageAttribute ###

The NeutralResourcesLanguage attribute informs the ResourceManager of the language that was used to display the resources of a neutral culture for an assembly. This improves lookup performance for the first resource that you load and can reduce your working set.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1824-mark-assemblies-with-neutralresourceslanguageattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1824-mark-assemblies-with-neutralresourceslanguageattribute)

### CA1825: Avoid zero-length array allocations. ###

Avoid unnecessary zero-length array allocations.  Use {0} instead.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### CA1826: Do not use Enumerable methods on indexable collections. Instead use the collection directly ###

This collection is directly indexable. Going through LINQ here causes unnecessary allocations and CPU work.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### CA2000: Dispose objects before losing scope ###

If a disposable object is not explicitly disposed before all references to it are out of scope, the object will be disposed at some indeterminate time when the garbage collector runs the finalizer of the object. Because an exceptional event might occur that will prevent the finalizer of the object from running, the object should be explicitly disposed instead.

Category: Reliability

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope](https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope)

### CA2002: Do not lock on objects with weak identity ###

An object is said to have a weak identity when it can be directly accessed across application domain boundaries. A thread that tries to acquire a lock on an object that has a weak identity can be blocked by a second thread in a different application domain that has a lock on the same object.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2002-do-not-lock-on-objects-with-weak-identity](https://docs.microsoft.com/visualstudio/code-quality/ca2002-do-not-lock-on-objects-with-weak-identity)

### CA2007: Do not directly await a Task ###

While authoring a library where the consumer is not known and when there is no need for a SynchronizationContext,  consider using ConfigureAwait(false). Otherwise, the consumers of the library may face deadlocks by consuming the asynchronous methods in a blocking fashion.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2008: Do not create tasks without passing a TaskScheduler ###

Do not create tasks unless you are using one of the overloads that takes a TaskScheduler. The default is to schedule on TaskScheduler.Current, which would lead to deadlocks. Either use TaskScheduler.Default to schedule on the thread pool, or explicitly pass TaskScheduler.Current to make your intentions clear.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2009: Do not call ToImmutableCollection on an ImmutableCollection value ###

Do not call {0} on an {1} value

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2010: Always consume the value returned by methods marked with PreserveSigAttribute ###

PreserveSigAttribute indicates that a method will return an HRESULT, rather than throwing an exception. Therefore, it is important to consume the HRESULT returned by the method, so that errors can be detected. Generally, this is done by calling Marshal.ThrowExceptionForHR.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2100: Review SQL queries for security vulnerabilities ###

SQL queries that directly use user input can be vulnerable to SQL injection attacks. Review this SQL query for potential vulnerabilities, and consider using a parameterized SQL query.

Category: Security

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities](https://docs.microsoft.com/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities)

### CA2101: Specify marshaling for P/Invoke string arguments ###

A platform invoke member allows partially trusted callers, has a string parameter, and does not explicitly marshal the string. This can cause a potential security vulnerability.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2101-specify-marshaling-for-p-invoke-string-arguments](https://docs.microsoft.com/visualstudio/code-quality/ca2101-specify-marshaling-for-p-invoke-string-arguments)

### CA2119: Seal methods that satisfy private interfaces ###

An inheritable public type provides an overridable method implementation of an internal (Friend in Visual Basic) interface. To fix a violation of this rule, prevent the method from being overridden outside the assembly.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2119-seal-methods-that-satisfy-private-interfaces](https://docs.microsoft.com/visualstudio/code-quality/ca2119-seal-methods-that-satisfy-private-interfaces)

### CA2153: Do Not Catch Corrupted State Exceptions ###

Catching corrupted state exceptions could mask errors (such as access violations), resulting in inconsistent state of execution or making it easier for attackers to compromise system. Instead, catch and handle a more specific set of exception type(s) or re-throw the exception

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2153-avoid-handling-corrupted-state-exceptions](https://docs.microsoft.com/visualstudio/code-quality/ca2153-avoid-handling-corrupted-state-exceptions)

### CA2200: Rethrow to preserve stack details. ###

Re-throwing caught exception changes stack information.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2200-rethrow-to-preserve-stack-details](https://docs.microsoft.com/visualstudio/code-quality/ca2200-rethrow-to-preserve-stack-details)

### CA2201: Do not raise reserved exception types ###

An exception of type that is not sufficiently specific or reserved by the runtime should never be raised by user code. This makes the original error difficult to detect and debug. If this exception instance might be thrown, use a different exception type.

Category: Usage

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2201-do-not-raise-reserved-exception-types](https://docs.microsoft.com/visualstudio/code-quality/ca2201-do-not-raise-reserved-exception-types)

### CA2207: Initialize value type static fields inline ###

A value type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2207-initialize-value-type-static-fields-inline](https://docs.microsoft.com/visualstudio/code-quality/ca2207-initialize-value-type-static-fields-inline)

### CA2208: Instantiate argument exceptions correctly ###

A call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2208-instantiate-argument-exceptions-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca2208-instantiate-argument-exceptions-correctly)

### CA2211: Non-constant fields should not be visible ###

Static fields that are neither constants nor read-only are not thread-safe. Access to such a field must be carefully controlled and requires advanced programming techniques to synchronize access to the class object.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2211-non-constant-fields-should-not-be-visible](https://docs.microsoft.com/visualstudio/code-quality/ca2211-non-constant-fields-should-not-be-visible)

### CA2213: Disposable fields should be disposed ###

A type that implements System.IDisposable declares fields that are of types that also implement IDisposable. The Dispose method of the field is not called by the Dispose method of the declaring type. To fix a violation of this rule, call Dispose on fields that are of types that implement IDisposable if you are responsible for allocating and releasing the unmanaged resources held by the field.

Category: Usage

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed](https://docs.microsoft.com/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed)

### CA2214: Do not call overridable methods in constructors ###

Virtual methods defined on the class should not be called from constructors. If a derived class has overridden the method, the derived class version will be called (before the derived class constructor is called).

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2214-do-not-call-overridable-methods-in-constructors](https://docs.microsoft.com/visualstudio/code-quality/ca2214-do-not-call-overridable-methods-in-constructors)

### CA2216: Disposable types should declare finalizer ###

A type that implements System.IDisposable and has fields that suggest the use of unmanaged resources does not implement a finalizer, as described by Object.Finalize.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2216-disposable-types-should-declare-finalizer](https://docs.microsoft.com/visualstudio/code-quality/ca2216-disposable-types-should-declare-finalizer)

### CA2217: Do not mark enums with FlagsAttribute ###

An externally visible enumeration is marked by using FlagsAttribute, and it has one or more values that are not powers of two or a combination of the other defined values on the enumeration.

Category: Usage

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2217-do-not-mark-enums-with-flagsattribute](https://docs.microsoft.com/visualstudio/code-quality/ca2217-do-not-mark-enums-with-flagsattribute)

### CA2218: Override GetHashCode on overriding Equals ###

GetHashCode returns a value, based on the current instance, that is suited for hashing algorithms and data structures such as a hash table. Two objects that are the same type and are equal must return the same hash code.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2218-override-gethashcode-on-overriding-equals](https://docs.microsoft.com/visualstudio/code-quality/ca2218-override-gethashcode-on-overriding-equals)

### CA2219: Do not raise exceptions in finally clauses ###

When an exception is raised in a finally clause, the new exception hides the active exception. This makes the original error difficult to detect and debug.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2219-do-not-raise-exceptions-in-exception-clauses](https://docs.microsoft.com/visualstudio/code-quality/ca2219-do-not-raise-exceptions-in-exception-clauses)

### CA2224: Override Equals on overloading operator equals ###

A public type implements the equality operator but does not override Object.Equals.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2224-override-equals-on-overloading-operator-equals](https://docs.microsoft.com/visualstudio/code-quality/ca2224-override-equals-on-overloading-operator-equals)

### CA2225: Operator overloads have named alternates ###

An operator overload was detected, and the expected named alternative method was not found. The named alternative member provides access to the same functionality as the operator and is provided for developers who program in languages that do not support overloaded operators.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2225-operator-overloads-have-named-alternates](https://docs.microsoft.com/visualstudio/code-quality/ca2225-operator-overloads-have-named-alternates)

### CA2226: Operators should have symmetrical overloads ###

A type implements the equality or inequality operator and does not implement the opposite operator.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2226-operators-should-have-symmetrical-overloads](https://docs.microsoft.com/visualstudio/code-quality/ca2226-operators-should-have-symmetrical-overloads)

### CA2227: Collection properties should be read only ###

A writable collection property allows a user to replace the collection with a different collection. A read-only property stops the collection from being replaced but still allows the individual members to be set.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2227-collection-properties-should-be-read-only](https://docs.microsoft.com/visualstudio/code-quality/ca2227-collection-properties-should-be-read-only)

### CA2229: Implement serialization constructors ###

To fix a violation of this rule, implement the serialization constructor. For a sealed class, make the constructor private; otherwise, make it protected.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2229-implement-serialization-constructors](https://docs.microsoft.com/visualstudio/code-quality/ca2229-implement-serialization-constructors)

### CA2231: Overload operator equals on overriding value type Equals ###

In most programming languages there is no default implementation of the equality operator (==) for value types. If your programming language supports operator overloads, you should consider implementing the equality operator. Its behavior should be identical to that of Equals

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2231-overload-operator-equals-on-overriding-valuetype-equals](https://docs.microsoft.com/visualstudio/code-quality/ca2231-overload-operator-equals-on-overriding-valuetype-equals)

### CA2234: Pass system uri objects instead of strings ###

A call is made to a method that has a string parameter whose name contains "uri", "URI", "urn", "URN", "url", or "URL". The declaring type of the method contains a corresponding method overload that has a System.Uri parameter.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2234-pass-system-uri-objects-instead-of-strings](https://docs.microsoft.com/visualstudio/code-quality/ca2234-pass-system-uri-objects-instead-of-strings)

### CA2235: Mark all non-serializable fields ###

An instance field of a type that is not serializable is declared in a type that is serializable.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2235-mark-all-non-serializable-fields](https://docs.microsoft.com/visualstudio/code-quality/ca2235-mark-all-non-serializable-fields)

### CA2237: Mark ISerializable types with serializable ###

To be recognized by the common language runtime as serializable, types must be marked by using the SerializableAttribute attribute even when the type uses a custom serialization routine through implementation of the ISerializable interface.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2237-mark-iserializable-types-with-serializableattribute](https://docs.microsoft.com/visualstudio/code-quality/ca2237-mark-iserializable-types-with-serializableattribute)

### CA2241: Provide correct arguments to formatting methods ###

The format argument that is passed to System.String.Format does not contain a format item that corresponds to each object argument, or vice versa.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2241-provide-correct-arguments-to-formatting-methods](https://docs.microsoft.com/visualstudio/code-quality/ca2241-provide-correct-arguments-to-formatting-methods)

### CA2242: Test for NaN correctly ###

This expression tests a value against Single.Nan or Double.Nan. Use Single.IsNan(Single) or Double.IsNan(Double) to test the value.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2242-test-for-nan-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca2242-test-for-nan-correctly)

### CA2243: Attribute string literals should parse correctly ###

The string literal parameter of an attribute does not parse correctly for a URL, a GUID, or a version.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2243-attribute-string-literals-should-parse-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca2243-attribute-string-literals-should-parse-correctly)

### CA2244: Do not duplicate indexed element initializations ###

Indexed elements in objects initializers must initialize unique elements. A duplicate index might overwrite a previous element initialization.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

### CA2300: Do not use insecure deserializer BinaryFormatter ###

The method '{0}' is insecure when deserializing untrusted data.  If you need to instead detect BinaryFormatter deserialization without a SerializationBinder set, then disable rule CA2300, and enable rules CA2301 and CA2302.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2301: Do not call BinaryFormatter.Deserialize without first setting BinaryFormatter.Binder ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2302: Ensure BinaryFormatter.Binder is set before calling BinaryFormatter.Deserialize ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2305: Do not use insecure deserializer LosFormatter ###

The method '{0}' is insecure when deserializing untrusted data.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2310: Do not use insecure deserializer NetDataContractSerializer ###

The method '{0}' is insecure when deserializing untrusted data.  If you need to instead detect NetDataContractSerializer deserialization without a SerializationBinder set, then disable rule CA2310, and enable rules CA2311 and CA2312.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2311: Do not deserialize without first setting NetDataContractSerializer.Binder ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2312: Ensure NetDataContractSerializer.Binder is set before deserializing ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2315: Do not use insecure deserializer ObjectStateFormatter ###

The method '{0}' is insecure when deserializing untrusted data.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3001: Review code for SQL injection vulnerabilities ###

Potential SQL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3002: Review code for XSS vulnerabilities ###

Potential cross-site scripting (XSS) vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3003: Review code for file path injection vulnerabilities ###

Potential file path injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3004: Review code for information disclosure vulnerabilities ###

Potential information disclosure vulnerability was found where '{0}' in method '{1}' may contain unintended information from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3005: Review code for LDAP injection vulnerabilities ###

Potential LDAP injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3006: Review code for process command injection vulnerabilities ###

Potential process command injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3007: Review code for open redirect vulnerabilities ###

Potential open redirect vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3008: Review code for XPath injection vulnerabilities ###

Potential XPath injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3009: Review code for XML injection vulnerabilities ###

Potential XML injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3010: Review code for XAML injection vulnerabilities ###

Potential XAML injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3011: Review code for DLL injection vulnerabilities ###

Potential DLL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3012: Review code for regex injection vulnerabilities ###

Potential regex injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3075: Insecure DTD processing in XML ###

Using XmlTextReader.Load(), creating an insecure XmlReaderSettings instance when invoking XmlReader.Create(), setting the InnerXml property of the XmlDocument and enabling DTD processing using XmlUrlResolver insecurely can lead to information disclosure. Replace it with a call to the Load() method overload that takes an XmlReader instance, use XmlReader.Create() to accept XmlReaderSettings arguments or consider explicitly setting secure values. The DataViewSettingCollectionString property of DataViewManager should always be assigned from a trusted source, the DtdProcessing property should be set to false, and the XmlResolver property should be changed to XmlSecureResolver or null. 

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3075-insecure-dtd-processing](https://docs.microsoft.com/visualstudio/code-quality/ca3075-insecure-dtd-processing)

### CA3076: Insecure XSLT script processing. ###

Providing an insecure XsltSettings instance and an insecure XmlResolver instance to XslCompiledTransform.Load method is potentially unsafe as it allows processing script within XSL, which on an untrusted XSL input may lead to malicious code execution. Either replace the insecure XsltSettings argument with XsltSettings.Default or an instance that has disabled document function and script execution, or replace the XmlResolver argurment with null or an XmlSecureResolver instance. This message may be suppressed if the input is known to be from a trusted source and external resource resolution from locations that are not known in advance must be supported.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3076-insecure-xslt-script-execution](https://docs.microsoft.com/visualstudio/code-quality/ca3076-insecure-xslt-script-execution)

### CA3077: Insecure Processing in API Design, XmlDocument and XmlTextReader ###

Enabling DTD processing on all instances derived from XmlTextReader or  XmlDocument and using XmlUrlResolver for resolving external XML entities may lead to information disclosure. Ensure to set the XmlResolver property to null, create an instance of XmlSecureResolver when processing untrusted input, or use XmlReader.Create method with a secure XmlReaderSettings argument. Unless you need to enable it, ensure the DtdProcessing property is set to false. 

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3077-insecure-processing-in-api-design-xml-document-and-xml-text-reader](https://docs.microsoft.com/visualstudio/code-quality/ca3077-insecure-processing-in-api-design-xml-document-and-xml-text-reader)

### CA3147: Mark Verb Handlers With Validate Antiforgery Token ###

Missing ValidateAntiForgeryTokenAttribute on controller action {0}.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3147-mark-verb-handlers-with-validateantiforgerytoken](https://docs.microsoft.com/visualstudio/code-quality/ca3147-mark-verb-handlers-with-validateantiforgerytoken)

### CA5350: Do Not Use Weak Cryptographic Algorithms ###

Cryptographic algorithms degrade over time as attacks become for advances to attacker get access to more computation. Depending on the type and application of this cryptographic algorithm, further degradation of the cryptographic strength of it may allow attackers to read enciphered messages, tamper with enciphered  messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA-2 512, SHA-2 384, or SHA-2 256.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca5350-do-not-use-weak-cryptographic-algorithms](https://docs.microsoft.com/visualstudio/code-quality/ca5350-do-not-use-weak-cryptographic-algorithms)

### CA5351: Do Not Use Broken Cryptographic Algorithms ###

An attack making it computationally feasible to break this algorithm exists. This allows attackers to break the cryptographic guarantees it is designed to provide. Depending on the type and application of this cryptographic algorithm, this may allow attackers to read enciphered messages, tamper with enciphered  messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA512, SHA384, or SHA256. Replace digital signature uses with RSA with a key length greater than or equal to 2048-bits, or ECDSA with a key length greater than or equal to 256 bits.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca5351-do-not-use-broken-cryptographic-algorithms](https://docs.microsoft.com/visualstudio/code-quality/ca5351-do-not-use-broken-cryptographic-algorithms)

### CA5358: Do Not Use Unsafe Cipher Modes ###

These modes are vulnerable to attacks. Use only approved modes (CBC, CTS).

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA5359: Do Not Disable Certificate Validation ###

A certificate can help authenticate the identity of the server. Clients should validate the server certificate to ensure requests are sent to the intended server. If the ServerCertificateValidationCallback always returns 'true', any certificate will pass validation.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5360: Do Not Call Dangerous Methods In Deserialization ###

Insecure Deserialization is a vulnerability which occurs when untrusted data is used to abuse the logic of an application, inflict a Denial-of-Service (DoS) attack, or even execute arbitrary code upon it being deserialized. It’s frequently possible for malicious users to abuse these deserialization features when the application is deserializing untrusted data which is under their control. Specifically, invoke dangerous methods in the process of deserialization. Successful insecure deserialization attacks could allow an attacker to carry out attacks such as DoS attacks, authentication bypasses, and remote code execution.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5361: Do Not Disable SChannel Use of Strong Crypto ###

Starting with the .NET Framework 4.6, the System.Net.ServicePointManager and System.Net.Security.SslStream classes are recommeded to use new protocols. The old ones have protocol weaknesses and are not supported. Setting Switch.System.Net.DontEnableSchUseStrongCrypto with true will use the old weak crypto check and opt out of the protocol migration.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5362: Do Not Refer Self In Serializable Class ###

This can allow an attacker to DOS or exhaust the memory of the process.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA5363: Do Not Disable Request Validation ###

Request validation is a feature in ASP.NET that examines HTTP requests and determines whether they contain potentially dangerous content. This check adds protection from markup or code in the URL query string, cookies, or posted form values that might have been added for malicious purposes. So, it is generally desirable and should be left enabled for defense in depth.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5364: Do Not Use Deprecated Security Protocols ###

Using a deprecated security protocol rather than the system default is risky.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5365: Do Not Disable HTTP Header Checking ###

HTTP header checking enables encoding of the carriage return and newline characters, \r and \n, that are found in response headers. This encoding can help to avoid injection attacks that exploit an application that echoes untrusted data contained by the header.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA9999: Analyzer version mismatch ###

Analyzers in this package are preview version and are tied to a specific API version of Microsoft.CodeAnalysis. You have a mismatch between the analyzer and Microsoft.CodeAnalysis version and should switch your analyzer NuGet package/VSIX to a matching version of the Microsoft.CodeAnalysis.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

