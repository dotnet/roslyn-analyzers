' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Collections.Immutable
Imports System.Linq
Imports Analyzer.Utilities
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.Maintainability
Imports DiagnosticCategory = Microsoft.CodeQuality.Analyzers.Maintainability.DiagnosticCategory

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
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
                                                        DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                        DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                        s_localizableDescription,
                                                        "https://msdn.microsoft.com/en-us/library/ms182278.aspx",
                                                        WellKnownDiagnosticTags.Telemetry)

        Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
            Get
                Return ImmutableArray.Create(Rule)
            End Get
        End Property

        Public Overrides Sub Initialize(analysisContext As AnalysisContext)
            ' TODO: Consider making this analyzer thread-safe.
            'analysisContext.EnableConcurrentExecution()

            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)

            analysisContext.RegisterOperationBlockStartAction(
                Sub(operationBlockContext)
                    Dim containingMethod = TryCast(operationBlockContext.OwningSymbol, IMethodSymbol)

                    If containingMethod IsNot Nothing Then
                        Dim mightBecomeUnusedLocals = New HashSet(Of ILocalSymbol)()

                        operationBlockContext.RegisterOperationAction(
                        Sub(operationContext)
                            Dim declarations = DirectCast(operationContext.Operation, IVariableDeclarationGroupOperation).Declarations

                            For Each declaration In declarations
                                ' Declarations with invocation expressions as initializers declare unused locals.
                                If declaration.Initializer Is Nothing OrElse
                                declaration.Initializer.Value.ConstantValue.HasValue OrElse
                                declaration.Initializer.Value.Kind() = OperationKind.DelegateCreation Then
                                    For Each local In declaration.GetDeclaredVariables()
                                        mightBecomeUnusedLocals.Add(local)
                                    Next
                                End If
                            Next
                        End Sub, OperationKind.VariableDeclarationGroup)

                        operationBlockContext.RegisterOperationAction(
                        Sub(operationContext)
                            Dim localReferenceExpression As ILocalReferenceOperation = DirectCast(operationContext.Operation, ILocalReferenceOperation)
                            Dim syntax = localReferenceExpression.Syntax
                            If syntax.Parent.IsKind(SyntaxKind.SimpleAssignmentStatement) Then
                                Dim parent = DirectCast(syntax.Parent, AssignmentStatementSyntax)
                                If parent.Left Is syntax Then
                                    Dim simpleAssignmentExpression As IAssignmentOperation = DirectCast(localReferenceExpression.Parent, IAssignmentOperation)
                                    If simpleAssignmentExpression.Value.ConstantValue.HasValue Then
                                        Return
                                    End If
                                End If
                            End If

                            mightBecomeUnusedLocals.Remove(localReferenceExpression.Local)
                        End Sub, OperationKind.LocalReference)

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