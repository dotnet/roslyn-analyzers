# How to use Microsoft.CodeAnalysis.PublicApiAnalyzers

The following files have to be added to any project referencing this package to enable analysis:

- `PublicAPI.Shipped.txt`
- `PublicAPI.Unshipped.txt`

This can be done by:

- In Visual Studio, right-click the project in Solution Explorer, choose "Add -> New Item...", and then select "Text File" in the "Add New Item" dialog. Then right-click each file, select "Properties", and choose "C# analyzer additional file" for "Build Action" in the "Properties" window.
- Or, create these two files at the location you desire, then add the following text to your project/target file (replace file path with its actual location):

```xml
  <ItemGroup>
    <AdditionalFiles Include="PublicAPI.Shipped.txt" />
    <AdditionalFiles Include="PublicAPI.Unshipped.txt" />
  </ItemGroup>
```

## Nullable reference type support

To enable support for [nullable reference types](https://docs.microsoft.com/dotnet/csharp/nullable-references) add the following at the top of each `PublicAPI.*.txt` file:

```xml
#nullable enable
```

At that point, reference types in annotated code will need to be annotated with either a `?` (nullable) or a `!` (non-nullable). For instance, `C.AnnotatedMethod(string! nonNullableParameter, string? nullableParameter, int valueTypeParameter) -> void`.

Any public API that haven't been annotated (i.e. uses an oblivious reference type) will be tracked with a `~` marker. The marker lets you track how many public APIs still lack annotations. For instance, `~C.ObliviousMethod() -> string`.

We recommend to enable [RS0041 warning](https://github.com/dotnet/roslyn-analyzers/blob/master/src/PublicApiAnalyzers/Microsoft.CodeAnalysis.PublicApiAnalyzers.md) if you start with a fresh project or your project has reached 100% annotation on your public API to ensure that all public APIs remain annotated. 
If you are in the process of annotating an existing project, we recommended to disable this warning until you complete the annotation. The rule can be disabled via `.editorconfig` with `dotnet_diagnostic.RS0041.severity = none`.

## Conditional API Differences

Sometimes APIs vary by compilation symbol such as target framework.

For example when using the [`#if` preprocessor directive](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives/preprocessor-if):

```c#
        public void Foo(string s)
        {}

#if NETCOREAPP3_0
        public void Foo(ReadOnlySpan<char> s)
        {}
#else
```

To correctly model the API differences between target frameworks (or any other property), use multiple instances of the `PublicAPI.*.txt` files.

If you have multiple target frameworks and APIs differ between them, use the following in your project file:

```xml
  <ItemGroup>
    <AdditionalFiles Include="PublicAPI/$(TargetFramework)/PublicAPI.Shipped.txt" />
    <AdditionalFiles Include="PublicAPI/$(TargetFramework)/PublicAPI.Unshipped.txt" />
  </ItemGroup>
```
