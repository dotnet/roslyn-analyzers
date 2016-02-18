// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1064: Exceptions should be public
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ExceptionsShouldBePublicAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1064";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ExceptionsShouldBePublicTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ExceptionsShouldBePublicMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ExceptionsShouldBePublicDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb264484.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly List<string> s_exceptionTypeNames = new List<string>()
        {
            "System.Exception",
            "System.SystemException",
            "System.ApplicationException"
        };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext csContext)
        {
            // Get named type symbols for targetted exception types
            ImmutableHashSet<INamedTypeSymbol> exceptionTypes = s_exceptionTypeNames
                .Select(name => csContext.Compilation.GetTypeByMetadataName(name))
                .Where(t => t != null)
                .ToImmutableHashSet();

            if (!exceptionTypes.IsEmpty)
            {
                // register symbol action for named types
                csContext.RegisterSymbolAction(saContext =>
                {
                    var symbol = (INamedTypeSymbol)saContext.Symbol;

                    // skip public symbols
                    if (symbol.IsPublic()) return;

                    // only report if base type matches 
                    if (exceptionTypes.Contains(symbol.BaseType))
                    {
                        saContext.ReportDiagnostic(symbol.CreateDiagnostic(Rule));
                    }
                },
                SymbolKind.NamedType);
            }
        }
    }
}