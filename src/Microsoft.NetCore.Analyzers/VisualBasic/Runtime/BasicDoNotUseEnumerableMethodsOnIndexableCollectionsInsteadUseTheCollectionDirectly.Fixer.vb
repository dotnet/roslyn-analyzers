' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.NetCore.Analyzers.Runtime
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports System.Collections.Immutable

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    ''' <summary>
    ''' RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer
        Inherits CodeFixProvider

        Public Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
            Get
                Return ImmutableArray.Create(DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.RuleId)
            End Get
        End Property

        Public Overrides Function GetFixAllProvider() As FixAllProvider
            return WellKnownFixAllProviders.BatchFixer
        End Function

        Public Overrides Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
            Return Task.CompletedTask
        End Function
    End Class
End Namespace
