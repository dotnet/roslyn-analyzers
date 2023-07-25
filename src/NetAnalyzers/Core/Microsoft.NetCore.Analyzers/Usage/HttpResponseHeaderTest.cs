// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Usage
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA2262: <inheritdoc cref="HttpResponseHeaderTest"/>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class HttpResponseHeaderTest : DiagnosticAnalyzer
    {
        private const string PropertyName = "MaxResponseHeadersLength";
        private const int MaximumAlertLimit = 128;
        internal const string RuleId = "CA2262";

        internal static readonly DiagnosticDescriptor EnsureMaxResponseHeaderLengthRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectlyTitle)),
            CreateLocalizableResourceString(nameof(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectlyMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: CreateLocalizableResourceString(nameof(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectlyDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(EnsureMaxResponseHeaderLengthRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var propertySymbol = context.Compilation
                                    .GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemNetHttpHttpClientHandler)
                                    ?.GetMembers(PropertyName)
                                    .FirstOrDefault();

                if (propertySymbol is null)
                {
                    return;
                }

                context.RegisterOperationAction(context => AnalyzeSimpleAssignmentOperationAndCreateDiagnostic(context, propertySymbol), OperationKind.SimpleAssignment);
            });
        }

        private static void AnalyzeSimpleAssignmentOperationAndCreateDiagnostic(OperationAnalysisContext context, ISymbol propertySymbol)
        {
            var assignmentOperation = (ISimpleAssignmentOperation)context.Operation;

            if (!IsValidPropertyAssignmentOperation(assignmentOperation, propertySymbol))
            {
                return;
            }

            if (assignmentOperation.Value is null || !assignmentOperation.Value.ConstantValue.HasValue || assignmentOperation.Value.ConstantValue.Value is not int propertyValue)
            {
                return;
            }

            if (propertyValue > MaximumAlertLimit)
            {
                context.ReportDiagnostic(context.Operation.CreateDiagnostic(EnsureMaxResponseHeaderLengthRule, propertyValue));
            }
        }

        private static bool IsValidPropertyAssignmentOperation(ISimpleAssignmentOperation operation, ISymbol propertySymbol)
        {
            if (operation.Target is not IPropertyReferenceOperation propertyReferenceOperation)
            {
                return false;
            }

            if (!propertyReferenceOperation.Member.Equals(propertySymbol))
            {
                return false;
            }

            return operation.Value is IFieldReferenceOperation or ILiteralOperation;
        }
    }
}