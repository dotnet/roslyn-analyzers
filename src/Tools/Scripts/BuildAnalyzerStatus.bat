@echo ON

set root=%~dp0..\..\..
set binaries=%root%\Binaries\Debug
set a2status=%binaries%\AnalyzersStatusGenerator.exe

pushd %binaries%

%a2status% ApiReview.Analyzers.dll ApiReview.CSharp.Analyzers.dll ApiReview.VisualBasic.Analyzers.dll Desktop.Analyzers.dll Desktop.CSharp.Analyzers.dll Desktop.VisualBasic.Analyzers.dll Microsoft.ApiDesignGuidelines.Analyzers.dll Microsoft.ApiDesignGuidelines.CSharp.Analyzers.dll Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers.dll Microsoft.CodeAnalysis.Analyzers.dll Microsoft.CodeAnalysis.CSharp.Analyzers.dll Microsoft.CodeAnalysis.VisualBasic.Analyzers.dll Roslyn.Diagnostics.CSharp.Analyzers.dll Roslyn.Diagnostics.Analyzers.dll Roslyn.Diagnostics.VisualBasic.Analyzers.dll System.Collections.Immutable.Analyzers.dll System.Collections.Immutable.CSharp.Analyzers.dll System.Collections.Immutable.VisualBasic.Analyzers.dll System.Runtime.Analyzers.dll System.Runtime.CSharp.Analyzers.dll System.Runtime.VisualBasic.Analyzers.dll System.Runtime.InteropServices.Analyzers.dll System.Runtime.InteropServices.CSharp.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.CSharp.Analyzers.dll System.Security.Cryptography.Hashing.Algorithms.VisualBasic.Analyzers.dll System.Threading.Tasks.Analyzers.dll System.Threading.Tasks.CSharp.Analyzers.dll System.Threading.Tasks.VisualBasic.Analyzers.dll XmlDocumentationComments.Analyzers.dll XmlDocumentationComments.CSharp.Analyzers.dll XmlDocumentationComments.VisualBasic.Analyzers.dll Microsoft.Composition.Analyzers.dll Microsoft.Composition.CSharp.Analyzers.dll Microsoft.Composition.VisualBasic.Analyzers.dll Microsoft.Maintainability.Analyzers.dll Microsoft.Maintainability.CSharp.Analyzers.dll Microsoft.Maintainability.VisualBasic.Analyzers.dll Microsoft.QualityGuidelines.Analyzers.dll Microsoft.QualityGuidelines.CSharp.Analyzers.dll Microsoft.QualityGuidelines.VisualBasic.Analyzers.dll System.Resources.Analyzers.dll System.Resources.CSharp.Analyzers.dll System.Resources.VisualBasic.Analyzers.dll Text.Analyzers.dll Text.CSharp.Analyzers.dll Text.VisualBasic.Analyzers.dll > status.json


popd