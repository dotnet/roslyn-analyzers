### RS0001: Use SpecializedCollections.EmptyEnumerable<T>() ###

Category: Performance

Severity: Warning

### RS0002: Use SpecializedCollections.SingletonEnumerable<T>() ###

Category: Performance

Severity: Warning

### RS0003: Do not directly await a Task ###

Category: Reliability

Severity: Warning

### RS0004: Invoke the correct property to ensure correct use site diagnostics. ###

Category: Usage

Severity: Error

### RS0005: Do not use generic CodeAction.Create to create CodeAction ###

Category: Performance

Severity: Warning

### RS0006: Do not mix attributes from different versions of MEF ###

Category: Reliability

Severity: Error

### RS0007: Avoid zero-length array allocations. ###

Category: Performance

Severity: Warning

### RS0008: Implement IEquatable<T> when overriding Object.Equals ###

Category: Performance

Severity: Warning

### RS0009: Override Object.Equals(object) when implementing IEquatable<T>  ###

Category: Reliability

Severity: Warning

### RS0010: Avoid using cref tags with a prefix ###

Use of cref tags with prefixes should be avoided, since it prevents the compiler from verifying references and the IDE from updating references during refactorings. It is permissible to suppress this error at a single documentation site if the cref must use a prefix because the type being mentioned is not findable by the compiler. For example, if a cref is mentioning a special attribute in the full framework but you're in a file that compiles against the portable framework, or if you want to reference a type at higher layer of Roslyn, you should suppress the error. You should not suppress the error just because you want to take a shortcut and avoid using the full syntax.

Category: Documentation

Severity: Warning

### RS0011: CancellationToken parameters must come last ###

Category: ApiDesign

Severity: Warning

### RS0012: Do not call ToImmutableArray on an ImmutableArray<T> value. ###

Category: Reliability

Severity: Warning

### RS0013: Do not invoke Diagnostic.Descriptor ###

Accessing the Descriptor property of Diagnostic in compiler layer leads to unnecessary string allocations for fields of the descriptor that are not utilized in command line compilation. Hence, you should avoid accessing the Descriptor of the compiler diagnostics here. Instead you should directly access these properties off the Diagnostic type.

Category: Performance

Severity: Warning

### RS0014: Do not use Enumerable methods on indexable collections.  Instead use the collection directly. ###

This collection is directly indexable.  Going through LINQ here causes unnecessary allocations and CPU work.

Category: Performance

Severity: Warning

### RS0015: Always consume the value returned by methods marked with PreserveSigAttribute ###

PreserveSigAttribute indicates that a method will return an HRESULT, rather than throwing an exception.  Therefore, it is important to consume the HRESULT returned by the method, so that errors can be detected.  Generally, this is done by calling Marshal.ThrowExceptionForHR.

Category: Reliability

Severity: Warning

### RS0016: Add public types and members to the declared API ###

All public types and members should be declared in PublicAPI.txt. This draws attention to API changes in the code reviews and source control history, and helps prevent breaking changes.

Category: ApiDesign

Severity: Error

### RS0017: Remove deleted types and members from the declared API ###

When removing a public type or member the corresponding entry in PublicAPI.txt should also be removed. This draws attention to API changes in the code reviews and source control history, and helps prevent breaking changes.

Category: ApiDesign

Severity: Error

### RS0018: Do not create tasks without passing a TaskScheduler ###

Do not create tasks unless you are using one of the overloads that takes a TaskScheduler. The default is to schedule on TaskScheduler.Current, which would lead to deadlocks. Either use TaskScheduler.Default to schedule on the thread pool, or explicitly pass TaskScheduler.Current to make your intentions clear.

Category: Reliability

Severity: Warning

### RS0019: SymbolDeclaredEvent must be generated for source symbols ###

Compilation event queue is required to generate symbol declared events for all declared source symbols. Hence, every source symbol type or one of it's base types must generate a symbol declared event.

Category: Reliability

Severity: Error

### RS0020: unused code ###

Category: Maintainability

Severity: Warning

### RS0021:  ###

Category: 

Severity: Hidden

### RS0022: Constructor make noninheritable base class inheritable ###

When a base class is noninheritable because its constructor is internal, a derived class should not make it inheritable by having a public or protected constructor.

Category: ApiDesign

Severity: Error

### RS0023: Parts exported with MEFv2 must be marked as Shared. ###

Category: Reliability

Severity: Error

### RS0024: The contents of the public API files are invalid ###

Category: ApiDesign

Severity: Error