// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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
                var exceptionType = WellKnownTypes.Exception(compilation);
                if (exceptionType == null)
                {
                    return;
                }
                
                // Get a list of interesting categories of methods to analyze.
                var methodCategories = GetMethodCategories(compilation);

                compilationStartContext.RegisterOperationBlockStartAction(operationBlockContext =>
                {
                    var methodSymbol = operationBlockContext.OwningSymbol as IMethodSymbol;
                    if (methodSymbol == null)
                    {
                        return;
                    }

                    // Find out if this given method is one of the interesting categories of methods.
                    // (For eg: certain Equals methods or certain accessors etc.
                    var methodCategory = methodCategories.FirstOrDefault(l => l.IsMatch(methodSymbol, compilation));
                    if (methodCategory == null)
                    {
                        return;
                    }

                    // For the interesting methods, register an operation action to catch all
                    // Throw statements.
                    operationBlockContext.RegisterOperationAction(operationContext =>
                    {
                        IThrowStatement operation = operationContext.Operation as IThrowStatement;
                        var type = operation.Thrown.ResultType as INamedTypeSymbol;
                        if (type != null && type.DerivesFrom(exceptionType))
                        {
                            // If no exceptions are allowed or if the thrown exceptions is not an allowed one..
                            if (methodCategory.AllowedExceptions.IsEmpty || !methodCategory.AllowedExceptions.Contains(type))
                            {
                                operationContext.ReportDiagnostic(
                                    operation.Syntax.CreateDiagnostic(methodCategory.Rule, methodSymbol.Name, type.Name));
                            }
                        }
                    }, OperationKind.ThrowStatement);
                });
            });
        }

        /// <summary>
        /// This object describes a class of methods where exception throwing statements should be analyzed.
        /// </summary>
        private class MethodCategory
        {
            /// <summary>
            /// Function used to determine whether a given method symbol should be analyzed.
            /// </summary>
            private readonly Func<IMethodSymbol, Compilation, bool> matchFunction;
            
            /// <summary>
            /// Determines if we should analyze non-public methods of a given type.
            /// </summary>
            private readonly bool analyzeOnlyPublicMethods;

            /// <summary>
            /// The rule that should be fired if there is an exception in this kind of method.
            /// </summary>
            public DiagnosticDescriptor Rule { get; }

            /// <summary>
            /// List of exception types which are allowed to be thrown inside this category of method.
            /// This list will be empty if no exceptions are allowed.
            /// </summary>
            public ImmutableArray<ITypeSymbol> AllowedExceptions { get; }

            public MethodCategory(Func<IMethodSymbol, Compilation, bool> matchFunction, bool analyzeOnlyPublicMethods, DiagnosticDescriptor rule, params ITypeSymbol[] allowedExceptionTypes)
            {
                this.matchFunction = matchFunction;
                this.analyzeOnlyPublicMethods = analyzeOnlyPublicMethods;
                this.Rule = rule;
                AllowedExceptions = ImmutableArray.Create(allowedExceptionTypes);
            }

            /// <summary>
            /// Checks if the given method belong this category
            /// </summary>
            public bool IsMatch(IMethodSymbol method, Compilation compilation)
            {
                // If we are supposed to analyze only public methods get the resultant visibility
                // i.e public method inside an internal class is not considered public.
                if (analyzeOnlyPublicMethods &&
                    method.GetResultantVisibility() != SymbolVisibility.Public)
                {
                    return false;
                }

                return matchFunction(method, compilation);
            }
        }
        
        private static List<MethodCategory> GetMethodCategories(Compilation compilation)
        {
            var methodCategories = new List<MethodCategory>(12);
            methodCategories.Add(new MethodCategory(IsPropertyGetter, true,
                PropertyGetterRule,
                WellKnownTypes.InvalidOperationException(compilation), WellKnownTypes.NotSupportedException(compilation)));

            methodCategories.Add(new MethodCategory(IsIndexerGetter, true,
                PropertyGetterRule,
                WellKnownTypes.InvalidOperationException(compilation), WellKnownTypes.NotSupportedException(compilation),
                WellKnownTypes.ArgumentException(compilation), WellKnownTypes.KeyNotFoundException(compilation)));

            methodCategories.Add(new MethodCategory(IsEventAccessor, true,
                HasAllowedExceptionsRule,
                WellKnownTypes.InvalidOperationException(compilation), WellKnownTypes.NotSupportedException(compilation),
                WellKnownTypes.ArgumentException(compilation)));

            methodCategories.Add(new MethodCategory(IsGetHashCodeInterfaceImplementation, true,
                HasAllowedExceptionsRule,
                WellKnownTypes.ArgumentException(compilation)));

            methodCategories.Add(new MethodCategory(IsEqualsOverrideOrInterfaceImplementation, true,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IsEqualityOperator, true,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IsGetHashCodeOverride, true,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IsToString, true,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IsImplicitCastOperator, true,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IsStaticConstructor, false,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IsFinalizer, false,
                NoAllowedExceptionsRule));

            methodCategories.Add(new MethodCategory(IMethodSymbolExtensions.IsDisposeImplementation, true,
                NoAllowedExceptionsRule));

            return methodCategories;
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

        private static bool IsEqualsOverrideOrInterfaceImplementation(IMethodSymbol method, Compilation compilation)
        {
            return method.IsEqualsOverride() || IsEqualsInterfaceImplementation(method, compilation);
        }

        /// <summary>
        /// Checks if a given method implements IEqualityComparer.Equals or IEquatable.Equals.
        /// </summary>
        private static bool IsEqualsInterfaceImplementation(IMethodSymbol method, Compilation compilation)
        {
            if (method.Name != "Equals")
            {
                return false;
            }

            int paramCount = method.Parameters.Length;
            if (method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                (paramCount == 1 || paramCount == 2))
            {
                // Substitue the type of the first parameter of Equals in the generic interface and then check if that
                // interface method is implemented by the given method.
                var iEqualityComparer = WellKnownTypes.GenericIEqualityComparer(compilation);
                var constructedIEqualityComparer = iEqualityComparer?.Construct(method.Parameters.First().Type);
                var iEqualityComparerEquals = constructedIEqualityComparer?.GetMembers("Equals").Single() as IMethodSymbol;

                if (iEqualityComparerEquals != null && method.ContainingType.FindImplementationForInterfaceMember(iEqualityComparerEquals) == method)
                {
                    return true;
                }

                // Substitue the type of the first parameter of Equals in the generic interface and then check if that
                // interface method is implemented by the given method.
                var iEquatable = WellKnownTypes.GenericIEquatable(compilation);
                var constructedIEquatable = iEquatable?.Construct(method.Parameters.First().Type);
                var iEquatableEquals = constructedIEquatable?.GetMembers("Equals").Single();

                if (iEquatableEquals != null && method.ContainingType.FindImplementationForInterfaceMember(iEquatableEquals) == method)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a given method implements IEqualityComparer.GetHashCode or IHashCodeProvider.GetHashCode.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="compilation"></param>
        /// <returns></returns>
        private static bool IsGetHashCodeInterfaceImplementation(IMethodSymbol method, Compilation compilation)
        {
            if (method.Name != "GetHashCode")
            {
                return false;
            }

            if (method.ReturnType.SpecialType == SpecialType.System_Int32 && method.Parameters.Length == 1)
            {
                // Substitue the type of the first parameter of Equals in the generic interface and then check if that
                // interface method is implemented by the given method.
                var iEqualityComparer = WellKnownTypes.GenericIEqualityComparer(compilation);
                var constructedIEqualityComparer = iEqualityComparer?.Construct(method.Parameters.First().Type);
                var iEqualityComparerGetHashCode = constructedIEqualityComparer?.GetMembers("GetHashCode").Single();

                if (iEqualityComparerGetHashCode != null && method.ContainingType.FindImplementationForInterfaceMember(iEqualityComparerGetHashCode) == method)
                {
                    return true;
                }

                var iHashCodeProvider = WellKnownTypes.IHashCodeProvider(compilation);
                var iHashCodeProviderGetHashCode = iHashCodeProvider?.GetMembers("GetHashCode").Single();

                if (iHashCodeProviderGetHashCode != null && method.ContainingType.FindImplementationForInterfaceMember(iHashCodeProviderGetHashCode) == method)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsGetHashCodeOverride(IMethodSymbol method, Compilation compilation)
        {
            return method.IsGetHashCodeOverride();
        }

        private static bool IsToString(IMethodSymbol method, Compilation compilation)
        {
            return method.IsToStringOverride();
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
