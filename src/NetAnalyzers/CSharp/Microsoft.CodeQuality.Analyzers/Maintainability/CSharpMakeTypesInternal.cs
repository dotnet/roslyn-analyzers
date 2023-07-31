// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpMakeTypesInternal : MakeTypesInternal<SyntaxKind>
    {
        protected override ImmutableArray<SyntaxKind> TypeKinds { get; } =
            ImmutableArray.Create(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.RecordDeclaration);

        protected override SyntaxKind EnumKind { get; } = SyntaxKind.EnumDeclaration;

        protected override void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var type = (TypeDeclarationSyntax)context.Node;
            if (type.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                context.ReportDiagnostic(type.Identifier.CreateDiagnostic(Rule));
            }
        }

        protected override void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var @enum = (EnumDeclarationSyntax)context.Node;
            if (@enum.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                context.ReportDiagnostic(@enum.Identifier.CreateDiagnostic(Rule));
            }
        }
    }
}