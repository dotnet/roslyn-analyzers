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
        internal const string DoNotUseWeakCryptographicRuleId = "CA5350";
        internal const string DoNotUseBrokenCryptographicRuleId = "CA5351";

        private static readonly LocalizableString s_localizableDoNotUseMD5Title = new LocalizableResourceString(nameof(SystemSecurityCryptographyResources.DoNotUseMD5), SystemSecurityCryptographyResources.ResourceManager, typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseMD5Description = new LocalizableResourceString(nameof(SystemSecurityCryptographyResources.DoNotUseMD5Description), SystemSecurityCryptographyResources.ResourceManager, typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseSHA1Title = new LocalizableResourceString(nameof(SystemSecurityCryptographyResources.DoNotUseSHA1), SystemSecurityCryptographyResources.ResourceManager, typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_localizableDoNotUseSHA1Description = new LocalizableResourceString(nameof(SystemSecurityCryptographyResources.DoNotUseSHA1Description), SystemSecurityCryptographyResources.ResourceManager, typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor DoNotUseBrokenCryptographicRule = CreateDiagnosticDescriptor(DoNotUseBrokenCryptographicRuleId,
                                                                                          s_localizableDoNotUseMD5Title,
                                                                                          s_localizableDoNotUseMD5Description);

        internal static DiagnosticDescriptor DoNotUseWeakCryptographicRule = CreateDiagnosticDescriptor(DoNotUseWeakCryptographicRuleId,
                                                                                           s_localizableDoNotUseSHA1Title,
                                                                                           s_localizableDoNotUseSHA1Description);

        protected abstract SyntaxNodeAnalyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DoNotUseBrokenCryptographicRule, DoNotUseWeakCryptographicRule);

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(string ruleId, LocalizableString title, LocalizableString description, string uri = null)
        {
            return new DiagnosticDescriptor(ruleId,
                                            title,
                                            title,
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

                if (type.DerivesFrom(_cryptTypes.MD5))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.SHA1) ||
                         type.DerivesFrom(_cryptTypes.HMACSHA1))
                {
                    rule = DoNotUseWeakCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.DES))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                }
                else if ((method.ContainingType.DerivesFrom(_cryptTypes.DSA) && method.MetadataName == SecurityMemberNames.CreateSignature) ||
                         (type == _cryptTypes.DSASignatureFormatter &&
                          method.ContainingType.DerivesFrom(_cryptTypes.DSASignatureFormatter) && method.MetadataName == WellKnownMemberNames.InstanceConstructorName))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACMD5))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.RC2))
                {
                    rule = DoNotUseBrokenCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.TripleDES))
                {
                    rule = DoNotUseWeakCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.RIPEMD160))
                {
                    rule = DoNotUseWeakCryptographicRule;
                }
                else if (type.DerivesFrom(_cryptTypes.HMACRIPEMD160))
                {
                    rule = DoNotUseWeakCryptographicRule;
                }

                if (rule != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
                }
            }
        }
    }
}

