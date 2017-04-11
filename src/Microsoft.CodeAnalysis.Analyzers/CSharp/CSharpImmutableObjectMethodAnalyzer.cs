﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpImmutableObjectMethodAnalyzer : DiagnosticAnalyzer
    {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationTitle), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationMessage), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationDescription), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));

        public static readonly DiagnosticDescriptor DoNotIgnoreReturnValueDiagnosticRule = new DiagnosticDescriptor(
            DiagnosticIds.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocation,
            s_localizableTitle,
            s_localizableMessage,
            AnalyzerDiagnosticCategory.AnalyzerCorrectness,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: true,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DoNotIgnoreReturnValueDiagnosticRule);

        private const string SolutionFullName = @"Microsoft.CodeAnalysis.Solution";
        private const string ProjectFullName = @"Microsoft.CodeAnalysis.Project";
        private const string DocumentFullName = @"Microsoft.CodeAnalysis.Document";
        private const string SyntaxNodeFullName = @"Microsoft.CodeAnalysis.SyntaxNode";
        private const string CompilationFullName = @"Microsoft.CodeAnalysis.Compilation";

        private static readonly ImmutableArray<string> s_immutableMethodNames = ImmutableArray.Create(
            "Add",
            "Remove",
            "Replace",
            "With");

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol solutionSymbol = compilationContext.Compilation.GetTypeByMetadataName(SolutionFullName);
                INamedTypeSymbol projectSymbol = compilationContext.Compilation.GetTypeByMetadataName(ProjectFullName);
                INamedTypeSymbol documentSymbol = compilationContext.Compilation.GetTypeByMetadataName(DocumentFullName);
                INamedTypeSymbol syntaxNodeSymbol = compilationContext.Compilation.GetTypeByMetadataName(SyntaxNodeFullName);
                INamedTypeSymbol compilationSymbol = compilationContext.Compilation.GetTypeByMetadataName(CompilationFullName);

                ImmutableArray<INamedTypeSymbol> immutableSymbols = ImmutableArray.Create(solutionSymbol, projectSymbol, documentSymbol, syntaxNodeSymbol, compilationSymbol);
                //Only register our node action if we can find the symbols for our immutable types
                if (immutableSymbols.Any(n => n == null))
                {
                    return;
                }

                compilationContext.RegisterSyntaxNodeAction(sc => AnalyzeInvocationForIgnoredReturnValue(sc, immutableSymbols), SyntaxKind.InvocationExpression);
            });
        }

        public void AnalyzeInvocationForIgnoredReturnValue(SyntaxNodeAnalysisContext context, ImmutableArray<INamedTypeSymbol> immutableTypeSymbols)
        {
            SemanticModel model = context.SemanticModel;
            var candidateInvocation = (InvocationExpressionSyntax)context.Node;

            //We're looking for invocations that are direct children of expression statements
            if (!(candidateInvocation.Parent.IsKind(SyntaxKind.ExpressionStatement)))
            {
                return;
            }

            //If we can't find the method symbol, quit
            var methodSymbol = model.GetSymbolInfo(candidateInvocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            //If the method doesn't start with something like "With" or "Replace", quit
            string methodName = methodSymbol.Name;
            if (!s_immutableMethodNames.Any(n => methodName.StartsWith(n, StringComparison.Ordinal)))
            {
                return;
            }

            //If we're not in one of the known immutable types, quit
            var parentType = methodSymbol.ReceiverType as INamedTypeSymbol;
            if (parentType == null)
            {
                return;
            }

            var baseTypesAndSelf = methodSymbol.ReceiverType.GetBaseTypes().ToList();
            baseTypesAndSelf.Add(parentType);

            if (!baseTypesAndSelf.Any(n => immutableTypeSymbols.Contains(n)))
            {
                return;
            }

            Location location = candidateInvocation.GetLocation();
            Diagnostic diagnostic = Diagnostic.Create(DoNotIgnoreReturnValueDiagnosticRule, location, methodSymbol.ReceiverType.Name, methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
