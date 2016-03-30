// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;
using System.Collections.Generic;
using System.Linq;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2219: Do not raise exceptions in exception clauses
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotRaiseExceptionsInExceptionClausesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2219";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageFinally = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesMessageFinally), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFilter = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesMessageFilter), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFault = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesMessageFault), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseExceptionsInExceptionClausesDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private const string helpLinkUrl = "https://msdn.microsoft.com/en-us/library/bb386041.aspx";

        internal static DiagnosticDescriptor FinallyRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFinally,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: helpLinkUrl,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FinallyRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationStartContext =>
            {
                Compilation compilation = compilationStartContext.Compilation;
                INamedTypeSymbol exceptionType = WellKnownTypes.Exception(compilation);
                if (exceptionType == null)
                {
                    return;
                }

                compilationStartContext.RegisterOperationAction(operationContext =>
                {
                    var tryStatement = operationContext.Operation as ITryStatement;

                    var throwStatements = tryStatement.FinallyHandler.Statements.OfType<IThrowStatement>();

                    foreach (var throwStatement in throwStatements)
                    {
                        operationContext.ReportDiagnostic(throwStatement.Syntax.CreateDiagnostic(FinallyRule));
                    }
                }, OperationKind.TryStatement);
            });
        }
    }
}