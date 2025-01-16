# Post-release activities

## Steps to generate Packages

Please follow the below steps after publishing analyzer NuGet packages from this repo onto NuGet.org:

1. Create a new release OR Update an existing draft release:
   1. Draft: Either click [here](https://github.com/dotnet/roslyn-analyzers/releases/new) to draft a new release OR update an [existing draft release](https://github.com/dotnet/roslyn-analyzers/releases). For reference, you can look at any of the existing releases, say [v2.9.3](https://github.com/dotnet/roslyn-analyzers/releases/edit/v2.9.3).
   2. Release notes: Follow the steps in the *Steps to generate Release Notes* below to generate Release notes and copy the generated notes to the description section of the new release.
   3. Publish: Mark the release as a pre-release if appropriate and click "Publish Release".
2. Repo changes:
   1. Checkout a new branch from latest sources of release branch.
   2. Update `VERSIONING.md`: Add a new row in the released version table.
   3. Update `.github\ISSUE_TEMPLATE\bug-report.md`: Update the package version in the example section to the latest released package version.
   4. Update `eng\Versions.props`:
      1. Bump up the `VersionPrefix`. If the new version prefix is greater then or equals the current `AnalyzerUtilitiesVersionPrefix`, then update `AnalyzerUtilitiesVersionPrefix` to `$(VersionPrefix)`.
      2. Reset `PreReleaseVersionLabel` to `beta1`.
      3. Update `DogfoodNetAnalyzersVersion` and/or `DogfoodAnalyzersVersion` to the latest released package version.
   5. Build the repo by invoking `eng\common\CIBuild.cmd` and fix/suppress any new CA diagnostics, as appropriate. This should also update the analyzer documentation files in the repo to use the new version prefix.
   6. Move all the entries from `AnalyzerReleases.Unshipped.md` to `AnalyzerReleases.Shipped.md` for various analyzer NuGet package projects under a new "Release" section in the shipped file.
   7. Create and submit a PR with the above changes.

## Steps to generate Release Notes

1. Go to the the NuGet page of [Microsoft.CodeAnalysis.NetAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers).
2. Click on the Versions tab and select the version for which you want to create a tag and release notes, then download the nupkg. For this example, we are going to use [7.0.4](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers/7.0.4).
3. Open the downloaded NuGet package as a zip, then open the `Microsoft.CodeAnalysis.NetAnalyzers.nuspec` file at the root.
4. Copy the commit `hash` located in the `<repository>` entry at the end of the nuspec file.
5. In your local roslyn-analyzers clone, fetch and checkout the latest bits in main (or the `release/X.Y.Zxx` branch if main does not contain that hash).
6. Checkout the commit hash you copied: `git checkout <hash>`.
7. Execute `git tag X.Y.Z` to create a new local tag, where `X.Y.Z` represents the package version. For this example, it's: `git tag 7.0.4`.
8. Make sure you have permissions to push to dotnet/roslyn-analyzers.
9. Assuming your dotnet/roslyn-analyzers remote is named `upstream`, execute `git push upstream X.Y.Z`. For example: `git push upstream 7.0.4`.
10. Go to the [dotnet/roslyn-analyzers](https://github.com/dotnet/roslyn-analyzers) GitHub page. On the right sidebar, click on Releases. Then click on the "Draft a new release" button.
11. Click on the "Choose a tag" dropdown menu and select the tag you just pushed, `7.0.4`. The dropdowns will now change.
12. Click on the "Previous tag: auto" dropdown menu that just showed up and select the version that was released before your chosen tag. For this example, the previous version was `7.0.3`.
13. Click on the "Generate release notes" button so GitHub autogenerates the "What's changed" and "Full Changelog" texts for you.
14. **Prepend** the following text at the beginning of that generated text:

      ```md
      Release build of [Microsoft.CodeAnalysis.NetAnalyzers CURRENT-VERSION](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers/CURRENT-VERSION) containing first-party [code quality analyzers ("CAxxxx rules")](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/overview#code-quality-analysis).

      Contains bug fixes on top of [PREVIOUS-VERSION](https://github.com/dotnet/roslyn-analyzers/releases/tag/PREVIOUS-VERSION) release.
      ```

15. Make sure to change all the `CURRENT-VERSION` placeholders to the current tag value (`7.0.4` in this example), and all the `PREVIOUS-VERSION` placeholders to the previous tag value (`7.0.3` in this example).
16. If necessary, check the "Set as the latest release" and/or "Set as a pre-release" checkboxes.
17. Click on "Publish release".
18. The new release should now show up under [dotnet/roslyn-analyzers/releases](https://github.com/dotnet/roslyn-analyzers/releases)

## Followup items

1. For `Microsoft.CodeAnalysis.Analyzers` package, update [`MicrosoftCodeAnalysisAnalyzersVersion`](https://github.com/dotnet/roslyn/blob/95809b0b922439465a213922ef7eb81e9b5a223f/eng/Versions.props#L82) in dotnet/roslyn to reference the new package version.
2. For `Roslyn.Diagnostics.Analyzers` package, update [`RoslynDiagnosticsNugetPackageVersion`](https://github.com/dotnet/roslyn/blob/95809b0b922439465a213922ef7eb81e9b5a223f/eng/Versions.props#L30) in dotnet/roslyn to reference the new package version.
