﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#nullable disable warnings

using System;
using System.Collections.Immutable;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.Lightup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Diagnostics.Analyzers;

namespace Roslyn.Diagnostics.CSharp.Analyzers
{
    using static RoslynDiagnosticsAnalyzersResources;

    /// <summary>
    /// RS0046: Avoid 'Opt' suffix for nullable enable code
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAvoidOptSuffixForNullableEnableCode : DiagnosticAnalyzer
    {
        internal const string OptSuffix = "Opt";

        internal static DiagnosticDescriptor Rule = new(
            RoslynDiagnosticIds.AvoidOptSuffixForNullableEnableCodeRuleId,
            CreateLocalizableResourceString(nameof(AvoidOptSuffixForNullableEnableCodeTitle)),
            CreateLocalizableResourceString(nameof(AvoidOptSuffixForNullableEnableCodeMessage)),
            DiagnosticCategory.RoslynDiagnosticsDesign,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(AvoidOptSuffixForNullableEnableCodeDescription)),
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(context =>
            {
                var parameter = (ParameterSyntax)context.Node;
                ReportOnInvalidIdentifier(parameter.Identifier, context.SemanticModel, context.ReportDiagnostic, context.CancellationToken);
            }, SyntaxKind.Parameter);

            context.RegisterSyntaxNodeAction(context =>
            {
                var variableDeclarator = (VariableDeclaratorSyntax)context.Node;
                ReportOnInvalidIdentifier(variableDeclarator.Identifier, context.SemanticModel, context.ReportDiagnostic, context.CancellationToken);
            }, SyntaxKind.VariableDeclarator);

            context.RegisterSyntaxNodeAction(context =>
            {
                var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
                ReportOnInvalidIdentifier(propertyDeclaration.Identifier, context.SemanticModel, context.ReportDiagnostic, context.CancellationToken);
            }, SyntaxKind.PropertyDeclaration);
        }

        private static void ReportOnInvalidIdentifier(SyntaxToken identifier, SemanticModel semanticModel, Action<Diagnostic> reportAction, CancellationToken cancellationToken)
        {
            if (!identifier.Text.EndsWith(OptSuffix, StringComparison.Ordinal) ||
                !semanticModel.GetNullableContext(identifier.SpanStart).AnnotationsEnabled())
            {
                return;
            }

            var symbol = semanticModel.GetDeclaredSymbol(identifier.Parent, cancellationToken);

            if (ShouldReport(symbol))
            {
                reportAction(identifier.CreateDiagnostic(Rule));
            }
        }

        private static bool ShouldReport(ISymbol symbol)
        {
            if (symbol?.GetMemberOrLocalOrParameterType()?.NullableAnnotation() != Analyzer.Utilities.Lightup.NullableAnnotation.Annotated)
            {
                // Not in a nullable context, bail-out
                return false;
            }

            return symbol.Kind switch
            {
                SymbolKind.Property => !symbol.IsOverride
                    && !symbol.IsImplementationOfAnyInterfaceMember(),

                SymbolKind.Parameter => symbol.ContainingSymbol != null
                    && !symbol.ContainingSymbol.IsOverride
                    && !symbol.ContainingSymbol.IsImplementationOfAnyInterfaceMember(),

                _ => true
            };
        }
    }
}
