# Microsoft.NetCore.Analyzers

## [CA1303](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1303): Do not pass literals as localized parameters

A method passes a string literal as a parameter to a constructor or method in the .NET Framework class library and that string should be localizable. To fix a violation of this rule, replace the string literal with a string retrieved through an instance of the ResourceManager class.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1304](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1304): Specify CultureInfo

A method or constructor calls a member that has an overload that accepts a System.Globalization.CultureInfo parameter, and the method or constructor does not call the overload that takes the CultureInfo parameter. When a CultureInfo or System.IFormatProvider object is not supplied, the default value that is supplied by the overloaded member might not have the effect that you want in all locales. If the result will be displayed to the user, specify 'CultureInfo.CurrentCulture' as the 'CultureInfo' parameter. Otherwise, if the result will be stored and accessed by software, such as when it is persisted to disk or to a database, specify 'CultureInfo.InvariantCulture'.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1305](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1305): Specify IFormatProvider

A method or constructor calls one or more members that have overloads that accept a System.IFormatProvider parameter, and the method or constructor does not call the overload that takes the IFormatProvider parameter. When a System.Globalization.CultureInfo or IFormatProvider object is not supplied, the default value that is supplied by the overloaded member might not have the effect that you want in all locales. If the result will be based on the input from/output displayed to the user, specify 'CultureInfo.CurrentCulture' as the 'IFormatProvider'. Otherwise, if the result will be stored and accessed by software, such as when it is loaded from disk/database and when it is persisted to disk/database, specify 'CultureInfo.InvariantCulture'.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1307](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1307): Specify StringComparison for clarity

A string comparison operation uses a method overload that does not set a StringComparison parameter. It is recommended to use the overload with StringComparison parameter for clarity of intent. If the result will be displayed to the user, such as when sorting a list of items for display in a list box, specify 'StringComparison.CurrentCulture' or 'StringComparison.CurrentCultureIgnoreCase' as the 'StringComparison' parameter. If comparing case-insensitive identifiers, such as file paths, environment variables, or registry keys and values, specify 'StringComparison.OrdinalIgnoreCase'. Otherwise, if comparing case-sensitive identifiers, specify 'StringComparison.Ordinal'.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA1308](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1308): Normalize strings to uppercase

Strings should be normalized to uppercase. A small group of characters cannot make a round trip when they are converted to lowercase. To make a round trip means to convert the characters from one locale to another locale that represents character data differently, and then to accurately retrieve the original characters from the converted characters.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1309](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1309): Use ordinal string comparison

A string comparison operation that is nonlinguistic does not set the StringComparison parameter to either Ordinal or OrdinalIgnoreCase. By explicitly setting the parameter to either StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase, your code often gains speed, becomes more correct, and becomes more reliable.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1310](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1310): Specify StringComparison for correctness

A string comparison operation uses a method overload that does not set a StringComparison parameter, hence its behavior could vary based on the current user's locale settings. It is strongly recommended to use the overload with StringComparison parameter for correctness and clarity of intent. If the result will be displayed to the user, such as when sorting a list of items for display in a list box, specify 'StringComparison.CurrentCulture' or 'StringComparison.CurrentCultureIgnoreCase' as the 'StringComparison' parameter. If comparing case-insensitive identifiers, such as file paths, environment variables, or registry keys and values, specify 'StringComparison.OrdinalIgnoreCase'. Otherwise, if comparing case-sensitive identifiers, specify 'StringComparison.Ordinal'.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1401](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1401): P/Invokes should not be visible

A public or protected method in a public type has the System.Runtime.InteropServices.DllImportAttribute attribute (also implemented by the Declare keyword in Visual Basic). Such methods should not be exposed.

|Item|Value|
|-|-|
|Category|Interoperability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1416](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1416): Validate platform compatibility

Using platform dependent API on a component makes the code no longer work across all platforms.

|Item|Value|
|-|-|
|Category|Interoperability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1417](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1417): Do not use 'OutAttribute' on string parameters for P/Invokes

String parameters passed by value with the 'OutAttribute' can destabilize the runtime if the string is an interned string.

|Item|Value|
|-|-|
|Category|Interoperability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1810](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1810): Initialize reference type static fields inline

