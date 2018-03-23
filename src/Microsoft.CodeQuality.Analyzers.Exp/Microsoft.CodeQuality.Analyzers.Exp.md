### CA1303: Do not pass literals as localized parameters ###

A method passes a string literal as a parameter to a constructor or method in the .NET Framework class library and that string should be localizable.

Category: Globalization

Severity: Warning

Help: [https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1303-do-not-pass-literals-as-localized-parameters](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1303-do-not-pass-literals-as-localized-parameters)

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