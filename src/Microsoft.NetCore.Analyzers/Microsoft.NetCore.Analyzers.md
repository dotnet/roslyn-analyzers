System.Resources
--------------------------
### CA1824: Mark assemblies with NeutralResourcesLanguageAttribute ###

The NeutralResourcesLanguage attribute informs the ResourceManager of the language that was used to display the resources of a neutral culture for an assembly. This improves lookup performance for the first resource that you load and can reduce your working set.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb385967.aspx](https://msdn.microsoft.com/en-us/library/bb385967.aspx)

System.Runtime
--------------------------
### CA1309: Use ordinal stringcomparison ###

A string comparison operation that is nonlinguistic does not set the StringComparison parameter to either Ordinal or OrdinalIgnoreCase. By explicitly setting the parameter to either StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase, your code often gains speed, becomes more correct, and becomes more reliable.

Category: Globalization

Severity: Warning

Help: [http://msdn.microsoft.com/library/bb385972.aspx](http://msdn.microsoft.com/library/bb385972.aspx)

### CA1810: Initialize reference type static fields inline ###

A reference type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182275.aspx](https://msdn.microsoft.com/en-us/library/ms182275.aspx)

### CA1813: Avoid unsealed attributes ###

The .NET Framework class library provides methods for retrieving custom attributes. By default, these methods search the attribute inheritance hierarchy. Sealing the attribute eliminates the search through the inheritance hierarchy and can improve performance.

Category: Performance

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182267.aspx](http://msdn.microsoft.com/library/ms182267.aspx)

### CA1820: Test for empty strings using string length ###

Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/library/ms182279.aspx](https://msdn.microsoft.com/library/ms182279.aspx)

### CA1825: Avoid zero-length array allocations. ###

Category: Performance

Severity: Warning

### CA2002: Do not lock on objects with weak identity ###

Category: Reliability

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182290.aspx](http://msdn.microsoft.com/library/ms182290.aspx)

### CA2201: Do not raise reserved exception types ###

An exception of type that is not sufficiently specific or reserved by the runtime should never be raised by user code. This makes the original error difficult to detect and debug. If this exception instance might be thrown, use a different exception type.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182338.aspx](https://msdn.microsoft.com/en-us/library/ms182338.aspx)

### CA2207: Initialize value type static fields inline ###

A value type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182346.aspx](https://msdn.microsoft.com/en-us/library/ms182346.aspx)

### CA2208: Instantiate argument exceptions correctly ###

A call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182347.aspx](https://msdn.microsoft.com/en-us/library/ms182347.aspx)

### CA2216: Disposable types should declare finalizer ###

A type that implements System.IDisposable and has fields that suggest the use of unmanaged resources does not implement a finalizer, as described by Object.Finalize.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182329.aspx](https://msdn.microsoft.com/en-us/library/ms182329.aspx)

### CA2241: Provide correct arguments to formatting methods ###

The format argument that is passed to System.String.Format does not contain a format item that corresponds to each object argument, or vice versa.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182361.aspx](https://msdn.microsoft.com/en-us/library/ms182361.aspx)

### CA2242: Test for NaN correctly ###

This expression tests a value against Single.Nan or Double.Nan. Use Single.IsNan(Single) or Double.IsNan(Double) to test the value.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb264491.aspx](https://msdn.microsoft.com/en-us/library/bb264491.aspx)

System.Runtime.InteropServices
---------------------------------------
### CA1401: P/Invokes should not be visible ###

A public or protected method in a public type has the System.Runtime.InteropServices.DllImportAttribute attribute (also implemented by the Declare keyword in Visual Basic). Such methods should not be exposed.

Category: Interoperability

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182209.aspx](http://msdn.microsoft.com/library/ms182209.aspx)

### CA2101: Specify marshaling for P/Invoke string arguments ###

A platform invoke member allows partially trusted callers, has a string parameter, and does not explicitly marshal the string. This can cause a potential security vulnerability.

Category: Globalization

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182319.aspx](http://msdn.microsoft.com/library/ms182319.aspx)

### RS0015: Always consume the value returned by methods marked with PreserveSigAttribute ###

PreserveSigAttribute indicates that a method will return an HRESULT, rather than throwing an exception. Therefore, it is important to consume the HRESULT returned by the method, so that errors can be detected. Generally, this is done by calling Marshal.ThrowExceptionForHR.

Category: Reliability

Severity: Warning

System.Security
-------------------------------------------
### CA5350: Do not use insecure cryptographic algorithm SHA1. ###

This type implements SHA1, a cryptographically insecure hashing function. Hash collisions are computationally feasible for the SHA-1 and SHA-0 algorithms. Replace this usage with a SHA-2 family hash algorithm (SHA512, SHA384, SHA256).

Category: Microsoft.Security

Severity: Warning

### CA5351: Do not use insecure cryptographic algorithm MD5. ###

This type implements MD5, a cryptographically insecure hashing function. Hash collisions are computationally feasible for the MD5 and HMACMD5 algorithms. Replace this usage with a SHA-2 family hash algorithm (SHA512, SHA384, SHA256).

Category: Microsoft.Security

Severity: Warning
