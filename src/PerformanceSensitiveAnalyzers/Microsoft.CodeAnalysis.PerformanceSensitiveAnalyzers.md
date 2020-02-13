
Rule ID | Title | Category | Enabled | Severity | CodeFix | Description |
--------|-------|----------|---------|----------|---------|--------------------------------------------------------------------------------------------------------------|
HAA0101 | Array allocation for params parameter | Performance | True | Warning | False | This call site is calling into a function with a 'params' parameter. This results in an array allocation |
HAA0102 | Non-overridden virtual method call on value type | Performance | True | Warning | False | Non-overridden virtual method call on a value type adds a boxing or constrained instruction |
[HAA0201](http://msdn.microsoft.com/en-us/library/2839d5h5(v=vs.110).aspx) | Implicit string concatenation allocation | Performance | True | Warning | False | Considering using StringBuilder |
[HAA0202](http://msdn.microsoft.com/en-us/library/yz2be5wk.aspx) | Value type to reference type conversion allocation for string concatenation | Performance | True | Warning | False | Value type ({0}) is being boxed to a reference type for a string concatenation. |
HAA0301 | Closure Allocation Source | Performance | True | Warning | False | Heap allocation of closure Captures: {0} |
HAA0302 | Display class allocation to capture closure | Performance | True | Warning | False | The compiler will emit a class that will hold this as a field to allow capturing of this closure |
HAA0303 | Lambda or anonymous method in a generic method allocates a delegate instance | Performance | True | Warning | False | Considering moving this out of the generic method |
HAA0401 | Possible allocation of reference type enumerator | Performance | True | Warning | False | Non-ValueType enumerator may result in a heap allocation |
HAA0501 | Explicit new array type allocation | Performance | True | Info | False | Explicit new array type allocation |
HAA0502 | Explicit new reference type allocation | Performance | True | Info | False | Explicit new reference type allocation |
[HAA0503](http://msdn.microsoft.com/en-us/library/bb397696.aspx) | Explicit new anonymous object allocation | Performance | True | Info | False | Explicit new anonymous object allocation |
HAA0506 | Let clause induced allocation | Performance | True | Info | False | Let clause induced allocation |
HAA0601 | Value type to reference type conversion causing boxing allocation | Performance | True | Warning | False | Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable |
HAA0602 | Delegate on struct instance caused a boxing allocation | Performance | True | Warning | False | Struct instance method being used for delegate creation, this will result in a boxing instruction |
HAA0603 | Delegate allocation from a method group | Performance | True | Warning | False | This will allocate a delegate instance |
HAA0604 | Delegate allocation from a method group | Performance | True | Info | False | This will allocate a delegate instance |
