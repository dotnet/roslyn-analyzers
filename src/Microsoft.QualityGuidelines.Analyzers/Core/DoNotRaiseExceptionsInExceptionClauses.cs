// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA2219: Do not raise exceptions in exception clauses
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotRaiseExceptionsInExceptionClausesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2219";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesMessageFinally), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        private const string helpLinkUrl = "https://msdn.microsoft.com/en-us/library/bb386041.aspx";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: helpLinkUrl,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterOperationBlockAction(operationBlockContext =>
            {
                foreach (var block in operationBlockContext.OperationBlocks)
                {
                    var walker = new ThrowInsideFinallyWalker();
                    walker.Visit(block);

                    foreach (var throwStatement in walker.ThrowStatements)
                    {
                        operationBlockContext.ReportDiagnostic(throwStatement.Syntax.CreateDiagnostic(Rule));
                    }
                }
            });
        }

        /// <summary>
        /// Walks a IOperation tree to find throw statements inside finally blocks.
        /// </summary>
        private class ThrowInsideFinallyWalker : OperationWalker
        {
            private int _finallyBlockNestingDepth;

            public List<IThrowStatement> ThrowStatements { get; private set; } = new List<IThrowStatement>();

            public override void VisitTryStatement(ITryStatement operation)
            {
                Visit(operation.Body);
                foreach (var catchClause in operation.Catches)
                {
                    Visit(catchClause);
                }

                _finallyBlockNestingDepth++;
                Visit(operation.FinallyHandler);
                _finallyBlockNestingDepth--;
            }

            public override void VisitThrowStatement(IThrowStatement operation)
            {
                if (_finallyBlockNestingDepth > 0)
                {
                    ThrowStatements.Add(operation);
                }

                base.VisitThrowStatement(operation);
            }
        }
    }
}