@echo ON

set root=%~dp0..\..\..
set binaries=%root%\Binaries\Debug
set a2md=%binaries%\a2md.exe

pushd %binaries%

%a2md% Desktop.Analyzers.dll Desktop.CSharp.Analyzers.dll Desktop.VisualBasic.Analyzers.dll > %root%\src\FxCop\Desktop.Analyzers\Desktop.Analyzers.md
%a2md% System.Runtime.InteropServices.Analyzers.dll System.Runtime.InteropServices.CSharp.Analyzers.dll System.Runtime.InteropServices.VisualBasic.Analyzers.pdb > %root%\src\Fxcop\System.Runtime.InteropServices.Analyzers\System.Runtime.InteropServices.Analyzers.md
%a2md% System.Runtime.Analyzers.dll System.Runtime.CSharp.Analyzers.dll System.Runtime.VisualBasic.Analyzers.dll > %root%\src\fxcop\System.Runtime.Analyzers\System.Runtime.Analyzers.md
%a2md% Roslyn.Diagnostics.Analyzers.CSharp.dll Roslyn.Diagnostics.Analyzers.dll Roslyn.Diagnostics.Analyzers.VisualBasic.dll > %root%\src\Roslyn\Microsoft.Net.RoslynDiagnostics.md
%a2md% Microsoft.CodeAnalysis.Analyzers.dll Microsoft.CodeAnalysis.CSharp.Analyzers.dll Microsoft.CodeAnalysis.VisualBasic.Analyzers.dll > %root%\src\CodeAnalysis\Microsoft.CodeAnalysis.Analyzers.md
%a2md% Microsoft.AnalyzerPowerPack.Common.dll Microsoft.AnalyzerPowerPack.CSharp.dll Microsoft.AnalyzerPowerPack.VisualBasic.dll > %root%\src\AnalyzerPowerPack\AnalyzerPowerPack.md
%a2md% AsyncPackage.dll > %root%\src\AsyncPackage\AsyncPackage.md

popd