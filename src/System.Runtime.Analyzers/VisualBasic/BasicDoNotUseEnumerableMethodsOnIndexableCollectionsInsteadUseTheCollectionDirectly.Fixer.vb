' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers
    ''' <summary>
    ''' RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer
        Inherits DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer

    End Class
End Namespace
