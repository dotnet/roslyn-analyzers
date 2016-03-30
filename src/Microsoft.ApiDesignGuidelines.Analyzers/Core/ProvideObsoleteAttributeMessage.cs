// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Linq;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1041: Provide ObsoleteAttribute message
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ProvideObsoleteAttributeMessageAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1041";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ProvideObsoleteAttributeMessageTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ProvideObsoleteAttributeMessageMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ProvideObsoleteAttributeMessageDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182166.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol obsoleteAttributeType = compilationContext.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
                if (obsoleteAttributeType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(sc => AnalyzeSymbol(sc, obsoleteAttributeType),
                    SymbolKind.NamedType,
                    SymbolKind.Method,
                    SymbolKind.Field,
                    SymbolKind.Property,
                    SymbolKind.Event);
            });
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol obsoleteAttributeType)
        {
            ImmutableArray<AttributeData> attributes = context.Symbol.GetAttributes();
            foreach (AttributeData attribute in attributes)
            {
                if (attribute.AttributeClass.Equals(obsoleteAttributeType))
                {
                    // ObsoleteAttribute has a constructor that takes no params and two 
                    // other constructors that take a message as the first param.
                    // If there are no arguments specificed or if the message argument is empty
                    // then report a diagnostic.
                    if (attribute.ConstructorArguments.IsEmpty ||
                        string.IsNullOrEmpty(attribute.ConstructorArguments.First().Value as string))
                    {
                        SyntaxNode node = attribute.ApplicationSyntaxReference.GetSyntax();
                        context.ReportDiagnostic(node.CreateDiagnostic(Rule, context.Symbol.Name));
                    }
                }
            }
        }
    }
}