' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeQuality.Analyzers.Usage

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Usage
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCallOrderByMultipleTimesFixer
        Inherits DoNotCallOrderByMultipleTimesFixer

        Protected Overrides Function ReplaceOrderByWithThenBy(document As Document, root As SyntaxNode, node As SyntaxNode) As Document
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
