' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Resources.Analyzers     
    ''' <summary>
    ''' CA1824: Mark assemblies with NeutralResourcesLanguageAttribute
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicMarkAssembliesWithNeutralResourcesLanguageFixer
        Inherits MarkAssembliesWithNeutralResourcesLanguageFixer 

    End Class
End Namespace
