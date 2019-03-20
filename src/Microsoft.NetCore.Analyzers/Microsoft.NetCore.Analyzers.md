### CA1303: Do not pass literals as localized parameters ###

A method passes a string literal as a parameter to a constructor or method in the .NET Framework class library and that string should be localizable. To fix a violation of this rule, replace the string literal with a string retrieved through an instance of the ResourceManager class.

Category: Globalization

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1303-do-not-pass-literals-as-localized-parameters](https://docs.microsoft.com/visualstudio/code-quality/ca1303-do-not-pass-literals-as-localized-parameters)

### CA1304: Specify CultureInfo ###

A method or constructor calls a member that has an overload that accepts a System.Globalization.CultureInfo parameter, and the method or constructor does not call the overload that takes the CultureInfo parameter. When a CultureInfo or System.IFormatProvider object is not supplied, the default value that is supplied by the overloaded member might not have the effect that you want in all locales. If the result will be displayed to the user, specify 'CultureInfo.CurrentCulture' as the 'CultureInfo' parameter. Otherwise, if the result will be stored and accessed by software, such as when it is persisted to disk or to a database, specify 'CultureInfo.InvariantCulture'.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1304-specify-cultureinfo](https://docs.microsoft.com/visualstudio/code-quality/ca1304-specify-cultureinfo)

### CA1305: Specify IFormatProvider ###

A method or constructor calls one or more members that have overloads that accept a System.IFormatProvider parameter, and the method or constructor does not call the overload that takes the IFormatProvider parameter. When a System.Globalization.CultureInfo or IFormatProvider object is not supplied, the default value that is supplied by the overloaded member might not have the effect that you want in all locales. If the result will be based on the input from/output displayed to the user, specify 'CultureInfo.CurrentCulture' as the 'IFormatProvider'. Otherwise, if the result will be stored and accessed by software, such as when it is loaded from disk/database and when it is persisted to disk/database, specify 'CultureInfo.InvariantCulture'

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1305-specify-iformatprovider](https://docs.microsoft.com/visualstudio/code-quality/ca1305-specify-iformatprovider)

### CA1307: Specify StringComparison ###

A string comparison operation uses a method overload that does not set a StringComparison parameter. If the result will be displayed to the user, such as when sorting a list of items for display in a list box, specify 'StringComparison.CurrentCulture' or 'StringComparison.CurrentCultureIgnoreCase' as the 'StringComparison' parameter. If comparing case-insensitive identifiers, such as file paths, environment variables, or registry keys and values, specify 'StringComparison.OrdinalIgnoreCase'. Otherwise, if comparing case-sensitive identifiers, specify 'StringComparison.Ordinal'.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1307-specify-stringcomparison](https://docs.microsoft.com/visualstudio/code-quality/ca1307-specify-stringcomparison)

### CA1308: Normalize strings to uppercase ###

Strings should be normalized to uppercase. A small group of characters cannot make a round trip when they are converted to lowercase. To make a round trip means to convert the characters from one locale to another locale that represents character data differently, and then to accurately retrieve the original characters from the converted characters.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1308-normalize-strings-to-uppercase](https://docs.microsoft.com/visualstudio/code-quality/ca1308-normalize-strings-to-uppercase)

### CA1309: Use ordinal stringcomparison ###

A string comparison operation that is nonlinguistic does not set the StringComparison parameter to either Ordinal or OrdinalIgnoreCase. By explicitly setting the parameter to either StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase, your code often gains speed, becomes more correct, and becomes more reliable.

Category: Globalization

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison](https://docs.microsoft.com/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison)

### CA1401: P/Invokes should not be visible ###

A public or protected method in a public type has the System.Runtime.InteropServices.DllImportAttribute attribute (also implemented by the Declare keyword in Visual Basic). Such methods should not be exposed.

Category: Interoperability

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1401-p-invokes-should-not-be-visible](https://docs.microsoft.com/visualstudio/code-quality/ca1401-p-invokes-should-not-be-visible)

### CA1810: Initialize reference type static fields inline ###

A reference type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1810-initialize-reference-type-static-fields-inline](https://docs.microsoft.com/visualstudio/code-quality/ca1810-initialize-reference-type-static-fields-inline)

### CA1813: Avoid unsealed attributes ###

The .NET Framework class library provides methods for retrieving custom attributes. By default, these methods search the attribute inheritance hierarchy. Sealing the attribute eliminates the search through the inheritance hierarchy and can improve performance.

Category: Performance

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1813-avoid-unsealed-attributes](https://docs.microsoft.com/visualstudio/code-quality/ca1813-avoid-unsealed-attributes)

### CA1816: Dispose methods should call SuppressFinalize ###

A method that is an implementation of Dispose does not call GC.SuppressFinalize; or a method that is not an implementation of Dispose calls GC.SuppressFinalize; or a method calls GC.SuppressFinalize and passes something other than this (Me in Visual?Basic).

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1816-call-gc-suppressfinalize-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca1816-call-gc-suppressfinalize-correctly)

