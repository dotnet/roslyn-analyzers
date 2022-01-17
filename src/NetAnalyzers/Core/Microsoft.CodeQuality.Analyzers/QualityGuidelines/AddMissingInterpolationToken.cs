// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    public abstract class AbstractAddMissingInterpolationTokenAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2259";

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
#pragma warning disable RS1032 // Define diagnostic message correctly - the analyzer wants a period after the existing question mark.
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
#pragma warning restore RS1032 // Define diagnostic message correctly
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(context =>
            {
                var literalOperation = (ILiteralOperation)context.Operation;
                if (ShouldReport(literalOperation))
                {
                    context.ReportDiagnostic(literalOperation.CreateDiagnostic(Rule));
                }
            }, OperationKind.Literal);
        }

        private bool ShouldReport(ILiteralOperation operation)
        {
            if (operation.ConstantValue.HasValue &&
                operation.ConstantValue.Value is string &&
                ParseStringLiteralAsInterpolatedString(operation) is SyntaxNode dummyNode)
            {
                var annotation = new SyntaxAnnotation();
                dummyNode = dummyNode.WithAdditionalAnnotations(annotation);
                var root = operation.Syntax.SyntaxTree.GetRoot();
                root = root.ReplaceNode(operation.Syntax, dummyNode);
                dummyNode = root.GetAnnotatedNodes(annotation).Single();
                return TryGetSpeculativeSemanticModel(operation, dummyNode, out var model) &&
                    AreAllInterpolationsBindable(dummyNode, model);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the given interpolated string node has the following properties:
        /// <list type="number">
        /// <item><description>All parts are bindable.</description></item>
        /// <item><description>There is at least one non-literal expression.</description></item>
        /// </list>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private protected abstract bool AreAllInterpolationsBindable(SyntaxNode node, SemanticModel model);

        private protected abstract SyntaxNode? ParseStringLiteralAsInterpolatedString(ILiteralOperation operation);

        private protected abstract bool TryGetSpeculativeSemanticModel(ILiteralOperation operation, SyntaxNode dummyNode, out SemanticModel model);
    }
}
