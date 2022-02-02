// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotGuardDictionaryRemoveByContainsKey : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1851";

        public const string AdditionalDocumentLocationInfoSeparator = ";;";
        public static readonly string[] AdditionalDocumentLocationInfoSeparatorArray = new[] { AdditionalDocumentLocationInfoSeparator };
        public const string ConditionalOperation = nameof(ConditionalOperation);
        public const string ChildStatementOperation = nameof(ChildStatementOperation);

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotGuardDictionaryRemoveByContainsKeyTitle)),
            CreateLocalizableResourceString(nameof(DoNotGuardDictionaryRemoveByContainsKeyMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(DoNotGuardDictionaryRemoveByContainsKeyDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false,
            additionalCustomTags: WellKnownDiagnosticTags.Unnecessary);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!TryGetDictionaryTypeAndContainsKeyeMethod(context.Compilation, out var dictionaryType, out var containsKeyMethod))
            {
                return;
            }

            context.RegisterOperationAction(context => AnalyzeOperation(context, dictionaryType, containsKeyMethod), OperationKind.Conditional);

            static void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol dictionaryType, IMethodSymbol containsKeyMethod)
            {
                var conditionalOperation = (IConditionalOperation)context.Operation;

                IInvocationOperation? invocationOperation = null;

                switch (conditionalOperation.Condition)
                {
                    case IInvocationOperation iOperation:
                        invocationOperation = iOperation;
                        break;
                    case IUnaryOperation unaryOperation when unaryOperation.OperatorKind == UnaryOperatorKind.Not:
                        if (unaryOperation.Operand is IInvocationOperation operand)
                            invocationOperation = operand;
                        break;
                    default:
                        return;
                }

                if (invocationOperation == null || !invocationOperation.TargetMethod.OriginalDefinition.Equals(containsKeyMethod, SymbolEqualityComparer.Default))
                {
                    return;
                }

                if (conditionalOperation.WhenTrue.Children.Any())
                {
                    var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                    properties[ConditionalOperation] = CreateLocationInfo(conditionalOperation.Syntax);

                    switch (conditionalOperation.WhenTrue.Children.First())
                    {
                        case IInvocationOperation childInvocationOperation:
                            if (childInvocationOperation.TargetMethod.Name == "Remove" &&
                                childInvocationOperation.TargetMethod.OriginalDefinition.ContainingType.Equals(dictionaryType, SymbolEqualityComparer.Default))
                            {
                                properties[ChildStatementOperation] = CreateLocationInfo(childInvocationOperation.Syntax.Parent);

                                context.ReportDiagnostic(invocationOperation.CreateDiagnostic(Rule, properties.ToImmutable()));
                            }

                            break;
                        case IExpressionStatementOperation childStatementOperation:
                            /*
                             * If the if statement contains a block, only proceed if one of the methods calls Remove.
                             * However, a fixer is only offered if there is a single method in the block.
                             */

                            var nestedInvocationOperation = childStatementOperation.Children.OfType<IInvocationOperation>()
                                                               .FirstOrDefault(op => op.TargetMethod.Name == "Remove" &&
                                                                op.TargetMethod.OriginalDefinition.ContainingType.Equals(dictionaryType, SymbolEqualityComparer.Default));

                            if (nestedInvocationOperation != null)
                            {
                                properties[ChildStatementOperation] = CreateLocationInfo(nestedInvocationOperation.Syntax.Parent);

                                context.ReportDiagnostic(invocationOperation.CreateDiagnostic(Rule, properties.ToImmutable()));
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            static bool TryGetDictionaryTypeAndContainsKeyeMethod(Compilation compilation, [NotNullWhen(true)] out INamedTypeSymbol? dictionaryType, [NotNullWhen(true)] out IMethodSymbol? containsKeyMethod)
            {
                containsKeyMethod = null;

                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericDictionary2, out dictionaryType))
                {
                    return false;
                }

                foreach (var m in dictionaryType.GetMembers().OfType<IMethodSymbol>())
                {
                    if (m.ReturnType.SpecialType == SpecialType.System_Boolean &&
                        m.Parameters.Length == 1 &&
                        m.Name == "ContainsKey" &&
                        m.Parameters[0].Name == "key")
                    {
                        containsKeyMethod = m;
                        return true;
                    }
                }

                return false;
            }
        }

        private static string CreateLocationInfo(SyntaxNode syntax)
        {
            // see DiagnosticDescriptorCreationAnalyzer

            var location = syntax.GetLocation();
            var span = location.SourceSpan;

            return $"{span.Start}{AdditionalDocumentLocationInfoSeparator}{span.Length}";
        }
    }
}
