// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1065: Do not raise exceptions in unexpected locations
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotRaiseExceptionsInUnexpectedLocationsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1065";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotRaiseExceptionsInUnexpectedLocationsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessagePropertyGetter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotRaiseExceptionsInUnexpectedLocationsMessagePropertyGetter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageHasAllowedExceptions = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotRaiseExceptionsInUnexpectedLocationsMessageHasAllowedExceptions), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNoAllowedExceptions = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotRaiseExceptionsInUnexpectedLocationsMessageNoAllowedExceptions), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotRaiseExceptionsInUnexpectedLocationsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor PropertyGetterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessagePropertyGetter,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor HasAllowedExceptionsRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageHasAllowedExceptions,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor NoAllowedExceptionsRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNoAllowedExceptions,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyGetterRule, HasAllowedExceptionsRule, NoAllowedExceptionsRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var locations = GetLocationTargets(compilation);
                var exceptionType = WellKnownTypes.Exception(compilation);

                if (exceptionType == null)
                {
                    return;
                }

                compilationStartContext.RegisterOperationBlockStartAction(operationBlockContext =>
                {
                    var methodSymbol = operationBlockContext.OwningSymbol as IMethodSymbol;
                    if (methodSymbol == null)
                    {
                        return;
                    }

                    var match = locations.FirstOrDefault(l => l.IsMatch(methodSymbol, compilation));
                    if (match == null)
                    {
                        return;
                    }

                    operationBlockContext.RegisterOperationAction(operationContext =>
                    {
                        IThrowStatement operation = operationContext.Operation as IThrowStatement;
                        var type = operation.Thrown.ResultType as INamedTypeSymbol;
                        if (type != null && type.DerivesFrom(exceptionType))
                        {
                            if (match.AllowedExceptions.IsEmpty || match.AllowedExceptions.Contains(type))
                            {
                                operation.Syntax.CreateDiagnostic(match.Rule, type.Name);
                            }
                        }
                    }, OperationKind.ThrowStatement);
                });
            });
        }

        private class LocationTarget
        {
            private readonly Func<IMethodSymbol, Compilation, bool> matchFunction;
            private readonly Visibility visibility;
            public DiagnosticDescriptor Rule { get; }
            public ImmutableArray<ITypeSymbol> AllowedExceptions { get; }

            public LocationTarget(Func<IMethodSymbol, Compilation, bool> matchFunction, Visibility visibility, DiagnosticDescriptor rule, params ITypeSymbol[] allowedExceptionTypes)
            {
                this.matchFunction = matchFunction;
                this.visibility = visibility;
                this.Rule = rule;
                AllowedExceptions = ImmutableArray.Create(allowedExceptionTypes);
            }

            public bool IsMatch(IMethodSymbol method, Compilation compilation)
            {
                if (visibility == Visibility.OutsideAssembly &&
                    method.GetResultantVisibility() != SymbolVisibility.Public)
                {
                    return false;
                }
                return matchFunction(method, compilation);
            }
        }

        private enum Visibility
        {
            All = 0,
            OutsideAssembly,
        }

        private static bool IsEqualsOverrideOrInterfaceImplementation(IMethodSymbol method, Compilation compilation)
        {
            return method.IsEqualsOverride() || method.IsEqualsInterfaceImplementation(compilation);
        }


        private static List<LocationTarget> GetLocationTargets(Compilation compilation)
        {
            var locationTargets = new List<LocationTarget>(12);
            locationTargets.Add(new LocationTarget(IsPropertyGetter, Visibility.OutsideAssembly,
                PropertyGetterRule,
                WellKnownTypes.InvalidOperationException(compilation), WellKnownTypes.NotSupportedException(compilation)));

            locationTargets.Add(new LocationTarget(IsIndexerGetter, Visibility.OutsideAssembly,
                PropertyGetterRule,
                WellKnownTypes.InvalidOperationException(compilation), WellKnownTypes.NotSupportedException(compilation),
                WellKnownTypes.ArgumentException(compilation), WellKnownTypes.KeyNotFoundException(compilation)));

            locationTargets.Add(new LocationTarget(IsEventAccessor, Visibility.OutsideAssembly,
                HasAllowedExceptionsRule,
                WellKnownTypes.InvalidOperationException(compilation), WellKnownTypes.NotSupportedException(compilation),
                WellKnownTypes.ArgumentException(compilation)));

            locationTargets.Add(new LocationTarget(IMethodSymbolExtensions.IsGetHashCodeInterfaceImplementation, Visibility.OutsideAssembly,
                HasAllowedExceptionsRule,
                WellKnownTypes.ArgumentException(compilation)));

            locationTargets.Add(new LocationTarget(IsEqualsOverrideOrInterfaceImplementation, Visibility.OutsideAssembly,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IsEqualityOperator, Visibility.OutsideAssembly,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IsGetHashCodeOverride, Visibility.OutsideAssembly,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IsToString, Visibility.OutsideAssembly,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IsImplicitCastOperator, Visibility.OutsideAssembly,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IsStaticConstructor, Visibility.All,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IsFinalizer, Visibility.All,
                NoAllowedExceptionsRule));

            locationTargets.Add(new LocationTarget(IMethodSymbolExtensions.IsDisposeImplementation, Visibility.OutsideAssembly,
                NoAllowedExceptionsRule));

            return locationTargets;
        }

        private static bool IsToString(IMethodSymbol method, Compilation compilation)
        {
            return method.IsToString();
        }

        private static bool IsGetHashCodeOverride(IMethodSymbol method, Compilation compilation)
        {
            return method.IsGetHashCodeOverride();
        }

        private static bool IsPropertyGetter(IMethodSymbol method, Compilation compilation)
        {
            return method.MethodKind == MethodKind.PropertyGet &&
                   method.AssociatedSymbol?.GetParameters().Length == 0;
        }

        private static bool IsIndexerGetter(IMethodSymbol method, Compilation compilation)
        {
            return method.MethodKind == MethodKind.PropertyGet &&
                   method.AssociatedSymbol.IsIndexer();
        }

        private static bool IsEventAccessor(IMethodSymbol method, Compilation compilation)
        {
            return method.MethodKind == MethodKind.EventAdd ||
                   method.MethodKind == MethodKind.EventRaise ||
                   method.MethodKind == MethodKind.EventRemove;
        }

        private static bool IsStaticConstructor(IMethodSymbol method, Compilation compilation)
        {
            return method.MethodKind == MethodKind.StaticConstructor;
        }

        private static bool IsFinalizer(IMethodSymbol method, Compilation compilation)
        {
            return method.IsFinalizer();
        }

        private static bool IsEqualityOperator(IMethodSymbol method, Compilation compilation)
        {
            if (!method.IsStatic || !method.IsPublic())
                return false;
            switch (method.Name)
            {
                case WellKnownMemberNames.EqualityOperatorName:
                case WellKnownMemberNames.InequalityOperatorName:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsImplicitCastOperator(IMethodSymbol method, Compilation compilation)
        {
            if (!method.IsStatic || !method.IsPublic())
                return false;
            return (method.Name == WellKnownMemberNames.ImplicitConversionName);
        }
    }
}
