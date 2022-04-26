' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.InteropServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic

Namespace Microsoft.NetCore.VisualBasic.Analyzers.InteropServices

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicPlatformCompatibilityAnalyzer
        Inherits PlatformCompatibilityAnalyzer

        Protected Overrides Function IsSingleLineComment(trivia As SyntaxTrivia) As Boolean
            Return trivia.IsKind(SyntaxKind.CommentTrivia)
        End Function

    End Class
End Namespace