A reference type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1813](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1813): Avoid unsealed attributes

The .NET Framework class library provides methods for retrieving custom attributes. By default, these methods search the attribute inheritance hierarchy. Sealing the attribute eliminates the search through the inheritance hierarchy and can improve performance.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|False|
|Severity|Warning|
|CodeFix|True|
---

## [CA1816](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1816): Dispose methods should call SuppressFinalize

A method that is an implementation of Dispose does not call GC.SuppressFinalize; or a method that is not an implementation of Dispose calls GC.SuppressFinalize; or a method calls GC.SuppressFinalize and passes something other than this (Me in Visual Basic).

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1820](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1820): Test for empty strings using string length

Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1824](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1824): Mark assemblies with NeutralResourcesLanguageAttribute

The NeutralResourcesLanguage attribute informs the ResourceManager of the language that was used to display the resources of a neutral culture for an assembly. This improves lookup performance for the first resource that you load and can reduce your working set.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA1825](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1825): Avoid zero-length array allocations

Avoid unnecessary zero-length array allocations.  Use {0} instead.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1826](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1826): Do not use Enumerable methods on indexable collections

This collection is directly indexable. Going through LINQ here causes unnecessary allocations and CPU work.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1827](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1827): Do not use Count() or LongCount() when Any() can be used

For non-empty collections, Count() and LongCount() enumerate the entire sequence, while Any() stops at the first item or the first item that satisfies a condition.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1828](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1828): Do not use CountAsync() or LongCountAsync() when AnyAsync() can be used

For non-empty collections, CountAsync() and LongCountAsync() enumerate the entire sequence, while AnyAsync() stops at the first item or the first item that satisfies a condition.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1829](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1829): Use Length/Count property instead of Count() when available

Enumerable.Count() potentially enumerates the sequence while a Length/Count property is a direct access.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1830](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1830): Prefer strongly-typed Append and Insert method overloads on StringBuilder

StringBuilder.Append and StringBuilder.Insert provide overloads for multiple types beyond System.String.  When possible, prefer the strongly-typed overloads over using ToString() and the string-based overload.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1831](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1831): Use AsSpan or AsMemory instead of Range-based indexers when appropriate

The Range-based indexer on string values produces a copy of requested portion of the string. This copy is usually unnecessary when it is implicitly used as a ReadOnlySpan or ReadOnlyMemory value. Use the AsSpan method to avoid the unnecessary copy.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1832](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1832): Use AsSpan or AsMemory instead of Range-based indexers when appropriate

The Range-based indexer on array values produces a copy of requested portion of the array. This copy is usually unnecessary when it is implicitly used as a ReadOnlySpan or ReadOnlyMemory value. Use the AsSpan method to avoid the unnecessary copy.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1833](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1833): Use AsSpan or AsMemory instead of Range-based indexers when appropriate

The Range-based indexer on array values produces a copy of requested portion of the array. This copy is often unwanted when it is implicitly used as a Span or Memory value. Use the AsSpan method to avoid the copy.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1834](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1834): Consider using 'StringBuilder.Append(char)' when applicable

'StringBuilder.Append(char)' is more efficient than 'StringBuilder.Append(string)' when the string is a single character. When calling 'Append' with a constant, prefer using a constant char rather than a constant string containing one character.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1835](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1835): Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'

'Stream' has a 'ReadAsync' overload that takes a 'Memory\<Byte>' as the first argument, and a 'WriteAsync' overload that takes a 'ReadOnlyMemory\<Byte>' as the first argument. Prefer calling the memory based overloads, which are more efficient.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1836](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1836): Prefer IsEmpty over Count

For determining whether the object contains or not any items, prefer using 'IsEmpty' property rather than retrieving the number of items from the 'Count' property and comparing it to 0 or 1.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1837](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1837): Use 'Environment.ProcessId'

'Environment.ProcessId' is simpler and faster than 'Process.GetCurrentProcess().Id'.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA1838](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1838): Avoid 'StringBuilder' parameters for P/Invokes

Marshalling of 'StringBuilder' always creates a native buffer copy, resulting in multiple allocations for one marshalling operation.

|Item|Value|
|-|-|
|Category|Performance|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2000](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2000): Dispose objects before losing scope

