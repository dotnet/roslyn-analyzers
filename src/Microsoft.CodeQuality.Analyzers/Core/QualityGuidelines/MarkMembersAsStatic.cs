// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA1822: Mark members as static
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class MarkMembersAsStaticAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1822";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Consider making this analyzer thread-safe.
            //analysisContext.EnableConcurrentExecution();

            // Don't report in generated code since that's not actionable.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                // Since property/event accessors cannot be marked static themselves and the associated symbol (property/event)
                // has to be marked static, we want to report the diagnostic once on the property/event. So we make a note
                // of the associated symbols on which we've reported diagnostics for this compilation so that we don't duplicate 
                // those.
                var reportedAssociatedSymbols = new HashSet<ISymbol>();

                compilationContext.RegisterOperationBlockStartAction(blockStartContext =>
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
                    }, OperationKind.InstanceReference);

                    blockStartContext.RegisterOperationBlockEndAction(blockEndContext =>
                    {
                        if (!isInstanceReferenced)
                        {
                            ISymbol reportingSymbol = methodSymbol;

                            if (methodSymbol.IsPropertyAccessor() || methodSymbol.IsEventAccessor())
                            {
                                // If we've already reported on this associated symbol (i.e property/event) then don't report again.
                                if (reportedAssociatedSymbols.Contains(methodSymbol.AssociatedSymbol))
                                {
                                    return;
                                }

                                reportingSymbol = methodSymbol.AssociatedSymbol;
                                reportedAssociatedSymbols.Add(reportingSymbol);
                            }

                            blockEndContext.ReportDiagnostic(reportingSymbol.CreateDiagnostic(Rule, reportingSymbol.Name));
                        }
                    });
                });
            });
        }

        private static bool ShouldAnalyze(IMethodSymbol methodSymbol, Compilation compilation)
        {
            // Modifiers that we don't care about
            if (methodSymbol.IsStatic || methodSymbol.IsOverride || methodSymbol.IsVirtual ||
                methodSymbol.IsExtern || methodSymbol.IsAbstract || methodSymbol.IsImplementationOfAnyInterfaceMember())
            {
                return false;
            }

            if (methodSymbol.IsConstructor() || methodSymbol.IsFinalizer())
            {
                return false;
            }

            // CA1000 says one shouldn't declare static members on generic types. So don't flag such cases.
            if (methodSymbol.ContainingType.IsGenericType && methodSymbol.IsExternallyVisible())
            {
                return false;
            }

            // FxCop doesn't check for the fully qualified name for these attributes - so we'll do the same.
            var skipAttributes = new[]
            {
                "WebMethodAttribute",
                "TestInitializeAttribute",
                "TestMethodAttribute",
                "TestCleanupAttribute",
            };

            if (methodSymbol.GetAttributes().Any(attribute => skipAttributes.Contains(attribute.AttributeClass.Name)))
            {
                return false;
            }

            // If this looks like an event handler don't flag such cases.
            if (methodSymbol.Parameters.Length == 2 &&
                methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                IsEventArgs(methodSymbol.Parameters[1].Type, compilation))
            {
                return false;
            }

            if (IsExplicitlyVisibleFromCom(methodSymbol, compilation))
            {
                return false;
            }

            return true;
        }

        private static bool IsEventArgs(ITypeSymbol type, Compilation compilation)
        {
            if (type.DerivesFrom(WellKnownTypes.EventArgs(compilation)))
            {
                return true;
            }

            if (type.IsValueType)
            {
                return type.Name.EndsWith("EventArgs", StringComparison.Ordinal);
            }

            return false;
        }

        private static bool IsExplicitlyVisibleFromCom(IMethodSymbol methodSymbol, Compilation compilation)
        {
            if (!methodSymbol.IsExternallyVisible() || methodSymbol.IsGenericMethod)
            {
                return false;
            }

            var comVisibleAttribute = WellKnownTypes.ComVisibleAttribute(compilation);
            if (comVisibleAttribute == null)
            {
                return false;
            }

            if (methodSymbol.GetAttributes().Any(attribute => attribute.AttributeClass.Equals(comVisibleAttribute)) ||
                methodSymbol.ContainingType.GetAttributes().Any(attribute => attribute.AttributeClass.Equals(comVisibleAttribute)))
            {
                return true;
            }

            return false;
        }
    }
}