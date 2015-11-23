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
        internal const string CA5350HelpLink = "http://aka.ms/CA5350";
        internal const string CA5351HelpLink = "http://aka.ms/CA5351";

        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsTitle = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseWeakCryptographicAlgorithms));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsMessage = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseWeakCryptographicAlgorithmsMessage));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsDescription = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseWeakCryptographicAlgorithmsDescription));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsTitle = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseBrokenCryptographicAlgorithms));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsMessage = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseBrokenCryptographicAlgorithmsMessage));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsDescription = DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseBrokenCryptographicAlgorithmsDescription));

        internal static DiagnosticDescriptor DoNotUseBrokenCryptographicRule = CreateDiagnosticDescriptor(DoNotUseBrokenCryptographicRuleId,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsTitle,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsMessage,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsDescription);

        internal static DiagnosticDescriptor DoNotUseWeakCryptographicRule = CreateDiagnosticDescriptor(DoNotUseWeakCryptographicRuleId,
                                                                                          s_localizableDoNotUseWeakAlgorithmsTitle,
                                                                                          s_localizableDoNotUseWeakAlgorithmsMessage,
                                                                                          s_localizableDoNotUseWeakAlgorithmsDescription);
        protected abstract Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

        private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(DoNotUseWeakCryptographicRule,
                                                                                                                    DoNotUseBrokenCryptographicRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => s_supportedDiagnostics;

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(string ruleId, LocalizableString title, LocalizableString message, LocalizableString description, string uri = null)
        {
            return new DiagnosticDescriptor(ruleId,
                                            title,
                                            message,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
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
                string[] messageArgs = new string[2];
                string owningParentName = string.Empty;
                SyntaxNode cur = node;

                while(cur.Parent != null)
                {
                    var pNode = cur.Parent;
                    ISymbol sym = SyntaxNodeHelper.GetSymbol(pNode, model);

                    if(sym != null &&
                        !string.IsNullOrEmpty(sym.Name)
                        && (
                            sym.Kind == SymbolKind.Method ||
                            sym.Kind == SymbolKind.NamedType
                           )
                    )
                    {
                        owningParentName = sym.Name;
                        break;
                    }

                    cur = pNode;
                }

                messageArgs[0] = owningParentName;

                if (type.IsDerivedFrom(this._cryptTypes.DES, baseTypesOnly: true))
                {
                    rule = DoNotUseBrokenCryptographicRule;                    
                    messageArgs[1] = _cryptTypes.DES.Name;
                }
                else if (method.MatchMethodDerived(_cryptTypes.DSA, SecurityMemberNames.CreateSignature) ||
                         (type == _cryptTypes.DSASignatureFormatter &&
                          method.MatchMethodDerived(_cryptTypes.DSASignatureFormatter, WellKnownMemberNames.InstanceConstructorName)))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.DSA.Name;
                }
                else if (type.IsDerivedFrom(_cryptTypes.HMACMD5, baseTypesOnly: true))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.HMACMD5.Name;
                }
                else if (type.IsDerivedFrom(_cryptTypes.RC2, baseTypesOnly: true))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.RC2.Name;
                }
                else if (type.IsDerivedFrom(_cryptTypes.TripleDES, baseTypesOnly: true))
                {
                    rule = DoNotUseWeakCryptographicRule;
                    messageArgs[1] = _cryptTypes.TripleDES.Name;
                }
                else if (type.IsDerivedFrom(_cryptTypes.RIPEMD160, baseTypesOnly: true))
                {
                    rule = DoNotUseWeakCryptographicRule;
                    messageArgs[1] = _cryptTypes.RIPEMD160.Name;
                }
                else if (type.IsDerivedFrom(_cryptTypes.HMACRIPEMD160, baseTypesOnly: true))
                {
                    rule = DoNotUseWeakCryptographicRule;
                    messageArgs[1] = _cryptTypes.HMACRIPEMD160.Name;
                }

                if (rule != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation(), messageArgs));
                }
            } 
        }
    }
}