If a disposable object is not explicitly disposed before all references to it are out of scope, the object will be disposed at some indeterminate time when the garbage collector runs the finalizer of the object. Because an exceptional event might occur that will prevent the finalizer of the object from running, the object should be explicitly disposed instead.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2002](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2002): Do not lock on objects with weak identity

An object is said to have a weak identity when it can be directly accessed across application domain boundaries. A thread that tries to acquire a lock on an object that has a weak identity can be blocked by a second thread in a different application domain that has a lock on the same object.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2008](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2008): Do not create tasks without passing a TaskScheduler

Do not create tasks unless you are using one of the overloads that takes a TaskScheduler. The default is to schedule on TaskScheduler.Current, which would lead to deadlocks. Either use TaskScheduler.Default to schedule on the thread pool, or explicitly pass TaskScheduler.Current to make your intentions clear.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2009](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2009): Do not call ToImmutableCollection on an ImmutableCollection value

Do not call {0} on an {1} value

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2012](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2012): Use ValueTasks correctly

ValueTasks returned from member invocations are intended to be directly awaited.  Attempts to consume a ValueTask multiple times or to directly access one's result before it's known to be completed may result in an exception or corruption.  Ignoring such a ValueTask is likely an indication of a functional bug and may degrade performance.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2013](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2013): Do not use ReferenceEquals with value types

Value type typed arguments are uniquely boxed for each call to this method, therefore the result is always false.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2014](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2014): Do not use stackalloc in loops

Stack space allocated by a stackalloc is only released at the end of the current method's invocation.  Using it in a loop can result in unbounded stack growth and eventual stack overflow conditions.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2015](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2015): Do not define finalizers for types derived from MemoryManager\<T>

Adding a finalizer to a type derived from MemoryManager\<T> may permit memory to be freed while it is still in use by a Span\<T>.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2016](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2016): Forward the 'CancellationToken' parameter to methods that take one

Forward the 'CancellationToken' parameter to methods that take one to ensure the operation cancellation notifications gets properly propagated, or pass in 'CancellationToken.None' explicitly to indicate intentionally not propagating the token.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2100](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2100): Review SQL queries for security vulnerabilities

SQL queries that directly use user input can be vulnerable to SQL injection attacks. Review this SQL query for potential vulnerabilities, and consider using a parameterized SQL query.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2101](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2101): Specify marshaling for P/Invoke string arguments

A platform invoke member allows partially trusted callers, has a string parameter, and does not explicitly marshal the string. This can cause a potential security vulnerability.

|Item|Value|
|-|-|
|Category|Globalization|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2201](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2201): Do not raise reserved exception types

An exception of type that is not sufficiently specific or reserved by the runtime should never be raised by user code. This makes the original error difficult to detect and debug. If this exception instance might be thrown, use a different exception type.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2207](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2207): Initialize value type static fields inline

A value type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2208](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2208): Instantiate argument exceptions correctly

A call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2213](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2213): Disposable fields should be disposed

A type that implements System.IDisposable declares fields that are of types that also implement IDisposable. The Dispose method of the field is not called by the Dispose method of the declaring type. To fix a violation of this rule, call Dispose on fields that are of types that implement IDisposable if you are responsible for allocating and releasing the unmanaged resources held by the field.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2215](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2215): Dispose methods should call base class dispose

A type that implements System.IDisposable inherits from a type that also implements IDisposable. The Dispose method of the inheriting type does not call the Dispose method of the parent type. To fix a violation of this rule, call base.Dispose in your Dispose method.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2216](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2216): Disposable types should declare finalizer

A type that implements System.IDisposable and has fields that suggest the use of unmanaged resources does not implement a finalizer, as described by Object.Finalize.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2229](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2229): Implement serialization constructors

To fix a violation of this rule, implement the serialization constructor. For a sealed class, make the constructor private; otherwise, make it protected.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2235](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2235): Mark all non-serializable fields

An instance field of a type that is not serializable is declared in a type that is serializable.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2237](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2237): Mark ISerializable types with serializable

To be recognized by the common language runtime as serializable, types must be marked by using the SerializableAttribute attribute even when the type uses a custom serialization routine through implementation of the ISerializable interface.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2241](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2241): Provide correct arguments to formatting methods

