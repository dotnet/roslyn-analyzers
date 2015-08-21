// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Desktop.Analyzers.Common;


namespace Desktop.Analyzers
{
    public abstract class DoNotUseInsecureCryptographicAlgorithmsAnalyzer : DiagnosticAnalyzer
    {
        internal const string DoNotUseMD5RuleId = "CA5350";
        internal const string DoNotUseDESRuleId = "CA5351";
        internal const string DoNotUseRC2RuleId = "CA5352";
        internal const string DoNotUseTripleDESRuleId = "CA5353"; 
        internal const string DoNotUseRIPEMD160RuleId = "CA5355";
        internal const string DoNotUseDSARuleId = "CA5356";
        internal const string DoNotUseRijndaelRuleId = "CA5357";

        private static readonly LocalizableString s_localizableDoNotUseMD5Title = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseMD5);
        private static readonly LocalizableString s_localizableDoNotUseMD5Description = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseMD5Description);
        private static readonly LocalizableString s_localizableDoNotUseDESTitle = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseDES);
        private static readonly LocalizableString s_localizableDoNotUseDESDescription = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseDESDescription);
        private static readonly LocalizableString s_localizableDoNotUseRC2Title = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseRC2);
        private static readonly LocalizableString s_localizableDoNotUseRC2Description = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseRC2Description);
        private static readonly LocalizableString s_localizableDoNotUseTripleDESTitle = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseTripleDES);
        private static readonly LocalizableString s_localizableDoNotUseTripleDESDescription = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseTripleDESDescription);
        private static readonly LocalizableString s_localizableDoNotUseRIPEMD160Title = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseRIPEMD160);
        private static readonly LocalizableString s_localizableDoNotUseRIPEMD160Description = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseRIPEMD160Description);
        private static readonly LocalizableString s_localizableDoNotUseDSATitle = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseDSA);
        private static readonly LocalizableString s_localizableDoNotUseDSADescription = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseDSADescription);
        private static readonly LocalizableString s_localizableDoNotUseRijndaelTitle = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseRijndael);
        private static readonly LocalizableString s_localizableDoNotUseRijndaelDescription = GetLocalizableResourceString(DesktopAnalyzersResources.DoNotUseRijndaelDescription);


        internal static DiagnosticDescriptor DoNotUseMD5Rule = CreateDiagnosticDescriptor(DoNotUseMD5RuleId,
                                                                                          s_localizableDoNotUseMD5Title,
                                                                                          s_localizableDoNotUseMD5Description);

        internal static DiagnosticDescriptor DoNotUseDESRule = CreateDiagnosticDescriptor(DoNotUseDESRuleId,
                                                                                          s_localizableDoNotUseDESTitle,
                                                                                          s_localizableDoNotUseDESDescription);

        internal static DiagnosticDescriptor DoNotUseRC2Rule = CreateDiagnosticDescriptor(DoNotUseRC2RuleId,
                                                                                          s_localizableDoNotUseRC2Title,
                                                                                          s_localizableDoNotUseRC2Description);

        internal static DiagnosticDescriptor DoNotUseTripleDESRule = CreateDiagnosticDescriptor(DoNotUseTripleDESRuleId,
                                                                                                s_localizableDoNotUseTripleDESTitle,
                                                                                                s_localizableDoNotUseTripleDESDescription);

        internal static DiagnosticDescriptor DoNotUseRIPEMD160Rule = CreateDiagnosticDescriptor(DoNotUseRIPEMD160RuleId,
                                                                                                s_localizableDoNotUseRIPEMD160Title,
                                                                                                s_localizableDoNotUseRIPEMD160Description);

        internal static DiagnosticDescriptor DoNotUseDSARule = CreateDiagnosticDescriptor(DoNotUseDSARuleId,
                                                                                          s_localizableDoNotUseDSATitle,
                                                                                          s_localizableDoNotUseDSADescription);

        internal static DiagnosticDescriptor DoNotUseRijndaelRule = CreateDiagnosticDescriptor(DoNotUseRijndaelRuleId,
                                                                                               s_localizableDoNotUseRijndaelTitle,
                                                                                               s_localizableDoNotUseRijndaelDescription);

        protected abstract Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

        private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(DoNotUseMD5Rule,
                                                                                                                  DoNotUseDESRule,
                                                                                                                  DoNotUseRC2Rule,
                                                                                                                  DoNotUseTripleDESRule, 
                                                                                                                  DoNotUseRIPEMD160Rule,
                                                                                                                  DoNotUseDSARule,
                                                                                                                  DoNotUseRijndaelRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => s_supportedDiagnostics;

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(string ruleId, LocalizableString title, LocalizableString description, string uri = null)
        {
            return new DiagnosticDescriptor(ruleId,
                                            title,
                                            title,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: uri,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }

        private static LocalizableResourceString GetLocalizableResourceString(string resourceString)
        {
            return new LocalizableResourceString(nameof(resourceString), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
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
            return types.DES != null
                || types.DSA != null
                || types.DSASignatureFormatter != null 
                || types.HMACMD5 != null
                || types.RC2 != null
                || types.Rijndael != null
                || types.TripleDES != null
                || types.RIPEMD160 != null
                || types.HMACRIPEMD160 != null;
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

                if (type.IsDerivedFrom(this._cryptTypes.DES, baseTypesOnly: true))
                {
                    rule = DoNotUseDESRule;
                }
                else if (method.MatchMethodDerived(_cryptTypes.DSA, SecurityMemberNames.CreateSignature) ||
                         (type == _cryptTypes.DSASignatureFormatter &&
                          method.MatchMethodDerived(_cryptTypes.DSASignatureFormatter, WellKnownMemberNames.InstanceConstructorName)))
                {
                    rule = DoNotUseDSARule;
                }
                else if (type.IsDerivedFrom(_cryptTypes.HMACMD5, baseTypesOnly: true))
                {
                    rule = DoNotUseMD5Rule;
                }
                else if (type.IsDerivedFrom(_cryptTypes.RC2, baseTypesOnly: true))
                {
                    rule = DoNotUseRC2Rule;
                }
                else if (type.IsDerivedFrom(_cryptTypes.Rijndael, baseTypesOnly: true))
                {
                    rule = DoNotUseRijndaelRule;
                }
                else if (type.IsDerivedFrom(_cryptTypes.TripleDES, baseTypesOnly: true))
                {
                    rule = DoNotUseTripleDESRule;
                }
                else if (type.IsDerivedFrom(_cryptTypes.RIPEMD160, baseTypesOnly: true) ||
                         type.IsDerivedFrom(_cryptTypes.HMACRIPEMD160, baseTypesOnly: true))
                {
                    rule = DoNotUseRIPEMD160Rule;
                }

                if (rule != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
                }
            } 
        }
    }
}

