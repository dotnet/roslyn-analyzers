// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Diagnostics.Analyzers;

namespace Roslyn.Diagnostics.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpInvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle =
            new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.InvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsTitle),
                RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage =
            new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.InvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsMessage),
                RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.UseSiteDiagnosticsCheckerRuleId,
            s_localizableTitle,
            s_localizableMessage,
            "Usage",
            DiagnosticSeverity.Error,
            false,
            null,
            null,
            WellKnownDiagnosticTags.Telemetry);

        private static readonly ImmutableDictionary<string, string> s_propertiesToValidateMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { s_baseTypeString, s_typeSymbolFullyQualifiedName},
                { s_interfacesString, s_typeSymbolFullyQualifiedName},
                { s_allInterfacesString, s_typeSymbolFullyQualifiedName},
                { s_typeArgumentsString, s_namedTypeSymbolFullyQualifiedName},
                { s_constraintTypesString, s_typeParameterSymbolFullyQualifiedName}
            }.ToImmutableDictionary();

        private const string s_baseTypeString = "BaseType";
        private const string s_interfacesString = "Interfaces";
        private const string s_allInterfacesString = "AllInterfaces";
        private const string s_typeArgumentsString = "TypeArguments";
        private const string s_constraintTypesString = "ConstraintTypes";

        private const string s_typeSymbolFullyQualifiedName = "Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol";
        private const string s_namedTypeSymbolFullyQualifiedName = "Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol";
        private const string s_typeParameterSymbolFullyQualifiedName = "Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var name = ((MemberAccessExpressionSyntax)context.Node).Name;

            if (name.Kind() == SyntaxKind.IdentifierName)
            {
                var identifier = (IdentifierNameSyntax)name;
                string containingTypeName = null;
                if (s_propertiesToValidateMap.TryGetValue(identifier.ToString(), out containingTypeName))
                {
                    ISymbol sym = context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol;
                    if (sym != null && sym.Kind == SymbolKind.Property)
                    {
                        if (containingTypeName == sym.ContainingType.ToDisplayString())
                        {
                            context.ReportDiagnostic(identifier.CreateDiagnostic(Rule, identifier.ToString()));
                        }
                    }
                }
            }
        }
    }
}
