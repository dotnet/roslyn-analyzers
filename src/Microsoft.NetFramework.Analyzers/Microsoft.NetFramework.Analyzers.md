### CA1058: Types should not extend certain base types ###

An externally visible type extends certain base types. Use one of the alternatives.

Category: Design

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca1058-types-should-not-extend-certain-base-types](https://docs.microsoft.com/visualstudio/code-quality/ca1058-types-should-not-extend-certain-base-types)

### CA2153: Do Not Catch Corrupted State Exceptions ###

Catching corrupted state exceptions could mask errors (such as access violations), resulting in inconsistent state of execution or making it easier for attackers to compromise system. Instead, catch and handle a more specific set of exception type(s) or re-throw the exception

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca2153-avoid-handling-corrupted-state-exceptions](https://docs.microsoft.com/visualstudio/code-quality/ca2153-avoid-handling-corrupted-state-exceptions)

### CA3075: Insecure DTD processing in XML ###

Using XmlTextReader.Load(), creating an insecure XmlReaderSettings instance when invoking XmlReader.Create(), setting the InnerXml property of the XmlDocument and enabling DTD processing using XmlUrlResolver insecurely can lead to information disclosure. Replace it with a call to the Load() method overload that takes an XmlReader instance, use XmlReader.Create() to accept XmlReaderSettings arguments or consider explicitly setting secure values. The DataViewSettingCollectionString property of DataViewManager should always be assigned from a trusted source, the DtdProcessing property should be set to false, and the XmlResolver property should be changed to XmlSecureResolver or null. 

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3075-insecure-dtd-processing](https://docs.microsoft.com/visualstudio/code-quality/ca3075-insecure-dtd-processing)

### CA3076: Insecure XSLT script processing. ###

Providing an insecure XsltSettings instance and an insecure XmlResolver instance to XslCompiledTransform.Load method is potentially unsafe as it allows processing script within XSL, which on an untrusted XSL input may lead to malicious code execution. Either replace the insecure XsltSettings argument with XsltSettings.Default or an instance that has disabled document function and script execution, or replace the XmlResolver argurment with null or an XmlSecureResolver instance. This message may be suppressed if the input is known to be from a trusted source and external resource resolution from locations that are not known in advance must be supported.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3076-insecure-xslt-script-execution](https://docs.microsoft.com/visualstudio/code-quality/ca3076-insecure-xslt-script-execution)

### CA3077: Insecure Processing in API Design, XmlDocument and XmlTextReader ###

Enabling DTD processing on all instances derived from XmlTextReader or  XmlDocument and using XmlUrlResolver for resolving external XML entities may lead to information disclosure. Ensure to set the XmlResolver property to null, create an instance of XmlSecureResolver when processing untrusted input, or use XmlReader.Create method with a secure XmlReaderSettings argument. Unless you need to enable it, ensure the DtdProcessing property is set to false. 

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3077-insecure-processing-in-api-design-xml-document-and-xml-text-reader](https://docs.microsoft.com/visualstudio/code-quality/ca3077-insecure-processing-in-api-design-xml-document-and-xml-text-reader)

### CA3147: Mark Verb Handlers With Validate Antiforgery Token ###

Missing ValidateAntiForgeryTokenAttribute on controller action {0}.

Category: Security

Severity: Warning

IsEnabledByDefault: True

Help: [https://docs.microsoft.com/visualstudio/code-quality/ca3147-mark-verb-handlers-with-validateantiforgerytoken](https://docs.microsoft.com/visualstudio/code-quality/ca3147-mark-verb-handlers-with-validateantiforgerytoken)

