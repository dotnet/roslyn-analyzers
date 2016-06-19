﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

Imports System.Composition
Imports Microsoft.ApiDesignGuidelines.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1028: Enum Storage should be Int32
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicEnumStorageShouldBeInt32Fixer
        Inherits EnumStorageShouldBeInt32Fixer

        Protected Overrides Function GetTargetNode(node As SyntaxNode) As SyntaxNode
            Dim enumDecl = DirectCast(node, EnumBlockSyntax).EnumStatement
            Dim asClause = DirectCast(enumDecl.UnderlyingType, SimpleAsClauseSyntax)
            Return asClause
        End Function
    End Class
End Namespace

