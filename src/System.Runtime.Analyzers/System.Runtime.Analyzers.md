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

### CA2241: Provide correct arguments to formatting methods ###

The format argument that is passed to System.String.Format does not contain a format item that corresponds to each object argument, or vice versa.

Category: Usage

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182361.aspx](https://msdn.microsoft.com/en-us/library/ms182361.aspx)