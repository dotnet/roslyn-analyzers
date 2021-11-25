// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA2248: Prevent invocation of non-pure methods from readonly value-type fields
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidNonPureMethodCallOnReadonlyValueSymbol : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2248";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidNonPureMethodCallOnReadonlyValueSymbolTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidNonPureMethodCallOnReadonlyValueSymbolMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidNonPureMethodCallOnReadonlyValueSymbolDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

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

            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            context.RegisterOperationAction(AnalyzeReference, OperationKind.MethodReference);
        }

        private void AnalyzeReference(OperationAnalysisContext context)
        {
            var referenceOperation = (IMethodReferenceOperation)context.Operation;

            bool isViolationFound = AnalyzeMethod(referenceOperation.Instance, referenceOperation.Method);
            if (!isViolationFound)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule,
                referenceOperation.Syntax.GetLocation(),
                GetMemberFullName(((IMemberReferenceOperation)referenceOperation.Instance).Member),
                referenceOperation.Method.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var callOperation = (IInvocationOperation)context.Operation;

            bool isViolationFound = AnalyzeMethod(callOperation.Instance, callOperation.TargetMethod);
            if (!isViolationFound)
            {
                return;
            }

            // Check we are not directly in the constructor's body
            OperationKind[] functions =
            {
                OperationKind.ConstructorBody,
                OperationKind.MethodBody,
                OperationKind.LocalFunction,
                OperationKind.AnonymousFunction,
                OperationKind.FlowAnonymousFunction
            };
            IOperation parent = callOperation.Parent;
            while (parent != null && !functions.Contains(parent.Kind))
            {
                parent = parent.Parent;
            }
            if (parent != null && parent.Kind == OperationKind.ConstructorBody)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule,
                callOperation.Syntax.GetLocation(),
                GetMemberFullName(((IMemberReferenceOperation)callOperation.Instance).Member),
                callOperation.TargetMethod.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static string GetMemberFullName(ISymbol member)
        {
            return $"{member.ContainingType.Name}.{member.Name}";
        }

        private static bool AnalyzeMethod(IOperation instance, IMethodSymbol method)
        {
            // This analyzer makes sense only for value type objects
            if (instance?.Type?.IsReferenceType != false)
            {
                return false;
            }

            // Check if member is readonly
            bool isMemberReadonly = instance switch
            {
                IFieldReferenceOperation fieldInstance =>
                    fieldInstance.Field.IsReadOnly,
                IPropertyReferenceOperation propertyInstance =>
                    propertyInstance.Property.IsReadOnly,
                _ => false,
            };
            if (!isMemberReadonly)
            {
                return false;
            }

            // Check if method if non-static and non-readonly
            if (method.IsStatic
                || method.IsReadOnly
                || method.ContainingType.IsReadOnly

                // Accounting for all attributes named PureAttribute since there are custom ones
                || method.GetAttributes().Any(a => a.AttributeClass.Name == nameof(PureAttribute)))
            {
                return false;
            }

            return true;
        }
    }
}
