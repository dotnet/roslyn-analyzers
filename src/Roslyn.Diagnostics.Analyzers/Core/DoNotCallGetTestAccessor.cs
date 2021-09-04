﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Roslyn.Diagnostics.Analyzers
{
    using static RoslynDiagnosticsAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallGetTestAccessor : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor DoNotCallGetTestAccessorRule = new(
            RoslynDiagnosticIds.DoNotCallGetTestAccessorRuleId,
            CreateLocalizableResourceString(nameof(DoNotCallGetTestAccessorTitle)),
            CreateLocalizableResourceString(nameof(DoNotCallGetTestAccessorMessage)),
            DiagnosticCategory.RoslynDiagnosticsMaintainability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(DoNotCallGetTestAccessorDescription)),
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotCallGetTestAccessorRule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterOperationBlockStartAction(context =>
            {
                if (!string.Equals(context.OwningSymbol.Name, TestAccessorHelper.GetTestAccessorMethodName, StringComparison.Ordinal)
                    && !string.Equals(context.OwningSymbol.ContainingType?.Name, TestAccessorHelper.TestAccessorTypeName, StringComparison.Ordinal))
                {
                    context.RegisterOperationAction(HandleMemberReference, OperationKinds.MemberReference);
                    context.RegisterOperationAction(HandleInvocation, OperationKind.Invocation);
                    context.RegisterOperationAction(HandleObjectCreation, OperationKind.ObjectCreation);
                }
            });
        }

        private void HandleMemberReference(OperationAnalysisContext context)
        {
            var memberReference = (IMemberReferenceOperation)context.Operation;
            if (string.Equals(memberReference.Member.ContainingType?.Name, TestAccessorHelper.TestAccessorTypeName, StringComparison.Ordinal))
            {
                context.ReportDiagnostic(memberReference.Syntax.CreateDiagnostic(DoNotCallGetTestAccessorRule));
            }
        }

        private void HandleInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            if (invocation.TargetMethod.Name.Equals(TestAccessorHelper.GetTestAccessorMethodName, StringComparison.Ordinal))
            {
                // Calling a type's GetTestAccessor method
                context.ReportDiagnostic(invocation.Syntax.CreateDiagnostic(DoNotCallGetTestAccessorRule));
            }
            else if (string.Equals(invocation.TargetMethod.ContainingType?.Name, TestAccessorHelper.TestAccessorTypeName, StringComparison.Ordinal))
            {
                // Calling a static method of a TestAccessor type
                context.ReportDiagnostic(invocation.Syntax.CreateDiagnostic(DoNotCallGetTestAccessorRule));
            }
        }

        private void HandleObjectCreation(OperationAnalysisContext context)
        {
            var objectCreation = (IObjectCreationOperation)context.Operation;
            if (objectCreation.Type.Name.Equals(TestAccessorHelper.TestAccessorTypeName, StringComparison.Ordinal))
            {
                // Directly constructing a TestAccessor instance
                context.ReportDiagnostic(objectCreation.Syntax.CreateDiagnostic(DoNotCallGetTestAccessorRule));
            }
        }
    }
}
