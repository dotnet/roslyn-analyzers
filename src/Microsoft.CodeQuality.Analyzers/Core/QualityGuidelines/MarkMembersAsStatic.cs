// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1822-mark-members-as-static",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

#pragma warning disable RS1026 // Enable concurrent execution
        public override void Initialize(AnalysisContext analysisContext)
#pragma warning restore RS1026 // Enable concurrent execution
        {
            // TODO: Consider making this analyzer thread-safe.
            //analysisContext.EnableConcurrentExecution();

            // Don't report in generated code since that's not actionable.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                // Since property/event accessors cannot be marked static themselves and the associated symbol (property/event)
                // has to be marked static, we want to report the diagnostic on the property/event.
                // So we make a note of the property/event symbols which have at least one accessor with no instance access.
                // At compilation end, we report candidate property/event symbols whose all accessors are candidates to be marked static.
                var propertyOrEventCandidates = new HashSet<ISymbol>();
                var accessorCandidates = new HashSet<IMethodSymbol>();

                // For candidate methods that are not externally visible, we only report a diagnostic if they are actually invoked via a method call in the compilation.
                // This prevents us from incorrectly flagging methods that are only invoked via delegate invocations: https://github.com/dotnet/roslyn-analyzers/issues/1511
                // and also reduces noise by not flagging dead code.
                var internalCandidates = new HashSet<IMethodSymbol>();
                var invokedInternalMethods = new HashSet<IMethodSymbol>();

                // Get all the possible attributes for a test method
                ImmutableArray<INamedTypeSymbol> skippedAttributes = GetSkippedAttributes(compilationContext.Compilation);

                compilationContext.RegisterOperationBlockStartAction(blockStartContext =>
                {
                    if (!(blockStartContext.OwningSymbol is IMethodSymbol methodSymbol) || !ShouldAnalyze(methodSymbol, blockStartContext.Compilation, skippedAttributes))
                    {
                        return;
                    }

                    bool isInstanceReferenced = false;

                    blockStartContext.RegisterOperationAction(operationContext =>
                    {
                        if (((IInstanceReferenceOperation)operationContext.Operation).ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
                        {
                            isInstanceReferenced = true;
                        }
                    }, OperationKind.InstanceReference);

                    blockStartContext.RegisterOperationAction(operationContext =>
                    {
                        var invocation = (IInvocationOperation)operationContext.Operation;
                        if (!invocation.TargetMethod.IsExternallyVisible())
                        {
                            invokedInternalMethods.Add(invocation.TargetMethod);
                        }
                    }, OperationKind.Invocation);

                    blockStartContext.RegisterOperationBlockEndAction(blockEndContext =>
                    {
                        // Methods referenced by other non static methods 
                        // and methods containing only NotImplementedException should not considered for marking them as static
                        if (!isInstanceReferenced && !blockEndContext.IsMethodNotImplementedOrSupported())
                        {
                            if (methodSymbol.IsAccessorMethod())
                            {
                                accessorCandidates.Add(methodSymbol);
                                propertyOrEventCandidates.Add(methodSymbol.AssociatedSymbol);
                            }
                            else if (methodSymbol.IsExternallyVisible())
                            {
                                blockEndContext.ReportDiagnostic(methodSymbol.CreateDiagnostic(Rule, methodSymbol.Name));
                            }
                            else
                            {
                                internalCandidates.Add(methodSymbol);
                            }
                        }
                    });
                });

                compilationContext.RegisterCompilationEndAction(compilationEndContext =>
                {
                    foreach (var candidate in internalCandidates)
                    {
                        if (invokedInternalMethods.Contains(candidate))
                        {
                            compilationEndContext.ReportDiagnostic(candidate.CreateDiagnostic(Rule, candidate.Name));
                        }
                    }

                    foreach (var candidatePropertyOrEvent in propertyOrEventCandidates)
                    {
                        var allAccessorsAreCandidates = true;
                        foreach (var accessor in candidatePropertyOrEvent.GetAccessors())
                        {
                            if (!accessorCandidates.Contains(accessor))
                            {
                                allAccessorsAreCandidates = false;
                                break;
                            }
                        }

                        if (allAccessorsAreCandidates)
                        {
                            compilationEndContext.ReportDiagnostic(candidatePropertyOrEvent.CreateDiagnostic(Rule, candidatePropertyOrEvent.Name));
                        }
                    }
                });
            });
        }

        private static bool ShouldAnalyze(IMethodSymbol methodSymbol, Compilation compilation, ImmutableArray<INamedTypeSymbol> skippedAttributes)
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
            if (methodSymbol.GetAttributes().Any(attribute => skippedAttributes.Any(attr => attribute.AttributeClass.Inherits(attr))))
            {
                return false;
            }

            // If this looks like an event handler don't flag such cases.
            // However, we do want to consider EventRaise accessor as a candidate
            // so we can flag the associated event if none of it's accessors need instance reference.
            if (methodSymbol.Parameters.Length == 2 &&
                methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                IsEventArgs(methodSymbol.Parameters[1].Type, compilation) &&
                methodSymbol.MethodKind != MethodKind.EventRaise)
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

        private static ImmutableArray<INamedTypeSymbol> GetSkippedAttributes(Compilation compilation)
        {
            ImmutableArray<INamedTypeSymbol>.Builder builder = null;

            void Add(INamedTypeSymbol symbol)
            {
                if (symbol != null)
                {
                    builder = builder ?? ImmutableArray.CreateBuilder<INamedTypeSymbol>();
                    builder.Add(symbol);
                }
            }

            Add(WellKnownTypes.WebMethodAttribute(compilation));

            // MSTest attributes
            Add(WellKnownTypes.TestInitializeAttribute(compilation));
            Add(WellKnownTypes.TestMethodAttribute(compilation));
            Add(WellKnownTypes.DataTestMethodAttribute(compilation));
            Add(WellKnownTypes.TestCleanupAttribute(compilation));

            // XUnit attributes
            Add(WellKnownTypes.XunitFact(compilation));

            // NUnit Attributes
            Add(WellKnownTypes.NunitSetUp(compilation));
            Add(WellKnownTypes.NunitOneTimeSetUp(compilation));
            Add(WellKnownTypes.NunitOneTimeTearDown(compilation));
            Add(WellKnownTypes.NunitTest(compilation));
            Add(WellKnownTypes.NunitTestCase(compilation));
            Add(WellKnownTypes.NunitTestCaseSource(compilation));
            Add(WellKnownTypes.NunitTheory(compilation));
            Add(WellKnownTypes.NunitTearDown(compilation));

            return builder?.ToImmutable() ?? ImmutableArray<INamedTypeSymbol>.Empty;
        }
    }
}