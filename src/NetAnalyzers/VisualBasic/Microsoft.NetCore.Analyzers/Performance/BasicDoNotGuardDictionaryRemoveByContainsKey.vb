' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotGuardDictionaryRemoveByContainsKeyFixer
        Inherits DoNotGuardDictionaryRemoveByContainsKeyFixer

        Protected Overrides Function OperationSupportedByFixer(conditionalOperation As SyntaxNode) As Boolean
            If TypeOf conditionalOperation Is IfStatementSyntax Then
                Return True
            End If

            If TypeOf conditionalOperation Is MultiLineIfBlockSyntax Then
                Return CType(conditionalOperation, MultiLineIfBlockSyntax).IfStatement.ChildNodes().Count() = 1
            End If

            Return False
        End Function
    End Class
End Namespace