The format argument that is passed to System.String.Format does not contain a format item that corresponds to each object argument, or vice versa.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2242](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2242): Test for NaN correctly

This expression tests a value against Single.Nan or Double.Nan. Use Single.IsNan(Single) or Double.IsNan(Double) to test the value.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2243](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2243): Attribute string literals should parse correctly

The string literal parameter of an attribute does not parse correctly for a URL, a GUID, or a version.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2247](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2247): Argument passed to TaskCompletionSource constructor should be TaskCreationOptions enum instead of TaskContinuationOptions enum

TaskCompletionSource has constructors that take TaskCreationOptions that control the underlying Task, and constructors that take object state that's stored in the task.  Accidentally passing a TaskContinuationOptions instead of a TaskCreationOptions will result in the call treating the options as state.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2248](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2248): Provide correct 'enum' argument to 'Enum.HasFlag'

'Enum.HasFlag' method expects the 'enum' argument to be of the same 'enum' type as the instance on which the method is invoked and that this 'enum' is marked with 'System.FlagsAttribute'. If these are different 'enum' types, an unhandled exception will be thrown at runtime. If the 'enum' type is not marked with 'System.FlagsAttribute' the call will always return 'false' at runtime.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2249](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2249): Consider using 'string.Contains' instead of 'string.IndexOf'

Calls to 'string.IndexOf' where the result is used to check for the presence/absence of a substring can be replaced by 'string.Contains'.

|Item|Value|
|-|-|
|Category|Usage|
|Enabled|True|
|Severity|Warning|
|CodeFix|True|
---

## [CA2300](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2300): Do not use insecure deserializer BinaryFormatter

The method '{0}' is insecure when deserializing untrusted data.  If you need to instead detect BinaryFormatter deserialization without a SerializationBinder set, then disable rule CA2300, and enable rules CA2301 and CA2302.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2301](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2301): Do not call BinaryFormatter.Deserialize without first setting BinaryFormatter.Binder

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2302](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2302): Ensure BinaryFormatter.Binder is set before calling BinaryFormatter.Deserialize

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2305](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2305): Do not use insecure deserializer LosFormatter

The method '{0}' is insecure when deserializing untrusted data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2310](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2310): Do not use insecure deserializer NetDataContractSerializer

The method '{0}' is insecure when deserializing untrusted data.  If you need to instead detect NetDataContractSerializer deserialization without a SerializationBinder set, then disable rule CA2310, and enable rules CA2311 and CA2312.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2311](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2311): Do not deserialize without first setting NetDataContractSerializer.Binder

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2312](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2312): Ensure NetDataContractSerializer.Binder is set before deserializing

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2315](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2315): Do not use insecure deserializer ObjectStateFormatter

The method '{0}' is insecure when deserializing untrusted data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2321](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2321): Do not deserialize with JavaScriptSerializer using a SimpleTypeResolver

The method '{0}' is insecure when deserializing untrusted data with a JavaScriptSerializer initialized with a SimpleTypeResolver. Initialize JavaScriptSerializer without a JavaScriptTypeResolver specified, or initialize with a JavaScriptTypeResolver that limits the types of objects in the deserialized object graph.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2322](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2322): Ensure JavaScriptSerializer is not initialized with SimpleTypeResolver before deserializing

The method '{0}' is insecure when deserializing untrusted data with a JavaScriptSerializer initialized with a SimpleTypeResolver. Ensure that the JavaScriptSerializer is initialized without a JavaScriptTypeResolver specified, or initialized with a JavaScriptTypeResolver that limits the types of objects in the deserialized object graph.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2326](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2326): Do not use TypeNameHandling values other than None

Deserializing JSON when using a TypeNameHandling value other than None can be insecure.  If you need to instead detect Json.NET deserialization when a SerializationBinder isn't specified, then disable rule CA2326, and enable rules CA2327, CA2328, CA2329, and CA2330.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2327](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2327): Do not use insecure JsonSerializerSettings

When deserializing untrusted input, allowing arbitrary types to be deserialized is insecure.  When using JsonSerializerSettings, use TypeNameHandling.None, or for values other than None, restrict deserialized types with a SerializationBinder.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2328](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2328): Ensure that JsonSerializerSettings are secure

