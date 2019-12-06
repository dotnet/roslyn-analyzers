' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Analyzer.Utilities.Extensions
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicEnumShouldNotHaveDuplicatedValues
        Inherits EnumShouldNotHaveDuplicatedValues

        Protected Overrides Sub AnalyzeEnumMemberValues(membersByValue As Dictionary(Of Object, List(Of SyntaxNode)), context As SymbolAnalysisContext)
            For Each kvp As KeyValuePair(Of Object, List(Of SyntaxNode)) In membersByValue
                For index = 0 To kvp.Value.Count - 1
                    Dim enumMember = CType(kvp.Value(index), EnumMemberDeclarationSyntax)


                    If enumMember.Initializer Is Nothing Then
                        ReportIfNotFirstItem(enumMember, index, context)
                    Else
                        If TypeOf enumMember.Initializer.Value Is IdentifierNameSyntax Then
                            ' It is allowed to reference another enum value through its identifier
                        ElseIf TypeOf enumMember.Initializer.Value Is BinaryExpressionSyntax Then
                            AnalyzeBinaryExpression(CType(enumMember.Initializer.Value, BinaryExpressionSyntax), context, index > 0)
                        Else
                            ReportIfNotFirstItem(enumMember, index, context)
                        End If
                    End If
                Next
            Next
        End Sub

        Private Shared Sub ReportIfNotFirstItem(enumMember As EnumMemberDeclarationSyntax, index As Integer, context As SymbolAnalysisContext)
            If index > 0 Then
                context.ReportDiagnostic(enumMember.CreateDiagnostic(RuleDuplicatedValue))
            End If
        End Sub

        Private Shared Sub AnalyzeBinaryExpression(binaryExpression As BinaryExpressionSyntax, context As SymbolAnalysisContext, isDuplicatedValue As Boolean)
            Dim hasBitwiseIssue = False
            Dim seen = New HashSet(Of Object)

            For Each identifier As IdentifierNameSyntax In binaryExpression.DescendantNodes().OfType(Of IdentifierNameSyntax)
                If identifier.Identifier.Value IsNot Nothing Then
                    If seen.Contains(identifier.Identifier.Value) Then
                        hasBitwiseIssue = True
                        context.ReportDiagnostic(identifier.CreateDiagnostic(RuleDuplicatedBitwiseValuePart))
                    Else
                        seen.Add(identifier.Identifier.Value)
                    End If
                End If
            Next

            ' This enum value doesn't have any duplicated bitwise part but duplicates another enum value so let's report
            If Not hasBitwiseIssue And isDuplicatedValue Then
                context.ReportDiagnostic(binaryExpression.Parent.Parent.CreateDiagnostic(RuleDuplicatedValue))
            End If
        End Sub
    End Class
End Namespace

