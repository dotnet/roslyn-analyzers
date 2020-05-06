# Getting started with .NetCore/.NetStandard Roslyn Analyzers

1. Read through The [.NET Compiler Platform SDK](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/) for understanding the different Roslyn elements **(Syntax Nodes, Tokens, Trivia)**. The factory methods and APIs are super useful.  
2. Learning this [tutorial](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix) for custom analyzer and trying it yourself is very useful to get started. It is pretty easy step by step tutorial, it is time saving as it has a template generated for us (with analyzer, fixer and unit test), has good explanation, would give you pretty good understanding on how Roslyn analyzers work. 
3. Clone roslyn-analyzers repo, install all required dependencies and build the repo [instructions](https://github.com/dotnet/roslyn-analyzers#getting-started). 
4. Follow the coding style of the analyzer repo. [More guidelines about new rule id and doc](https://github.com/dotnet/roslyn-analyzers/blob/master/GuidelinesForNewRules.md). 
5. Open RoslynAnalyzers.sln and open the project where you are creating your analyzer. In our case, it is mostly Microsoft.NetCore.Analyzers. Create your analyzer and/or fixer class in the corresponding folder.  
6. Add a message, title and description for your analyzer into MicrosoftNetCoreAnalyzersResources.resx and build the repo before using them, the language specific resources will be generated. 
7. Make sure you have done everything from the [Definition of done list](#definition-of-done) below. 

## Definition of done 

- Analyzer implemented to work for C# and VB. 
	- Unit tests for C# 
		- All scenarios covered. 
	- Unit tests for VB. 
		- Obvious positive and negative scenarios covered. 
		- If the implementation uses any syntax-specific code, then all scenarios must be covered. 
- Fixer implemented for C#, using the language-agnostic APIs if possible. 
	- If the fixer can be entirely implemented with language-agnostic APIs `(IOperation)`, then VB support is essentially free. 
	- With a language-agnostic fixer, apply the attribute to indicate the fixer also applies to VB and add mainline VB tests. 
	- If language-specific APIs are needed to implement the fixer, the VB fixer is not required. 
- Run the analyzer locally against dotnet/runtime [instructions](#Testing-against-the-Runtime-repo). 
	- Use the failures to discover nuance and guide the implementation details. 
	- Run the analyzer against dotnet/roslyn [instruction](#Testing-against-the-Roslyn-repo), and with dotnet/aspnetcore if feasable. 
	- Review each of the failures in those repositories and determine the course of action for each. 
- Document for review: matching and non-matching scenarios, including any discovered nuance. 
- Document for review: severity, default, categorization, numbering, and messages. 
- Document for the new analyzer and rules on [microsoftDocs/visualstudio-docs-pr](https://github.com/microsoftDocs/visualstudio-docs-pr/). (need permission, [instructions](#Adding-documentation-for-new-CA-rules)). Documentation PR must be submitted within **ONE WEEK** of the rule implementation being merged. 
- PR merged into roslyn-analyzers. 
- Failures in dotnet/runtime addressed. 

## Testing against the Runtime repo 

1. Navigate to the root of the Roslyn-analyzers repo and run these commands: 
	- cd roslyn-analyzers 
	- set RUNTIMEPACKAGEVERSION=3.0.0 
	- build.cmd /p:AssemblyVersion=%RUNTIMEPACKAGEVERSION% /p:OfficialBuild=true 
	- cd artifacts\bin\Microsoft.NetCore.CSharp.Analyzers\Debug\netstandard2.0 
2. Copy the two DLLs and replace the NuGet cache entries used by dotnet/runtime. They might be in "runtime/.packages/..." or "%USERPROFILE%/.nuget/packages/... ". You can check the exact path by building something in runtime with /bl and checking the binlog file. Example: 
	- copy /y *.dll %USERPROFILE%\.nuget\packages\Microsoft.NetCore.Analyzers\%RUNTIMEPACKAGEVERSION%\analyzers\dotnet\cs 
3.    Switch to the runtime project. 
4.    Introduce an error somewhere to prove that the rule ran. 
	- Be careful about in which project you are producing an error, choose an API not having reference from other APIs, else all dependent API's will fail. 
5. Build the runtime repo, either do a complete build or build each repo separately (coreclr, libraries, mono). 

## Testing against the Roslyn repo 

1. Build Roslyn with this command: 
	- .\Build.cmd -restore -Configuration Release 
2. Build roslyn-analyzers in debug mode. 
3. Run AnalyzerRunner from the Roslyn root directory to get the diagnostics. 
	- .\artifacts\bin\AnalyzerRunner\Release\netcoreapp3.1\AnalyzerRunner.exe ..\roslyn-analyzers\artifacts\bin\Microsoft.NetCore.Analyzers.Package\Debug\netstandard2.0 .\Roslyn.sln /stats /concurrent /a AnalyzerNameToTest /log Output.txt  
The diagnostics reported by the analyzer will be listed in Output.txt. 

## Adding documentation for new CA rules 

1. Documentation PRs are submitted to microsoftDocs/visualstudio-docs-pr, which is a private repo with some extra validation as compared to the public one microsoftDocs/visualstudio-docs. We are required to submit PRs to the former. Docs team will flow these changes into the public repo and get them published. 
2. Getting permissions to the repo: here. 
3. All existing CA rules are documented here: MicrosoftDocs/visualstudio-docs-pr/tree/master/docs/code-quality 
	1. You can see this folder has a separate file for each CA rule. For example, for CA1000 we have ca1000.md, which gets published at https://docs.microsoft.com/visualstudio/code-quality/ca1000 
	2. Each PR can clone an existing CA file and update it appropriately. Apart from this, the rule ID needs to be added to couple of existing tables for categorized grouping and easy user navigation. 
	3. Example PR: https://github.com/MicrosoftDocs/visualstudio-docs-pr/pull/5405/files. I think most of the doc PRs would have similar file changes. 
	4. Roslyn-analyzers repo is already setup so that each new analyzer implementation CAxxxx will automatically link to https://docs.microsoft.com/visualstudio/code-quality/caxxxx for its help link. 
4. Finally, if for some reason if we are unable to submit a docs PR at the time of implementing it, please file a tracking issue at MicrosoftDocs/visualstudio-docs-pr/issues, so someone can get to implementing it in future. Example issue: MicrosoftDocs/visualstudio-docs/issues/3454. 