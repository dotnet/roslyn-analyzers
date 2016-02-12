' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.Composition.Analyzers
    ''' <summary>
    ''' RS0023: Parts exported with MEFv2 must be marked as Shared
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicPartsExportedWithMEFv2MustBeMarkedAsSharedFixer
        Inherits PartsExportedWithMEFv2MustBeMarkedAsSharedFixer

    End Class
End Namespace
