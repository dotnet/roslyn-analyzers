' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Roslyn.Diagnostics.Analyzers

Namespace Roslyn.Diagnostics.VisualBasic.Analyzers
    <ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(VisualBasicMainThreadDependencyShouldBeVerifiedCodeFix))>
    <[Shared]>
    Public Class VisualBasicMainThreadDependencyShouldBeVerifiedCodeFix
        Inherits AbstractMainThreadDependencyShouldBeVerifiedCodeFix

        Protected Overrides Function IsAttributeArgumentNamedVerified(attributeArgument As SyntaxNode) As Boolean
            Dim syntax = TryCast(attributeArgument, SimpleArgumentSyntax)
            Return CaseInsensitiveComparison.Comparer.Equals(syntax?.NameColonEquals?.Name.Identifier.ValueText, "Verified")
        End Function
    End Class
End Namespace
