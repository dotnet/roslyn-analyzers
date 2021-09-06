' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotPassMutableValueTypesByValueAnalyzer : Inherits DoNotPassMutableValueTypesByValueAnalyzer
        Private Protected Overrides Function GetMethodReturnTypeLocations(methodSymbol As IMethodSymbol, token As CancellationToken) As IEnumerable(Of Location)
            Return methodSymbol.DeclaringSyntaxReferences.Select(
                Function(syntaxReference)
                    Dim node = DirectCast(syntaxReference.GetSyntax(token), MethodBlockSyntax)
                    Return node.SubOrFunctionStatement.Identifier.GetLocation()
                End Function)
        End Function

        Private Protected Overrides Function GetPropertyReturnTypeLocations(propertySymbol As IPropertySymbol, token As CancellationToken) As IEnumerable(Of Location)
            Return propertySymbol.DeclaringSyntaxReferences.Select(
                Function(syntaxReference)
                    Dim node = DirectCast(syntaxReference.GetSyntax(token), PropertyBlockSyntax)
                    Return node.PropertyStatement.Identifier.GetLocation()
                End Function)
        End Function
    End Class
End Namespace

