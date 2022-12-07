' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance

    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix
        Inherits UseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix

        Protected Overrides Function AppendElasticMarker(replacement As SyntaxNode) As SyntaxNode
            Return replacement.WithTrailingTrivia(SyntaxFactory.ElasticMarker)
        End Function
    End Class
End Namespace