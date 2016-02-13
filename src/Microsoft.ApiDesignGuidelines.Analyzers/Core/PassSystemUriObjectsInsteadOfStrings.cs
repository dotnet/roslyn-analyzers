// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2234: Pass system uri objects instead of strings
    /// </summary>
    public abstract class PassSystemUriObjectsInsteadOfStringsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2234";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PassSystemUriObjectsInsteadOfStringsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PassSystemUriObjectsInsteadOfStringsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PassSystemUriObjectsInsteadOfStringsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182360.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // this is stateless analyzer, can run concurrently
            analysisContext.EnableConcurrentExecution();

            // this has no meaning on running on generated code which user can't control
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(c =>
            {
                var analyzer = new Analyzer(c.Compilation, GetInvocationExpression);
                c.RegisterOperationAction(analyzer.Analyze, OperationKind.InvocationExpression);
            });
        }

        protected abstract SyntaxNode GetInvocationExpression(SyntaxNode invocationNode);

        private struct Analyzer
        {
            private static readonly string[] uriWords = new string[] { "uri", "urn", "url" };

#pragma warning disable RS1008 // this type will be created per compilation
            private readonly Compilation _compilation;
            private readonly INamedTypeSymbol _string;
            private readonly INamedTypeSymbol _uri;
            private readonly Func<SyntaxNode, SyntaxNode> _expressionGetter;
#pragma warning restore RS1008

            public Analyzer(Compilation compilation, Func<SyntaxNode, SyntaxNode> expressionGetter)
            {
                _compilation = compilation;
                _string = WellKnownTypes.String(compilation);
                _uri = WellKnownTypes.Uri(compilation);

                _expressionGetter = expressionGetter;
            }

            public void Analyze(OperationAnalysisContext context)
            {
                if (context.Operation.IsInvalid)
                {
                    // not interested in invalid expression
                    return;
                }

                var invocation = (IInvocationExpression)context.Operation;
                var method = invocation.TargetMethod;

                var node = context.Operation.Syntax;
                if (node == null || node.SyntaxTree == null)
                {
                    // this is not from user code. we are not interested.
                    return;
                }

                var expressionNode = _expressionGetter(node);
                if (expressionNode == null)
                {
                    // we don't have right expression node to check overloads
                    return;
                }

                // REVIEW: why IOperation doesn't contain things like compilation and semantic model?
                //         it seems wierd that I need to do this to get thsoe.
                var model = _compilation.GetSemanticModel(node.SyntaxTree);

                if (!CheckCandidate(model, method, expressionNode, context.CancellationToken))
                {
                    // not a method we are interested in.
                    return;
                }

                context.ReportDiagnostic(expressionNode.CreateDiagnostic(Rule));
            }

            private bool CheckCandidate(SemanticModel model, IMethodSymbol method, SyntaxNode node, CancellationToken cancellationToken)
            {
                // due to limitation of using "this" in lambda in struct
                var stringType = _string;
                var stringParameters = method.Parameters.Where(p => p.Type?.Equals(stringType) == true);
                if (!stringParameters.Any())
                {
                    // no string parameter. not interested.
                    return false;
                }

                // now do cheap string check whether those string parameter contains uri word list we are looking for.
                if (!CheckStringParametersQuick(stringParameters, cancellationToken))
                {
                    // no string parameter that contains what we are looking for.
                    return false;
                }

                // now we make sure we actually have overloads that contains uri type parameter
                if (!CheckOverloads(model, node, cancellationToken))
                {
                    // no overload that contains uri as parameter
                    return false;
                }

                // now we do more expensive word parsing to see whether this is really right one to report it.
                return CheckStringParametersSlow(stringParameters, cancellationToken);
            }

            private bool CheckOverloads(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
            {
                var uriType = _uri;
                foreach (var overload in model.GetMemberGroup(node, cancellationToken).OfType<IMethodSymbol>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (overload.Parameters.Any(p => p.Type?.Equals(uriType) == true))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool CheckStringParametersQuick(IEnumerable<IParameterSymbol> stringParameters, CancellationToken cancellationToken)
            {
                foreach (var parameter in stringParameters)
                {
                    if (CheckStringParameter(parameter, cancellationToken))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool CheckStringParameter(IParameterSymbol parameter, CancellationToken cancellationToken)
            {
                foreach (var word in uriWords)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (parameter.Name?.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool CheckStringParametersSlow(IEnumerable<IParameterSymbol> stringParameters, CancellationToken cancellationToken)
            {
                foreach (var parameter in stringParameters)
                {
                    if (parameter.Name == null ||
                        !CheckStringParameter(parameter, cancellationToken))
                    {
                        // quick check failed
                        continue;
                    }

                    string word;
                    var parser = new WordParser(parameter.Name, WordParserOptions.SplitCompoundWords);
                    while ((word = parser.NextWord()) != null)
                    {
                        foreach (var uriWord in uriWords)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            // we found uri word in parameter words.
                            if (string.Equals(uriWord, word, StringComparison.OrdinalIgnoreCase))
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
}