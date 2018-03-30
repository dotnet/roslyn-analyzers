### CA1062: Validate arguments of public methods ###

An externally visible method dereferences one of its reference arguments without verifying whether that argument is null (Nothing in Visual Basic). All reference arguments that are passed to externally visible methods should be checked against null.

Category: Maintainability.

Severity: Warning

Help: [https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1062-validate-arguments-of-public-methods](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1062-validate-arguments-of-public-methods)

### CA1508: Avoid dead conditional code ###

Conditional expressions which are always true/false and null checks for operations that are always null/non-null lead to dead code. Such conditional expressions should be removed or refactored to avoid dead code.

Category: Maintainability.

Severity: Warning

Help: <To be added>

### CA2000: Dispose objects before losing scope ###

A local object of a IDisposable type is created but the object is not disposed before all references to the object are out of scope.

Category: Reliability

Severity: Warning

Help: [https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope)

### CA2100: Review SQL queries for security vulnerabilities ###

A method sets the System.Data.IDbCommand.CommandText property by using a string that is built from a string argument to the method.

Category: Security

Severity: Warning

Help: [https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities)

### CA2213: Disposable fields should be disposed ###

A type that implements System.IDisposable declares fields that are of types that also implement IDisposable. The Dispose method of the field is not called by the Dispose method of the declaring type.

Category: Usage

Severity: Warning

Help: [https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed)
