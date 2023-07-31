// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;

    public abstract class MakeTypesInternal<TSyntaxKind> : DiagnosticAnalyzer
        where TSyntaxKind : struct, Enum
    {
        internal const string RuleId = "CA1514";

        protected static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(MakeTypesInternalTitle)),
            CreateLocalizableResourceString(nameof(MakeTypesInternalMessage)),
            DiagnosticCategory.Maintainability,
            RuleLevel.Disabled,
            description: CreateLocalizableResourceString(nameof(MakeTypesInternalDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                if (compilation.Options.OutputKind is not (OutputKind.ConsoleApplication or OutputKind.WindowsApplication or OutputKind.WindowsRuntimeApplication))
                {
                    return;
                }

                compilationStartContext.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, TypeKinds);
                compilationStartContext.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration, EnumKind);
            });
        }

        protected abstract ImmutableArray<TSyntaxKind> TypeKinds { get; }

        protected abstract TSyntaxKind EnumKind { get; }

        protected abstract void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context);

        protected abstract void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    }
}