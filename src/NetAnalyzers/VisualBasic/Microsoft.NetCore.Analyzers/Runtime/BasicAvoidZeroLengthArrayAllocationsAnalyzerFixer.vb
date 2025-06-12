' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.Runtime
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports System.Composition

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicAvoidZeroLengthArrayAllocationsFixer
        Inherits AvoidZeroLengthArrayAllocationsFixer

        Protected Overrides Function AddElasticMarker(Of T As SyntaxNode)(syntaxNode As T) As T
            Return syntaxNode.WithTrailingTrivia(syntaxNode.GetTrailingTrivia().Add(SyntaxFactory.ElasticMarker))
        End Function
    End Class
End Namespace