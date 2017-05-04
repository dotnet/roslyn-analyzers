' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.ImmutableCollections
Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.NetCore.VisualBasic.Analyzers.ImmutableCollections
    ''' <summary>
    ''' RS0012: Do not call ToImmutableArray on an ImmutableArray value
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCallToImmutableArrayOnAnImmutableArrayValueFixer
        Inherits DoNotCallToImmutableArrayOnAnImmutableArrayValueFixer

    End Class
End Namespace
