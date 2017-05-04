.NET Compiler Platform ("Roslyn") Analyzers
===========================================

This repository contains a number of [Roslyn](https://github.com/dotnet/roslyn) diagnostic analyzers initially developed to help flesh out the design and implementation of the static analysis APIs. They have been migrated from the main [dotnet/roslyn](https://github.com/dotnet/roslyn) repository in order to continue and speed their further development.

Debug | Release
------|--------
[![Build Status](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers/job/master/job/windows_debug/badge/icon)](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers/job/master/job/windows_debug/) | [![Build Status](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers/job/master/job/windows_release/badge/icon)](http://dotnet-ci.cloudapp.net/job/dotnet_roslyn-analyzers/job/master/job/windows_release/)

[![Join the chat at https://gitter.im/dotnet/roslyn](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/roslyn?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


Projects
========

Microsoft.NetFramework.Analyzers
-----------------

*Latest stable version:* [1.1.0](https://www.nuget.org/packages/Desktop.Analyzers/)

Analyzers for APIs specific to the desktop .NET Framework.

[More info](src/Microsoft.NetFramework.Analyzers/Microsoft.NetFramework.Analyzers.md)

Microsoft.ApiDesignGuidelines.Analyzers
--------------------------------

Provides analyzers which implement the .NET design guidelines for API authors.

[More info](src/Microsoft.CodeQuality.Analyzers/Documentation/ApiDesignGuidelines/Microsoft.ApiDesignGuidelines.Analyzers.md)


Microsoft.CodeAnalysis.Analyzers
--------------------------------

*Latest stable version:* [1.1.0](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers/)

Provides guidelines for using .NET Compiler Platform ("Roslyn") APIs.

[More info](src/Microsoft.CodeAnalysis.Analyzers/Microsoft.CodeAnalysis.Analyzers.md)


Roslyn.Diagnostics.Analyzers
-------------------------------

Contains analyzers specific to the .NET Compiler Platform ("Roslyn") project.

[More info](src/Roslyn.Diagnostics.Analyzers/Roslyn.Diagnostics.Analyzers.md)

System.Collections.Immutable.Analyzers
------------------------

Provides guidelines for using APIs in the System.Collections.Immutable contract.

[More info](src/Microsoft.NetCore.Analyzers/Documentation/System.Collections.Immutable.Analyzers/System.Collections.Immutable.Analyzers.md)

System.Runtime.Analyzers
------------------------

*Latest stable version:* [1.1.0](https://www.nuget.org/packages/System.Runtime.Analyzers/)

Provides guidelines for using APIs in the System.Runtime contract.

[More info](src/Microsoft.NetCore.Analyzers/Documentation/System.Runtime.Analyzers/System.Runtime.Analyzers.md)


System.Runtime.InteropServices.Analyzers
----------------------------------------

*Latest stable version:* [1.1.0](https://www.nuget.org/packages/System.Runtime.InteropServices.Analyzers/)

Provides guidelines for using APIs in the System.Runtime.InteropServices contract.

[More info](src/Microsoft.NetCore.Analyzers/Documentation/System.Runtime.InteropServices.Analyzers/System.Runtime.InteropServices.Analyzers.md)

System.Security.Cryptography.Hashing.Algorithms.Analyzers
------------------------

*Latest stable version:* [1.1.0](https://www.nuget.org/packages/System.Security.Cryptography.Hashing.Algorithms.Analyzers/)

Provides guidelines for using APIs in the System.Cryptography.Hashing.Algorithms contract.

[More info](src/Microsoft.NetCore.Analyzers/Documentation/System.Security.Cryptography.Hashing.Algorithms.Analyzers/System.Security.Cryptography.Hashing.Algorithms.Analyzers.md)

MetaCompilation
---------------

*Created by summer 2015 interns [Zoë Petard](https://github.com/zoepetard), [Jessica Petty](https://github.com/jepetty), and [Daniel King](https://github.com/daking2014)*

The MetaCompilation Analyzer is an analyzer that functions as a tutorial to teach users how to write an analyzer. It uses diagnostics and code fixes to guide the user through the various steps required to create a simple analyzer. It is designed for a novice analyzer programmer with some previous programming experience.

For instructions on using this tutorial, see [Instructions](src/MetaCompilation.Analyzers/Core/ReadMe.md#instructions).


Getting Started
===============

1. Clone the repository
2. Install NuGet packages: `msbuild /t:restore RoslynAnalyzers.sln`
3. Build: `msbuild RoslynAnalyzers.sln`

Execute `cibuild.cmd` to clean, restore, build and runs tests

Submitting Pull Requests
========================

Prior to submitting a pull request, ensure the build and all tests pass using BuildAndTest.proj:
```
msbuild BuildAndTest.proj
```

Versioning Scheme for Analyzer Packages
=======================================

See [VERSIONING.md](.//VERSIONING.md) for the versioning scheme for all analyzer packages built out of this repo.
