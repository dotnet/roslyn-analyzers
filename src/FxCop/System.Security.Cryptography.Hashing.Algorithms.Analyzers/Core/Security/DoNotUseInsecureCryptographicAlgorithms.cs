// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using System.Security.Cryptography.Hashing.Algorithms.Analyzers.Common;


namespace System.Security.Cryptography.Hashing.Algorithms.Analyzers
{
    public abstract class DoNotUseInsecureCryptographicAlgorithmsAnalyzer : DiagnosticAnalyzer
    {

        internal const string DoNotUseMD5RuleId = "CA5350";
        internal const string DoNotUseSHA1RuleId = "CA5354";

        private static readonly LocalizableString s_localizableDoNotUseMD5Title = DiagnosticHelpers.GetLocalizableResourceString(nameof(SystemSecurityCryptographyHashingAlgorithmsAnalyzersResources.DoNotUseMD5));
        private static readonly LocalizableString s_localizableDoNotUseMD5Description = DiagnosticHelpers.GetLocalizableResourceString(nameof(SystemSecurityCryptographyHashingAlgorithmsAnalyzersResources.DoNotUseMD5Description));
        private static readonly LocalizableString s_localizableDoNotUseSHA1Title = DiagnosticHelpers.GetLocalizableResourceString(nameof(SystemSecurityCryptographyHashingAlgorithmsAnalyzersResources.DoNotUseSHA1));
        private static readonly LocalizableString s_localizableDoNotUseSHA1Description = DiagnosticHelpers.GetLocalizableResourceString(nameof(SystemSecurityCryptographyHashingAlgorithmsAnalyzersResources.DoNotUseSHA1Description));
        

        internal static DiagnosticDescriptor DoNotUseMD5Rule = CreateDiagnosticDescriptor(DoNotUseMD5RuleId,
                                                                                          s_localizableDoNotUseMD5Title,
                                                                                          s_localizableDoNotUseMD5Description);
        
        internal static DiagnosticDescriptor DoNotUseSHA1Rule = CreateDiagnosticDescriptor(DoNotUseSHA1RuleId,
                                                                                           s_localizableDoNotUseSHA1Title,
                                                                                           s_localizableDoNotUseSHA1Description);
         
        protected abstract Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

        private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(DoNotUseMD5Rule, 
                                                                                                                    DoNotUseSHA1Rule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => s_supportedDiagnostics;

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(string ruleId, LocalizableString title, LocalizableString description, string uri = null)
        {
            return new DiagnosticDescriptor(ruleId,
                                            title,
                                            title,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            true,
                                            description: description,
                                            helpLinkUri: uri,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    var cryptTypes = new CompilationSecurityTypes(context.Compilation);
                    if (ReferencesAnyTargetType(cryptTypes))
                    {
                        Analyzer analyzer = GetAnalyzer(context, cryptTypes);
                    }
                });
        }

        private static bool ReferencesAnyTargetType(CompilationSecurityTypes types)
        {
            return types.MD5 != null 
                || types.SHA1 != null
                || types.HMACSHA1 != null;
        }

        protected class Analyzer
        {
            private CompilationSecurityTypes _cryptTypes; 

            public Analyzer(CompilationSecurityTypes cryptTypes)
            {
                _cryptTypes = cryptTypes; 
            }

            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                ISymbol symbol = SyntaxNodeHelper.GetSymbol(node, model);
                IMethodSymbol method = symbol as IMethodSymbol;
                if (method == null)
                {
                    return;
                }

                INamedTypeSymbol type = method.ContainingType;
                DiagnosticDescriptor rule = null;

                if (type.IsDerivedFrom(_cryptTypes.MD5, baseTypesOnly: true))
                {
                    rule = DoNotUseMD5Rule;
                } 
                else if (type.IsDerivedFrom(_cryptTypes.SHA1, baseTypesOnly: true) ||
                         type.IsDerivedFrom(_cryptTypes.HMACSHA1, baseTypesOnly: true))
                {
                    rule = DoNotUseSHA1Rule;
                }

                if (rule != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
                }
            } 
        }
    }
}

