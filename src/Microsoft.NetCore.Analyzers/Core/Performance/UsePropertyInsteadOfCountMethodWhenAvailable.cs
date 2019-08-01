// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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

        private static readonly ImmutableHashSet<string> propertyNames = ImmutableHashSet.Create("Length", "Count");

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
                var operationActionsHandler = context.Compilation.Language == LanguageNames.CSharp
                    ? (OperationActionsHandler)new CSharpOperationActionsHandler(containingSymbol: enumerableType, rule: s_rule)
                    : (OperationActionsHandler)new BasicOperationActionsHandler(containingSymbol: enumerableType, rule: s_rule);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeInvocationOperation,
                    OperationKind.Invocation);
            }
        }

        /// <summary>
        /// Handler for operaction actions.
        /// </summary>
        private abstract class OperationActionsHandler
        {
            private readonly INamedTypeSymbol containingSymbol;
            private readonly DiagnosticDescriptor rule;

            protected OperationActionsHandler(INamedTypeSymbol containingSymbol, DiagnosticDescriptor rule)
            {
                this.containingSymbol = containingSymbol;
                this.rule = rule;
            }

            internal void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var invocationOperation = (IInvocationOperation)context.Operation;

                if (GetEnumerableCountInvocationTargetType(invocationOperation, this.containingSymbol) is ITypeSymbol invocationTarget &&
                    GetReplacementProperty(invocationTarget) is string propertyName)
                {
                    var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                    propertiesBuilder.Add("PropertyName", propertyName);

                    var diagnostic = Diagnostic.Create(
                        this.rule,
                        invocationOperation.Syntax.GetLocation(),
                        propertiesBuilder.ToImmutable(),
                        propertyName);

                    context.ReportDiagnostic(diagnostic);
                }
            }

            protected abstract ITypeSymbol GetEnumerableCountInvocationTargetType(IInvocationOperation invocationOperation, INamedTypeSymbol containingSymbol);

            private static string GetReplacementProperty(ITypeSymbol invocationTarget)
            {
                if (invocationTarget.TypeKind == TypeKind.Array)
                {
                    return nameof(Array.Length);
                }

                foreach (var member in invocationTarget.GetMembers())
                {
                    if (member is IPropertySymbol property && propertyNames.Contains(property.Name))
                    {
                        return property.Name;
                    }
                }

                foreach (var type in invocationTarget.AllInterfaces)
                {
                    foreach (var member in type.GetMembers())
                    {
                        if (member is IPropertySymbol property && propertyNames.Contains(property.Name))
                        {
                            return property.Name;
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Handler for operaction actions for C#. This class cannot be inherited.
        /// Implements the <see cref="Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsHandler" />
        /// </summary>
        /// <seealso cref="Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.OperationActionsHandler" />
        private sealed class CSharpOperationActionsHandler : OperationActionsHandler
        {
            internal CSharpOperationActionsHandler(INamedTypeSymbol containingSymbol, DiagnosticDescriptor rule) : base(containingSymbol, rule)
            {
            }

            protected override ITypeSymbol GetEnumerableCountInvocationTargetType(IInvocationOperation invocationOperation, INamedTypeSymbol containingSymbol)
            {
                var method = invocationOperation.TargetMethod;

                if (invocationOperation.Arguments.Length == 1 &&
                    method.Name.Equals(nameof(Enumerable.Count), StringComparison.Ordinal) &&
                    method.ContainingSymbol.Equals(containingSymbol))
                {
                    var targetType = invocationOperation.Arguments[0].Value is IConversionOperation convertionOperation
                        ? convertionOperation.Operand.Type
                        : invocationOperation.Arguments[0].Value.Type;

                    return targetType as ITypeSymbol;
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
            internal BasicOperationActionsHandler(INamedTypeSymbol containingSymbol, DiagnosticDescriptor rule) : base(containingSymbol, rule)
            {
            }

            protected override ITypeSymbol GetEnumerableCountInvocationTargetType(IInvocationOperation invocationOperation, INamedTypeSymbol containingSymbol)
            {
                var method = invocationOperation.TargetMethod;

                if (invocationOperation.Arguments.Length == 0 &&
                    method.Name.Equals(nameof(Enumerable.Count), StringComparison.Ordinal) &&
                    method.ContainingSymbol.Equals(containingSymbol))
                {
                    var targetType = invocationOperation.Instance is IConversionOperation convertionOperation
                        ? convertionOperation.Operand.Type
                        : invocationOperation.Instance.Type;

                    return targetType as ITypeSymbol;
                }

                return null;
            }
        }
    }
}
