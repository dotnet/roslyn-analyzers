// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Collections.Generic;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{                   
    /// <summary>
    /// CA1064: Exceptions should be public
    /// </summary>
    public abstract class ExceptionsShouldBePublicAnalyzer : DiagnosticAnalyzer
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
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(AnalyzeException, SymbolKind.NamedType);
        }

        private void AnalyzeException(SymbolAnalysisContext context)
        {            
            var symbol = (INamedTypeSymbol)context.Symbol;

            // skip public symbols
            if (symbol.IsPublic()) return;

            // only report if base type matches 
            var baseType = symbol.BaseType;
            if (IsExceptionType(context, baseType))
            {                
                context.ReportDiagnostic(symbol.CreateDiagnostic(Rule));
            }
        }

        private bool IsExceptionType(SymbolAnalysisContext context, INamedTypeSymbol baseType)
        {
            return GetExceptionTypes(context).Contains(baseType);
        }

        private HashSet<INamedTypeSymbol> exceptionTypes;
        private HashSet<INamedTypeSymbol> GetExceptionTypes(SymbolAnalysisContext context)
        {
            if (exceptionTypes == null)
            {
                exceptionTypes = new HashSet<INamedTypeSymbol>() {
                    context.Compilation.GetTypeByMetadataName("System.Exception"),
                    context.Compilation.GetTypeByMetadataName("System.SystemException"),
                    context.Compilation.GetTypeByMetadataName("System.ApplicationException")
                };
            }
            return exceptionTypes;
        }
    }
}