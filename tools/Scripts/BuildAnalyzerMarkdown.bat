@echo ON

set root=%~dp0..\..\..
set binaries=%root%\Binaries\Debug
set a2md=%binaries%\a2md.exe

pushd %binaries%

%a2md% ApiReview.Analyzers.dll ApiReview.CSharp.Analyzers.dll ApiReview.VisualBasic.Analyzers.dll > %root%\src\ApiReview.Analyzers\ApiReview.Analyzers.md
%a2md% Desktop.Analyzers.dll Desktop.CSharp.Analyzers.dll Desktop.VisualBasic.Analyzers.dll > %root%\src\Desktop.Analyzers\Desktop.Analyzers.md
%a2md% Microsoft.ApiDesignGuidelines.Analyzers.dll Microsoft.ApiDesignGuidelines.CSharp.Analyzers.dll Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers.dll > %root%\src\Microsoft.ApiDesignGuidelines.Analyzers\Microsoft.ApiDesignGuidelines.Analyzers.md
%a2md% Microsoft.CodeAnalysis.Analyzers.dll Microsoft.CodeAnalysis.CSharp.Analyzers.dll Microsoft.CodeAnalysis.VisualBasic.Analyzers.dll > %root%\src\CodeAnalysis\Microsoft.CodeAnalysis.Analyzers.md
%a2md% Microsoft.Composition.Analyzers.dll Microsoft.Composition.CSharp.Analyzers.dll Microsoft.Composition.VisualBasic.Analyzers.dll > %root%\src\Microsoft.Composition.Analyzers\Microsoft.Composition.Analyzers.md
%a2md% Microsoft.Maintainability.Analyzers.dll Microsoft.Maintainability.CSharp.Analyzers.dll Microsoft.Maintainability.VisualBasic.Analyzers.dll > %root%\src\Microsoft.Maintainability.Analyzers\Microsoft.Maintainability.Analyzers.md
%a2md% Microsoft.QualityGuidelines.Analyzers.dll Microsoft.QualityGuidelines.CSharp.Analyzers.dll Microsoft.QualityGuidelines.VisualBasic.Analyzers.dll > %root%\src\Microsoft.QualityGuidelines.Analyzers\Microsoft.QualityGuidelines.Analyzers.md
%a2md% System.Resources.Analyzers.dll System.Resources.CSharp.Analyzers.dll System.Resources.VisualBasic.Analyzers.dll > %root%\src\System.Resources.Analyzers\System.Resources.Analyzers.md
%a2md% Text.Analyzers.dll Text.CSharp.Analyzers.dll Text.VisualBasic.Analyzers.dll > %root%\src\Text.Analyzers\Text.Analyzers.md
%a2md% Roslyn.Diagnostics.CSharp.Analyzers.dll Roslyn.Diagnostics.Analyzers.dll Roslyn.Diagnostics.VisualBasic.Analyzers.dll > %root%\src\Roslyn.Diagnostics.Analyzers\Roslyn.Diagnostics.Analyzers.md
%a2md% System.Collections.Immutable.Analyzers.dll System.Collections.Immutable.CSharp.Analyzers.dll System.Collections.Immutable.VisualBasic.Analyzers.dll > %root%\src\System.Collections.Immutable.Analyzers\System.Collections.Immutable.Analyzers.md
%a2md% System.Runtime.Analyzers.dll System.Runtime.CSharp.Analyzers.dll System.Runtime.VisualBasic.Analyzers.dll > %root%\src\System.Runtime.Analyzers\System.Runtime.Analyzers.md
%a2md% System.Runtime.InteropServices.Analyzers.dll System.Runtime.InteropServices.CSharp.Analyzers.dll System.Runtime.InteropServices.VisualBasic.Analyzers.pdb > %root%\src\System.Runtime.InteropServices.Analyzers\System.Runtime.InteropServices.Analyzers.md
%a2md% System.Security.Cryptography.Hashing.Algorithms.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.CSharp.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.VisualBasic.Analyzers.dll > %root%\src\System.Security.Cryptography.Hashing.Algorithms.Analyzers\System.Runtime.Analyzers.md
%a2md% System.Threading.Tasks.Analyzers.dll System.Threading.Tasks.CSharp.Analyzers.dll System.Threading.Tasks.VisualBasic.Analyzers.dll > %root%\src\System.Threading.Tasks.Analyzers\System.Threading.Tasks.Analyzers.md
%a2md% XmlDocumentationComments.Analyzers.dll XmlDocumentationComments.CSharp.Analyzers.dll XmlDocumentationComments.VisualBasic.Analyzers.dll > %root%\src\XmlDocumentationComments.Analyzers\XmlDocumentationComments.Analyzers.md

popd