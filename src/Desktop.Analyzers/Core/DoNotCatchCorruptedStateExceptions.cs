﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{
    public abstract class DoNotCatchCorruptedStateExceptionsAnalyzer<TLanguageKindEnum, TCatchClauseSyntax, TThrowStatementSyntax> : DiagnosticAnalyzer
        where TLanguageKindEnum : struct
        where TCatchClauseSyntax : SyntaxNode
        where TThrowStatementSyntax : SyntaxNode
    {
        internal const string RuleId = "CA2153";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotCatchCorruptedStateExceptions), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotCatchCorruptedStateExceptionsMessage), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotCatchCorruptedStateExceptionsDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Security,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "http://aka.ms/CA2153",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        protected abstract CodeBlockAnalyzer GetAnalyzer(CompilationSecurityTypes compilationTypes, ISymbol owningSymbol, SyntaxNode codeBlock);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Initialize the analyzer.
        /// </summary>
        /// <param name="analysisContext">Analyzer Context.</param>
        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            analysisContext.RegisterCompilationStartAction(
                compilationStartAnalysisContext =>
                {
                    var compilationTypes = new CompilationSecurityTypes(compilationStartAnalysisContext.Compilation);
                    if (compilationTypes.HandleProcessCorruptedStateExceptionsAttribute != null)
                    {
                        compilationStartAnalysisContext.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                        codeBlockStartContext =>
                        {
                            ISymbol owningSymbol = codeBlockStartContext.OwningSymbol;
                            if (owningSymbol.Kind == SymbolKind.Method)
                            {
                                var method = (IMethodSymbol)owningSymbol;

                                ImmutableArray<AttributeData> attributes = method.GetAttributes();
                                if (attributes.Any(attribute => attribute.AttributeClass.Equals(compilationTypes.HandleProcessCorruptedStateExceptionsAttribute)))
                                {
                                    CodeBlockAnalyzer analyzer = GetAnalyzer(compilationTypes, owningSymbol, codeBlockStartContext.CodeBlock);
                                    codeBlockStartContext.RegisterSyntaxNodeAction(analyzer.AnalyzeCatchClause, analyzer.CatchClauseKind);
                                    codeBlockStartContext.RegisterSyntaxNodeAction(analyzer.AnalyzeThrowStatement, analyzer.ThrowStatementKind);
                                    codeBlockStartContext.RegisterCodeBlockEndAction(analyzer.AnalyzeCodeBlockEnd);
                                }
                            }
                        });
                    }
                });
        }

        protected abstract class CodeBlockAnalyzer
        {
            private readonly ISymbol _owningSymbol;
            private readonly SyntaxNode _codeBlock;
            private readonly ConcurrentDictionary<TCatchClauseSyntax, ISymbol> _catchAllCatchClauses;

            public abstract TLanguageKindEnum CatchClauseKind { get; }
            public abstract TLanguageKindEnum ThrowStatementKind { get; }
            protected CompilationSecurityTypes TypesOfInterest { get; }
            protected abstract ISymbol GetExceptionTypeSymbolFromCatchClause(TCatchClauseSyntax catchNode, SemanticModel model);
            protected abstract bool IsThrowStatementWithNoArgument(TThrowStatementSyntax throwNode);
            protected abstract bool IsCatchClause(SyntaxNode node);
            protected abstract bool IslambdaExpression(SyntaxNode node);

            protected CodeBlockAnalyzer(CompilationSecurityTypes compilationTypes, ISymbol owningSymbol, SyntaxNode codeBlock)
            {
                _owningSymbol = owningSymbol;
                _codeBlock = codeBlock;
                _catchAllCatchClauses = new ConcurrentDictionary<TCatchClauseSyntax, ISymbol>();
                TypesOfInterest = compilationTypes;
            }

            public void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
            {
                var catchNode = (TCatchClauseSyntax)context.Node;
                ISymbol exceptionTypeSymbol = GetExceptionTypeSymbolFromCatchClause(catchNode, context.SemanticModel);

                if (IsCatchTypeTooGeneral(exceptionTypeSymbol))
                {
                    SyntaxNode parentNode = catchNode.Parent;
                    while (parentNode != _codeBlock)
                    {
                        // for now there doesn't seem to be any way to annotate lambdas with attributes
                        if (IslambdaExpression(parentNode))
                        {
                            return;
                        }
                        parentNode = parentNode.Parent;
                    }

                    _catchAllCatchClauses.AddOrUpdate(catchNode, exceptionTypeSymbol, (key, oldValue) => exceptionTypeSymbol);
                }
            }

            public void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
            {
                var throwNode = (TThrowStatementSyntax)context.Node;

                // throwNode is a throw statement with no argument, which is not allowed outside of a catch clause
                if (IsThrowStatementWithNoArgument(throwNode))
                {
                    TCatchClauseSyntax enclosingCatchClause = (TCatchClauseSyntax)throwNode.Ancestors().First(IsCatchClause);

                    ISymbol dummy;
                    _catchAllCatchClauses.TryRemove(enclosingCatchClause, out dummy);
                }
            }

            public void AnalyzeCodeBlockEnd(CodeBlockAnalysisContext context)
            {
                foreach (KeyValuePair<TCatchClauseSyntax, ISymbol> pair in _catchAllCatchClauses)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            pair.Key.GetLocation(),
                            _owningSymbol.ToDisplayString(),
                            pair.Value.ToDisplayString()));
                }
            }

            private bool IsCatchTypeTooGeneral(ISymbol catchTypeSym)
            {
                return catchTypeSym == null
                        || catchTypeSym == TypesOfInterest.SystemException
                        || catchTypeSym == TypesOfInterest.SystemSystemException
                        || catchTypeSym == TypesOfInterest.SystemObject;
            }
        }
    }
}
