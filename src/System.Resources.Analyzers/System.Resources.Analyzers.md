### CA1824: Mark assemblies with NeutralResourcesLanguageAttribute ###

The NeutralResourcesLanguage attribute informs the ResourceManager of the language that was used to display the resources of a neutral culture for an assembly. This improves lookup performance for the first resource that you load and can reduce your working set.

Category: Performance

Severity: Warning

Help: [https://msdn.microsoft.com/en-us/library/bb385967.aspx](https://msdn.microsoft.com/en-us/library/bb385967.aspx)