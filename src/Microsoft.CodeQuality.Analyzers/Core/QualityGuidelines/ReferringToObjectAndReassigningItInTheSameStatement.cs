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
    /// ReferenceChanged: Prevent objects from being referenced in statements where they are reassigned
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]

    public sealed class ReferringToObjectAndReassigningItInTheSameStatement : DiagnosticAnalyzer
    {
        internal const string RuleId = "ReferenceChanged";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.ReferringToObjectAndReassigningItInTheSameStatementTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.ReferringToObjectAndReassigningItInTheSameStatementMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        }

        private void AnalyzeAssignment(OperationAnalysisContext context)
        {
            var assignmentOperation = (ISimpleAssignmentOperation)context.Operation;

            // Check if there are more then one assignment in a statement
            if (!(assignmentOperation.Target is IMemberReferenceOperation operationTarget))
            {
                return;
            }

            // This analyzer makes sense only for reference type objects
            if (operationTarget.Instance?.Type.IsValueType == true)
            {
                return;
            }

            // Search for object equal to operationTarget.Instance further in assignment chain
            bool isViolationFound = false;
            string violatingObjectName;
            if (operationTarget.Instance is ILocalReferenceOperation localInstance)
            {
                violatingObjectName = localInstance.Local.Name;
                isViolationFound = AnalyzeMemberAssignment(assignmentOperation, localInstance, (a, b) => a.Local == b.Local);
            }
            else if (operationTarget.Instance is IMemberReferenceOperation memberInstance)
            {
                violatingObjectName = memberInstance.Member.Name;
                isViolationFound = AnalyzeMemberAssignment(assignmentOperation, memberInstance, (a, b) => a.Member == b.Member);
            }
            else
            {
                return;
            }

            if (isViolationFound)
            {
                var diagnostic = Diagnostic.Create(Rule, operationTarget.Syntax.GetLocation(), violatingObjectName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool AnalyzeMemberAssignment<T>(ISimpleAssignmentOperation assignmentOperation, T instance, Func<T, T, bool> equalityComparer) where T : class, IOperation
        {
            // Check every simple assignments target in a statement for equality to `instance`
            while (assignmentOperation.Value != null && assignmentOperation.Value.Kind == OperationKind.SimpleAssignment)
            {
                assignmentOperation = (ISimpleAssignmentOperation)assignmentOperation.Value;

                var operationValue = assignmentOperation.Target as T;
                if (equalityComparer(instance, operationValue))
                {
                    return true;
                }
            }
            return false;
        }
    }
}