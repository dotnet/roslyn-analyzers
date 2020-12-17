﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeQuality.Analyzers;

namespace Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA2246: Prevent objects from being referenced in statements where they are reassigned
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAssigningSymbolAndItsMemberInSameStatement : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2246";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AssigningSymbolAndItsMemberInSameStatementTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AssigningSymbolAndItsMemberInSameStatementMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AssigningSymbolAndItsMemberInSameStatementDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        }

        private void AnalyzeAssignment(OperationAnalysisContext context)
        {
            var assignmentOperation = (ISimpleAssignmentOperation)context.Operation;

            // Check if there are more then one assignment in a statement
            if (assignmentOperation.Target is not IMemberReferenceOperation operationTarget)
            {
                return;
            }

            // This analyzer makes sense only for reference type objects
            if (operationTarget.Instance?.Type?.IsReferenceType != true)
            {
                return;
            }

            bool isViolationFound = operationTarget.Instance switch
            {
                ILocalReferenceOperation localInstance =>
                    AnalyzeAssignmentToMember(assignmentOperation, localInstance, (a, b) => a.Local.Equals(b.Local)),
                IMemberReferenceOperation memberInstance =>
                    AnalyzeAssignmentToMember(assignmentOperation, memberInstance, (a, b) => a.Member.Equals(b.Member) && a.Instance?.Syntax.ToString() == b.Instance?.Syntax.ToString()),
                IParameterReferenceOperation parameterInstance =>
                    AnalyzeAssignmentToMember(assignmentOperation, parameterInstance, (a, b) => a.Parameter.Equals(b.Parameter)),
                _ => false,
            };

            if (isViolationFound)
            {
                var diagnostic = operationTarget.CreateDiagnostic(Rule, operationTarget.Instance.Syntax, operationTarget.Member.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool AnalyzeAssignmentToMember<T>(ISimpleAssignmentOperation assignmentOperation, T instance, Func<T, T, bool> equalityComparer) where T : class, IOperation
        {
            // Check every simple assignments target in a statement for equality to `instance`
            while (assignmentOperation.Value.Kind == OperationKind.SimpleAssignment)
            {
                assignmentOperation = (ISimpleAssignmentOperation)assignmentOperation.Value;

                if (assignmentOperation.Target is T operationValue &&
                    equalityComparer(instance, operationValue))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
