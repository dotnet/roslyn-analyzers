// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotGuardDictionaryRemoveByContainsKey : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1839";

        private static readonly LocalizableString s_localizableTitle = CreateResource(nameof(Resx.DoNotGuardDictionaryRemoveByContainsKeyTitle));
        private static readonly LocalizableString s_localizableMessage = CreateResource(nameof(Resx.DoNotGuardDictionaryRemoveByContainsKeyMessage));
        private static readonly LocalizableString s_localizableDescription = CreateResource(nameof(Resx.DoNotGuardDictionaryRemoveByContainsKeyDescription));

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
            isDataflowRule: false);

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

            context.RegisterOperationAction(AnalyzeNamedType, OperationKind.Invocation);

            static void AnalyzeNamedType(OperationAnalysisContext context)
            {
                if (context.Operation is not CodeAnalysis.Operations.IInvocationOperation invocationOperation)
                    return;

                if (invocationOperation.TargetMethod.Name != "ContainsKey")
                    return;

                if (invocationOperation.Parent is not CodeAnalysis.Operations.IConditionalOperation parentConditionalOperation)
                    return;

                // we only want to report this diagnostic if the Contains/Remove pair is all there is

                if (parentConditionalOperation.WhenFalse == null &&
                    parentConditionalOperation.WhenTrue.Children.HasExactly(1))
                {
                    var properties = ImmutableDictionary.CreateBuilder<string, string>();
                    properties[ConditionalOperation] = CreateLocationInfo(parentConditionalOperation.Syntax);

                    switch (parentConditionalOperation.WhenTrue.Children.First())
                    {
                        case CodeAnalysis.Operations.IInvocationOperation childInvocationOperation:
                            if (childInvocationOperation.TargetMethod.Name == "Remove")
                            {
                                properties[ChildStatementOperation] = CreateLocationInfo(childInvocationOperation.Syntax.Parent);

                                context.ReportDiagnostic(Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation(), properties.ToImmutable()));
                            }

                            break;
                        case CodeAnalysis.Operations.IExpressionStatementOperation childStatementOperation:
                            // if the if statement contains a block, only proceed if that block contains a single statement

                            if (childStatementOperation.Children.HasExactly(1) &&
                                childStatementOperation.Children.First() is CodeAnalysis.Operations.IInvocationOperation nestedInvocationOperation &&
                                nestedInvocationOperation.TargetMethod.Name == "Remove")
                            {
                                properties[ChildStatementOperation] = CreateLocationInfo(nestedInvocationOperation.Syntax.Parent);

                                context.ReportDiagnostic(Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation(), properties.ToImmutable()));
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static LocalizableString CreateResource(string resourceName)
            => new LocalizableResourceString(resourceName, Resx.ResourceManager, typeof(Resx));

        private static string CreateLocationInfo(SyntaxNode syntax)
        {
            // see DiagnosticDescriptorCreationAnalyzer

            var location = syntax.GetLocation();
            var span = location.SourceSpan;

            return $"{span.Start}{AdditionalDocumentLocationInfoSeparator}{span.Length}";
        }
    }
}
