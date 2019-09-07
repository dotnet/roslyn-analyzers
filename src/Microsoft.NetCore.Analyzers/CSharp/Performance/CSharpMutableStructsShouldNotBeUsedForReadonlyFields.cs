// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer : MutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer
    {
        protected override void RegisterDiagnosticAction(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(NodeAction, SyntaxKind.FieldDeclaration);
        }

        private static void NodeAction(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            var fieldTypeInfo = context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);

            if (!(fieldTypeInfo.Type is INamedTypeSymbol fieldType))
            {
                return;
            }

            var typesToCheck = MutableValueTypesOfInterest.Select(typeName => context.Compilation.GetTypeByMetadataName(typeName)).Where(type => type != null).ToList();

            if (typesToCheck.Contains(fieldType) && fieldDeclaration.Modifiers.Any(_ => _.IsKind(SyntaxKind.ReadOnlyKeyword)))
            {
                ReportDiagnostic(context,
                    fieldDeclaration.Modifiers.First(modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword))
                        .GetLocation(),
                    fieldDeclaration.Declaration.Variables.First().Identifier.ToString(), fieldType.Name);
            }
        }
    }
}
