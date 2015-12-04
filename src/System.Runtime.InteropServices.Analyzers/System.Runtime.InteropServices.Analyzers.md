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