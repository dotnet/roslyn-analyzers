# Roslyn Analyzers

|          |Windows Debug|Windows Release|Ubuntu Debug|Ubuntu Release|
|:--------:|:-----------:|:-------------:|:----------:|:------------:|
|**main**| [![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/roslyn-analyzers/roslyn-analyzers-CI?branchName=main&jobName=Windows&configuration=Windows%20Debug&label=build)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=457&branchName=main) [![codecov](https://codecov.io/gh/dotnet/roslyn-analyzers/branch/main/graph/badge.svg)](https://codecov.io/gh/dotnet/roslyn-analyzers) | [![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/roslyn-analyzers/roslyn-analyzers-CI?branchName=main&jobName=Windows&configuration=Windows%20Release&label=build)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=457&branchName=main) | [![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/roslyn-analyzers/roslyn-analyzers-CI?branchName=main&jobName=Ubuntu&configuration=Ubuntu%20Debug&label=build)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=457&branchName=main) | [![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/roslyn-analyzers/roslyn-analyzers-CI?branchName=main&jobName=Ubuntu&configuration=Ubuntu%20Release&label=build)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=457&branchName=main) |

## What is Roslyn?

Roslyn is the compiler platform for .NET. It consists of the compiler itself and a powerful set of APIs to interact with the compiler. The Roslyn platform is hosted at [github.com/dotnet/roslyn](https://github.com/dotnet/roslyn).

## What are Roslyn Analyzers?

Roslyn analyzers analyze your code for style, quality and maintainability, design and other issues. The documentation for Roslyn Analyzers can be found at [learn.microsoft.com/dotnet/fundamentals/code-analysis/overview](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview).

Microsoft created a set of analyzers called [Microsoft.CodeAnalysis.NetAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers) that contains the most important "FxCop" rules from static code analysis, converted to Roslyn analyzers, in addition to more analyzers. These analyzers check your code for security, performance, and design issues, among others. The documentation for .NET analyzers can be found [here](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview#code-quality-analysis).

## Main analyzers

Recently the set of analyzer packages produced by this repository have been consolidated. The following table summarizes this information:

| NuGet Package Name | Version | Summary |
|----------|:-------:|---------|
| Microsoft.CodeAnalysis.NetAnalyzers       | [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.NetAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers) | ✔️ Primary analyzer package for this repo. Included default for .NET 5+. For earlier targets [read more](#microsoftcodeanalysisnetanalyzers). |
| Microsoft.CodeAnalysis.FxCopAnalyzers     | [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.FxCopAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers) | ⛔ Use `Microsoft.CodeAnalysis.NetAnalyzers` instead. [Read more](#microsoftcodeanalysisfxcopanalyzers). |
| Microsoft.CodeQuality.Analyzers           | [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeQuality.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeQuality.Analyzers) | ⛔ Use `Microsoft.CodeAnalysis.NetAnalyzers` instead. [Read more](#microsoftcodequalityanalyzers). |
| Microsoft.NetCore.Analyzers               | [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetCore.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetCore.Analyzers) | ⛔ Use `Microsoft.CodeAnalysis.NetAnalyzers` instead. [Read more](#microsoftnetcoreanalyzers). |
| Microsoft.NetFramework.Analyzers          | [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetFramework.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetFramework.Analyzers) | ⛔ Use `Microsoft.CodeAnalysis.NetAnalyzers` instead. [Read more](#microsoftnetcoreanalyzers). |

### Microsoft.CodeAnalysis.NetAnalyzers

*Latest stable version:* <sub>[![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.NetAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers)</sub>

*Latest pre-release version (.NET8 analyzers):* [here](https://dev.azure.com/dnceng/public/_artifacts/feed/dotnet8/NuGet/Microsoft.CodeAnalysis.NetAnalyzers/versions)

This is the **primary analyzer package** for this repo that contains all **the .NET code analysis rules (CAxxxx)** that are built into the .NET SDK starting .NET5 release. The documentation for CA rules can be found at [learn.microsoft.com/visualstudio/code-quality/code-analysis-for-managed-code-warnings](https://learn.microsoft.com/visualstudio/code-quality/code-analysis-for-managed-code-warnings).

You do not need to manually install this NuGet package to your project if you are using .NET5 SDK or later. These analyzers are enabled by default for projects targeting .NET5 or later. For projects targeting earlier .NET frameworks, you can enable them in your MSBuild project file by setting one of the following properties:

1. *EnableNETAnalyzers*

   ```xml
   <PropertyGroup>
     <EnableNETAnalyzers>true</EnableNETAnalyzers>
   </PropertyGroup>
   ```

2. *AnalysisLevel*

   ```xml
   <PropertyGroup>
     <AnalysisLevel>latest</AnalysisLevel>
   </PropertyGroup>
   ```

### Microsoft.CodeAnalysis.FxCopAnalyzers

**NOTE:** Starting version `3.3.2`, `Microsoft.CodeAnalysis.FxCopAnalyzers` has been **deprecated** in favor of `Microsoft.CodeAnalysis.NetAnalyzers`. Documentation to migrate from FxCopAnalyzers to NetAnalyzers is available [here](https://learn.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers).

*Latest stable version:* <sub>[![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.FxCopAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers)</sub>

This is a migration analyzer package for existing binary FxCop users. It contains all **the ported FxCop code analysis rules (CAxxxx)**. It's recommended to use Microsoft.CodeAnalysis.NetAnalyzers instead. The documentation for that can be found at [learn.microsoft.com/visualstudio/code-quality/install-net-analyzers](https://learn.microsoft.com/visualstudio/code-quality/install-net-analyzers).

The documentation for all the ported and unported FxCop rules can be found at [learn.microsoft.com/visualstudio/code-quality/fxcop-rule-port-status](https://learn.microsoft.com/visualstudio/code-quality/fxcop-rule-port-status).

This analyzer package contains all the ported FxCop rules that are applicable for both *.NetCore/.NetStandard* and *Desktop .NetFramework* projects. You **do not need to install any separate analyzer package from this repo to get target-framework specific FxCop rules**.

#### The following are subpackages or NuGet dependencies that are automatically installed when you install the Microsoft.CodeAnalysis.FxCopAnalyzers package:

**NOTE:** Starting version `3.3.2`, `Microsoft.CodeQuality.Analyzers`, `Microsoft.NetCore.Analyzers` and `Microsoft.NetFramework.Analyzers` have also been **deprecated** in favor of `Microsoft.CodeAnalysis.NetAnalyzers`. Documentation to migrate to NetAnalyzers is available [here](https://learn.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers).

#### Microsoft.CodeQuality.Analyzers

*Latest stable version:* <sub>[![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeQuality.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeQuality.Analyzers)</sub>

This package contains common code quality improvement rules that are not specific to usage of any particular API. For example, [CA1801](https://learn.microsoft.com/visualstudio/code-quality/ca1801-review-unused-parameters) (ReviewUnusedParameters) flags parameters that are unused and is part of this package.

#### Microsoft.NetCore.Analyzers

*Latest stable version:* <sub>[![NuGet](https://img.shields.io/nuget/v/Microsoft.NetCore.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetCore.Analyzers)</sub>

This package contains rules for correct usage of APIs that are present in *.NetCore/.NetStandard* framework libraries. For example, [CA1309](https://learn.microsoft.com/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison) (UseOrdinalStringComparison) flags usages of string compare APIs that don't specify a `StringComparison` argument. [Getting started with NetCore Analyzers](docs/NetCore_GettingStarted.md)

**NOTE:** This analyzer package is applicable for both *.NetCore/.NetStandard* and *Desktop .NetFramework* projects. If the API whose usage is being checked exists only in *.NetCore/.NetStandard* libraries, then the analyzer will bail out silently for *Desktop .NetFramework* projects. Otherwise, if the API exists in both *.NetCore/.NetStandard* and *Desktop .NetFramework* libraries, the analyzer will run correctly for both  *.NetCore/.NetStandard* and *Desktop .NetFramework* projects.

#### Microsoft.NetFramework.Analyzers

*Latest stable version:* <sub>[![NuGet](https://img.shields.io/nuget/v/Microsoft.NetFramework.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetFramework.Analyzers)</sub>

This package contains rules for correct usage of APIs that are present only in *Desktop .NetFramework* libraries.

**NOTE:** The analyzers in this package will silently bail out if installed on a *.NetCore/.NetStandard* project that do not have the underlying API whose usage is being checked. If future versions of *.NetCore/.NetStandard* libraries include these APIs, the analyzers will automatically light up on *.NetCore/.NetStandard* projects that target these libraries.

## Other Analyzer Packages

The following analyzers have moved into the [Roslyn](https://github.com/dotnet/roslyn) repo:
- Microsoft.CodeAnalysis.Analyzers
- Roslyn.Diagnostics.Analyzers
- Microsoft.CodeAnalysis.BannedApiAnalyzers
- Microsoft.CodeAnalysis.PublicApiAnalyzers

## Getting Started

1. Install Visual Studio 2022 or later, with at least the following workloads:
   1. .NET desktop development
   2. .NET Core cross-platform development
   3. Visual Studio extension development
2. Clone this repository
3. Install .NET SDK version specified in `.\global.json` with `"dotnet":`
   - Currently, the repository requires the .NET 8 SDK, for which the installer isn't yet published in [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet-core).
   - To install the .NET 8 SDK, see [dotnet/installer](https://github.com/dotnet/installer#table).
4. Open a command prompt and go to the directory of the Roslyn Analyzer Repo
5. Run the restore and build command: `build.cmd`(in the command prompt) or `.\build.cmd`(in PowerShell).
6. Execute tests: `test.cmd` (in the command prompt) or `.\test.cmd` (in PowerShell).

## Submitting Pull Requests

Prior to submitting a pull request, ensure the build and all tests pass using using steps 4 and 5 above.

## Guidelines for contributing a new Code Analysis (CA) rule to the repo

See [GuidelinesForNewRules.md](.//GuidelinesForNewRules.md) for contributing a new Code Analysis rule to the repo.

## Versioning Scheme for Analyzer Packages

See [VERSIONING.md](.//VERSIONING.md) for the versioning scheme for all analyzer packages built out of this repo.

## Recommended version of Analyzer Packages

Recommended Analyzer Package Version: <sub>[![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.NetAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers)</sub>

Required Visual Studio Version: **Visual Studio 2019 16.9 RTW or later**

Required .NET SDK Version: **.NET 5.0 SDK or later**

The documentation for .NET SDK Analyzers can be found [here](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview#code-quality-analysis)
