// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;
using System.Linq;

namespace Microsoft.QualityGuidelines.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class RemoveEmptyFinalizersAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1821";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizers), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizers), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizersDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Performance,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: true,
                                                                         description: s_localizableDescription,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/bb264476.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol conditionalAttributeSymbol = WellKnownTypes.ConditionalAttribute(compilationContext.Compilation);

                compilationContext.RegisterOperationBlockAction(context =>
                {
                    var method = context.OwningSymbol as IMethodSymbol;
                    if (method == null)
                    {
                        return;
                    }

                    if (!method.IsFinalizer())
                    {
                        return;
                    }

                    if (IsEmptyFinalizer(context.OperationBlocks, conditionalAttributeSymbol))
                    {
                        context.ReportDiagnostic(context.OwningSymbol.CreateDiagnostic(Rule));
                    }
                });
            });
        }

        private bool IsEmptyFinalizer(ImmutableArray<IOperation> operationBlocks, ISymbol conditionalAttributeSymbol)
        {
            if (operationBlocks != null && operationBlocks.Length == 1)
            {
                var block = operationBlocks[0] as IBlockStatement;
                if (block == null)
                {
                    return true;
                }

                // Empty method
                if (block.Statements.Length == 0)
                {
                    return true;
                }

                if (block.Statements.Length == 1)
                {
                    IStatement statement = block.Statements[0];

                    // Just a throw statement.
                    if (statement.Kind == OperationKind.ThrowStatement)
                    {
                        return true;
                    }

                    if (statement.Kind == OperationKind.ExpressionStatement &&
                        ((IExpressionStatement)statement).Expression.Kind == OperationKind.InvocationExpression)
                    {
                        var invocation = ((IExpressionStatement)statement).Expression as IInvocationExpression;
                        IMethodSymbol method = invocation.TargetMethod;

                        if (method.GetAttributes().Any(n => n.AttributeClass.Equals(conditionalAttributeSymbol)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
