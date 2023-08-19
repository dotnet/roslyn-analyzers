<!--

Make sure you have read the contribution guidelines: 
- https://learn.microsoft.com/contribute/dotnet/dotnet-contribute-code-analysis#contribute-docs-for-caxxxx-rules
- https://github.com/dotnet/roslyn-analyzers/blob/main/GuidelinesForNewRules.md

If your Pull Request is doing one of the following:

- Adding a new diagnostic analyzer or a code fix
- Adding or updating resource strings used by analyzers and code fixes
- Updating analyzer package versions in [Versions.props](../eng/Versions.props)

Then, make sure to run `msbuild /t:pack /v:m` in the repository root; otherwise, the CI build will fail.

- This command must be run from a Visual Studio Developer Command Prompt
- Alternatively, `dotnet msbuild RoslynAnalyzers.sln -t:pack -v:m` can be used from a standard terminal window

Note: Consider merging the PR base branch (`2.9.x`, `main`, or `release/*`) into your branch before you run the pack command to reduce merge conflicts.

-->
