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
        internal const string DoNotUseWeakCryptographicRuleId = "CA5350";
        internal const string DoNotUseBrokenCryptographicRuleId = "CA5351";

        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsTitle = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseWeakCryptographicAlgorithms));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsDescription = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseWeakCryptographicAlgorithmsDescription));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsTitle = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseBrokenCryptographicAlgorithms));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsDescription = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseBrokenCryptographicAlgorithmsDescription));

        internal static DiagnosticDescriptor DoNotUseWeakAlgorithmsRule = CreateDiagnosticDescriptor(DoNotUseWeakCryptographicRuleId,
                                                                                  s_localizableDoNotUseWeakAlgorithmsTitle,
                                                                                  s_localizableDoNotUseWeakAlgorithmsDescription);

        internal static DiagnosticDescriptor DoNotUseBrokenAlgorithmsRule = CreateDiagnosticDescriptor(DoNotUseBrokenCryptographicRuleId,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsTitle,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsDescription);

        protected abstract Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

        private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(DoNotUseWeakAlgorithmsRule,
                                                                                                                    DoNotUseBrokenAlgorithmsRule);

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

        private static DiagnosticDescriptor CreateCA5350DiagnosticDescriptor(string type, string name)
        {
            return CreateDiagnosticDescriptor(
                        DoNotUseWeakCryptographicRuleId,
                        s_localizableDoNotUseWeakAlgorithmsTitle,
                            DiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.DoNotUseWeakCryptographicAlgorithmsDescription),
                                type,
                                name
                            )
                        );
        }

        private static DiagnosticDescriptor CreateCA5351DiagnosticDescriptor(string type, string name)
        {
            return CreateDiagnosticDescriptor(
                        DoNotUseBrokenCryptographicRuleId,
                        s_localizableDoNotUseBrokenAlgorithmsTitle,
                            DiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.DoNotUseBrokenCryptographicAlgorithmsDescription),
                                type,
                                name
                            )
                        );
        }
        
        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    var cryptTypes = new CompilationSecurityTypes(context.Compilation);
                    if (ReferencesAnyTargetType(cryptTypes))
                    {
                        GetAnalyzer(context, cryptTypes);
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
                    rule = CreateCA5351DiagnosticDescriptor(type.Name, _cryptTypes.DES.Name);
                }
                else if (method.MatchMethodDerived(_cryptTypes.DSA, SecurityMemberNames.CreateSignature) ||
                         (type == _cryptTypes.DSASignatureFormatter &&
                          method.MatchMethodDerived(_cryptTypes.DSASignatureFormatter, WellKnownMemberNames.InstanceConstructorName)))
                {
                    rule = CreateCA5351DiagnosticDescriptor(type.Name, _cryptTypes.DSA.Name);
                }
                else if (type.IsDerivedFrom(_cryptTypes.HMACMD5, baseTypesOnly: true))
                {
                    rule = CreateCA5351DiagnosticDescriptor(type.Name, _cryptTypes.HMACMD5.Name);
                }
                else if (type.IsDerivedFrom(_cryptTypes.RC2, baseTypesOnly: true))
                {
                    rule = CreateCA5351DiagnosticDescriptor(type.Name, _cryptTypes.RC2.Name);
                }
                else if (type.IsDerivedFrom(_cryptTypes.TripleDES, baseTypesOnly: true))
                {
                    rule = CreateCA5350DiagnosticDescriptor(type.Name, _cryptTypes.TripleDES.Name);
                }
                else if (type.IsDerivedFrom(_cryptTypes.RIPEMD160, baseTypesOnly: true))
                {
                    rule = CreateCA5350DiagnosticDescriptor(type.Name, _cryptTypes.RIPEMD160.Name);
                }
                else if (type.IsDerivedFrom(_cryptTypes.HMACRIPEMD160, baseTypesOnly: true))
                {
                    rule = CreateCA5350DiagnosticDescriptor(type.Name, _cryptTypes.HMACRIPEMD160.Name);
                }

                if (rule != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
                }
            } 
        }
    }
}

