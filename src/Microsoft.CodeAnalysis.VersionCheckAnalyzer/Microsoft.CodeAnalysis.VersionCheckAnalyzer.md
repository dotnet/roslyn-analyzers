
Sr. No. | Rule ID | Title | Category | Enabled | CodeFix | Description |
--------|---------|-------|----------|---------|---------|--------------------------------------------------------------------------------------------------------------|
1 | CA9999 | Analyzer version mismatch | Reliability | True | False | Analyzers in this package are preview version and are tied to a specific API version of Microsoft.CodeAnalysis. You have a mismatch between the analyzer and Microsoft.CodeAnalysis version and should switch your analyzer NuGet package/VSIX to a matching version of the Microsoft.CodeAnalysis. |
