.NET Compiler Platform ("Roslyn") Analyzers
===========================================

[![Join the chat at https://gitter.im/roslyn-analyzers/Lobby](https://badges.gitter.im/roslyn-analyzers/Lobby.svg)](https://gitter.im/roslyn-analyzers/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This repository contains a number of [Roslyn](https://github.com/dotnet/roslyn) diagnostic analyzers initially developed to help flesh out the design and implementation of the static analysis APIs. They have been migrated from the main [dotnet/roslyn](https://github.com/dotnet/roslyn) repository in order to continue and speed their further development.

Pre-release builds are available on MyGet gallery: https://dotnet.myget.org/Gallery/roslyn-analyzers.

[//]: # (Begin current test results)

|          |Windows Debug|Windows Release|
|:--------:|:-----------:|:-------------:|
|**master**|[![Build Status](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Debug/badge/icon)](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Debug/) [![codecov](https://codecov.io/gh/dotnet/roslyn-analyzers/branch/master/graph/badge.svg)](https://codecov.io/gh/dotnet/roslyn-analyzers)|[![Build Status](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Release/badge/icon)](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/job/Windows_NT_Release/)|

[//]: # (End current test results)

Projects
========

MetaCompilation
---------------

*Created by summer 2015 interns [Zoë Petard](https://github.com/zoepetard), [Jessica Petty](https://github.com/jepetty), and [Daniel King](https://github.com/daking2014)*

The MetaCompilation Analyzer is an analyzer that functions as a tutorial to teach users how to write an analyzer. It uses diagnostics and code fixes to guide the user through the various steps required to create a simple analyzer. It is designed for a novice analyzer programmer with some previous programming experience.

For instructions on using this tutorial, see [Instructions](src/MetaCompilation.Analyzers/Core/ReadMe.md#instructions).

Microsoft.CodeAnalysis.Analyzers
--------------------------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers)

Provides guidelines for using .NET Compiler Platform ("Roslyn") APIs.

[More info](src/Microsoft.CodeAnalysis.Analyzers/Microsoft.CodeAnalysis.Analyzers.md)

Microsoft.CodeQuality.Analyzers
--------------------------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.CodeQuality.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.CodeQuality.Analyzers)

Provides common code quality guidelines.

[More info](src/Microsoft.CodeQuality.Analyzers/Microsoft.CodeQuality.Analyzers.md)

Microsoft.CodeQuality.Analyzers.Exp
--------------------------------

*Latest pre-release version:* [![NuGet](https://img.shields.io/myget/v/Microsoft.CodeQuality.Analyzers.Exp.svg)](https://dotnet.myget.org/feed/roslyn-analyzers/package/nuget/Microsoft.CodeQuality.Analyzers.Exp)

Experimental package based on Dataflow Analysis to provide code quality guidelines in executable code.

[More info](src/Microsoft.CodeQuality.Analyzers.Exp/Microsoft.CodeQuality.Analyzers.Exp.md)

Microsoft.NetCore.Analyzers
-----------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetCore.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetCore.Analyzers)

Analyzers for APIs specific to .NET Core.

[More info](src/Microsoft.NetCore.Analyzers/Microsoft.NetCore.Analyzers.md)

Microsoft.NetFramework.Analyzers
-----------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Microsoft.NetFramework.Analyzers.svg)](https://www.nuget.org/packages/Microsoft.NetFramework.Analyzers)

Analyzers for APIs specific to the desktop .NET Framework.

[More info](src/Microsoft.NetFramework.Analyzers/Microsoft.NetFramework.Analyzers.md)

Roslyn.Diagnostics.Analyzers
-------------------------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Roslyn.Diagnostics.Analyzers.svg)](https://www.nuget.org/packages/Roslyn.Diagnostics.Analyzers)

Contains analyzers specific to the .NET Compiler Platform ("Roslyn") project.

[More info](src/Roslyn.Diagnostics.Analyzers/Roslyn.Diagnostics.Analyzers.md)

Text.Analyzers
-------------------------------

*Latest stable version:* [![NuGet](https://img.shields.io/nuget/v/Text.Analyzers.svg)](https://www.nuget.org/packages/Text.Analyzers)

Contains analyzers for text included in code, such as comments.

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

