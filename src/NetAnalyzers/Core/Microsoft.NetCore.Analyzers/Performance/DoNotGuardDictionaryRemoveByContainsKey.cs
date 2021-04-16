// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotGuardDictionaryRemoveByContainsKey : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1839";

        private static readonly LocalizableString s_localizableTitle =
            new LocalizableResourceString(nameof(Resx.DoNotGuardDictionaryRemoveByContainsKeyTitle), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_localizableMessage =
            new LocalizableResourceString(nameof(Resx.DoNotGuardDictionaryRemoveByContainsKeyMessage), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_localizableDescription =
            new LocalizableResourceString(nameof(Resx.DoNotGuardDictionaryRemoveByContainsKeyDescription), Resx.ResourceManager, typeof(Resx));

        public const string AdditionalDocumentLocationInfoSeparator = ";;";

        public const string ConditionalOperation = nameof(ConditionalOperation);
        public const string ChildStatementOperation = nameof(ChildStatementOperation);

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false,
            additionalCustomTags: WellKnownDiagnosticTags.Unnecessary);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;

            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericDictionary2, out var dictionaryType))
                return;

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.Conditional);

            static void AnalyzeOperation(OperationAnalysisContext context)
            {
                var conditionalOperation = (IConditionalOperation)context.Operation;

                IInvocationOperation? invocationOperation = null;

                switch (conditionalOperation.Condition)
                {
                    case IInvocationOperation:
                        invocationOperation = (IInvocationOperation)conditionalOperation.Condition;
                        break;
                    case IUnaryOperation unaryOperation when unaryOperation.OperatorKind == UnaryOperatorKind.Not:
                        if (unaryOperation.Operand is IInvocationOperation operand)
                            invocationOperation = operand;
                        break;
                    default:
                        return;
                }

                if (invocationOperation!.TargetMethod.Name != "ContainsKey")
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
                            if (childInvocationOperation.TargetMethod.Name == "Remove")
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
                                                                                   .FirstOrDefault(op => op.TargetMethod.Name == "Remove");

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
