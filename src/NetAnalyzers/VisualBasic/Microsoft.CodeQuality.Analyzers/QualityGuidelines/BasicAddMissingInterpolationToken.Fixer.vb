' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeQuality.Analyzers.QualityGuidelines

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicAddMissingInterpolationTokenFixer
        Inherits AbstractAddMissingInterpolationTokenFixer

        Private Protected Overrides Function GetReplacement(node As SyntaxNode) As SyntaxNode
            Return SyntaxFactory.ParseExpression("$" + node.ToString())
        End Function
    End Class
End Namespace