When deserializing untrusted input, allowing arbitrary types to be deserialized is insecure.  When using JsonSerializerSettings, ensure TypeNameHandling.None is specified, or for values other than None, ensure a SerializationBinder is specified to restrict deserialized types.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2329](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2329): Do not deserialize with JsonSerializer using an insecure configuration

When deserializing untrusted input, allowing arbitrary types to be deserialized is insecure. When using deserializing JsonSerializer, use TypeNameHandling.None, or for values other than None, restrict deserialized types with a SerializationBinder.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2330](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2330): Ensure that JsonSerializer has a secure configuration when deserializing

When deserializing untrusted input, allowing arbitrary types to be deserialized is insecure. When using deserializing JsonSerializer, use TypeNameHandling.None, or for values other than None, restrict deserialized types with a SerializationBinder.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2350](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2350): Do not use DataTable.ReadXml() with untrusted data

The method '{0}' is insecure when deserializing untrusted data

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2351](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2351): Do not use DataSet.ReadXml() with untrusted data

The method '{0}' is insecure when deserializing untrusted data

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2352](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2352): Unsafe DataSet or DataTable in serializable type can be vulnerable to remote code execution attacks

When deserializing untrusted input with an IFormatter-based serializer, deserializing a {0} object is insecure. '{1}' either is or derives from {0}.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2353](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2353): Unsafe DataSet or DataTable in serializable type

When deserializing untrusted input, deserializing a {0} object is insecure. '{1}' either is or derives from {0}

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2354](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2354): Unsafe DataSet or DataTable in deserialized object graph can be vulnerable to remote code execution attacks

When deserializing untrusted input, deserializing a {0} object is insecure. '{1}' either is or derives from {0}

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2355](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2355): Unsafe DataSet or DataTable type found in deserializable object graph

When deserializing untrusted input, deserializing a {0} object is insecure. '{1}' either is or derives from {0}

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2356](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2356): Unsafe DataSet or DataTable type in web deserializable object graph

When deserializing untrusted input, deserializing a {0} object is insecure. '{1}' either is or derives from {0}

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2361](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2361): Ensure auto-generated class containing DataSet.ReadXml() is not used with untrusted data

The method '{0}' is insecure when deserializing untrusted data. Make sure that auto-generated class containing the '{0}' call is not deserialized with untrusted data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA2362](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2362): Unsafe DataSet or DataTable in auto-generated serializable type can be vulnerable to remote code execution attacks

When deserializing untrusted input with an IFormatter-based serializer, deserializing a {0} object is insecure. '{1}' either is or derives from {0}. Ensure that the auto-generated type is never deserialized with untrusted data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3001](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3001): Review code for SQL injection vulnerabilities

Potential SQL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3002](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3002): Review code for XSS vulnerabilities

Potential cross-site scripting (XSS) vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3003](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3003): Review code for file path injection vulnerabilities

Potential file path injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3004](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3004): Review code for information disclosure vulnerabilities

Potential information disclosure vulnerability was found where '{0}' in method '{1}' may contain unintended information from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3005](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3005): Review code for LDAP injection vulnerabilities

Potential LDAP injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3006](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3006): Review code for process command injection vulnerabilities

Potential process command injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3007](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3007): Review code for open redirect vulnerabilities

Potential open redirect vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3008](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3008): Review code for XPath injection vulnerabilities

Potential XPath injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3009](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3009): Review code for XML injection vulnerabilities

Potential XML injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3010](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3010): Review code for XAML injection vulnerabilities

Potential XAML injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3011](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3011): Review code for DLL injection vulnerabilities

Potential DLL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3012](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3012): Review code for regex injection vulnerabilities

Potential regex injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA3061](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3061): Do Not Add Schema By URL

This overload of XmlSchemaCollection.Add method internally enables DTD processing on the XML reader instance used, and uses UrlResolver for resolving external XML entities. The outcome is information disclosure. Content from file system or network shares for the machine processing the XML can be exposed to attacker. In addition, an attacker can use this as a DoS vector.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5350](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5350): Do Not Use Weak Cryptographic Algorithms

