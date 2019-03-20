### HAA0101: Array allocation for params parameter ###

This call site is calling into a function with a 'params' parameter. This results in an array allocation even if no parameter is passed in for the params parameter

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0102: Non-overridden virtual method call on value type ###

Non-overridden virtual method call on a value type adds a boxing or constrained instruction

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0201: Implicit string concatenation allocation ###

Considering using StringBuilder

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [http://msdn.microsoft.com/en-us/library/2839d5h5(v=vs.110).aspx](http://msdn.microsoft.com/en-us/library/2839d5h5(v=vs.110).aspx)

### HAA0202: Value type to reference type conversion allocation for string concatenation ###

Value type ({0}) is being boxed to a reference type for a string concatenation.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [http://msdn.microsoft.com/en-us/library/yz2be5wk.aspx](http://msdn.microsoft.com/en-us/library/yz2be5wk.aspx)

### HAA0301: Closure Allocation Source ###

Heap allocation of closure Captures: {0}

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0302: Display class allocation to capture closure ###

The compiler will emit a class that will hold this as a field to allow capturing of this closure

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0303: Lambda or anonymous method in a generic method allocates a delegate instance ###

Considering moving this out of the generic method

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0401: Possible allocation of reference type enumerator ###

Non-ValueType enumerator may result in a heap allocation

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0501: Explicit new array type allocation ###

Explicit new array type allocation

Category: Performance

Severity: Info

IsEnabledByDefault: True

### HAA0502: Explicit new reference type allocation ###

Explicit new reference type allocation

Category: Performance

Severity: Info

IsEnabledByDefault: True

### HAA0503: Explicit new anonymous object allocation ###

Explicit new anonymous object allocation

Category: Performance

Severity: Info

IsEnabledByDefault: True

Help: [http://msdn.microsoft.com/en-us/library/bb397696.aspx](http://msdn.microsoft.com/en-us/library/bb397696.aspx)

### HAA0504: Implicit new array creation allocation ###

Implicit new array creation allocation

Category: Performance

Severity: Info

IsEnabledByDefault: True

### HAA0505: Initializer reference type allocation ###

Initializer reference type allocation

Category: Performance

Severity: Info

IsEnabledByDefault: True

### HAA0506: Let clause induced allocation ###

Let clause induced allocation

Category: Performance

Severity: Info

IsEnabledByDefault: True

### HAA0601: Value type to reference type conversion causing boxing allocation ###

Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0602: Delegate on struct instance caused a boxing allocation ###

Struct instance method being used for delegate creation, this will result in a boxing instruction

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HAA0603: Delegate allocation from a method group ###

This will allocate a delegate instance

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### HeapAnalyzerReadonlyMethodGroupAllocationRule: Delegate allocation from a method group ###

This will allocate a delegate instance

Category: Performance

Severity: Info

IsEnabledByDefault: True

