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