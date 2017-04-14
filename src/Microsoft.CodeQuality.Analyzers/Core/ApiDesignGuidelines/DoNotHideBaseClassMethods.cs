// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1061: Do not hide base class methods
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotHideBaseClassMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1061";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotHideBaseClassMethodsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotHideBaseClassMethodsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotHideBaseClassMethodsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true,
            description: s_localizableDescription,
            helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182143.aspx",
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            //analysisContext.EnableConcurrentExecution();
            analysisContext.RegisterSymbolAction(SymbolAnalyzer, SymbolKind.Method);
        }

        private void SymbolAnalyzer(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            // Bail out if this method overrides another (parameter types cannot be changed) 
            // or doesn't have any parameters
            if (method.IsOverride || !method.Parameters.Any())
            {
                return;
            }

            foreach (var hiddenMethod in GetMethodsHiddenByMethod(method, method.ContainingType.BaseType))
            {
                var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], method.ToDisplayString(), hiddenMethod.ToDisplayString());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private IEnumerable<IMethodSymbol> GetMethodsHiddenByMethod(IMethodSymbol method, INamedTypeSymbol baseType)
        {
            while (true)
            {
                if (baseType?.BaseType == null)
                {
                    // There are no other base types to check - we're at the top of the hierarchy
                    yield break;
                }

                var baseTypeMethods = baseType.GetMembers(method.Name)
                    .Where(x => !x.IsStatic && !x.IsVirtual && x.DeclaredAccessibility != Accessibility.Private)
                    .OfType<IMethodSymbol>();

                foreach (var baseTypeMethod in baseTypeMethods)
                {
                    if (method.Parameters.Length != baseTypeMethod.Parameters.Length)
                    {
                        continue;
                    }

                    var zippedParams = method.Parameters.Zip(
                        baseTypeMethod.Parameters, 
                        (x, y) => new {DerivedParm = x, BaseParam = y}).ToList();

                    // Does the matching base type method also have the same return type, the same parameter names, 
                    // and at least one parameter with a type that is more derived than the derived type's corresponding parameter? 
                    if (method.ReturnType.Equals(baseTypeMethod.ReturnType) 
                        && zippedParams.All(x => x.DerivedParm.Name == x.BaseParam.Name) 
                        && zippedParams.Any(x => !x.BaseParam.Type.Equals(x.DerivedParm.Type) 
                                            && x.BaseParam.Type.DerivesFrom(x.DerivedParm.Type)))
                    {
                        // Yes - the derived type method is "hiding" the base type method (we can't call it)
                        yield return baseTypeMethod;
                    }
                }

                // Repeat the same checks with the base type of this base type
                baseType = baseType.BaseType;
            }
        }
    }
}
