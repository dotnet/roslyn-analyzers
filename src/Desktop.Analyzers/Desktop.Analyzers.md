### CA1058: Types should not extend certain base types ###

An externally visible type extends certain base types. Use one of the alternatives.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/ms182171.aspx](https://msdn.microsoft.com/en-us/library/ms182171.aspx)

### CA2153: Do Not Catch Corrupted State Exceptions ###

Catching corrupted state exceptions could mask errors (such as access violations), resulting in inconsistent state of execution or making it easier for attackers to compromise system. Instead, catch and handle a more specific set of exception type(s) or re-throw the exception

Category: Security

Severity: Warning

Help: [http://aka.ms/CA2153](http://aka.ms/CA2153)

### CA2229: Implement serialization constructors ###

To fix a violation of this rule, implement the serialization constructor. For a sealed class, make the constructor private; otherwise, make it protected.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182343.aspx](http://msdn.microsoft.com/library/ms182343.aspx)

### CA2235: Mark all non-serializable fields ###

An instance field of a type that is not serializable is declared in a type that is serializable.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182349.aspx](http://msdn.microsoft.com/library/ms182349.aspx)

### CA2237: Mark ISerializable types with serializable ###

To be recognized by the common language runtime as serializable, types must be marked by using the SerializableAttribute attribute even when the type uses a custom serialization routine through implementation of the ISerializable interface.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182350.aspx](http://msdn.microsoft.com/library/ms182350.aspx)

### CA3075: Insecure DTD processing in XML ###

Using XmlTextReader.Load(), creating an insecure XmlReaderSettings instance when invoking XmlReader.Create(), setting the InnerXml property of the XmlDocument and enabling DTD processing using XmlUrlResolver insecurely can lead to information disclosure. Replace it with a call to the Load() method overload that takes an XmlReader instance, use XmlReader.Create() to accept XmlReaderSettings arguments or consider explicitly setting secure values. The DataViewSettingCollectionString property of DataViewManager should always be assigned from a trusted source, the DtdProcessing property should be set to false, and the XmlResolver property should be changed to XmlSecureResolver or null.ÿ

Category: Security

Severity: Warning

Help: [http://aka.ms/CA3075](http://aka.ms/CA3075)

### CA5350: Do Not Use Weak Cryptographic Algorithms ###

Cryptographic algorithms degrade over time as attacks become for advances to attacker get access to more computation. Depending on the type and application of this cryptographic algorithm, further degradation of the cryptographic strength of it may allow attackers to read enciphered messages, tamper with enciphered? messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA-2 512, SHA-2 384, or SHA-2 256.

Category: Security

Severity: Warning

Help: [http://aka.ms/CA5350](http://aka.ms/CA5350)

### CA5351: Do Not Use Broken Cryptographic Algorithms ###

An attack making it computationally feasible to break this algorithm exists. This allows attackers to break the cryptographic guarantees it is designed to provide. Depending on the type and application of this cryptographic algorithm, this may allow attackers to read enciphered messages, tamper with enciphered? messages, forge digital signatures, tamper with hashed content, or otherwise compromise any cryptosystem based on this algorithm. Replace encryption uses with the AES algorithm (AES-256, AES-192 and AES-128 are acceptable) with a key length greater than or equal to 128 bits. Replace hashing uses with a hashing function in the SHA-2 family, such as SHA512, SHA384, or SHA256. Replace digital signature uses with RSA with a key length greater than or equal to 2048-bits, or ECDSA with a key length greater than or equal to 256 bits.

Category: Security

Severity: Warning

Help: [http://aka.ms/CA5351](http://aka.ms/CA5351)