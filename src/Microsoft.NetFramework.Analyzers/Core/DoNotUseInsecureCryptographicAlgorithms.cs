// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.NetFramework.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetFramework.Analyzers
{
    public abstract class DoNotUseInsecureCryptographicAlgorithmsAnalyzer : DiagnosticAnalyzer
    {
        internal const string DoNotUseWeakCryptographicRuleId = "CA5350";
        internal const string DoNotUseBrokenCryptographicRuleId = "CA5351";
        internal const string CA5350HelpLink = "http://aka.ms/CA5350";
        internal const string CA5351HelpLink = "http://aka.ms/CA5351";

        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsTitle = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseWeakCryptographicAlgorithms), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsMessage = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseWeakCryptographicAlgorithmsMessage), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsDescription = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseWeakCryptographicAlgorithmsDescription), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsTitle = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseBrokenCryptographicAlgorithms), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsMessage = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseBrokenCryptographicAlgorithmsMessage), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsDescription = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseBrokenCryptographicAlgorithmsDescription), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor DoNotUseBrokenCryptographicRule = CreateDiagnosticDescriptor(DoNotUseBrokenCryptographicRuleId,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsTitle,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsMessage,
                                                                                          s_localizableDoNotUseBrokenAlgorithmsDescription,
                                                                                          CA5351HelpLink);

        internal static DiagnosticDescriptor DoNotUseWeakCryptographicRule = CreateDiagnosticDescriptor(DoNotUseWeakCryptographicRuleId,
                                                                                          s_localizableDoNotUseWeakAlgorithmsTitle,
                                                                                          s_localizableDoNotUseWeakAlgorithmsMessage,
                                                                                          s_localizableDoNotUseWeakAlgorithmsDescription,
                                                                                          CA5350HelpLink);
        protected abstract SyntaxNodeAnalyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

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
                                            DiagnosticHelpers.DefaultDiagnosticSeverity,
                                            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                            description: description,
                                            helpLinkUri: uri,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics in generated code.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

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

        protected class SyntaxNodeAnalyzer
        {
            private readonly CompilationSecurityTypes _cryptTypes;

            public SyntaxNodeAnalyzer(CompilationSecurityTypes cryptTypes)
            {
                _cryptTypes = cryptTypes;
            }

            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;
                ISymbol symbol = node.GetDeclaredOrReferencedSymbol(model);
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

                while (cur.Parent != null)
                {
                    SyntaxNode pNode = cur.Parent;
                    ISymbol sym = pNode.GetDeclaredOrReferencedSymbol(model);

                    if (sym != null &&
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

                if (type.DerivesFrom(_cryptTypes.DES))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.DES.Name;
                }
                else if ((method.ContainingType.DerivesFrom(_cryptTypes.DSA) && method.MetadataName == SecurityMemberNames.CreateSignature) ||
                         (type == _cryptTypes.DSASignatureFormatter &&
                          method.ContainingType.DerivesFrom(_cryptTypes.DSASignatureFormatter) && method.MetadataName == WellKnownMemberNames.InstanceConstructorName))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.DSA.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACMD5))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.HMACMD5.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.RC2))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                    messageArgs[1] = _cryptTypes.RC2.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.TripleDES))
                {
                    rule = DoNotUseWeakCryptographicRule;
                    messageArgs[1] = _cryptTypes.TripleDES.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.RIPEMD160))
                {
                    rule = DoNotUseWeakCryptographicRule;
                    messageArgs[1] = _cryptTypes.RIPEMD160.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACRIPEMD160))
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

