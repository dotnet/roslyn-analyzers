### CA1814: Prefer jagged arrays over multidimensional ###

A jagged array is an array whose elements are arrays. The arrays that make up the elements can be of different sizes, leading to less wasted space for some sets of data.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182277.aspx](https://msdn.microsoft.com/en-us/library/ms182277.aspx)

### CA1821: Remove empty Finalizers ###

Finalizers should be avoided where possible, to avoid the additional performance overhead involved in tracking object lifetime.

Category: Performance

Severity: Warning

Help: [http://msdn.microsoft.com/library/bb264476.aspx](http://msdn.microsoft.com/library/bb264476.aspx)

### CA2200: Rethrow to preserve stack details. ###

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182363.aspx](http://msdn.microsoft.com/library/ms182363.aspx)

### CA2214: Do not call overridable methods in constructors ###

Virtual methods defined on the class should not be called from constructors. If a derived class has overridden the method, the derived class version will be called (before the derived class constructor is called).

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182331.aspx](http://msdn.microsoft.com/library/ms182331.aspx)