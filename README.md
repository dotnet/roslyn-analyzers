.NET Compiler Platform ("Roslyn") Analyzers, including ported [FxCop analyzers](https://docs.microsoft.com/en-us/visualstudio/code-quality/fxcop-analyzers)
===========================================

[//]: # (Begin current test results)

|          |Windows Debug|Windows Release|
|:--------:|:-----------:|:-------------:|
|**master**|[![Build Status](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Debug/badge/icon)](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Debug/) [![codecov](https://codecov.io/gh/dotnet/roslyn-analyzers/branch/master/graph/badge.svg)](https://codecov.io/gh/dotnet/roslyn-analyzers)|[![Build Status](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Release/badge/icon)](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Release/)|

[//]: # (End current test results)

This repository contains Roslyn [diagnostic analyzers and code fix providers](https://docs.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview) for general _code quality_ improvement for C# and Visual Basic projects that target either _.NET Core, .NET Standard,_ or _.NET Framework for desktop apps_.

Microsoft.CodeAnalysis.FxCopAnalyzers
--------------------------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.FxCopAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers)

This is the **primary analyzer package** for this repo that contains all **the ported FxCop code analysis rules (CAxxxx)**. Refer to [FxCop analyzers](https://docs.microsoft.com/en-us/visualstudio/code-quality/fxcop-analyzers) for documentation and FAQs about migrating from legacy post-build static analysis ("Analyze" &rarr; "Run Code Analysis" command in Visual Studio), also known as "FxCop", to FxCop analyzers.

This analyzer package contains all the ported FxCop rules that are applicable for both _.NetCore/.NetStandard_ and _Desktop .NetFramework_ projects. You **do not need to install any separate analyzer package from this repo to get target-framework specific FxCop rules**.

This analyzer package contains the following subpackages or NuGet dependencies that are automatically installed when you install the Microsoft.CodeAnalysis.FxCopAnalyzers package:

### Microsoft.CodeQuality.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeQuality.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeQuality.Analyzers)

This package contains common code quality improvement rules that are not specific to usage of any particular API. For example, [CA1801](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1801-review-unused-parameters) (ReviewUnusedParameters) flags parameters that are unused and is part of this package. 

[More info](src/Microsoft.CodeQuality.Analyzers/Microsoft.CodeQuality.Analyzers.md)

### Microsoft.NetCore.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetCore.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetCore.Analyzers)

This package contains rules for correct usage of APIs that are present in _.NetCore/.NetStandard_ framework libraries. For example, [CA1309](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1309-use-ordinal-stringcomparison) (UseOrdinalStringComparison) flags usages of string compare APIs that don't specify a `StringComparison` argument.

**NOTE:** This analyzer package is applicable for both _.NetCore/.NetStandard_ and _Desktop .NetFramework_ projects. If the API whose usage is being checked exists only in _.NetCore/.NetStandard_ libraries, then the analyzer will bail out silently for _Desktop .NetFramework_ projects. Otherwise, if the API exists in both _.NetCore/.NetStandard_ and _Desktop .NetFramework_ libraries, the analyzer will run correctly for both  _.NetCore/.NetStandard_ and _Desktop .NetFramework_ projects.

[More info](src/Microsoft.NetCore.Analyzers/Microsoft.NetCore.Analyzers.md)

### Microsoft.NetFramework.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetFramework.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetFramework.Analyzers)

This package contains rules for correct usage of APIs that are present only in _Desktop .NetFramework_ libraries.

**NOTE:** The analyzers in this package will silently bail out if installed on a _.NetCore/.NetStandard_ project that do not have the underlying API whose usage is being checked. If future versions of _.NetCore/.NetStandard_ libraries include these APIs, the analyzers will automatically light up on _.NetCore/.NetStandard_ projects that target these libraries.

[More info](src/Microsoft.NetFramework.Analyzers/Microsoft.NetFramework.Analyzers.md)


Other analyzer packages
--------------------------------

### Microsoft.CodeAnalysis.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers)

This package contains rules for correct usage of APIs from the [Microsoft.CodeAnalysis](https://www.nuget.org/packages/Microsoft.CodeAnalysis) NuGet package, i.e. .NET Compiler Platform ("Roslyn") APIs. These are primarily aimed towards helping authors of diagnostic analyzers and code fix providers to invoke the Microsoft.CodeAnalysis APIs in a recommended manner.

[More info](src/Microsoft.CodeAnalysis.Analyzers/Microsoft.CodeAnalysis.Analyzers.md)


### Roslyn.Diagnostics.Analyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Roslyn.Diagnostics.Analyzers.svg)](https://www.nuget.org/packages/Roslyn.Diagnostics.Analyzers)

This package contains rules that are very specific to the .NET Compiler Platform ("Roslyn") project, i.e. https://github.com/dotnet/roslyn repo. This analyzer package is _not intended for general consumption_ outside the Roslyn repo.

[More info](src/Roslyn.Diagnostics.Analyzers/Roslyn.Diagnostics.Analyzers.md)


### Microsoft.CodeAnalysis.BannedApiAnalyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.BannedApiAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BannedApiAnalyzers)

This package contains customizable rules for identifying references to banned APIs.

[More info](src/Microsoft.CodeAnalysis.BannedApiAnalyzers/Microsoft.CodeAnalysis.BannedApiAnalyzers.md)

### Microsoft.CodeAnalysis.PublicApiAnalyzers

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.PublicApiAnalyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.PublicApiAnalyzers)

This package contains rules to help library authors monitoring change to their public APIs.

[More info](src/src/PublicApiAnalyzers/Microsoft.CodeAnalysis.PublicApiAnalyzers.md)

### MetaCompilation (prototype)

*Created by summer 2015 interns [Zoë Petard](https://github.com/zoepetard), [Jessica Petty](https://github.com/jepetty), and [Daniel King](https://github.com/daking2014)*

The MetaCompilation Analyzer is an analyzer that functions as a tutorial to teach users how to write an analyzer. It uses diagnostics and code fixes to guide the user through the various steps required to create a simple analyzer. It is designed for novice analyzer developers who have some previous programming experience.

For instructions on using this tutorial, see [Instructions](src/MetaCompilation.Analyzers/Core/ReadMe.md#instructions).

Getting Started
===============

1. Clone the repository
2. Restore and build: `build.cmd`
3. Execute tests: `test.cmd`

Submitting Pull Requests
========================

Prior to submitting a pull request, ensure the build and all tests pass using using steps 2 and 3 above.

Versioning Scheme for Analyzer Packages
=======================================

See [VERSIONING.md](.//VERSIONING.md) for the versioning scheme for all analyzer packages built out of this repo.

Recommended version of Analyzer Packages
=======================================

Recommended Visual Studio Version: **Visual Studio 2017 15.5 Preview5, RTW or later**

Recommended Analyzer Package Version: **Version 2.6.3**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/2.6.3

Due to the fact that large number of our analyzers were based on the *not-yet-shipped IOperation APIs* in Roslyn, they were in beta stage and tied to a specific compiler/Visual Studio version. IOperation API shipped in **Visual Studio 2017 15.5 Preview5**, and we have released fully supported **version 2.6.0** analyzer packages that should work on all future compiler/Visual Studio versions. Please use the following guidelines when choosing the version of analyzer packages to use on a specific version of Visual Studio/compiler toolset:

1. **Visual Studio 2015 RTW**: Analyzer package **Version 1.0.1**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/1.0.1
2. **Visual Studio 2015 Update 1**: Analyzer package **Version 1.1.0**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/1.1.0
3. **Visual Studio 2015 Update 2 and 3**: Analyzer package **Version 1.2.0-beta2**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/1.2.0-beta2
4. **Visual Studio 2017 RTW (15.0), 15.1 and 15.2**: Analyzer package **Version 2.0.0-beta2**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/2.0.0-beta2
5.  **Visual Studio 2017 15.3**: Analyzer package **Version 2.3.0-beta1**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/2.3.0-beta1
6.  **Visual Studio 2017 15.5 Preview5, RTW or later**: Analyzer package **Version 2.6.3**, for example https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/2.6.3

On Visual Studio 2017 15.5 and later releases, you can also install a custom **Microsoft Code Analysis VSIX** containing these analyzers as a Visual Studio extension for all your managed projects. See details here: https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.MicrosoftCodeAnalysis2017

