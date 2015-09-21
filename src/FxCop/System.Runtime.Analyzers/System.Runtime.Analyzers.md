### CA1001: Types that own disposable fields should be disposable ###

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182172.aspx](http://msdn.microsoft.com/library/ms182172.aspx)

### CA1003: Use System.EventHandler<T> where T inherits System.EventArgs or use System.EventHandler ###

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182178.aspx](http://msdn.microsoft.com/library/ms182178.aspx)

### CA1014: Mark assemblies with CLSCompliantAttribute ###

Assemblies should explicitly state their CLS compliance using the CLSCompliant attribute. An assembly without this attribute is not CLS-compliant. Assemblies, modules, and types can be CLS-compliant even if some parts of the assembly, module, or type are not CLS-compliant. The following rules apply: 1) If the element is marked CLSCompliant, any noncompliant members must have the CLSCompliant attribute present with its argument set to false. 2) A comparable CLS-compliant alternative member must be supplied for each member that is not CLS-compliant.

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182156.aspx](http://msdn.microsoft.com/library/ms182156.aspx)

### CA1016: Assemblies should be marked with AssemblyVersionAttribute ###

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182155.aspx](http://msdn.microsoft.com/library/ms182155.aspx)

### CA1017: Mark all assemblies with ComVisible ###

The System.Runtime.InteropServices.ComVisible attribute indicates whether COM clients can use the library. Good design dictates that developers explicitly indicate COM visibility. The default value for this attribute is 'true'. However, the best design is to mark the assembly ComVisible false, and then mark types, interfaces, and individual members as ComVisible true, as appropriate.

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182157.aspx](http://msdn.microsoft.com/library/ms182157.aspx)

### CA1018: Custom attributes should have AttributeUsage attribute defined. ###

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182158.aspx](http://msdn.microsoft.com/library/ms182158.aspx)

### CA1019: Define accessors for attribute arguments. ###

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182136.aspx](http://msdn.microsoft.com/library/ms182136.aspx)

### CA1027: Mark Enum with FlagsAttribute ###

The enumeration appears to be made up of combinable flags. If this true, apply the Flags attribute to the enumeration.

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182159.aspx](http://msdn.microsoft.com/library/ms182159.aspx)

### CA1036: Overload operator Equals and comparison operators when implementing System.IComparable ###

Types that implement IComparable should redefine Equals and comparison operators to keep the meanings of less than, greater than, and equals consistent throughout the type.

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182163.aspx](http://msdn.microsoft.com/library/ms182163.aspx)

### CA1309: String comparison should use StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase ###

For non-linguistic comparisons, StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase should be used instead of the linguistically-sensitive StringComparison.InvariantCulture.

Category: Globalization
Severity: Warning

Help: [http://msdn.microsoft.com/library/bb385972.aspx](http://msdn.microsoft.com/library/bb385972.aspx)

### CA1813: Avoid unsealed attributes. ###

Category: Performance
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182267.aspx](http://msdn.microsoft.com/library/ms182267.aspx)

### CA1820: Test for empty strings using 'string.Length' property or 'string.IsNullOrEmpty' method instead of an Equality check. ###

Comparing strings using the string.Length property or the string.IsNullOrEmpty method is significantly faster than using string.Equals. This is because Equals executes significantly more MSIL instructions than either IsNullOrEmpty or the number of instructions executed to retrieve the Length property value and compare it to zero.

Category: Performance
Severity: Warning

Help: [https://msdn.microsoft.com/library/ms182279.aspx](https://msdn.microsoft.com/library/ms182279.aspx)

### CA2002: Do not lock on objects with weak identity. ###

Category: Reliability
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182290.aspx](http://msdn.microsoft.com/library/ms182290.aspx)

### CA2213: Disposable fields should be disposed ###

Category: Usage
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182328.aspx](http://msdn.microsoft.com/library/ms182328.aspx)

### CA2217: Do not mark Enum with FlagsAttribute ###

The enumeration does not appear to contain a consistent set values that can be combined using  the OR (|) and AND (&amp;amp;amp;) operators. Using the Flags attribute on an enumeration suggests that the enumeration values are meant to be combined using the AND and OR operators. Typically, a flags enumeration uses values that are either powers of two, or combine other values that are powers of two.

Category: Usage
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182335.aspx](http://msdn.microsoft.com/library/ms182335.aspx)

### CA2231: Overload operator equals on overriding ValueType.Equals ###

Value types that redefine System.ValueType.Equals should redefine the equality operator as well to ensure that these members return the same results. This helps ensure that types that rely on Equals (such as ArrayList and Hashtable) behave in a manner that is expected and consistent with the equality operator.

Category: Usage
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182359.aspx](http://msdn.microsoft.com/library/ms182359.aspx)