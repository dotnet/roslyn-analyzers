' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.Maintainability.Analyzers

Namespace Microsoft.Maintainability.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1500: Variable names should not match field names
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicVariableNamesShouldNotMatchFieldNamesFixer
        Inherits VariableNamesShouldNotMatchFieldNamesFixer

    End Class
End Namespace
