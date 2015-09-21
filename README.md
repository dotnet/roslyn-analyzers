.NET Compiler Platform ("Roslyn") Analyzers
===========================================

This repository contains a number of [Roslyn](https://github.com/dotnet/roslyn) diagnostic analyzers initially developed to help flesh out the design and implementation of the static analysis APIs. They have been migrated from the main [dotnet/roslyn](https://github.com/dotnet/roslyn) repository in order to continue and speed their further development.

Debug | Release
------|--------
[![Build Status](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers_windows_debug/badge/icon)](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers_windows_debug/) | [![Build Status](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers_windows_release/badge/icon)](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers_windows_release/)

[![Join the chat at https://gitter.im/dotnet/roslyn](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/roslyn?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


Projects
========

AsyncPackage
-----------

*Created by summer 2014 interns Chardai Page, [Kendra Havens](https://github.com/kendrahavens), and Vivian Morgowicz*

The AsyncPackage analyzer enforces good practices when writing code that makes use of C#'s `async` and `await` language features.

[More info](src/AsyncPackage/AsyncPackage.md)


Desktop.Analyzers
-----------------

Analyzers for APIs specific to the desktop .NET Framework.

[More info](src/FxCop/Desktop.Analyzers/Desktop.Analyzers.md)


MetaCompilation
---------------

*Created by summer 2015 interns [Zoë Petard](https://github.com/zoepetard), [Jessica Petty](https://github.com/jepetty), and [Daniel King](https://github.com/daking2014)*

The MetaCompilation Analyzer is an analyzer that functions as a tutorial to teach users how to write an analyzer. It uses diagnostics and code fixes to guide the user through the various steps required to create a simple analyzer. It is designed for a novice analyzer programmer with some previous programming experience.

For instructions on using this tutorial, see [Instructions](src/MetaCompilation/MetaCompilation/MetaCompilation/ReadMe.md#instructions).


Microsoft.AnalyzerPowerPack
---------------------------

*Latest stable version:* [1.0.1](https://www.nuget.org/packages/Microsoft.AnalyzerPowerPack/)

General language rules implemented as analyzers using the .NET Compiler Platform ("Roslyn").

[More info](src/AnalyzerPowerPack/AnalyzerPowerPack.md)


Microsoft.CodeAnalysis.Analyzers
--------------------------------

*Latest stable version:* [1.0.0](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers/)

Provides guidelines for using .NET Compiler Platform ("Roslyn") APIs.

[More info](src/CodeAnalysis/Microsoft.CodeAnalysis.Analyzers.md)


Microsoft.Net.RoslynDiagnostics
-------------------------------

Contains analyzers specific to the .NET Compiler Platform ("Roslyn") project.

[More info](src/Roslyn/Microsoft.Net.RoslynDiagnostics.md)


System.Runtime.Analyzers
------------------------

*Latest stable version:* [1.0.1](https://www.nuget.org/packages/System.Runtime.Analyzers/)

Provides guidelines for using APIs in the System.Runtime namespace.

[More info](src/FxCop/System.Runtime.Analyzers/System.Runtime.Analyzers.md)


System.Runtime.InteropServices.Analyzers
----------------------------------------

*Latest stable version:* [1.0.1](https://www.nuget.org/packages/System.Runtime.InteropServices.Analyzers/)

Provides guidelines for using APIs in the System.Runtime.InteropServices namespace.

[More info](src/FxCop/System.Runtime.InteropServices.Analyzers/System.Runtime.InteropServices.Analyzers.md)


Getting Started
===============

1. Clone the repository
2. Install NuGet packages: `powershell -executionpolicy bypass src\.nuget\NuGetRestore.ps1`
3. Build: `msbuild src\Analyzers.sln`


Submitting Pull Requests
========================

Prior to submitting a pull request, ensure the build and all tests pass using BuildAndTest.proj:
```
msbuild BuildAndTest.proj
```