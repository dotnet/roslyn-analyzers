// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA2119: Seal methods that satisfy private interfaces
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SealMethodsThatSatisfyPrivateInterfacesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2119";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.SealMethodsThatSatisfyPrivateInterfacesTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.SealMethodsThatSatisfyPrivateInterfacesMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.SealMethodsThatSatisfyPrivateInterfacesDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Security,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182313.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(CheckTypes, SymbolKind.NamedType);
        }

        private void CheckTypes(SymbolAnalysisContext context)
        {
            var type = (ITypeSymbol)context.Symbol;

            // only classes can have overridable members
            if (type.TypeKind == TypeKind.Class)
            {
                // look for implementations of interfaces members declared on this type
                foreach (var iface in type.Interfaces)
                {
                    // only matters if the interface is defined to be internal
                    if (iface.DeclaredAccessibility == Accessibility.Internal)
                    {
                        // look for implementation of interface members
                        foreach (var imember in iface.GetMembers())
                        {
                            var member = type.FindImplementationForInterfaceMember(imember);

                            // only matters if member can be overridden
                            if (member != null && CanBeOverridden(member))
                            {
                                if (member.ContainingType != null && member.ContainingType.Equals(type))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0]));
                                }
                                else 
                                {
                                    // we have a member and its not declared on this type?  
                                    // must be implicit implementation of base member
                                    context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0]));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CanBeOverridden(ISymbol member)
        {
            return (member.IsAbstract || member.IsVirtual || member.IsOverride)
                            && !(member.IsSealed || member.IsStatic || member.DeclaredAccessibility == Accessibility.Private)
                            && member.ContainingType != null
                            && member.ContainingType.TypeKind == TypeKind.Class;
        }
    }
}