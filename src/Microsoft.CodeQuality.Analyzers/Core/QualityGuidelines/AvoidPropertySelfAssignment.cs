// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA2245: Prevent properties from being assigned to themselves
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]

    public sealed class AvoidPropertySelfAssignment : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2245";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidPropertySelfAssignmentTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidPropertySelfAssignmentMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(operationContext =>
            {
                var assignmentOperation = (IAssignmentOperation)operationContext.Operation;

                if (!(assignmentOperation.Target is IPropertyReferenceOperation operationTarget))
                {
                    return;
                }

                if (!(assignmentOperation.Value is IPropertyReferenceOperation operationValue))
                {
                    return;
                }

                if (!Equals(operationTarget.Property, operationValue.Property))
                {
                    return;
                }

                if (operationTarget.Instance is IInstanceReferenceOperation targetInstanceReference &&
                    targetInstanceReference.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance &&
                    operationValue.Instance is IInstanceReferenceOperation valueInstanceReference &&
                    valueInstanceReference.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
                {
                    var diagnostic = Diagnostic.Create(Rule, valueInstanceReference.Syntax.GetLocation(), operationTarget.Property.Name);
                    operationContext.ReportDiagnostic(diagnostic);
                }
            }, OperationKind.SimpleAssignment);
        }
    }
}