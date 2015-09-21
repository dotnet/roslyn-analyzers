### CA1008: Enums should have zero value. ###

An enum should generally have a zero value. If the enum is not decorated with the Flags attribute, it should have a member with a value of zero that represents the empty state. Optionally, this value is named 'None'. For a Flags-attributed enum, a zero-valued member is optional and, if it exists, should always be named 'None'. This value should indicate that no values have been set in the enum. Using a zero-valued member for other purposes is contrary to the use of the Flags attribute in that the bitwise AND and OR operators are useless with the member.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182149.aspx](http://msdn.microsoft.com/library/ms182149.aspx)

### CA1012: Abstract classes should not have public constructors ###

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182126.aspx](http://msdn.microsoft.com/library/ms182126.aspx)

### CA1024: Use properties where appropriate. ###

Properties should be used instead of Get/Set methods in most situations. Methods are preferable to properties in the following situations: the operation is a conversion, is expensive or has an observable side-effect; the order of execution is important; calling the member twice in succession creates different results; a member is static but returns a mutable value; or the member returns an array.

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182181.aspx](http://msdn.microsoft.com/library/ms182181.aspx)

### CA1033: Interface methods should be callable by child types ###

An unsealed externally visible type provides an explicit method implementation of a public interface and does not provide an alternative externally visible method that has the same name.
Consider a base type that explicitly implements a public interface method. A type that derives from the base type can access the inherited interface method only through a reference to the current instance that is cast to the interface. If the derived type re-implements (explicitly) the inherited interface method, the base implementation can no longer be accessed. The call through the current instance reference will invoke the derived implementation; this causes recursion and an eventual stack overflow.

Category: Design

Severity: Warning

Help: [https://msdn.microsoft.com/library/ms182153.aspx](https://msdn.microsoft.com/library/ms182153.aspx)

### CA1052: Static holder types should be Static or NotInheritable ###

Category: Design

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182168.aspx](http://msdn.microsoft.com/library/ms182168.aspx)

### CA1708: Identifier names should differ by more than case ###

Do not use names that require case sensitivity for uniqueness. Components must be fully usable from both case-sensitive and case-insensitive languages. Since case-insensitive languages cannot distinguish between two names within the same context that differ only by case, components must avoid this situation.

Category: Naming

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182242.aspx](http://msdn.microsoft.com/library/ms182242.aspx)

### CA1715: Interface names should be prefixed with 'I' ###

Category: Naming

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182243.aspx](http://msdn.microsoft.com/library/ms182243.aspx)

### CA1821: Remove empty finalizers ###

Finalizers should be avoided where possible, to avoid the additional performance overhead involved in tracking object lifetime.

Category: Performance

Severity: Warning

Help: [http://msdn.microsoft.com/library/bb264476.aspx](http://msdn.microsoft.com/library/bb264476.aspx)

### CA2200: Rethrow to preserve stack details. ###

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182363.aspx](http://msdn.microsoft.com/library/ms182363.aspx)

### CA2214: Do not call overridable methods in constructors ###

Virtual methods defined on the class should not be called from constructors. If a derived class has overridden the method, the derived class version will be called (before the derived class constructor is called).

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182331.aspx](http://msdn.microsoft.com/library/ms182331.aspx)

### CA2229: Implement Serialization constructor ###

The constructor signature accepts the same arguments as ISerializable.GetObjectData, namely, a SerializationInfo instance and a StreamingContext instance. The constructor should be protected for non-sealed classes and private for sealed classes. Failure to implement a serialization constructor will cause deserialization to fail, and throw a SerializationException.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182343.aspx](http://msdn.microsoft.com/library/ms182343.aspx)

### CA2235: Mark all non-serializable fields. ###

All fields that cannot be serialized directly should have the NonSerializedAttribute. Types that have the SerializableAttribute should not have fields of types that do not have the SerializableAttribute unless the fields are marked with the NonSerializedAttribute.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182349.aspx](http://msdn.microsoft.com/library/ms182349.aspx)

### CA2237: Mark ISerializable types with SerializableAttribute. ###

The System.Runtime.Serialization.ISerializable interface allows the type to customize its serialization, while the Serializable attribute enables the runtime to recognize the type as being serializable.

Category: Usage

Severity: Warning

Help: [http://msdn.microsoft.com/library/ms182350.aspx](http://msdn.microsoft.com/library/ms182350.aspx)