### CA1820: Test for empty strings using string length ###

Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1820-test-for-empty-strings-using-string-length](https://docs.microsoft.com/visualstudio/code-quality/ca1820-test-for-empty-strings-using-string-length)

### CA1824: Mark assemblies with NeutralResourcesLanguageAttribute ###

The NeutralResourcesLanguage attribute informs the ResourceManager of the language that was used to display the resources of a neutral culture for an assembly. This improves lookup performance for the first resource that you load and can reduce your working set.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1824-mark-assemblies-with-neutralresourceslanguageattribute](https://docs.microsoft.com/visualstudio/code-quality/ca1824-mark-assemblies-with-neutralresourceslanguageattribute)

### CA1825: Avoid zero-length array allocations. ###

Avoid unnecessary zero-length array allocations.  Use {0} instead.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### CA1826: Do not use Enumerable methods on indexable collections. Instead use the collection directly ###

This collection is directly indexable. Going through LINQ here causes unnecessary allocations and CPU work.

Category: Performance

Severity: Warning

IsEnabledByDefault: True

### CA2000: Dispose objects before losing scope ###

If a disposable object is not explicitly disposed before all references to it are out of scope, the object will be disposed at some indeterminate time when the garbage collector runs the finalizer of the object. Because an exceptional event might occur that will prevent the finalizer of the object from running, the object should be explicitly disposed instead.

Category: Reliability

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope](https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope)

### CA2002: Do not lock on objects with weak identity ###

An object is said to have a weak identity when it can be directly accessed across application domain boundaries. A thread that tries to acquire a lock on an object that has a weak identity can be blocked by a second thread in a different application domain that has a lock on the same object.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2002-do-not-lock-on-objects-with-weak-identity](https://docs.microsoft.com/visualstudio/code-quality/ca2002-do-not-lock-on-objects-with-weak-identity)

### CA2008: Do not create tasks without passing a TaskScheduler ###

Do not create tasks unless you are using one of the overloads that takes a TaskScheduler. The default is to schedule on TaskScheduler.Current, which would lead to deadlocks. Either use TaskScheduler.Default to schedule on the thread pool, or explicitly pass TaskScheduler.Current to make your intentions clear.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2009: Do not call ToImmutableCollection on an ImmutableCollection value ###

Do not call {0} on an {1} value

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2010: Always consume the value returned by methods marked with PreserveSigAttribute ###

PreserveSigAttribute indicates that a method will return an HRESULT, rather than throwing an exception. Therefore, it is important to consume the HRESULT returned by the method, so that errors can be detected. Generally, this is done by calling Marshal.ThrowExceptionForHR.

Category: Reliability

Severity: Warning

IsEnabledByDefault: True

### CA2100: Review SQL queries for security vulnerabilities ###

SQL queries that directly use user input can be vulnerable to SQL injection attacks. Review this SQL query for potential vulnerabilities, and consider using a parameterized SQL query.

Category: Security

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities](https://docs.microsoft.com/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities)

### CA2101: Specify marshaling for P/Invoke string arguments ###

A platform invoke member allows partially trusted callers, has a string parameter, and does not explicitly marshal the string. This can cause a potential security vulnerability.

Category: Globalization

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2101-specify-marshaling-for-p-invoke-string-arguments](https://docs.microsoft.com/visualstudio/code-quality/ca2101-specify-marshaling-for-p-invoke-string-arguments)

### CA2201: Do not raise reserved exception types ###

An exception of type that is not sufficiently specific or reserved by the runtime should never be raised by user code. This makes the original error difficult to detect and debug. If this exception instance might be thrown, use a different exception type.

Category: Usage

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2201-do-not-raise-reserved-exception-types](https://docs.microsoft.com/visualstudio/code-quality/ca2201-do-not-raise-reserved-exception-types)

### CA2207: Initialize value type static fields inline ###

A value type declares an explicit static constructor. To fix a violation of this rule, initialize all static data when it is declared and remove the static constructor.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2207-initialize-value-type-static-fields-inline](https://docs.microsoft.com/visualstudio/code-quality/ca2207-initialize-value-type-static-fields-inline)

### CA2208: Instantiate argument exceptions correctly ###

A call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2208-instantiate-argument-exceptions-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca2208-instantiate-argument-exceptions-correctly)

