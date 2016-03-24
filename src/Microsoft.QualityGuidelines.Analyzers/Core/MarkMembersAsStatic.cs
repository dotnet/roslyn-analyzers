// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA1822: Mark members as static
    /// </summary>
    public abstract class MarkMembersAsStaticAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1822";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterOperationBlockStartAction(blockStartContext =>
            {
                var methodSymbol = blockStartContext.OwningSymbol as IMethodSymbol;
                if (methodSymbol == null || !ShouldAnalyze(methodSymbol, blockStartContext.Compilation))
                {
                    return;
                }
                
                bool isInstanceReferenced = false;

                blockStartContext.RegisterOperationAction(operationContext =>
                {
                    isInstanceReferenced = true;
                }, OperationKind.InstanceReferenceExpression);

                blockStartContext.RegisterOperationBlockEndAction(blockEndContext =>
                {
                    if (!isInstanceReferenced)
                    {
                        var reportingSymbol = methodSymbol.IsPropertyAccessor() ? methodSymbol.AssociatedSymbol : methodSymbol;
                        blockEndContext.ReportDiagnostic(reportingSymbol.CreateDiagnostic(Rule, reportingSymbol.Name));
                    }
                });
            });
        }

        private static bool ShouldAnalyze(IMethodSymbol methodSymbol, Compilation compilation)
        {
            // Modifiers that we don't care about
            if (methodSymbol.IsStatic || methodSymbol.IsOverride || methodSymbol.IsVirtual ||
                methodSymbol.IsExtern || methodSymbol.IsAbstract)
            {
                return false;
            }

            // Method kinds that we don't care about
            //if (methodSymbol.IsFinalizer() || 
            //    methodSymbol.HasAttribute("System.Web.Services.WebMethodAttribute", compilation) ||
            //    methodSymbol.HasAttribute("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute", compilation) ||
            //    methodSymbol.HasAttribute("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute", compilation) ||
            //    methodSymbol.HasAttribute("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute", compilation))
            //{
            //    return false;
            //}

            return true;
        }
    }
}