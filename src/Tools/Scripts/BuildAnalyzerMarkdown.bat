@echo ON

set root=%~dp0..\..\..
set binaries=%root%\Binaries\Debug
set a2md=%binaries%\a2md.exe

pushd %binaries%

%a2md% ApiReview.Analyzers.dll ApiReview.CSharp.Analyzers.dll ApiReview.VisualBasic.Analyzers.dll > %root%\src\ApiReview.Analyzers\ApiReview.Analyzers.md
%a2md% Desktop.Analyzers.dll Desktop.CSharp.Analyzers.dll Desktop.VisualBasic.Analyzers.dll > %root%\src\Desktop.Analyzers\Desktop.Analyzers.md
%a2md% Microsoft.ApiDesignGuidelines.Analyzers.dll Microsoft.ApiDesignGuidelines.CSharp.Analyzers.dll Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers.dll > %root%\src\Microsoft.ApiDesignGuidelines.Analyzers\Microsoft.ApiDesignGuidelines.Analyzers.md
%a2md% Microsoft.CodeAnalysis.Analyzers.dll Microsoft.CodeAnalysis.CSharp.Analyzers.dll Microsoft.CodeAnalysis.VisualBasic.Analyzers.dll > %root%\src\CodeAnalysis\Microsoft.CodeAnalysis.Analyzers.md
%a2md% Roslyn.Diagnostics.Analyzers.CSharp.dll Roslyn.Diagnostics.Analyzers.dll Roslyn.Diagnostics.Analyzers.VisualBasic.dll > %root%\src\Roslyn\Microsoft.Net.RoslynDiagnostics.md
%a2md% System.Collections.Immutable.Analyzers.dll System.Collections.Immutable.CSharp.Analyzers.dll System.Collections.Immutable.VisualBasic.Analyzers.dll > %root%\src\System.Collections.Immutable.Analyzers\System.Collections.Immutable.Analyzers.md
%a2md% System.Runtime.Analyzers.dll System.Runtime.CSharp.Analyzers.dll System.Runtime.VisualBasic.Analyzers.dll > %root%\src\System.Runtime.Analyzers\System.Runtime.Analyzers.md
%a2md% System.Runtime.InteropServices.Analyzers.dll System.Runtime.InteropServices.CSharp.Analyzers.dll System.Runtime.InteropServices.VisualBasic.Analyzers.pdb > %root%\src\System.Runtime.InteropServices.Analyzers\System.Runtime.InteropServices.Analyzers.md
%a2md% System.Security.Cryptography.Hashing.Algorithms.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.CSharp.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.VisualBasic.Analyzers.dll > %root%\src\System.Security.Cryptography.Hashing.Algorithms.Analyzers\System.Runtime.Analyzers.md
%a2md% System.Threading.Tasks.Analyzers.dll System.Threading.Tasks.CSharp.Analyzers.dll System.Threading.Tasks.VisualBasic.Analyzers.dll > %root%\src\System.Threading.Tasks.Analyzers\System.Threading.Tasks.Analyzers.md
%a2md% XmlDocumentationComments.Analyzers.dll XmlDocumentationComments.CSharp.Analyzers.dll XmlDocumentationComments.VisualBasic.Analyzers.dll > %root%\src\XmlDocumentationComments.Analyzers\XmlDocumentationComments.Analyzers.md

popd