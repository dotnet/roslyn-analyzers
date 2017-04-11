﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    // TODO: This should be updated to follow the flow of array creation expressions
    // that are eventually converted to and leave a given method as IEnumerable<T> once we have
    // the ability to do more thorough data-flow analysis in diagnostic analyzers.
    public abstract class SpecializedEnumerableCreationAnalyzer : DiagnosticAnalyzer
    {
        internal const string SpecializedCollectionsMetadataName = "Roslyn.Utilities.SpecializedCollections";
        internal const string IEnumerableMetadataName = "System.Collections.Generic.IEnumerable`1";
        internal const string LinqEnumerableMetadataName = "System.Linq.Enumerable";
        internal const string EmptyMethodName = "Empty";

        private static readonly LocalizableString s_localizableTitleUseEmptyEnumerable = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.UseSpecializedCollectionsEmptyEnumerableTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageUseEmptyEnumerable = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.UseSpecializedCollectionsEmptyEnumerableMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static readonly DiagnosticDescriptor UseEmptyEnumerableRule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.UseEmptyEnumerableRuleId,
            s_localizableTitleUseEmptyEnumerable,
            s_localizableMessageUseEmptyEnumerable,
            "Performance",
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly LocalizableString s_localizableTitleUseSingletonEnumerable = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.UseSpecializedCollectionsSingletonEnumerableTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageUseSingletonEnumerable = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.UseSpecializedCollectionsSingletonEnumerableMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static readonly DiagnosticDescriptor UseSingletonEnumerableRule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.UseSingletonEnumerableRuleId,
            s_localizableTitleUseSingletonEnumerable,
            s_localizableMessageUseSingletonEnumerable,
            "Performance",
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UseEmptyEnumerableRule, UseSingletonEnumerableRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Confirm if the analyzer is thread-safe and uncomment below.
            //analysisContext.EnableConcurrentExecution();

            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    INamedTypeSymbol specializedCollectionsSymbol = context.Compilation.GetTypeByMetadataName(SpecializedCollectionsMetadataName);
                    if (specializedCollectionsSymbol == null)
                    {
                        // TODO: In the future, we may want to run this analyzer even if the SpecializedCollections
                        // type cannot be found in this compilation. In some cases, we may want to add a reference
                        // to SpecializedCollections as a linked file or an assembly that contains it. With this
                        // check, we will not warn where SpecializedCollections is not yet referenced.
                        return;
                    }

                    INamedTypeSymbol genericEnumerableSymbol = context.Compilation.GetTypeByMetadataName(IEnumerableMetadataName);
                    if (genericEnumerableSymbol == null)
                    {
                        return;
                    }

                    INamedTypeSymbol linqEnumerableSymbol = context.Compilation.GetTypeByMetadataName(LinqEnumerableMetadataName);
                    if (linqEnumerableSymbol == null)
                    {
                        return;
                    }

                    var genericEmptyEnumerableSymbol = linqEnumerableSymbol.GetMembers(EmptyMethodName).FirstOrDefault() as IMethodSymbol;
                    if (genericEmptyEnumerableSymbol == null ||
                        genericEmptyEnumerableSymbol.Arity != 1 ||
                        genericEmptyEnumerableSymbol.Parameters.Length != 0)
                    {
                        return;
                    }

                    GetCodeBlockStartedAnalyzer(context, genericEnumerableSymbol, genericEmptyEnumerableSymbol);
                });
        }

        protected abstract void GetCodeBlockStartedAnalyzer(CompilationStartAnalysisContext context, INamedTypeSymbol genericEnumerableSymbol, IMethodSymbol genericEmptyEnumerableSymbol);

        protected abstract class AbstractCodeBlockStartedAnalyzer<TLanguageKindEnum> where TLanguageKindEnum : struct
        {
            private readonly INamedTypeSymbol _genericEnumerableSymbol;
            private readonly IMethodSymbol _genericEmptyEnumerableSymbol;

            public AbstractCodeBlockStartedAnalyzer(INamedTypeSymbol genericEnumerableSymbol, IMethodSymbol genericEmptyEnumerableSymbol)
            {
                _genericEnumerableSymbol = genericEnumerableSymbol;
                _genericEmptyEnumerableSymbol = genericEmptyEnumerableSymbol;
            }

            protected abstract void GetSyntaxAnalyzer(CodeBlockStartAnalysisContext<TLanguageKindEnum> context, INamedTypeSymbol genericEnumerableSymbol, IMethodSymbol genericEmptyEnumerableSymbol);

            public void Initialize(CodeBlockStartAnalysisContext<TLanguageKindEnum> context)
            {
                if (context.OwningSymbol is IMethodSymbol methodSymbol &&
    methodSymbol.ReturnType.OriginalDefinition == _genericEnumerableSymbol)
                {
                    GetSyntaxAnalyzer(context, _genericEnumerableSymbol, _genericEmptyEnumerableSymbol);
                }
            }
        }

        protected abstract class AbstractSyntaxAnalyzer
        {
            protected INamedTypeSymbol genericEnumerableSymbol;
            private readonly IMethodSymbol _genericEmptyEnumerableSymbol;

            public AbstractSyntaxAnalyzer(INamedTypeSymbol genericEnumerableSymbol, IMethodSymbol genericEmptyEnumerableSymbol)
            {
                this.genericEnumerableSymbol = genericEnumerableSymbol;
                _genericEmptyEnumerableSymbol = genericEmptyEnumerableSymbol;
            }

            public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UseEmptyEnumerableRule, UseSingletonEnumerableRule);

            protected bool ShouldAnalyzeArrayCreationExpression(SyntaxNode expression, SemanticModel semanticModel)
            {
                TypeInfo typeInfo = semanticModel.GetTypeInfo(expression);
                var arrayType = typeInfo.Type as IArrayTypeSymbol;

                return typeInfo.ConvertedType != null &&
                    typeInfo.ConvertedType.OriginalDefinition == genericEnumerableSymbol &&
                    arrayType != null &&
                    arrayType.Rank == 1;
            }

            // TODO: Remove the below suppression once the following Roslyn bug is fixed: https://github.com/dotnet/roslyn/issues/8884
#pragma warning disable CA1801
            protected void AnalyzeMemberAccessName(SyntaxNode name, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic)
#pragma warning restore CA1801
            {
                if (semanticModel.GetSymbolInfo(name).Symbol is IMethodSymbol methodSymbol &&
                    methodSymbol.OriginalDefinition == _genericEmptyEnumerableSymbol)
                {
                    addDiagnostic(Diagnostic.Create(UseEmptyEnumerableRule, name.Parent.GetLocation()));
                }
            }

            protected static void AnalyzeArrayLength(int length, SyntaxNode arrayCreationExpression, Action<Diagnostic> addDiagnostic)
            {
                if (length == 0)
                {
                    addDiagnostic(Diagnostic.Create(UseEmptyEnumerableRule, arrayCreationExpression.GetLocation()));
                }
                else if (length == 1)
                {
                    addDiagnostic(Diagnostic.Create(UseSingletonEnumerableRule, arrayCreationExpression.GetLocation()));
                }
            }
        }
    }
}
