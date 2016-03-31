' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Semantics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.Maintainability.Analyzers
    ''' <summary>
    ''' CA1804: Remove unused locals
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicRemoveUnusedLocalsAnalyzer
        Inherits DiagnosticAnalyzer
        Friend Const RuleId As String = "CA1804"

        Private Shared ReadOnly s_localizableTitle As LocalizableString = New LocalizableResourceString(NameOf(MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, GetType(MicrosoftMaintainabilityAnalyzersResources))
        Private Shared ReadOnly s_localizableMessage As LocalizableString = New LocalizableResourceString(NameOf(MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, GetType(MicrosoftMaintainabilityAnalyzersResources))
        Private Shared ReadOnly s_localizableDescription As LocalizableString = New LocalizableResourceString(NameOf(MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, GetType(MicrosoftMaintainabilityAnalyzersResources))

        Friend Shared Rule As DiagnosticDescriptor = New DiagnosticDescriptor(
                                                        RuleId,
                                                        s_localizableTitle,
                                                        s_localizableMessage,
                                                        DiagnosticCategory.Performance,
                                                        DiagnosticSeverity.Warning,
                                                        True,
                                                        s_localizableDescription,
                                                        "https://msdn.microsoft.com/en-us/library/ms182278.aspx",
                                                        WellKnownDiagnosticTags.Telemetry)

        Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
            Get
                Return ImmutableArray.Create(Rule)
            End Get
        End Property

        Public Overrides Sub Initialize(analysisContext As AnalysisContext)

            analysisContext.RegisterOperationBlockStartAction(
            Sub(operationBlockContext)
                Dim containingMethod = TryCast(operationBlockContext.OwningSymbol, IMethodSymbol)

                If containingMethod IsNot Nothing Then
                    Dim mightBecomeUnusedLocals = New HashSet(Of ILocalSymbol)()

                    operationBlockContext.RegisterOperationAction(
                    Sub(operationContext)
                        Dim variables = DirectCast(operationContext.Operation, IVariableDeclarationStatement).Variables

                        For Each variable In variables
                            Dim local = variable.Variable

                            mightBecomeUnusedLocals.Add(local)
                        Next
                    End Sub, OperationKind.VariableDeclarationStatement)

                    operationBlockContext.RegisterOperationAction(
                    Sub(operationContext)
                        Dim localReferenceExpression As ILocalReferenceExpression = DirectCast(operationContext.Operation, ILocalReferenceExpression)
                        Dim syntax = localReferenceExpression.Syntax

                        ' The writeonly references should be ignored
                        If syntax.Parent.IsKind(SyntaxKind.SimpleAssignmentStatement) AndAlso
                            DirectCast(syntax.Parent, AssignmentStatementSyntax).Left Is syntax Then
                            Return
                        End If

                        mightBecomeUnusedLocals.Remove(localReferenceExpression.Local)
                    End Sub, OperationKind.LocalReferenceExpression)

                    operationBlockContext.RegisterOperationBlockEndAction(
                    Sub(operationBlockEndContext)
                        For Each local In mightBecomeUnusedLocals
                            operationBlockEndContext.ReportDiagnostic(Diagnostic.Create(Rule, local.Locations.FirstOrDefault()))
                        Next
                    End Sub)
                End If
            End Sub)
        End Sub
    End Class
End Namespace