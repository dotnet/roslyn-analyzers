' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' CA1724: Type names should not match namespaces
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicTypeNamesShouldNotMatchNamespacesFixer
        Inherits TypeNamesShouldNotMatchNamespacesFixer

    End Class
End Namespace
