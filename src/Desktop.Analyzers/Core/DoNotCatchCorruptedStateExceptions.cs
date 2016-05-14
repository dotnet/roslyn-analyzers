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
                        var walker = new CatchInsideBlockWalker(compilationTypes);
                        walker.Visit(operation);

                        foreach (var catchClause in walker.CatchAllCatchClauses)
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
        /// Walks an IOperation tree to find catch blocks that contain no empty throw statements.
        /// </summary>
        private class CatchInsideBlockWalker : OperationWalker
        {
            private readonly CompilationSecurityTypes _compilationTypes;

            public IList<ICatchClause> CatchAllCatchClauses { get; } = new List<ICatchClause>();

            public CatchInsideBlockWalker(CompilationSecurityTypes compilationTypes)
            {
                _compilationTypes = compilationTypes;
            }

            public override void VisitCatch(ICatchClause operation)
            {
                Visit(operation.Filter);

                if (IsCaughtTypeTooGeneral(operation.CaughtType))
                {
                    // TODO: Abort in case parent is a lambda.
                    // for now there doesn't seem to be any way to annotate lambdas with attributes

                    var walter = new ThrowInsideCatchWalker();
                    walter.Visit(operation.Handler);

                    if (!walter.EmptyThrowStatements.Any())
                    {
                        CatchAllCatchClauses.Add(operation);
                    }
                }

                Visit(operation.Handler);
            }

            private bool IsCaughtTypeTooGeneral(ITypeSymbol caughtType)
            {
                return caughtType == null ||
                       caughtType == _compilationTypes.SystemException ||
                       caughtType == _compilationTypes.SystemSystemException ||
                       caughtType == _compilationTypes.SystemObject;
            }
        }

        /// <summary>
        /// Walks an IOperation tree to find empty throw statements.
        /// </summary>
        private class ThrowInsideCatchWalker : OperationWalker
        {
            private int _catchBlockNestingDepth;

            public List<IThrowStatement> EmptyThrowStatements { get; } = new List<IThrowStatement>();

            public override void VisitCatch(ICatchClause operation)
            {
                _catchBlockNestingDepth++;

                Visit(operation.Filter);
                Visit(operation.Handler);

                _catchBlockNestingDepth--;
            }

            public override void VisitThrowStatement(IThrowStatement operation)
            {
                if (_catchBlockNestingDepth == 0 && operation.ThrownObject == null)
                {
                    EmptyThrowStatements.Add(operation);
                }

                base.VisitThrowStatement(operation);
            }
        }
    }
}