Cryptographic algorithms degrade over time as attacks become for advances to attacker get access to more computation. Depending on the type and application of this cryptographic algorithm, further degradation of the cryptographic strength of it may allow attackers to read enciphered messages, tamper with enciphered  messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA-2 512, SHA-2 384, or SHA-2 256.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5351](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5351): Do Not Use Broken Cryptographic Algorithms

An attack making it computationally feasible to break this algorithm exists. This allows attackers to break the cryptographic guarantees it is designed to provide. Depending on the type and application of this cryptographic algorithm, this may allow attackers to read enciphered messages, tamper with enciphered  messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA512, SHA384, or SHA256. Replace digital signature uses with RSA with a key length greater than or equal to 2048-bits, or ECDSA with a key length greater than or equal to 256 bits.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5358](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5358): Review cipher mode usage with cryptography experts

These cipher modes might be vulnerable to attacks. Consider using recommended modes (CBC, CTS).

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5359](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5359): Do Not Disable Certificate Validation

A certificate can help authenticate the identity of the server. Clients should validate the server certificate to ensure requests are sent to the intended server. If the ServerCertificateValidationCallback always returns 'true', any certificate will pass validation.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5360](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5360): Do Not Call Dangerous Methods In Deserialization

Insecure Deserialization is a vulnerability which occurs when untrusted data is used to abuse the logic of an application, inflict a Denial-of-Service (DoS) attack, or even execute arbitrary code upon it being deserialized. It’s frequently possible for malicious users to abuse these deserialization features when the application is deserializing untrusted data which is under their control. Specifically, invoke dangerous methods in the process of deserialization. Successful insecure deserialization attacks could allow an attacker to carry out attacks such as DoS attacks, authentication bypasses, and remote code execution.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5361](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5361): Do Not Disable SChannel Use of Strong Crypto

Starting with the .NET Framework 4.6, the System.Net.ServicePointManager and System.Net.Security.SslStream classes are recommended to use new protocols. The old ones have protocol weaknesses and are not supported. Setting Switch.System.Net.DontEnableSchUseStrongCrypto with true will use the old weak crypto check and opt out of the protocol migration.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5362](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5362): Potential reference cycle in deserialized object graph

Review code that processes untrusted deserialized data for handling of unexpected reference cycles. An unexpected reference cycle should not cause the code to enter an infinite loop. Otherwise, an unexpected reference cycle can allow an attacker to DOS or exhaust the memory of the process when deserializing untrusted data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5363](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5363): Do Not Disable Request Validation

Request validation is a feature in ASP.NET that examines HTTP requests and determines whether they contain potentially dangerous content. This check adds protection from markup or code in the URL query string, cookies, or posted form values that might have been added for malicious purposes. So, it is generally desirable and should be left enabled for defense in depth.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5364](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5364): Do Not Use Deprecated Security Protocols

Using a deprecated security protocol rather than the system default is risky.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5365](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5365): Do Not Disable HTTP Header Checking

HTTP header checking enables encoding of the carriage return and newline characters, \r and \n, that are found in response headers. This encoding can help to avoid injection attacks that exploit an application that echoes untrusted data contained by the header.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5366](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5366): Use XmlReader for 'DataSet.ReadXml()'

Processing XML from untrusted data may load dangerous external references, which should be restricted by using an XmlReader with a secure resolver or with DTD processing disabled.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5367](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5367): Do Not Serialize Types With Pointer Fields

Pointers are not "type safe" in the sense that you cannot guarantee the correctness of the memory they point at. So, serializing types with pointer fields is dangerous, as it may allow an attacker to control the pointer.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5368](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5368): Set ViewStateUserKey For Classes Derived From Page

Setting the ViewStateUserKey property can help you prevent attacks on your application by allowing you to assign an identifier to the view-state variable for individual users so that they cannot use the variable to generate an attack. Otherwise, there will be cross-site request forgery vulnerabilities.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5369](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5369): Use XmlReader for 'XmlSerializer.Deserialize()'

Processing XML from untrusted data may load dangerous external references, which should be restricted by using an XmlReader with a secure resolver or with DTD processing disabled.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5370](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5370): Use XmlReader for XmlValidatingReader constructor

Processing XML from untrusted data may load dangerous external references, which should be restricted by using an XmlReader with a secure resolver or with DTD processing disabled.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5371](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5371): Use XmlReader for 'XmlSchema.Read()'

