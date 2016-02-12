// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.Maintainability.Analyzers
{
    /// <summary>
    /// CA1812: Avoid uninstantiated internal classes
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidUninstantiatedInternalClassesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1812";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUninstantiatedInternalClassesTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUninstantiatedInternalClassesMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUninstantiatedInternalClassesDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182265.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(startContext =>
            {
                var instantiatedTypes = new List<INamedTypeSymbol>();
                var internalTypes = new List<INamedTypeSymbol>();

                startContext.RegisterOperationAction(context =>
                {
                    IObjectCreationExpression expr = (IObjectCreationExpression)context.Operation;
                    var namedType = expr.ResultType as INamedTypeSymbol;
                    if (namedType != null)
                    {
                        instantiatedTypes.Add(namedType);
                    }
                }, OperationKind.ObjectCreationExpression);

                startContext.RegisterSymbolAction(context =>
                {
                    INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
                    if (type.GetResultantVisibility() != SymbolVisibility.Public
                        && type.TypeKind == TypeKind.Class
                        && !type.IsStatic
                        && !type.IsAbstract)
                    {
                        internalTypes.Add(type);
                    }
                }, SymbolKind.NamedType);

                startContext.RegisterCompilationEndAction(context =>
                {
                    IEnumerable<INamedTypeSymbol> uninstantiatedInternalTypes =
                        internalTypes.Except(instantiatedTypes);

                    foreach (INamedTypeSymbol type in uninstantiatedInternalTypes)
                    {
                        context.ReportDiagnostic(type.CreateDiagnostic(Rule, type.FormatMemberName()));
                    }
                });
            });
        }
    }
}