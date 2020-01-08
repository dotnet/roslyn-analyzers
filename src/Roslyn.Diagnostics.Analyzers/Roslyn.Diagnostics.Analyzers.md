
Sr. No. | Rule ID | Title | Category | Enabled | CodeFix | Description |
--------|---------|-------|----------|---------|---------|--------------------------------------------------------------------------------------------------------------|
1 | RS0001 | Use SpecializedCollections.EmptyEnumerable() | RoslynDiagnosticsPerformance | True | False | Use SpecializedCollections.EmptyEnumerable() |
2 | RS0002 | Use SpecializedCollections.SingletonEnumerable() | RoslynDiagnosticsPerformance | True | False | Use SpecializedCollections.SingletonEnumerable() |
3 | RS0004 | Invoke the correct property to ensure correct use site diagnostics. | Usage | False | False | Invoke the correct property to ensure correct use site diagnostics. |
4 | RS0005 | Do not use generic CodeAction.Create to create CodeAction | RoslynDiagnosticsPerformance | True | False | Do not use generic CodeAction.Create to create CodeAction |
5 | RS0006 | Do not mix attributes from different versions of MEF | RoslynDiagnosticsReliability | True | False | Do not mix attributes from different versions of MEF |
6 | RS0013 | Do not invoke Diagnostic.Descriptor | RoslynDiagnosticsPerformance | False | False | Accessing the Descriptor property of Diagnostic in compiler layer leads to unnecessary string allocations for fields of the descriptor that are not utilized in command line compilation. Hence, you should avoid accessing the Descriptor of the compiler diagnostics here. Instead you should directly access these properties off the Diagnostic type. |
7 | RS0019 | SymbolDeclaredEvent must be generated for source symbols | RoslynDiagnosticsReliability | False | False | Compilation event queue is required to generate symbol declared events for all declared source symbols. Hence, every source symbol type or one of its base types must generate a symbol declared event. |
8 | RS0023 | Parts exported with MEFv2 must be marked as Shared | RoslynDiagnosticsReliability | True | False | Part exported with MEFv2 must be marked with the Shared attribute. |
9 | RS0032 | Test exports should not be discoverable | RoslynDiagnosticsReliability | False | True | Test exports should not be discoverable |
10 | RS0033 | Importing constructor should be [Obsolete] | RoslynDiagnosticsReliability | True | True | Importing constructor should be [Obsolete] |
11 | RS0034 | Exported parts should have [ImportingConstructor] | RoslynDiagnosticsReliability | True | True | Exported parts should have [ImportingConstructor] |
