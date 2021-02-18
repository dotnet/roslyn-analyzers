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
            new LocalizableResourceString(Resx.DoNotGuardDictionaryRemoveByContainsKeyTitle, Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_localizableMessage =
            new LocalizableResourceString(Resx.DoNotGuardDictionaryRemoveByContainsKeyMessage, Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_localizableDescription =
            new LocalizableResourceString(Resx.DoNotGuardDictionaryRemoveByContainsKeyDescription, Resx.ResourceManager, typeof(Resx));

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

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);

            static void AnalyzeOperation(OperationAnalysisContext context)
            {
                var invocationOperation = (IInvocationOperation)context.Operation;

                if (invocationOperation.TargetMethod.Name != "ContainsKey")
                    return;

                if (invocationOperation.Parent is not IConditionalOperation parentConditionalOperation)
                    return;

                // we only want to report this diagnostic if the Contains/Remove pair is all there is

                if (parentConditionalOperation.WhenFalse == null &&
                    parentConditionalOperation.WhenTrue.Children.HasExactly(1))
                {
                    var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                    properties[ConditionalOperation] = CreateLocationInfo(parentConditionalOperation.Syntax);

                    switch (parentConditionalOperation.WhenTrue.Children.First())
                    {
                        case IInvocationOperation childInvocationOperation:
                            if (childInvocationOperation.TargetMethod.Name == "Remove")
                            {
                                properties[ChildStatementOperation] = CreateLocationInfo(childInvocationOperation.Syntax.Parent);

                                context.ReportDiagnostic(invocationOperation.CreateDiagnostic(Rule, properties.ToImmutable()));
                            }

                            break;
                        case IExpressionStatementOperation childStatementOperation:
                            // if the if statement contains a block, only proceed if that block contains a single statement

                            if (childStatementOperation.Children.HasExactly(1) &&
                                childStatementOperation.Children.First() is IInvocationOperation nestedInvocationOperation &&
                                nestedInvocationOperation.TargetMethod.Name == "Remove")
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
