// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Resources
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
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1824-mark-assemblies-with-neutralresourceslanguageattribute",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        protected abstract void RegisterAttributeAnalyzer(CompilationStartAnalysisContext context, Action onResourceFound);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // this analyzer is safe from running concurrently.
            analysisContext.EnableConcurrentExecution();

            // set generated file mode to analyze since I only analyze generated files and doesn't report
            // any diagnostics from it.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

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

                    if (TryCheckNeutralResourcesLanguageAttribute(ce, out AttributeData data))
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

            if (!(constValue.Value is string stringValue))
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
            attributeData = null;

            foreach (AttributeData data in context.Compilation.Assembly.GetAttributes())
            {
                if (attributeData is null)
                {
                    // set default out value
                    attributeData = data;
                }

                if (data.AttributeClass?.Equals(attribute) == true
                    && data.ConstructorArguments.Any(c => c.Type?.Equals(@string) == true && !string.IsNullOrWhiteSpace((string)c.Value)))
                {
                    // found one that already does right thing.
                    attributeData = data;
                    return true;
                }
            }

            // either we couldn't find one or existing one is wrong.
            return false;
        }
    }
}