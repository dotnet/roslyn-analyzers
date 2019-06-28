REM GenDiffNotes scratchDir nugetSource fxCopAnalyzersOldVersion fxCopAnalyzersNewVersion
mkdir %1
pushd %1
nuget.exe install -source %2 Microsoft.CodeAnalysis.FxCopAnalyzers -version %3
nuget.exe install -source %2 Microsoft.CodeAnalysis.FxCopAnalyzers -version %4
popd
dotnet.exe ReleaseNotesUtil.dll getrulesjson %1 %3 %1\%3-rules.json
dotnet.exe ReleaseNotesUtil.dll getrulesjson %1 %4 %1\%4-rules.json
dotnet.exe ReleaseNotesUtil.dll diffrules %1\%3-rules.json %1\%4-rules.json %1\%3_%4-notes.md