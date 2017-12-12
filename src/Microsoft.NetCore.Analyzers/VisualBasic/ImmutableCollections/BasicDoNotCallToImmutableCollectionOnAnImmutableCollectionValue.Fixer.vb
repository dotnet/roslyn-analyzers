' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.ImmutableCollections
Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.NetCore.VisualBasic.Analyzers.ImmutableCollections
    ''' <summary>
    ''' RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer
        Inherits DoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer

    End Class
End Namespace
