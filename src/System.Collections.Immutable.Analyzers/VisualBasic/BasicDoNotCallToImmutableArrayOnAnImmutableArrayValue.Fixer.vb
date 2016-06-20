' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable.Analyzers
Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Collections.Immutable.VisualBasic.Analyzers
    ''' <summary>
    ''' RS0012: Do not call ToImmutableArray on an ImmutableArray value
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCallToImmutableArrayOnAnImmutableArrayValueFixer
        Inherits DoNotCallToImmutableArrayOnAnImmutableArrayValueFixer

    End Class
End Namespace
