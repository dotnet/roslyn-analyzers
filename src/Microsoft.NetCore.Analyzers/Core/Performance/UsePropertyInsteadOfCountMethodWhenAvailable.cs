// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// CA1829: Use property instead of <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>, when available.
    /// Implements the <see cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    /// </summary>
    /// <remarks>
    /// Flags the use of <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> on types that are know to have a property with the same semantics:
    /// <c>Length</c>, <c>Count</c>.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1829";
        private const string CountPropertyName = "Count";
        private const string LengthPropertyName = "Length";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UsePropertyInsteadOfCountMethodWhenAvailableTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UsePropertyInsteadOfCountMethodWhenAvailableMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UsePropertyInsteadOfCountMethodWhenAvailableDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
            description: s_localizableDescription,
#pragma warning disable CA1308 // Normalize strings to uppercase
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/" + RuleId.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        /// <value>The supported diagnostics.</value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s_rule);

        /// <summary>
        /// Called once at session start to register actions in the analysis context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        /// <summary>
        /// Called on compilation start.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (WellKnownTypes.Enumerable(context.Compilation) is INamedTypeSymbol enumerableType)
            {
                var operationActionsContext = new UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsContext(
                    context.Compilation,
                    enumerableType,
                    WellKnownTypes.ICollection(context.Compilation)?.GetMembers(CountPropertyName).OfType<IPropertySymbol>().SingleOrDefault(),
                    WellKnownTypes.GenericICollection(context.Compilation)?.GetMembers(CountPropertyName).OfType<IPropertySymbol>().SingleOrDefault(),
                    WellKnownTypes.ImmutableArray(context.Compilation));

                var operationActionsHandler = context.Compilation.Language == LanguageNames.CSharp
                    ? (OperationActionsHandler)new CSharpOperationActionsHandler(operationActionsContext)
                    : (OperationActionsHandler)new BasicOperationActionsHandler(operationActionsContext);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeInvocationOperation,
                    OperationKind.Invocation);
            }
        }

        private sealed class OperationActionsContext
        {
            public OperationActionsContext(Compilation compilation, INamedTypeSymbol enumerableType, IPropertySymbol collectionCountProperty, IPropertySymbol collectionOfTCountProperty, INamedTypeSymbol immutableArrayType)
            {
                Compilation = compilation;
                EnumerableType = enumerableType;
                CollectionCountProperty = collectionCountProperty;
                CollectionOfTCountProperty = collectionOfTCountProperty;
                ImmutableArrayType = immutableArrayType;
            }

            public Compilation Compilation { get; }
            public INamedTypeSymbol EnumerableType { get; }
            public IPropertySymbol CollectionCountProperty { get; }
            public IPropertySymbol CollectionOfTCountProperty { get; }
            public INamedTypeSymbol ImmutableArrayType { get; }
        }

        /// <summary>
        /// Handler for operaction actions.
        /// </summary>
        private abstract class OperationActionsHandler
        {
            protected OperationActionsHandler(OperationActionsContext context)
            {
                Context = context;
            }

            public OperationActionsContext Context { get; }

            internal void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var invocationOperation = (IInvocationOperation)context.Operation;

                if (GetEnumerableCountInvocationTargetType(invocationOperation) is ITypeSymbol invocationTarget &&
                    GetReplacementProperty(invocationTarget) is string propertyName)
                {
                    var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                    propertiesBuilder.Add("PropertyName", propertyName);

                    var diagnostic = Diagnostic.Create(
                        s_rule,
                        invocationOperation.Syntax.GetLocation(),
                        propertiesBuilder.ToImmutable(),
                        propertyName);

                    context.ReportDiagnostic(diagnostic);
                }
            }

            protected abstract ITypeSymbol GetEnumerableCountInvocationTargetType(IInvocationOperation invocationOperation);

            private string GetReplacementProperty(ITypeSymbol invocationTarget)
            {
                if (invocationTarget.TypeKind == TypeKind.Array)
                {
                    return LengthPropertyName;
                }

                if (invocationTarget.OriginalDefinition is ITypeSymbol originalDefinition &&
                    originalDefinition.MetadataName.ToString().Equals(typeof(ImmutableArray<>).Name, StringComparison.Ordinal) &&
                    originalDefinition.ContainingNamespace.ToString().Equals(typeof(ImmutableArray<>).Namespace, StringComparison.Ordinal))
                {
                    return LengthPropertyName;
                }

                if (invocationTarget.FindImplementationForInterfaceMember(this.Context.CollectionCountProperty) is IPropertySymbol countProperty &&
                    !countProperty.ExplicitInterfaceImplementations.Any())
                {
                    return CountPropertyName;
                }

                if (findImplementationForCollectionOfTInterfaceCountProperty(invocationTarget))
                {
                    return CountPropertyName;
                }

                return null;

                bool findImplementationForCollectionOfTInterfaceCountProperty(ITypeSymbol invocationTarget)
                {
                    if (isCollectionOfTInterface(invocationTarget))
                    {
                        return true;
                    }

                    if (invocationTarget.TypeKind == TypeKind.Interface)
                    {
                        if (invocationTarget.GetMembers(CountPropertyName).OfType<IPropertySymbol>().Any())
                        {
                            return false;
                        }

                        foreach (var @interface in invocationTarget.AllInterfaces)
                        {
                            if (@interface.OriginalDefinition is INamedTypeSymbol originalInterfaceDefinition &&
                                isCollectionOfTInterface(originalInterfaceDefinition))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var @interface in invocationTarget.AllInterfaces)
                        {
                            if (@interface.OriginalDefinition is INamedTypeSymbol originalInterfaceDefinition &&
                                isCollectionOfTInterface(originalInterfaceDefinition))
                            {
                                if (invocationTarget.FindImplementationForInterfaceMember(@interface.GetMembers(CountPropertyName)[0]) is IPropertySymbol propertyImplementation &&
                                    !propertyImplementation.ExplicitInterfaceImplementations.Any())
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;

                    bool isCollectionOfTInterface(ITypeSymbol type)
                        => type.OriginalDefinition?.Equals(this.Context.CollectionOfTCountProperty.ContainingSymbol) ?? false;
                }
            }
        }

        /// <summary>
        /// Handler for operaction actions for C#. This class cannot be inherited.
        /// Implements the <see cref="Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsHandler" />
        /// </summary>
        /// <seealso cref="Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsHandler" />
        private sealed class CSharpOperationActionsHandler : OperationActionsHandler
        {
            internal CSharpOperationActionsHandler(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsContext context)
                : base(context)
            {
            }

            protected override ITypeSymbol GetEnumerableCountInvocationTargetType(IInvocationOperation invocationOperation)
            {
                var method = invocationOperation.TargetMethod;

                if (invocationOperation.Arguments.Length == 1 &&
                    method.Name.Equals(nameof(Enumerable.Count), StringComparison.Ordinal) &&
                    method.ContainingSymbol.Equals(this.Context.EnumerableType) &&
                    ((INamedTypeSymbol)(method.Parameters[0].Type)).TypeArguments[0] is ITypeSymbol methodSourceItemType)
                {
                    return invocationOperation.Arguments[0].Value is IConversionOperation convertionOperation
                        ? convertionOperation.Operand.Type
                        : invocationOperation.Arguments[0].Value.Type;
                }

                return null;
            }
        }

        /// <summary>
        /// Handler for operaction actions fro Visual Basic. This class cannot be inherited.
        /// Implements the <see cref="Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsHandler" />
        /// </summary>
        /// <seealso cref="Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsHandler" />
        private sealed class BasicOperationActionsHandler : OperationActionsHandler
        {
            internal BasicOperationActionsHandler(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsContext context)
                : base(context)
            {
            }

            protected override ITypeSymbol GetEnumerableCountInvocationTargetType(IInvocationOperation invocationOperation)
            {
                var method = invocationOperation.TargetMethod;

                if (invocationOperation.Arguments.Length == 0 &&
                    method.Name.Equals(nameof(Enumerable.Count), StringComparison.Ordinal) &&
                    method.ContainingSymbol.Equals(this.Context.EnumerableType) &&
                    ((INamedTypeSymbol)(invocationOperation.Instance.Type)).TypeArguments[0] is ITypeSymbol methodSourceItemType)
                {
                    return invocationOperation.Instance is IConversionOperation convertionOperation
                       ? convertionOperation.Operand.Type
                       : invocationOperation.Instance.Type;
                }

                return null;
            }
        }
    }
}