### CA2213: Disposable fields should be disposed ###

A type that implements System.IDisposable declares fields that are of types that also implement IDisposable. The Dispose method of the field is not called by the Dispose method of the declaring type. To fix a violation of this rule, call Dispose on fields that are of types that implement IDisposable if you are responsible for allocating and releasing the unmanaged resources held by the field.

Category: Usage

Severity: Warning

IsEnabledByDefault: False

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed](https://docs.microsoft.com/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed)

### CA2216: Disposable types should declare finalizer ###

A type that implements System.IDisposable and has fields that suggest the use of unmanaged resources does not implement a finalizer, as described by Object.Finalize.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2216-disposable-types-should-declare-finalizer](https://docs.microsoft.com/visualstudio/code-quality/ca2216-disposable-types-should-declare-finalizer)

### CA2229: Implement serialization constructors ###

To fix a violation of this rule, implement the serialization constructor. For a sealed class, make the constructor private; otherwise, make it protected.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2229-implement-serialization-constructors](https://docs.microsoft.com/visualstudio/code-quality/ca2229-implement-serialization-constructors)

### CA2235: Mark all non-serializable fields ###

An instance field of a type that is not serializable is declared in a type that is serializable.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2235-mark-all-non-serializable-fields](https://docs.microsoft.com/visualstudio/code-quality/ca2235-mark-all-non-serializable-fields)

### CA2237: Mark ISerializable types with serializable ###

To be recognized by the common language runtime as serializable, types must be marked by using the SerializableAttribute attribute even when the type uses a custom serialization routine through implementation of the ISerializable interface.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2237-mark-iserializable-types-with-serializableattribute](https://docs.microsoft.com/visualstudio/code-quality/ca2237-mark-iserializable-types-with-serializableattribute)

### CA2241: Provide correct arguments to formatting methods ###

The format argument that is passed to System.String.Format does not contain a format item that corresponds to each object argument, or vice versa.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2241-provide-correct-arguments-to-formatting-methods](https://docs.microsoft.com/visualstudio/code-quality/ca2241-provide-correct-arguments-to-formatting-methods)

### CA2242: Test for NaN correctly ###

This expression tests a value against Single.Nan or Double.Nan. Use Single.IsNan(Single) or Double.IsNan(Double) to test the value.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2242-test-for-nan-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca2242-test-for-nan-correctly)

### CA2243: Attribute string literals should parse correctly ###

The string literal parameter of an attribute does not parse correctly for a URL, a GUID, or a version.