Processing XML from untrusted data may load dangerous external references, which should be restricted by using an XmlReader with a secure resolver or with DTD processing disabled.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5372](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5372): Use XmlReader for XPathDocument constructor

Processing XML from untrusted data may load dangerous external references, which should be restricted by using an XmlReader with a secure resolver or with DTD processing disabled.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5373](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5373): Do not use obsolete key derivation function

Password-based key derivation should use PBKDF2 with SHA-2. Avoid using PasswordDeriveBytes since it generates a PBKDF1 key. Avoid using Rfc2898DeriveBytes.CryptDeriveKey since it doesn't use the iteration count or salt.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5374](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5374): Do Not Use XslTransform

Do not use XslTransform. It does not restrict potentially dangerous external references.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5375](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5375): Do Not Use Account Shared Access Signature

Shared Access Signatures(SAS) are a vital part of the security model for any application using Azure Storage, they should provide limited and safe permissions to your storage account to clients that don't have the account key. All of the operations available via a service SAS are also available via an account SAS, that is, account SAS is too powerful. So it is recommended to use Service SAS to delegate access more carefully.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5376](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5376): Use SharedAccessProtocol HttpsOnly

HTTPS encrypts network traffic. Use HttpsOnly, rather than HttpOrHttps, to ensure network traffic is always encrypted to help prevent disclosure of sensitive data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5377](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5377): Use Container Level Access Policy

No access policy identifier is specified, making tokens non-revocable.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5378](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5378): Do not disable ServicePointManagerSecurityProtocols

Do not set Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols to true.  Setting this switch limits Windows Communication Framework (WCF) to using Transport Layer Security (TLS) 1.0, which is insecure and obsolete.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5379](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5379): Ensure Key Derivation Function algorithm is sufficiently strong

Some implementations of the Rfc2898DeriveBytes class allow for a hash algorithm to be specified in a constructor parameter or overwritten in the HashAlgorithm property. If a hash algorithm is specified, then it should be SHA-256 or higher.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5380](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5380): Do Not Add Certificates To Root Store

By default, the Trusted Root Certification Authorities certificate store is configured with a set of public CAs that has met the requirements of the Microsoft Root Certificate Program. Since all trusted root CAs can issue certificates for any domain, an attacker can pick a weak or coercible CA that you install by yourself to target for an attack – and a single vulnerable, malicious or coercible CA undermines the security of the entire system. To make matters worse, these attacks can go unnoticed quite easily.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5381](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5381): Ensure Certificates Are Not Added To Root Store

By default, the Trusted Root Certification Authorities certificate store is configured with a set of public CAs that has met the requirements of the Microsoft Root Certificate Program. Since all trusted root CAs can issue certificates for any domain, an attacker can pick a weak or coercible CA that you install by yourself to target for an attack – and a single vulnerable, malicious or coercible CA undermines the security of the entire system. To make matters worse, these attacks can go unnoticed quite easily.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5382](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5382): Use Secure Cookies In ASP.Net Core

Applications available over HTTPS must use secure cookies.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5383](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5383): Ensure Use Secure Cookies In ASP.Net Core

Applications available over HTTPS must use secure cookies.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5384](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5384): Do Not Use Digital Signature Algorithm (DSA)

DSA is too weak to use.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5385](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5385): Use Rivest–Shamir–Adleman (RSA) Algorithm With Sufficient Key Size

Encryption algorithms are vulnerable to brute force attacks when too small a key size is used.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5386](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5386): Avoid hardcoding SecurityProtocolType value

Avoid hardcoding SecurityProtocolType {0}, and instead use SecurityProtocolType.SystemDefault to allow the operating system to choose the best Transport Layer Security protocol to use.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5387](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5387): Do Not Use Weak Key Derivation Function With Insufficient Iteration Count

When deriving cryptographic keys from user-provided inputs such as password, use sufficient iteration count (at least 100k).

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5388](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5388): Ensure Sufficient Iteration Count When Using Weak Key Derivation Function

When deriving cryptographic keys from user-provided inputs such as password, use sufficient iteration count (at least 100k).

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5389](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5389): Do Not Add Archive Item's Path To The Target File System Path

