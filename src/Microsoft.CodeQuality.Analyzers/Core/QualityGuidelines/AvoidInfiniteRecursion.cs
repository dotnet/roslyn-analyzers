// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidInfiniteRecursion : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2008";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidInfiniteRecursionTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidInfiniteRecursionMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
            DiagnosticCategory.Reliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationBlockStartAction(operationBlockStartContext =>
            {
                if (operationBlockStartContext.OwningSymbol is IMethodSymbol methodSymbol &&
                    methodSymbol.MethodKind == MethodKind.PropertySet)
                {
                    operationBlockStartContext.RegisterOperationAction(operationContext =>
                    {
                        var assignmentOperation = (IAssignmentOperation)operationContext.Operation;

                        if (assignmentOperation.Target is IPropertyReferenceOperation operationTarget &&
                            operationTarget.Member.Equals(methodSymbol.AssociatedSymbol))
                        {
                            operationContext.ReportDiagnostic(Diagnostic.Create(Rule, assignmentOperation.Syntax.GetLocation(), operationTarget.Property.Name));
                        }
                    }, OperationKind.SimpleAssignment);
                }
            });
        }
    }
}
