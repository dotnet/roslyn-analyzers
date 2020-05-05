# Roslyn Analyzers

|          |Windows Debug|Windows Release|
|:--------:|:-----------:|:-------------:|
|**master**| [![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/roslyn-analyzers/roslyn-analyzers-CI?branchName=master&jobName=Windows&configuration=Windows%20Debug&label=build)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=457&branchName=master) [![codecov](https://codecov.io/gh/dotnet/roslyn-analyzers/branch/master/graph/badge.svg)](https://codecov.io/gh/dotnet/roslyn-analyzers) | [![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/roslyn-analyzers/roslyn-analyzers-CI?branchName=master&jobName=Windows&configuration=Windows%20Release&label=build)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=457&branchName=master) |

# What is Roslyn?

Roslyn is the compiler platform for .NET. It consists of the compiler itself and a powerful set of APIs to interact with the compiler. The Roslyn platform is hosted at [github.com/dotnet/roslyn](https://github.com/dotnet/roslyn). 

# What are Roslyn Analyzers?

Roslyn analyzers analyze your code for style, quality and maintainability, design and other issues. The documentation for Roslyn Analyzers can be found at [docs.microsoft.com/visualstudio/code-quality/roslyn-analyzers-overview](https://docs.microsoft.com/visualstudio/code-quality/roslyn-analyzers-overview).

Microsoft created a set of analyzers called [Microsoft.CodeAnalysis.FxCopAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers) that contains the most important "FxCop" rules from static code analysis, converted to Roslyn analyzers. These analyzers check your code for security, performance, and design issues, among others. The documentation for FxCop analyzers in Visual Studio can be found at [docs.microsoft.com/visualstudio/code-quality/install-fxcop-analyzers](https://docs.microsoft.com/visualstudio/code-quality/install-fxcop-analyzers).


# Microsoft.CodeAnalysis.FxCopAnalyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.FxCopAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers)

This is the **primary analyzer package** for this repo that contains all **the ported FxCop code analysis rules (CAxxxx)**. The documentation for FxCop Analyzers and FAQs about migrating from legacy post-build static analysis also known as "FxCop" to FxCop Analyzers can be found at [docs.microsoft.com/visualstudio/code-quality/install-fxcop-analyzers](https://docs.microsoft.com/visualstudio/code-quality/install-fxcop-analyzers).

The documentation for all the ported and unported FxCop rules can be found at [docs.microsoft.com/en-us/visualstudio/code-quality/fxcop-rule-port-status](https://docs.microsoft.com/visualstudio/code-quality/fxcop-rule-port-status).

This analyzer package contains all the ported FxCop rules that are applicable for both _.NetCore/.NetStandard_ and _Desktop .NetFramework_ projects. You **do not need to install any separate analyzer package from this repo to get target-framework specific FxCop rules**. [More info about rules in this package](src/Microsoft.CodeAnalysis.FxCopAnalyzers/Microsoft.CodeAnalysis.FxCopAnalyzers.md)

## The following are subpackages or NuGet dependencies that are automatically installed when you install the Microsoft.CodeAnalysis.FxCopAnalyzers package:

### Microsoft.CodeQuality.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeQuality.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeQuality.Analyzers)

This package contains common code quality improvement rules that are not specific to usage of any particular API. For example, [CA1801](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1801-review-unused-parameters) (ReviewUnusedParameters) flags parameters that are unused and is part of this package. [More info about rules in this package](src/Microsoft.CodeQuality.Analyzers/Microsoft.CodeQuality.Analyzers.md)

### Microsoft.NetCore.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetCore.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetCore.Analyzers)

This package contains rules for correct usage of APIs that are present in _.NetCore/.NetStandard_ framework libraries. For example, [CA1309](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison) (UseOrdinalStringComparison) flags usages of string compare APIs that don't specify a `StringComparison` argument.

**NOTE:** This analyzer package is applicable for both _.NetCore/.NetStandard_ and _Desktop .NetFramework_ projects. If the API whose usage is being checked exists only in _.NetCore/.NetStandard_ libraries, then the analyzer will bail out silently for _Desktop .NetFramework_ projects. Otherwise, if the API exists in both _.NetCore/.NetStandard_ and _Desktop .NetFramework_ libraries, the analyzer will run correctly for both  _.NetCore/.NetStandard_ and _Desktop .NetFramework_ projects. [More info about rules in this package](src/Microsoft.NetCore.Analyzers/Microsoft.NetCore.Analyzers.md)

### Microsoft.NetFramework.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetFramework.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetFramework.Analyzers)

This package contains rules for correct usage of APIs that are present only in _Desktop .NetFramework_ libraries.

**NOTE:** The analyzers in this package will silently bail out if installed on a _.NetCore/.NetStandard_ project that do not have the underlying API whose usage is being checked. If future versions of _.NetCore/.NetStandard_ libraries include these APIs, the analyzers will automatically light up on _.NetCore/.NetStandard_ projects that target these libraries. [More info about rules in this package](src/Microsoft.NetFramework.Analyzers/Microsoft.NetFramework.Analyzers.md)


# Other Analyzer Packages

### Microsoft.CodeAnalysis.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers)

This package contains rules for correct usage of APIs from the [Microsoft.CodeAnalysis](https://www.nuget.org/packages/Microsoft.CodeAnalysis) NuGet package, i.e. .NET Compiler Platform ("Roslyn") APIs. These are primarily aimed towards helping authors of diagnostic analyzers and code fix providers to invoke the Microsoft.CodeAnalysis APIs in a recommended manner. [More info about rules in this package](src/Microsoft.CodeAnalysis.Analyzers/Microsoft.CodeAnalysis.Analyzers.md)


### Roslyn.Diagnostics.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Roslyn.Diagnostics.Analyzers.svg)](https://www.nuget.org/packages/Roslyn.Diagnostics.Analyzers)

This package contains rules that are very specific to the .NET Compiler Platform ("Roslyn") project, i.e. https://github.com/dotnet/roslyn repo. This analyzer package is _not intended for general consumption_ outside the Roslyn repo. [More info about rules in this package](src/Roslyn.Diagnostics.Analyzers/Roslyn.Diagnostics.Analyzers.md)


### Microsoft.CodeAnalysis.BannedApiAnalyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.BannedApiAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BannedApiAnalyzers)

This package contains customizable rules for identifying references to banned APIs. [More info about rules in this package](src/Microsoft.CodeAnalysis.BannedApiAnalyzers/Microsoft.CodeAnalysis.BannedApiAnalyzers.md)

### Microsoft.CodeAnalysis.PublicApiAnalyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.PublicApiAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.PublicApiAnalyzers)

This package contains rules to help library authors monitoring change to their public APIs. [More info about rules in this package](src/PublicApiAnalyzers/Microsoft.CodeAnalysis.PublicApiAnalyzers.md)

For instructions on using this analyzer, see [Instructions](src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md).


### MetaCompilation (prototype)
Created by summer 2015 interns [Zoë Petard](https://github.com/zoepetard), [Jessica Petty](https://github.com/jepetty), and [Daniel King](https://github.com/daking2014)

The MetaCompilation Analyzer is an analyzer that functions as a tutorial to teach users how to write an analyzer. It uses diagnostics and code fixes to guide the user through the various steps required to create a simple analyzer. It is designed for novice analyzer developers who have some previous programming experience.

For instructions on using this tutorial, see [Instructions](https://github.com/dotnet/roslyn-analyzers/blob/master/src/MetaCompilation.Analyzers/Core/ReadMe.md#instructions).

# Getting Started

1. Install Visual Studio 2019 or later, with at least the following workloads:
   1. .NET desktop development
   2. .NET Core cross-platform development
   3. Visual Studio extension development
2. Clone this repository
3. Open the VS developer command prompt or PowerShell
4. Go to the directory of the Roslyn Analyzer Repo
5. Run the restore and build command: `build.cmd`(in the command prompt) or `.\build.cmd`(in PowerShell).
6. Execute tests: `test.cmd`(in the command prompt) or `.\test.cmd`(in PowerShell).

# Submitting Pull Requests

Prior to submitting a pull request, ensure the build and all tests pass using using steps 4 and 5 above.

# Guidelines for contributing a new Code Analysis (CA) rule to the repo

See [GuidelinesForNewRules.md](.//GuidelinesForNewRules.md) for contributing a new Code Analysis rule to the repo.

# Versioning Scheme for Analyzer Packages

See [VERSIONING.md](.//VERSIONING.md) for the versioning scheme for all analyzer packages built out of this repo.

# Recommended version of Analyzer Packages

Recommended Visual Studio Version: **Visual Studio 2017 16.0 RTW or later**

Recommended Analyzer Package Version: **Version 3.0.0**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/3.0.0

The documentation for FxCopAnalyzers package versions can be found at [docs.microsoft.com/visualstudio/code-quality/install-fxcop-analyzers](https://docs.microsoft.com/visualstudio/code-quality/install-fxcop-analyzers)

You can also install a custom **Microsoft Code Analysis VSIX** containing these analyzers as a Visual Studio extension for all your managed projects.
1. For Visual Studio 2017 15.5 or later: https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.MicrosoftCodeAnalysis2017
2. For Visual Studio 2019 16.0 or later: https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.MicrosoftCodeAnalysis2019
