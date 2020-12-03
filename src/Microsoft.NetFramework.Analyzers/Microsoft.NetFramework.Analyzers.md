# Microsoft.NetFramework.Analyzers

## [CA1058](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1058): Types should not extend certain base types

An externally visible type extends certain base types. Use one of the alternatives.

|Item|Value|
|-|-|
|Category|Design|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA2153](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2153): Do Not Catch Corrupted State Exceptions

Catching corrupted state exceptions could mask errors (such as access violations), resulting in inconsistent state of execution or making it easier for attackers to compromise system. Instead, catch and handle a more specific set of exception type(s) or re-throw the exception.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA3075](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3075): Insecure DTD processing in XML

Using XmlTextReader.Load(), creating an insecure XmlReaderSettings instance when invoking XmlReader.Create(), setting the InnerXml property of the XmlDocument and enabling DTD processing using XmlUrlResolver insecurely can lead to information disclosure. Replace it with a call to the Load() method overload that takes an XmlReader instance, use XmlReader.Create() to accept XmlReaderSettings arguments or consider explicitly setting secure values. The DataViewSettingCollectionString property of DataViewManager should always be assigned from a trusted source, the DtdProcessing property should be set to false, and the XmlResolver property should be changed to XmlSecureResolver or null.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA3076](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3076): Insecure XSLT script processing.

Providing an insecure XsltSettings instance and an insecure XmlResolver instance to XslCompiledTransform.Load method is potentially unsafe as it allows processing script within XSL, which on an untrusted XSL input may lead to malicious code execution. Either replace the insecure XsltSettings argument with XsltSettings.Default or an instance that has disabled document function and script execution, or replace the XmlResolver argument with null or an XmlSecureResolver instance. This message may be suppressed if the input is known to be from a trusted source and external resource resolution from locations that are not known in advance must be supported.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA3077](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3077): Insecure Processing in API Design, XmlDocument and XmlTextReader

Enabling DTD processing on all instances derived from XmlTextReader or  XmlDocument and using XmlUrlResolver for resolving external XML entities may lead to information disclosure. Ensure to set the XmlResolver property to null, create an instance of XmlSecureResolver when processing untrusted input, or use XmlReader.Create method with a secure XmlReaderSettings argument. Unless you need to enable it, ensure the DtdProcessing property is set to false.

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA3147](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca3147): Mark Verb Handlers With Validate Antiforgery Token

Missing ValidateAntiForgeryTokenAttribute on controller action {0}

|Item|Value|
|-|-|
|Category|Security|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---

## [CA9998](https://docs.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers): Analyzer package has been deprecated

FxCopAnalyzers package has been deprecated in favor of 'Microsoft.CodeAnalysis.NetAnalyzers', that ships with the .NET SDK. Please refer to https://docs.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers to migrate to .NET analyzers.

|Item|Value|
|-|-|
|Category|Reliability|
|Enabled|True|
|Severity|Warning|
|CodeFix|False|
---
