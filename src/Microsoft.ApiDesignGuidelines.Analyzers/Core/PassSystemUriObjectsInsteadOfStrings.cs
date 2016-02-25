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
                INamedTypeSymbol @string = WellKnownTypes.String(c.Compilation);
                INamedTypeSymbol uri = WellKnownTypes.Uri(c.Compilation);
                if (@string == null || uri == null)
                {
                    // we don't have required types
                    return;
                }

                var analyzer = new Analyzer(c.Compilation, @string, uri, GetInvocationExpression);

                // REVIEW: I need to do this thing because OperationAnalysisContext doesn't give me OwningSymbol
                c.RegisterOperationBlockStartAction(sc =>
                {
                    sc.RegisterOperationAction(oc => analyzer.Analyze(oc, sc.OwningSymbol), OperationKind.InvocationExpression);
                });
            });
        }

        protected abstract SyntaxNode GetInvocationExpression(SyntaxNode invocationNode);

        private struct Analyzer
        {
            private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
                SymbolDisplayDelegateStyle.NameAndParameters,
                SymbolDisplayExtensionMethodStyle.InstanceMethod,
                SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeOptionalBrackets,
                SymbolDisplayPropertyStyle.NameOnly,
                SymbolDisplayLocalOptions.IncludeType,
                SymbolDisplayKindOptions.None,
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            private static readonly ImmutableHashSet<string> s_uriWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "uri", "urn", "url");

            // this type will be created per compilation 
            // this is actually a bug - https://github.com/dotnet/roslyn-analyzers/issues/845
#pragma warning disable RS1008 
            private readonly Compilation _compilation;
            private readonly INamedTypeSymbol _string;
            private readonly INamedTypeSymbol _uri;
            private readonly Func<SyntaxNode, SyntaxNode> _expressionGetter;
