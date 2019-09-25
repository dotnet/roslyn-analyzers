Post-release activities
=================================================================

Please follow the below steps after publishing analyzer NuGet packages from this repo onto NuGet.org:

1. Create a new release OR Update an existing draft release:
   1. Draft: Either click [here](https://github.com/dotnet/roslyn-analyzers/releases/new) to draft a new release OR update an [existing draft release](https://github.com/dotnet/roslyn-analyzers/releases). For reference, you can look at any of the existing releases, say [v2.9.3](https://github.com/dotnet/roslyn-analyzers/releases/edit/v2.9.3).
   2. Release notes: Follow the steps in the *Steps to generate Release Notes* below to generate Release notes and copy the generated notes to the description section of the new release.
   3. Publish: Mark the release as a pre-release if appropriate and click "Publish Release".
2. Repo changes:
   1. Checkout a new branch from latest sources of release branch.
   2. Update `VERSIONING.md`: Add a new row in the released version table.
   3. Update `.github\ISSUE_TEMPLATE.md`: Update the package version in the example section to the latest released package version.
   4. Update `eng\Versions.props`:
      1. Bump up the `VersionPrefix`. If the new version prefix is greater then or equals the current `FlowAnalysisUtilitiesVersionPrefix`, then update `FlowAnalysisUtilitiesVersionPrefix` to `$(VersionPrefix)`.
      2. Reset `PreReleaseVersionLabel` to `beta1`.
      3. Update `MicrosoftCodeAnalysisFXCopAnalyersVersion` to the latest released package version.
   5. Build the repo by invoking `eng\common\CIBuild.cmd` and fix/suppress any new CA diagnostics, as appropriate. This should also update the analyzer documentation files in the repo to use the new version prefix.
   6. Create and submit a PR with the above changes.

Steps to generate Release Notes
=================================================================

1. Checkout the sources for the release branch locally. This would normally be the master branch.
2. Build.
3. Ensure that nuget.exe is on path.
4. Generate notes: Switch to the output directory, say `artifacts\bin\ReleaseNotesUtil\Debug\netcoreapp2.0` and execute `GenDiffNotes.cmd` to generate release notes.  Example command line for v2.9.4 to v2.9.5: `GenDiffNotes.cmd C:\scratch nuget.org 2.9.3 2.9.4`.
