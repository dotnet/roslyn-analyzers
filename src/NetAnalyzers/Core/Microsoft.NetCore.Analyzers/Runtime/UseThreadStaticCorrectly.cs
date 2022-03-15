// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseThreadStaticCorrectly : DiagnosticAnalyzer
    {
        internal const string ThreadStaticNonStaticFieldRuleId = "CA2259";
        internal const string ThreadStaticInitializedInlineRuleId = "CA2019";

        // [ThreadStatic]
        // private object t_nonStaticField;
        internal static readonly DiagnosticDescriptor ThreadStaticOnNonStaticFieldRule = DiagnosticDescriptorHelper.Create(ThreadStaticNonStaticFieldRuleId,
            CreateLocalizableResourceString(nameof(ThreadStaticOnNonStaticFieldTitle)),
            CreateLocalizableResourceString(nameof(ThreadStaticOnNonStaticFieldMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            CreateLocalizableResourceString(nameof(ThreadStaticOnNonStaticFieldDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        // [ThreadStatic]
        // private static object t_field = new object();
        internal static readonly DiagnosticDescriptor ThreadStaticInitializedInlineRule = DiagnosticDescriptorHelper.Create(ThreadStaticInitializedInlineRuleId,
            CreateLocalizableResourceString(nameof(ThreadStaticInitializedInlineTitle)),
            CreateLocalizableResourceString(nameof(ThreadStaticInitializedInlineMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(ThreadStaticInitializedInlineDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ThreadStaticOnNonStaticFieldRule, ThreadStaticInitializedInlineRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(context =>
            {
                // Ensure ThreadStatic exists
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadStaticAttribute, out var threadStaticAttributeType))
                {
                    return;
                }

                // Warn on any [ThreadStatic] instance field.
                context.RegisterSymbolAction(context =>
                {
                    var field = (IFieldSymbol)context.Symbol;
                    if (field.HasAttribute(threadStaticAttributeType) && !field.IsStatic)
                    {
                        context.ReportDiagnostic(field.CreateDiagnostic(ThreadStaticOnNonStaticFieldRule));
                    }
                }, SymbolKind.Field);

                // Warn on any [ThreadStatic] field inline initialization.
                context.RegisterOperationAction(context =>
                {
                    var fieldInit = (IFieldInitializerOperation)context.Operation;
                    foreach (IFieldSymbol field in fieldInit.InitializedFields)
                    {
                        if (field.IsStatic && field.HasAttribute(threadStaticAttributeType))
                        {
                            context.ReportDiagnostic(fieldInit.CreateDiagnostic(ThreadStaticInitializedInlineRule));
                        }
                    }

                }, OperationKind.FieldInitializer);
            });
        }
    }
}