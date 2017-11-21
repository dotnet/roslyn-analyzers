﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.NetFramework.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetFramework.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCatchCorruptedStateExceptionsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2153";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotCatchCorruptedStateExceptions), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotCatchCorruptedStateExceptionsMessage), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotCatchCorruptedStateExceptionsDescription), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Security,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
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

                    var method = (IMethodSymbol)operationBlockAnalysisContext.OwningSymbol;

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

            public ISet<ICatchClauseOperation> CatchAllCatchClausesWithoutEmptyThrow { get; } = new HashSet<ICatchClauseOperation>();

            public EmptyThrowInsideCatchAllWalker(CompilationSecurityTypes compilationTypes)
            {
                _compilationTypes = compilationTypes;
            }

            public override void VisitAnonymousFunction(IAnonymousFunctionOperation operation)
            {
                // for now there doesn't seem to be any way to annotate lambdas with attributes
            }

            public override void VisitCatchClause(ICatchClauseOperation operation)
            {
                _seenEmptyThrowInCatchClauses.Push(false);

                Visit(operation.Filter);
                Visit(operation.Handler);

                bool seenEmptyThrow = _seenEmptyThrowInCatchClauses.Pop();

                if (IsCaughtTypeTooGeneral(operation.ExceptionType) && !seenEmptyThrow)
                {
                    CatchAllCatchClausesWithoutEmptyThrow.Add(operation);
                }
            }

            public override void VisitThrow(IThrowOperation operation)
            {
                if (operation.Exception == null && _seenEmptyThrowInCatchClauses.Count > 0 && !_seenEmptyThrowInCatchClauses.Peek())
                {
                    _seenEmptyThrowInCatchClauses.Pop();
                    _seenEmptyThrowInCatchClauses.Push(true);
                }

                base.VisitThrow(operation);
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