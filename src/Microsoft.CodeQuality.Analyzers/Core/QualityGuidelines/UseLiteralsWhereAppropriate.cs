// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA1802: Use literals where appropriate
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseLiteralsWhereAppropriateAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1802";
        internal const string Uri = @"https://msdn.microsoft.com/en-us/library/ms182280.aspx";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateMessageDefault), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageEmptyString = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateMessageEmptyString), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor EmptyStringRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageEmptyString,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, EmptyStringRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(saContext =>
            {
                var fieldInitializer = saContext.Operation as IFieldInitializerOperation;

                // Diagnostics are reported on the last initialized field to retain the previous FxCop behavior
                var lastField = fieldInitializer?.InitializedFields.LastOrDefault();
                var fieldInitializerValue = fieldInitializer?.Value;
                if (fieldInitializerValue == null || lastField.IsConst ||
                    lastField.IsExternallyVisible() || !lastField.IsStatic ||
                    !lastField.IsReadOnly || !fieldInitializerValue.ConstantValue.HasValue)
                {
                    return;
                }

                var initializerValue = fieldInitializerValue.ConstantValue.Value;

                // Though null is const we don't fire the diagnostic to be FxCop Compact
                if (initializerValue != null)
                {
                    if (fieldInitializerValue.Type?.SpecialType == SpecialType.System_String &&
                        ((string)initializerValue).Length == 0)
                    {
                        saContext.ReportDiagnostic(lastField.CreateDiagnostic(EmptyStringRule, lastField.Name));
                        return;
                    }

                    saContext.ReportDiagnostic(lastField.CreateDiagnostic(DefaultRule, lastField.Name));
                }
            },
            OperationKind.FieldInitializer);
        }
    }
}