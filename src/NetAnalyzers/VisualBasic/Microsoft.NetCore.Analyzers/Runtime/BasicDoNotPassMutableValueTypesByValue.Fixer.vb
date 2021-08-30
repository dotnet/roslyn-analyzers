' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    Public NotInheritable Class BasicDoNotPassMutableValueTypesByValueFixer : Inherits DoNotPassMutableValueTypesByValueFixer
        Private Protected Overrides Function ConvertToByRefParameter(parameterNode As SyntaxNode) As SyntaxNode
            Dim cast = DirectCast(parameterNode, ParameterSyntax)
            Dim byrefModifierToken = SyntaxFactory.Token(SyntaxKind.ByRefKeyword)

            Return cast.AddModifiers(byrefModifierToken)
        End Function
    End Class
End Namespace

