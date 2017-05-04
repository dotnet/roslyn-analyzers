// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
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
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
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

                var analyzer = new PerCompilationAnalyzer(c.Compilation, @string, uri, GetInvocationExpression);

                // REVIEW: I need to do this thing because OperationAnalysisContext doesn't give me OwningSymbol
                c.RegisterOperationBlockStartActionInternal(sc =>
                {
                    sc.RegisterOperationActionInternal(oc => analyzer.Analyze(oc, sc.OwningSymbol), OperationKind.InvocationExpression);
                });
            });
        }

        protected abstract SyntaxNode GetInvocationExpression(SyntaxNode invocationNode);

        private struct PerCompilationAnalyzer
        {
            // this type will be created per compilation 
            // this is actually a bug - https://github.com/dotnet/roslyn-analyzers/issues/845
#pragma warning disable RS1008 
            private readonly Compilation _compilation;
            private readonly INamedTypeSymbol _string;
            private readonly INamedTypeSymbol _uri;
            private readonly Func<SyntaxNode, SyntaxNode> _expressionGetter;
#pragma warning restore RS1008

            public PerCompilationAnalyzer(
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
                var method = invocation.TargetMethod;

                // check basic stuff that FxCop checks. 
                if (method.IsFromMscorlib(_compilation))
                {
                    // Methods defined within mscorlib are excluded from this rule,
                    // since mscorlib cannot depend on System.Uri, which is defined 
                    // in System.dll
                    return;
                }

                if (method.GetResultantVisibility() != SymbolVisibility.Public)
                {
                    // only apply to methods that are exposed outside
                    return;
                }

                var node = _expressionGetter(context.Operation.Syntax);
                if (node == null)
                {
                    // we don't have right expression node to check overloads
                    return;
                }

                // REVIEW: why IOperation doesn't contain things like compilation and semantic model?
                //         it seems wierd that I need to do this to get thsoe.
                var model = _compilation.GetSemanticModel(context.Operation.Syntax.SyntaxTree);

                var stringParameters = method.Parameters.GetParametersOfType(_string);
                if (!stringParameters.Any())
                {
                    // no string parameter. not interested.
                    return;
                }

                // now do cheap string check whether those string parameter contains uri word list we are looking for.
                if (!stringParameters.ParameterNamesContainUriWordSubstring(context.CancellationToken))
                {
                    // no string parameter that contains what we are looking for.
                    return;
                }

                // now we make sure we actually have overloads that contains uri type parameter
                var overloads = model.GetMemberGroup(node, context.CancellationToken).OfType<IMethodSymbol>();
                if (!overloads.HasOverloadWithParameterOfType(method, _uri, context.CancellationToken))
                {
                    // no overload that contains uri as parameter
                    return;
                }

                // now we do more expensive word parsing to find exact parameter that contains url in parameter name
                var indicesSet = new HashSet<int>(method.GetParameterIndices(stringParameters.GetParametersThatContainUriWords(context.CancellationToken), context.CancellationToken));

                // now we search exact match. this is exactly same behavior as old FxCop
                foreach (IMethodSymbol overload in overloads)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (method.Equals(overload) || overload.Parameters.Length != method.Parameters.Length)
                    {
                        // either itself, or signature is not same
                        continue;
                    }

                    if (!method.ParameterTypesAreSame(overload, Enumerable.Range(0, method.Parameters.Length).Where(i => !indicesSet.Contains(i)), context.CancellationToken))
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
                        if (!method.ParameterTypesAreSame(overload, indicesSet.Where(i => i != index), context.CancellationToken))
                        {
                            continue;
                        }

                        // okay all other type match. check the main one
                        if (overload.Parameters[index].Type?.Equals(_uri) == true)
                        {
                            context.ReportDiagnostic(
                                node.CreateDiagnostic(
                                    Rule,
                                    owningSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                    overload.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                    method.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));

                            // we no longer interested in this overload. there can be only 1 match
                            break;
                        }
                    }
                }
            }
        }
    }
}