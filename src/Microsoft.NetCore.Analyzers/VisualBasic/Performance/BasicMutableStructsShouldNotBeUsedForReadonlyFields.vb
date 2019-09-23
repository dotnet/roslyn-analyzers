' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer
        Inherits MutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer

        Protected Overrides Sub RegisterDiagnosticAction(context As AnalysisContext)
            context.RegisterSyntaxNodeAction(AddressOf NodeAction, SyntaxKind.FieldDeclaration)
        End Sub

        Private Shared Sub NodeAction(context As SyntaxNodeAnalysisContext)
            Dim fieldDeclaration = CType(context.Node, FieldDeclarationSyntax)
            Dim fieldTypeInfo = context.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Declarators.First().Names.First())

            Dim fieldSymbol = TryCast(fieldTypeInfo, IFieldSymbol)

            If fieldSymbol Is Nothing
                Return
            End If

            Dim fieldType = fieldSymbol.Type

            Dim typesToCheck = MutableValueTypesOfInterest.Select(function(typeName) context.Compilation.GetTypeByMetadataName(typeName)).Where(function(symbol) symbol IsNot Nothing).ToList()

            If typesToCheck.Any(function(symbol) symbol.Equals(fieldType)) And fieldDeclaration.Modifiers.Any(function(x) x.IsKind(SyntaxKind.ReadOnlyKeyword))
                ReportDiagnostic(context, fieldDeclaration.Modifiers.First(function(x) x.IsKind(SyntaxKind.ReadOnlyKeyword)).GetLocation(), fieldSymbol.Name, fieldType.Name)
            End If
        End Sub
    End Class
End Namespace