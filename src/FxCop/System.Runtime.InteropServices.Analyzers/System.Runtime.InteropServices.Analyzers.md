### CA1060: Move P/Invokes to native methods class ###

Category: Design
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182161.aspx](http://msdn.microsoft.com/library/ms182161.aspx)

### CA1401: P/Invokes should not be visible. ###

Category: Interoperability
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182209.aspx](http://msdn.microsoft.com/library/ms182209.aspx)

### CA2101: Specify marshaling for P/Invoke string arguments ###

When marshaling strings as ANSI (or as Auto on Win9x), some characters may be changed. If best-fit mapping is on, strings that appear different in Unicode will be marshaled to identical ANSI strings, which may lead to incorrect security decisions. Turning best-fit mapping off reduces this risk, as all characters without equivalents are mapped to '?'. Also, note that CharSet.Ansi is the default setting for all string marshaling; Unicode marshaling must be specified explicitly, either as a CharSet setting of DllImport or StructLayout, or as a MarshalAs attribute with a Unicode (or system-dependent) UnmanagedType.

Category: Globalization
Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182319.aspx](http://msdn.microsoft.com/library/ms182319.aspx)