When extracting files from an archive and using the archive item's path, check if the path is safe. Archive path can be relative and can lead to file system access outside of the expected file system target path, leading to malicious config changes and remote code execution via lay-and-wait technique.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5390](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5390): Do not hard-code encryption key

SymmetricAlgorithm's .Key property, or a method's rgbKey parameter, should never be a hard-coded value.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5391](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5391): Use antiforgery tokens in ASP.NET Core MVC controllers

Handling a POST, PUT, PATCH, or DELETE request without validating an antiforgery token may be vulnerable to cross-site request forgery attacks. A cross-site request forgery attack can send malicious requests from an authenticated user to your ASP.NET Core MVC controller.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5392](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5392): Use DefaultDllImportSearchPaths attribute for P/Invokes

By default, P/Invokes using DllImportAttribute probe a number of directories, including the current working directory for the library to load. This can be a security issue for certain applications, leading to DLL hijacking.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5393](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5393): Do not use unsafe DllImportSearchPath value

There could be a malicious DLL in the default DLL search directories. Or, depending on where your application is run from, there could be a malicious DLL in the application's directory. Use a DllImportSearchPath value that specifies an explicit search path instead. The DllImportSearchPath flags that this rule looks for can be configured in .editorconfig.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5394](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5394): Do not use insecure randomness

Using a cryptographically weak pseudo-random number generator may allow an attacker to predict what security-sensitive value will be generated. Use a cryptographically strong random number generator if an unpredictable value is required, or ensure that weak pseudo-random numbers aren't used in a security-sensitive manner.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5395](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5395): Miss HttpVerb attribute for action methods

All the methods that create, edit, delete, or otherwise modify data do so in the [HttpPost] overload of the method, which needs to be protected with the anti forgery attribute from request forgery. Performing a GET operation should be a safe operation that has no side effects and doesn't modify your persisted data.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5396](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5396): Set HttpOnly to true for HttpCookie

As a defense in depth measure, ensure security sensitive HTTP cookies are marked as HttpOnly. This indicates web browsers should disallow scripts from accessing the cookies. Injected malicious scripts are a common way of stealing cookies.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5397](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5397): Do not use deprecated SslProtocols values

Older protocol versions of Transport Layer Security (TLS) are less secure than TLS 1.2 and TLS 1.3, and are more likely to have new vulnerabilities. Avoid older protocol versions to minimize risk.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA5398](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5398): Avoid hardcoded SslProtocols values

Current Transport Layer Security protocol versions may become deprecated if vulnerabilities are found. Avoid hardcoding SslProtocols values to keep your application secure. Use 'None' to let the Operating System choose a version.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5399](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5399): HttpClients should enable certificate revocation list checks

Using HttpClient without providing a platform specific handler (WinHttpHandler or CurlHandler or HttpClientHandler) where the CheckCertificateRevocationList property is set to true, will allow revoked certificates to be accepted by the HttpClient as valid.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5400](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5400): Ensure HttpClient certificate revocation list check is not disabled

Using HttpClient without providing a platform specific handler (WinHttpHandler or CurlHandler or HttpClientHandler) where the CheckCertificateRevocationList property is set to true, will allow revoked certificates to be accepted by the HttpClient as valid.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5401](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5401): Do not use CreateEncryptor with non-default IV

Symmetric encryption should always use a non-repeatable initialization vector to prevent dictionary attacks.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5402](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5402): Use CreateEncryptor with the default IV

Symmetric encryption should always use a non-repeatable initialization vector to prevent dictionary attacks.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
|Severity|Warning|
|CodeFix|False|
---

## [CA5403](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5403): Do not hard-code certificate

Hard-coded certificates in source code are vulnerable to being exploited.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|False|
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

## [IL3000](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/il3000): Avoid using accessing Assembly file path when publishing as a single-file

'{0}' always returns an empty string for assemblies embedded in a single-file app. If the path to the app directory is needed, consider calling 'System.AppContext.BaseDirectory'.

|Item|Value|
|-|-|
|Category|Publish|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [IL3001](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/il3001): Avoid using accessing Assembly file path when publishing as a single-file

'{0}' will throw for assemblies embedded in a single-file app

|Item|Value|
|-|-|
|Category|Publish|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---
