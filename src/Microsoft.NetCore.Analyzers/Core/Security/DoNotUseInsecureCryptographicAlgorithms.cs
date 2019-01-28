// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.NetCore.Analyzers.Security.Helpers;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Security
{
    public abstract class DoNotUseInsecureCryptographicAlgorithmsAnalyzer : DiagnosticAnalyzer
    {
        internal const string DoNotUseWeakCryptographyRuleId = "CA5350";
        internal const string DoNotUseBrokenCryptographyRuleId = "CA5351";

        internal const string CA5350HelpLink = "https://aka.ms/CA5350";
        internal const string CA5351HelpLink = "https://aka.ms/CA5351";

        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsTitle = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseWeakCryptographicAlgorithms),
            SystemSecurityCryptographyResources.ResourceManager, 
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsMessage = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseWeakCryptographicAlgorithmsMessage), 
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseWeakAlgorithmsDescription = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseWeakCryptographicAlgorithmsDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsTitle = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseBrokenCryptographicAlgorithms),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsMessage = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseBrokenCryptographicAlgorithmsMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseBrokenAlgorithmsDescription = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseBrokenCryptographicAlgorithmsDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor DoNotUseBrokenCryptographyRule =
            new DiagnosticDescriptor(
                DoNotUseBrokenCryptographyRuleId,
                s_localizableDoNotUseBrokenAlgorithmsTitle,
                s_localizableDoNotUseBrokenAlgorithmsMessage,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_localizableDoNotUseBrokenAlgorithmsDescription,
                helpLinkUri: CA5351HelpLink,
                customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor DoNotUseWeakCryptographyRule =
            new DiagnosticDescriptor(
                DoNotUseWeakCryptographyRuleId,
                s_localizableDoNotUseWeakAlgorithmsTitle,
                s_localizableDoNotUseWeakAlgorithmsMessage,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_localizableDoNotUseWeakAlgorithmsDescription,
                helpLinkUri: CA5350HelpLink,
                customTags: WellKnownDiagnosticTags.Telemetry);

        protected abstract SyntaxNodeAnalyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DoNotUseBrokenCryptographyRule, DoNotUseWeakCryptographyRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            analysisContext.RegisterCompilationStartAction(
                context =>
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
            return types.MD5 != null
                || types.SHA1 != null
                || types.HMACSHA1 != null
                || types.DES != null
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

                if (type.DerivesFrom(_cryptTypes.MD5))
                {
                    rule = DoNotUseBrokenCryptographyRule;
                    messageArgs[1] = _cryptTypes.MD5.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.SHA1))
                {
                    rule = DoNotUseWeakCryptographyRule;
                    messageArgs[1] = _cryptTypes.SHA1.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACSHA1))
                {
                    rule = DoNotUseWeakCryptographyRule;
                    messageArgs[1] = _cryptTypes.HMACSHA1.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.DES))
                {
                    rule = DoNotUseBrokenCryptographyRule;
                    messageArgs[1] = _cryptTypes.DES.Name;
                }
                else if ((method.ContainingType.DerivesFrom(_cryptTypes.DSA)
                          && method.MetadataName == SecurityMemberNames.CreateSignature) 
                    || (type == _cryptTypes.DSASignatureFormatter
                        && method.ContainingType.DerivesFrom(_cryptTypes.DSASignatureFormatter)
                        && method.MetadataName == WellKnownMemberNames.InstanceConstructorName))
                {
                    rule = DoNotUseBrokenCryptographyRule;
                    messageArgs[1] = _cryptTypes.DSA.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACMD5))
                {
                    rule = DoNotUseBrokenCryptographyRule;
                    messageArgs[1] = _cryptTypes.HMACMD5.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.RC2))
                {
                    rule = DoNotUseBrokenCryptographyRule;
                    messageArgs[1] = _cryptTypes.RC2.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.TripleDES))
                {
                    rule = DoNotUseWeakCryptographyRule;
                    messageArgs[1] = _cryptTypes.TripleDES.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.RIPEMD160))
                {
                    rule = DoNotUseWeakCryptographyRule;
                    messageArgs[1] = _cryptTypes.RIPEMD160.Name;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACRIPEMD160))
                {
                    rule = DoNotUseWeakCryptographyRule;
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

