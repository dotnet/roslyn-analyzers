// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Resources.Analyzers
{
    /// <summary>
    /// CA1824: Mark assemblies with NeutralResourcesLanguageAttribute
    /// </summary>
    public abstract class MarkAssembliesWithNeutralResourcesLanguageAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1824";

        protected const string GeneratedCodeAttribute = "GeneratedCodeAttribute";
        protected const string StronglyTypedResourceBuilder = "StronglyTypedResourceBuilder";
        private const string Designer = ".Designer.";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemResourcesAnalyzersResources.MarkAssembliesWithNeutralResourcesLanguageTitle), SystemResourcesAnalyzersResources.ResourceManager, typeof(SystemResourcesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemResourcesAnalyzersResources.MarkAssembliesWithNeutralResourcesLanguageMessage), SystemResourcesAnalyzersResources.ResourceManager, typeof(SystemResourcesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemResourcesAnalyzersResources.MarkAssembliesWithNeutralResourcesLanguageDescription), SystemResourcesAnalyzersResources.ResourceManager, typeof(SystemResourcesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb385967.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        protected abstract void RegisterAttributeAnalyzer(CompilationStartAnalysisContext context, Action onResourceFound);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // set generated file mode to analyze since I only analyze generated files and doesn't report
            // any diagnostics from it.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            // this analyzer is safe from running concurrently.
            analysisContext.EnableConcurrentExecution();

            analysisContext.RegisterCompilationStartAction(cc =>
            {
                var hasResource = false;

                RegisterAttributeAnalyzer(cc, () => hasResource = true);

                cc.RegisterCompilationEndAction(ce =>
                {
                    // there is nothing to do.
                    if (!hasResource)
                    {
                        return;
                    }

                    AttributeData data;
                    if (TryCheckNeutralResourcesLanguageAttribute(ce, out data))
                    {
                        // attribute already exist
                        return;
                    }

                    if (data != null)
                    {
                        // we have the attribute but its doing it wrong.
                        ce.ReportDiagnostic(data.ApplicationSyntaxReference.GetSyntax(ce.CancellationToken).CreateDiagnostic(Rule));
                        return;
                    }

                    // attribute just don't exist
                    ce.ReportDiagnostic(Diagnostic.Create(Rule, Location.None));
                });
            });
        }

        protected static bool CheckDesignerFile(SyntaxTree tree)
        {
            return tree.FilePath?.IndexOf(Designer, StringComparison.OrdinalIgnoreCase) > 0;
        }

        protected static bool CheckResxGeneratedFile(SemanticModel model, SyntaxNode attribute, SyntaxNode argument, CancellationToken cancellationToken)
        {
            if (!CheckDesignerFile(model.SyntaxTree))
            {
                return false;
            }

            INamedTypeSymbol generatedCode = WellKnownTypes.GeneratedCodeAttribute(model.Compilation);
            if (model.GetSymbolInfo(attribute, cancellationToken).Symbol?.ContainingType?.Equals(generatedCode) != true)
            {
                return false;
            }

            Optional<object> constValue = model.GetConstantValue(argument);
            if (!constValue.HasValue)
            {
                return false;
            }

            var stringValue = constValue.Value as string;
            if (stringValue == null)
            {
                return false;
            }

            if (stringValue.IndexOf(StronglyTypedResourceBuilder, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            return true;
        }

        private static bool TryCheckNeutralResourcesLanguageAttribute(CompilationAnalysisContext context, out AttributeData attributeData)
        {
            INamedTypeSymbol attribute = WellKnownTypes.NeutralResourcesLanguageAttribute(context.Compilation);
            INamedTypeSymbol @string = WellKnownTypes.String(context.Compilation);

            Collections.Generic.IEnumerable<AttributeData> attributes = context.Compilation.Assembly.GetAttributes().Where(d => d.AttributeClass?.Equals(attribute) == true);
            foreach (AttributeData data in attributes)
            {
                if (data.ConstructorArguments.Any(c => c.Type?.Equals(@string) == true && !string.IsNullOrWhiteSpace((string)c.Value)))
                {
                    // found one that already does right thing.
                    attributeData = data;
                    return true;
                }
            }

            // either we couldn't find one or existing one is wrong.
            attributeData = attributes.FirstOrDefault();
            return false;
        }
    }
}