Category: Usage

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2243-attribute-string-literals-should-parse-correctly](https://docs.microsoft.com/visualstudio/code-quality/ca2243-attribute-string-literals-should-parse-correctly)

### CA2300: Do not use insecure deserializer BinaryFormatter ###

The method '{0}' is insecure when deserializing untrusted data.  If you need to instead detect BinaryFormatter deserialization without a SerializationBinder set, then disable rule CA2300, and enable rules CA2301 and CA2302.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2301: Do not call BinaryFormatter.Deserialize without first setting BinaryFormatter.Binder ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2302: Ensure BinaryFormatter.Binder is set before calling BinaryFormatter.Deserialize ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2305: Do not use insecure deserializer LosFormatter ###

The method '{0}' is insecure when deserializing untrusted data.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2310: Do not use insecure deserializer NetDataContractSerializer ###

The method '{0}' is insecure when deserializing untrusted data.  If you need to instead detect NetDataContractSerializer deserialization without a SerializationBinder set, then disable rule CA2310, and enable rules CA2311 and CA2312.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2311: Do not deserialize without first setting NetDataContractSerializer.Binder ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2312: Ensure NetDataContractSerializer.Binder is set before deserializing ###

The method '{0}' is insecure when deserializing untrusted data without a SerializationBinder to restrict the type of objects in the deserialized object graph.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA2315: Do not use insecure deserializer ObjectStateFormatter ###

The method '{0}' is insecure when deserializing untrusted data.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3001: Review code for SQL injection vulnerabilities ###

Potential SQL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3002: Review code for XSS vulnerabilities ###

Potential cross-site scripting (XSS) vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3003: Review code for file path injection vulnerabilities ###

Potential file path injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3004: Review code for information disclosure vulnerabilities ###

Potential information disclosure vulnerability was found where '{0}' in method '{1}' may contain unintended information from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3005: Review code for LDAP injection vulnerabilities ###

Potential LDAP injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3006: Review code for process command injection vulnerabilities ###

Potential process command injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3007: Review code for open redirect vulnerabilities ###

Potential open redirect vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3008: Review code for XPath injection vulnerabilities ###

Potential XPath injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3009: Review code for XML injection vulnerabilities ###

Potential XML injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3010: Review code for XAML injection vulnerabilities ###

Potential XAML injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3011: Review code for DLL injection vulnerabilities ###

Potential DLL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA3012: Review code for regex injection vulnerabilities ###

Potential regex injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA5350: Do Not Use Weak Cryptographic Algorithms ###

Cryptographic algorithms degrade over time as attacks become for advances to attacker get access to more computation. Depending on the type and application of this cryptographic algorithm, further degradation of the cryptographic strength of it may allow attackers to read enciphered messages, tamper with enciphered  messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA-2 512, SHA-2 384, or SHA-2 256.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca5350-do-not-use-weak-cryptographic-algorithms](https://docs.microsoft.com/visualstudio/code-quality/ca5350-do-not-use-weak-cryptographic-algorithms)

### CA5351: Do Not Use Broken Cryptographic Algorithms ###

An attack making it computationally feasible to break this algorithm exists. This allows attackers to break the cryptographic guarantees it is designed to provide. Depending on the type and application of this cryptographic algorithm, this may allow attackers to read enciphered messages, tamper with enciphered  messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA512, SHA384, or SHA256. Replace digital signature uses with RSA with a key length greater than or equal to 2048-bits, or ECDSA with a key length greater than or equal to 256 bits.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca5351-do-not-use-broken-cryptographic-algorithms](https://docs.microsoft.com/visualstudio/code-quality/ca5351-do-not-use-broken-cryptographic-algorithms)

### CA5358: Do Not Use Unsafe Cipher Modes ###

These modes are vulnerable to attacks. Use only approved modes (CBC, CTS).

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA5359: Do Not Disable Certificate Validation ###

A certificate can help authenticate the identity of the server. Clients should validate the server certificate to ensure requests are sent to the intended server. If the ServerCertificateValidationCallback always returns 'true', any certificate will pass validation.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5360: Do Not Call Dangerous Methods In Deserialization ###

Insecure Deserialization is a vulnerability which occurs when untrusted data is used to abuse the logic of an application, inflict a Denial-of-Service (DoS) attack, or even execute arbitrary code upon it being deserialized. It’s frequently possible for malicious users to abuse these deserialization features when the application is deserializing untrusted data which is under their control. Specifically, invoke dangerous methods in the process of deserialization. Successful insecure deserialization attacks could allow an attacker to carry out attacks such as DoS attacks, authentication bypasses, and remote code execution.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5361: Do Not Disable SChannel Use of Strong Crypto ###

Starting with the .NET Framework 4.6, the System.Net.ServicePointManager and System.Net.Security.SslStream classes are recommeded to use new protocols. The old ones have protocol weaknesses and are not supported. Setting Switch.System.Net.DontEnableSchUseStrongCrypto with true will use the old weak crypto check and opt out of the protocol migration.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5362: Do Not Refer Self In Serializable Class ###

This can allow an attacker to DOS or exhaust the memory of the process.

Category: Security

Severity: Warning

IsEnabledByDefault: False

### CA5363: Do Not Disable Request Validation ###

Request validation is a feature in ASP.NET that examines HTTP requests and determines whether they contain potentially dangerous content. This check adds protection from markup or code in the URL query string, cookies, or posted form values that might have been added for malicious purposes. So, it is generally desirable and should be left enabled for defense in depth.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5364: Do Not Use Deprecated Security Protocols ###

Using a deprecated security protocol rather than the system default is risky.

Category: Security

Severity: Warning

IsEnabledByDefault: True

### CA5365: Do Not Disable HTTP Header Checking ###

HTTP header checking enables encoding of the carriage return and newline characters, \r and \n, that are found in response headers. This encoding can help to avoid injection attacks that exploit an application that echoes untrusted data contained by the header.

Category: Security

Severity: Warning

IsEnabledByDefault: True

