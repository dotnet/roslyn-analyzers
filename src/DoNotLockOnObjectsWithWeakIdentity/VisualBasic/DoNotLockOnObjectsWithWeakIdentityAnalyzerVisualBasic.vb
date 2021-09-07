' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace CA2002.VisualBasic

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class DoNotLockOnObjectsWithWeakIdentityAnalyzerVisualBasic
        Inherits DoNotLockOnObjectsWithWeakIdentityAnalyzerBase

        Protected Overrides Function IsThisExpression(node As SyntaxNode) As Boolean
            Return TryCast(node, MeExpressionSyntax) IsNot Nothing
        End Function
    End Class

End Namespace
