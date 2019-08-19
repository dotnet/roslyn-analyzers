' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicUsePropertyInsteadOfCountMethodWhenAvailableAnalyzer
        Inherits UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer

        Protected Overrides Function CreateOperationActionsHandler(context As OperationActionsContext) As OperationActionsHandler

            Return New BasicOperationActionsHandler(context)

        End Function

        Private NotInheritable Class BasicOperationActionsHandler
            Inherits OperationActionsHandler

            Public Sub New(context As OperationActionsContext)
                MyBase.New(context)
            End Sub

            Protected Overrides Function GetEnumerableCountInvocationTargetType(invocationOperation As IInvocationOperation) As ITypeSymbol

                Dim method = invocationOperation.TargetMethod

                If invocationOperation.Arguments.Length = 0 AndAlso
                    method.Name.Equals(NameOf(Enumerable.Count), StringComparison.Ordinal) AndAlso
                    Me.Context.IsEnumerableType(method.ContainingSymbol) Then

                    Dim methodSourceItemType = TryCast(DirectCast(invocationOperation.Instance.Type, INamedTypeSymbol).TypeArguments.Item(0), ITypeSymbol)

                    If Not methodSourceItemType Is Nothing Then

                        Dim convertionOperation = TryCast(invocationOperation.Instance, IConversionOperation)

                        Return If(Not convertionOperation Is Nothing,
                            convertionOperation.Operand.Type,
                            invocationOperation.Instance.Type)

                    End If

                End If

                Return Nothing

            End Function

        End Class

    End Class

End Namespace
