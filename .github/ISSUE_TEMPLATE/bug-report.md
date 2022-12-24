---
name: Bug report
about: Report a bug, false-positive or false-negative for a CAxxxx rule. For IDExxxx, please open an issue in dotnet/roslyn repository.
title: ''
labels: ''
assignees: ''

---

### Analyzer

**Diagnostic ID**: [CA2013](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2013): `Do not use ReferenceEquals with value types`

### Analyzer source

**SDK**: [Built-in CA analyzers in .NET 5 SDK or later](https://learn.microsoft.com/dotnet/fundamentals/productivity/code-analysis)

**Version**: [SDK 5.0.100](https://dotnet.microsoft.com/download/dotnet/5.0)

<!--
Note: Updates to NuGet package are more frequent than SDK, which means the NuGet package has fixes that haven't yet made it into the SDK.
      Please check whether you can reproduce the bug you encountered in the latest NuGet package.
-->

_OR_

**NuGet Package**: [Microsoft.CodeAnalysis.NetAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers)

**Version**: 5.0.3 (Latest)

<!--
NOTE: `Microsoft.CodeAnalysis.FxCopAnalyzers` package has been deprecated in favor of 'Microsoft.CodeAnalysis.NetAnalyzers', that ships with the .NET SDK.
      Please refer to https://learn.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers to migrate to .NET analyzers.
-->

### Describe the bug

<!-- A clear and concise description of what the bug is. -->

### Steps To Reproduce

<!--
Provide the steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error
-->

### Expected behavior

### Actual behavior

## Additional context

<!-- Add any other context about the problem here. -->
