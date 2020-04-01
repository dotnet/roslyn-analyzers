// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// CA1831, CA1832, CA1833: Use AsSpan or AsMemory instead of Range-based indexers when appropriate.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseAsSpanInsteadOfRangeIndexerAnalyzer : DiagnosticAnalyzer
    {
        internal const string StringRuleId = "CA1831";
        internal const string ArrayReadOnlyRuleId = "CA1832";
        internal const string ArrayReadWriteRuleId = "CA1833";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseAsSpanInsteadOfRangeIndexerTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseAsSpanInsteadOfRangeIndexerMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableStringDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseAsSpanInsteadOfStringRangeIndexerDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableArrayReadDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseAsSpanReadOnlyInsteadOfArrayRangeIndexerDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableArrayWriteDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseAsSpanInsteadOfArrayRangeIndexerDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor StringRule = DiagnosticDescriptorHelper.Create(
            StringRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableStringDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ArrayReadOnlyRule = DiagnosticDescriptorHelper.Create(
            ArrayReadOnlyRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            description: s_localizableArrayReadDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ArrayReadWriteRule = DiagnosticDescriptorHelper.Create(
            ArrayReadWriteRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            description: s_localizableArrayWriteDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(StringRule, ArrayReadOnlyRule, ArrayReadWriteRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;
            var stringType = compilation.GetSpecialType(SpecialType.System_String);
            var span = compilation.GetOrCreateTypeByMetadataName("System.Span`1");
            var readOnlySpan = compilation.GetOrCreateTypeByMetadataName("System.ReadOnlySpan`1");
            var memory = compilation.GetOrCreateTypeByMetadataName("System.Memory`1");
            var readOnlyMemory = compilation.GetOrCreateTypeByMetadataName("System.ReadOnlyMemory`1");
            var range = compilation.GetOrCreateTypeByMetadataName("System.Range");

            if (stringType == null || span == null || readOnlySpan == null || memory == null || readOnlyMemory == null || range == null)
            {
                return;
            }

            var spanTypes = ImmutableArray.Create(span, readOnlySpan);
            var readOnlyTypes = ImmutableArray.Create(readOnlySpan, readOnlyMemory);
            var targetTypes = ImmutableArray.Create(span, readOnlySpan, memory, readOnlyMemory);

            context.RegisterOperationAction(
                operationContext =>
                {
                    IOperation indexerArgument;
                    ITypeSymbol containingType;

                    if (operationContext.Operation is IPropertyReferenceOperation propertyReference)
                    {
                        if (!propertyReference.Property.IsIndexer || propertyReference.Arguments.Length != 1)
                        {
                            return;
                        }

                        indexerArgument = propertyReference.Arguments[0].Value;
                        containingType = propertyReference.Property.ContainingType;
                    }
                    else if (operationContext.Operation is IArrayElementReferenceOperation elementReference)
                    {
                        if (elementReference.Indices.Length != 1)
                        {
                            return;
                        }

                        indexerArgument = elementReference.Indices[0];
                        containingType = elementReference.ArrayReference.Type;
                    }
                    else
                    {
                        return;
                    }

                    if (!(indexerArgument is IRangeOperation rangeOperation))
                    {
                        return;
                    }

                    if (!(operationContext.Operation.Parent is IConversionOperation conversionOperation))
                    {
                        return;
                    }

                    if (!conversionOperation.IsImplicit)
                    {
                        return;
                    }

                    var targetType = conversionOperation.Type.OriginalDefinition;

                    if (!targetTypes.Contains(targetType))
                    {
                        return;
                    }

                    DiagnosticDescriptor rule;

                    if (stringType.Equals(containingType))
                    {
                        rule = StringRule;
                    }
                    else if (containingType.TypeKind == TypeKind.Array)
                    {
                        rule = readOnlyTypes.Contains(targetType) ? ArrayReadOnlyRule : ArrayReadWriteRule;
                    }
                    else
                    {
                        return;
                    }

                    operationContext.ReportDiagnostic(
                        operationContext.Operation.CreateDiagnostic(
                            rule,
                            spanTypes.Contains(targetType) ? nameof(MemoryExtensions.AsSpan) : nameof(MemoryExtensions.AsMemory),
                            rangeOperation.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                            containingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                },
                OperationKind.PropertyReference,
                OperationKind.ArrayElementReference);
        }
    }
}
