// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA1099: Prevent properties from being assigned to themselves
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]

    public sealed class AvoidPropertySelfAssignment : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1099";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidPropertySelfAssignmentTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidPropertySelfAssignmentMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidPropertySelfAssignmentDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
            description: s_localizableDescription,
            helpLinkUri: "");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // You would need to register an operation action for OperationKind.SimpleAssignment
            analysisContext.RegisterOperationAction(operationContext =>
            {
                //referencing the same property symbol and whose Instance is an IInstanceReferenceOperation whose ReferenceKind is InstanceReferenceKind.ContainingTypeInstance
                var assignmentOperation = (IAssignmentOperation)operationContext.Operation;

                var operationTarget = (IPropertyReferenceOperation)assignmentOperation?.Target;
                if (operationTarget == null)
                {
                    return;
                }

                var operationValue = (IPropertyReferenceOperation)assignmentOperation.Value;
                if (operationValue == null)
                {
                    return;
                }

                if (!Equals(operationTarget.Property, operationValue.Property))
                {
                    return;
                }

                var reference = (InstanceReferenceKind)operationTarget.Property.RefKind;
                if (reference != InstanceReferenceKind.ContainingTypeInstance)
                {
                    return;
                }

                Diagnostic diagnostic = Diagnostic.Create(Rule, operationContext.Operation.Syntax.GetLocation(), operationTarget.Property.Name);
                operationContext.ReportDiagnostic(diagnostic);

            }, OperationKind.SimpleAssignment);
        }
    }
}