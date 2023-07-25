// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// An analyzer that recommends using the char overload in various string methods.
    /// IDs: CA1865, CA1866, CA1867
    /// </summary>
    public abstract class UseStringMethodCharOverloadWithSingleCharacters : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor SafeTransformationRule = DiagnosticDescriptorHelper.Create(
            "CA1865",
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersTitle)),
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor NoSpecifiedComparisonRule = DiagnosticDescriptorHelper.Create(
            "CA1866",
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersTitle)),
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor AnyOtherSpecifiedComparisonRule = DiagnosticDescriptorHelper.Create(
            "CA1867",
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersTitle)),
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.Disabled,
            CreateLocalizableResourceString(nameof(UseStringMethodCharOverloadWithSingleCharactersDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly ImmutableHashSet<string> _targetMethods = new[]
        {
             nameof(string.StartsWith),
             nameof(string.EndsWith),
             nameof(string.IndexOf),
             nameof(string.LastIndexOf),
        }.ToImmutableHashSet();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(SafeTransformationRule, NoSpecifiedComparisonRule, AnyOtherSpecifiedComparisonRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(CheckIfRuleIsApplicableAndRegister);
        }

        protected abstract SyntaxNode? GetArgumentList(SyntaxNode argumentNode);

        private void CheckIfRuleIsApplicableAndRegister(CompilationStartAnalysisContext context)
        {
            var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);
            var charType = context.Compilation.GetSpecialType(SpecialType.System_Char);

            if (stringType is null || charType is null)
                return;

            var stringTypeHasCharOverload = stringType.GetMembers(_targetMethods.First())
                .OfType<IMethodSymbol>()
                .Any(m =>
                {
                    return
                        m.Parameters.Length > 0 &&
                        m.Parameters[0].Type.SpecialType == SpecialType.System_Char;
                });

            if (!stringTypeHasCharOverload)
                return;

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            var invocationOperation = (IInvocationOperation)context.Operation;
            if (TryMatchTargetMethod(invocationOperation, out var method, out var isOrdinalComparison, out var isInvariantCulture) &&
                TryGetCharArgument(invocationOperation, out var stringArgument, out var c))
            {
                DiagnosticDescriptor? rule;

                // CA1865: Method(string, StringComparison.Ordinal) or Method(string, false/true, CultureInfo.InvariantCulture)
                if (isOrdinalComparison == true ||
                    (isInvariantCulture == true && c.IsASCII()))
                {
                    rule = SafeTransformationRule;
                }
                // CA1866: Method(string)
                else if (isOrdinalComparison == null)
                {
                    rule = NoSpecifiedComparisonRule;
                }
                // CA1867: Anything else
                else
                {
                    rule = AnyOtherSpecifiedComparisonRule;
                }

                var arg0 = $"string.{method}(char)";
                var arg1 = $"string.{method}(string)";
                var argumentList = GetArgumentList(stringArgument.Syntax);
                if (argumentList != null)
                {
                    context.ReportDiagnostic(argumentList.CreateDiagnostic(rule, new[] { arg0, arg1 }));
                }
            }

            bool TryMatchTargetMethod(
                IInvocationOperation invocationOperation,
                [MaybeNullWhen(false)] out string method,
                out bool? isOrdinalComparison,
                out bool? isInvariantCulture)
            {
                method = null;
                isOrdinalComparison = null;
                isInvariantCulture = null;

                var typeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                var cultureInfoType = typeProvider.GetOrCreateTypeByMetadataName("System.Globalization.CultureInfo");
                var stringComparisonType = typeProvider.GetOrCreateTypeByMetadataName("System.StringComparison");

                if (cultureInfoType == null || stringComparisonType == null)
                {
                    return false;
                }

                if (invocationOperation.TargetMethod is IMethodSymbol invokedMethod &&
                    invokedMethod.ContainingType.SpecialType == SpecialType.System_String &&
                    _targetMethods.Contains(invokedMethod.Name) &&
                    invokedMethod.Parameters.Length > 0 &&
                    invokedMethod.Parameters[0].Type.SpecialType == SpecialType.System_String)
                {
                    method = invokedMethod.Name;

                    foreach (var argument in invocationOperation.Arguments)
                    {
                        if (argument.Value.Type == null)
                            continue;

                        if (argument.Value.Type.Equals(cultureInfoType))
                        {
                            var invariantCultureSymbol = cultureInfoType.GetMembers(nameof(CultureInfo.InvariantCulture)).First();
                            isInvariantCulture =
                                argument.Value is IPropertyReferenceOperation propertyReferenceOperation &&
                                propertyReferenceOperation.Property == invariantCultureSymbol;
                        }
                        else if (argument.Value.Type.Equals(stringComparisonType))
                        {
                            var ordinalStringComparisonSymbol = stringComparisonType.GetMembers(nameof(StringComparison.Ordinal)).First();
                            isOrdinalComparison =
                                argument.Value is IFieldReferenceOperation fieldReferenceOperation &&
                                fieldReferenceOperation.Field == ordinalStringComparisonSymbol;
                        }
                    }

                    return true;
                }

                return false;
            }

            static bool TryGetCharArgument(
                IInvocationOperation invocationOperation,
                [MaybeNullWhen(false)] out IArgumentOperation stringArgument,
                [MaybeNullWhen(false)] out char c)
            {
                stringArgument = null;
                c = (char)0;

                var argument = invocationOperation.Arguments.GetArgumentForParameterAtIndex(0);
                if (argument.Value is ILiteralOperation literalOperation &&
                    literalOperation.ConstantValue.HasValue &&
                    literalOperation.ConstantValue.Value is string constantString &&
                    constantString.Length == 1)
                {
                    c = constantString[0];
                    stringArgument = argument;
                    return true;
                }

                return false;
            }
        }
    }
}