#pragma warning restore RS1008

            public Analyzer(
                Compilation compilation,
                INamedTypeSymbol @string,
                INamedTypeSymbol uri,
                Func<SyntaxNode, SyntaxNode> expressionGetter)
            {
                _compilation = compilation;
                _string = @string;
                _uri = uri;
                _expressionGetter = expressionGetter;
            }

            public void Analyze(OperationAnalysisContext context, ISymbol owningSymbol)
            {
                if (context.Operation.IsInvalid)
                {
                    // not interested in invalid expression
                    return;
                }

                var invocation = (IInvocationExpression)context.Operation;
                IMethodSymbol method = invocation.TargetMethod;

                SyntaxNode node = _expressionGetter(context.Operation.Syntax);
                if (node == null)
                {
                    // we don't have right expression node to check overloads
                    return;
                }

                // REVIEW: why IOperation doesn't contain things like compilation and semantic model?
                //         it seems wierd that I need to do this to get thsoe.
                SemanticModel model = _compilation.GetSemanticModel(context.Operation.Syntax.SyntaxTree);

                // due to limitation of using "this" in lambda in struct
                INamedTypeSymbol stringType = _string;
                IEnumerable<IParameterSymbol> stringParameters = method.Parameters.Where(p => p.Type?.Equals(stringType) == true);
                if (!stringParameters.Any())
                {
                    // no string parameter. not interested.
                    return;
                }

                // now do cheap string check whether those string parameter contains uri word list we are looking for.
                if (!CheckStringParametersContainUriWords(stringParameters, context.CancellationToken))
                {
                    // no string parameter that contains what we are looking for.
                    return;
                }

                // now we make sure we actually have overloads that contains uri type parameter
                if (!CheckOverloadsContainUriParameters(model, method, node, context.CancellationToken))
                {
                    // no overload that contains uri as parameter
                    return;
                }

                // now we do more expensive word parsing to find exact parameter that contains url in parameter name
                var indicesSet = new HashSet<int>(GetParameterIndices(method, GetStringParametersThatContainsUriWords(stringParameters, context.CancellationToken), context.CancellationToken));

                // now we search exact match. this is exactly same behavior as old FxCop
                foreach (IMethodSymbol overload in model.GetMemberGroup(node, context.CancellationToken).OfType<IMethodSymbol>())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (method.Equals(overload) || overload.Parameters.Length != method.Parameters.Length)
                    {
                        // either itself, or signature is not same
                        continue;
                    }

                    if (!CheckParameterTypes(method, overload, Enumerable.Range(0, method.Parameters.Length).Where(i => !indicesSet.Contains(i)), context.CancellationToken))
                    {
                        // check whether remaining parameters match existing types, otherwise, we are not interested
                        continue;
                    }

                    // original FxCop implementation doesnt account for case where original method call contains 
                    // 2+ string uri parameters that has overload with matching uri parameters. original implementation works
                    // when there is exactly 1 parameter having matching uri overload. this implementation follow that.
                    foreach (int index in indicesSet)
                    {
                        // check other string uri parameters matches original type
                        if (!CheckParameterTypes(method, overload, indicesSet.Where(i => i != index), context.CancellationToken))
                        {
                            continue;
                        }

                        // okay all other type match. check the main one
                        if (overload.Parameters[index].Type?.Equals(_uri) == true)
                        {
                            context.ReportDiagnostic(node.CreateDiagnostic(Rule, owningSymbol.ToDisplayString(s_symbolDisplayFormat), overload.ToDisplayString(s_symbolDisplayFormat), method.ToDisplayString(s_symbolDisplayFormat)));

                            // we no longer interested in this overload. there can be only 1 match
                            break;
                        }
                    }
                }
            }

            private bool CheckParameterTypes(IMethodSymbol method, IMethodSymbol overload, IEnumerable<int> parameterIndices, CancellationToken cancellationToken)
            {
                foreach (int index in parameterIndices)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // this doesnt account for type conversion but FxCop implementation seems doesnt either
                    // so this should match FxCop implementation.
                    if (overload.Parameters[index].Type?.Equals(method.Parameters[index].Type) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            private IEnumerable<int> GetParameterIndices(IMethodSymbol method, IEnumerable<IParameterSymbol> parameters, CancellationToken cancellationToken)
            {
                var set = new HashSet<IParameterSymbol>(parameters);
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (set.Contains(method.Parameters[i]))
                    {
                        yield return i;
                    }
                }
            }

            private bool CheckOverloadsContainUriParameters(SemanticModel model, IMethodSymbol method, SyntaxNode node, CancellationToken cancellationToken)
            {
                INamedTypeSymbol uriType = _uri;
                foreach (IMethodSymbol overload in model.GetMemberGroup(node, cancellationToken).OfType<IMethodSymbol>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (method.Equals(overload))
                    {
                        continue;
                    }

                    if (overload.Parameters.Any(p => p.Type?.Equals(uriType) == true))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool CheckStringParametersContainUriWords(IEnumerable<IParameterSymbol> stringParameters, CancellationToken cancellationToken)
            {
                foreach (IParameterSymbol parameter in stringParameters)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (CheckStringParameterContainsUriWords(parameter, cancellationToken))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool CheckStringParameterContainsUriWords(IParameterSymbol parameter, CancellationToken cancellationToken)
            {
                foreach (string word in s_uriWords)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (parameter.Name?.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            private IEnumerable<IParameterSymbol> GetStringParametersThatContainsUriWords(IEnumerable<IParameterSymbol> stringParameters, CancellationToken cancellationToken)
            {
                foreach (IParameterSymbol parameter in stringParameters)
                {
                    if (parameter.Name == null || !CheckStringParameterContainsUriWords(parameter, cancellationToken))
                    {
                        // quick check failed
                        continue;
                    }

                    string word;
                    var parser = new WordParser(parameter.Name, WordParserOptions.SplitCompoundWords);
                    while ((word = parser.NextWord()) != null)
                    {
                        if (s_uriWords.Contains(word))
                        {
                            yield return parameter;
                            break;
                        }
                    }
                }
            }
        }
    }
}