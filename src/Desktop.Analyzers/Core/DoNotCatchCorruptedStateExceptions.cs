// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;

namespace Desktop.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCatchCorruptedStateExceptionsAnalyzer : DiagnosticAnalyzer
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            analysisContext.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var compilationTypes = new CompilationSecurityTypes(compilationStartAnalysisContext.Compilation);
                if (compilationTypes.HandleProcessCorruptedStateExceptionsAttribute == null)
                {
                    return;
                }

                compilationStartAnalysisContext.RegisterOperationBlockAction(operationBlockAnalysisContext =>
                {
                    if (operationBlockAnalysisContext.OwningSymbol.Kind != SymbolKind.Method)
                    {
                        return;
                    }

                    var method = (IMethodSymbol) operationBlockAnalysisContext.OwningSymbol;

                    if (!ContainsHandleProcessCorruptedStateExceptionsAttribute(method, compilationTypes))
                    {
                        return;
                    }

                    foreach (var operation in operationBlockAnalysisContext.OperationBlocks)
                    {
                        var walker = new EmptyThrowInsideCatchAllWalker(compilationTypes);
                        walker.Visit(operation);

                        foreach (var catchClause in walker.CatchAllCatchClausesWithoutEmptyThrow)
                        {
                            operationBlockAnalysisContext.ReportDiagnostic(catchClause.Syntax.CreateDiagnostic(Rule,
                                method.ToDisplayString()));
                        }
                    }
                });
            });
        }

        private bool ContainsHandleProcessCorruptedStateExceptionsAttribute(IMethodSymbol method, CompilationSecurityTypes compilationTypes)
        {
            ImmutableArray<AttributeData> attributes = method.GetAttributes();
            return attributes.Any(
                attribute => attribute.AttributeClass.Equals(compilationTypes.HandleProcessCorruptedStateExceptionsAttribute));
        }

        /// <summary>
        /// Walks an IOperation tree to find catch-all blocks that contain no "throw;" statements.
        /// </summary>
        private class EmptyThrowInsideCatchAllWalker : OperationWalker
        {
            private readonly CompilationSecurityTypes _compilationTypes;
            private readonly Stack<bool> _seenEmptyThrowInCatchClauses = new Stack<bool>();

            public ISet<ICatchClause> CatchAllCatchClausesWithoutEmptyThrow { get; } = new HashSet<ICatchClause>();

            public EmptyThrowInsideCatchAllWalker(CompilationSecurityTypes compilationTypes)
            {
                _compilationTypes = compilationTypes;
            }

            public override void VisitCatch(ICatchClause operation)
            {
                _seenEmptyThrowInCatchClauses.Push(false);

                Visit(operation.Filter);
                Visit(operation.Handler);

                bool seenEmptyThrow = _seenEmptyThrowInCatchClauses.Pop();

                if (IsCaughtTypeTooGeneral(operation.CaughtType) && !seenEmptyThrow)
                {
                    // TODO: Abort in case parent is a lambda.
                    // for now there doesn't seem to be any way to annotate lambdas with attributes

                    CatchAllCatchClausesWithoutEmptyThrow.Add(operation);
                }
            }

            public override void VisitThrowStatement(IThrowStatement operation)
            {
                if (operation.ThrownObject == null && _seenEmptyThrowInCatchClauses.Count > 0 && !_seenEmptyThrowInCatchClauses.Peek())
                {
                    _seenEmptyThrowInCatchClauses.Pop();
                    _seenEmptyThrowInCatchClauses.Push(true);
                }

                base.VisitThrowStatement(operation);
            }

            private bool IsCaughtTypeTooGeneral(ITypeSymbol caughtType)
            {
                return caughtType == null ||
                       caughtType == _compilationTypes.SystemException ||
                       caughtType == _compilationTypes.SystemSystemException ||
                       caughtType == _compilationTypes.SystemObject;
            }
        }
    }
}