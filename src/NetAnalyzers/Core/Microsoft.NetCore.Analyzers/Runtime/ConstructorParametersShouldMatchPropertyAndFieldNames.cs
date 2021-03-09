// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1071: Constructor parameters should match property and field names
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1071";
        internal const string ReferencedFieldOrPropertyName = "ReferencedFieldOrPropertyName";
        internal const string DiagnosticReason = "DiagnosticReason";
        internal const string UnreferencedParameterName = "UnreferencedParameterName";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageProperty = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageField = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchFieldName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessagePropertyPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyWithPublicVisibility), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionPropertyPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyWithPublicVisibilityDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFieldPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchFieldWithPublicVisibility), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionFieldPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchFieldWithPublicVisibilityDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageUnreferencedParameter = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldBeReferenced), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionUnreferencedParameter = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldBeReferencedDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor PropertyNameRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageProperty,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);
        internal static DiagnosticDescriptor FieldRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageField,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);
        internal static DiagnosticDescriptor PropertyPublicRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessagePropertyPublic,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescriptionPropertyPublic,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);
        internal static DiagnosticDescriptor FieldPublicRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFieldPublic,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescriptionFieldPublic,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);
        internal static DiagnosticDescriptor UnreferencedParameterRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageUnreferencedParameter,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescriptionUnreferencedParameter,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyNameRule, FieldRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(
                (compilationStartContext) =>
                {
                    INamedTypeSymbol? jsonConstructorAttributeNamedSymbol = compilationStartContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextJsonSerializationJsonConstructorAttribute);
                    if (jsonConstructorAttributeNamedSymbol == null)
                    {
                        return;
                    }

                    var paramAnalyzer = new ParameterAnalyzer(jsonConstructorAttributeNamedSymbol);

                    compilationStartContext.RegisterSymbolStartAction((symbolStartContext) =>
                    {
                        var constructors = ((INamedTypeSymbol)symbolStartContext.Symbol).InstanceConstructors;

                        foreach (var ctor in constructors)
                        {
                            if (paramAnalyzer.ShouldAnalyzeMethod(ctor))
                            {
                                var referencedParameters = PooledConcurrentSet<IParameterSymbol>.GetInstance();

                                symbolStartContext.RegisterOperationAction(
                                    context => ParameterAnalyzer.AnalyzeOperationAndReport(context, referencedParameters),
                                    OperationKind.ParameterReference);

                                symbolStartContext.RegisterSymbolEndAction(
                                    context => ParameterAnalyzer.ReportUnusedParameters(context, ctor, referencedParameters));
                            }
                        }
                    }, SymbolKind.NamedType);
                });
        }

        internal enum ParameterDiagnosticReason
        {
            NameMismatch,
            PropertyInappropriateVisibility,
            FieldInappropriateVisibility,
            UnreferencedParameter,
        }

        private sealed class ParameterAnalyzer
        {
            private readonly INamedTypeSymbol _jsonConstructorAttributeInfoType;

            public ParameterAnalyzer(INamedTypeSymbol jsonConstructorAttributeInfoType)
            {
                _jsonConstructorAttributeInfoType = jsonConstructorAttributeInfoType;
            }

            public static void AnalyzeOperationAndReport(OperationAnalysisContext context, PooledConcurrentSet<IParameterSymbol> referencedParameters)
            {
                var operation = (IParameterReferenceOperation)context.Operation;

                referencedParameters.Add(operation.Parameter);

                IMemberReferenceOperation? memberReferenceOperation = TryGetMemberReferenceOperation(operation);
                ISymbol? referencedSymbol = memberReferenceOperation?.GetReferencedMemberOrLocalOrParameter();

                if (referencedSymbol == null || referencedSymbol.IsStatic)
                {
                    return;
                }

                IParameterSymbol param = operation.Parameter;

                if (referencedSymbol is IFieldSymbol field)
                {
                    if (!IsParamMatchFieldName(param, field))
                    {
                        ReportFieldDiagnostic(context, FieldRule, ParameterDiagnosticReason.NameMismatch, param, field);
                    }

                    if (!field.IsPublic())
                    {
                        ReportFieldDiagnostic(context, FieldPublicRule, ParameterDiagnosticReason.FieldInappropriateVisibility, param, field);
                    }
                }
                else if (referencedSymbol is IPropertySymbol prop)
                {
                    if (!IsParamMatchPropName(param, prop))
                    {
                        ReportPropertyDiagnostic(context, PropertyNameRule, ParameterDiagnosticReason.NameMismatch, param, prop);
                    }

                    if (!prop.IsPublic())
                    {
                        ReportPropertyDiagnostic(context, PropertyPublicRule, ParameterDiagnosticReason.PropertyInappropriateVisibility, param, prop);
                    }
                }
            }

            public bool ShouldAnalyzeMethod(IMethodSymbol method)
            {
                // We only care about constructors with parameters.
                if (method.Parameters.IsEmpty)
                {
                    return false;
                }

                // We only care about constructors that are marked with JsonConstructor attribute.
                return this.IsJsonConstructor(method);
            }

            public static void ReportUnusedParameters(SymbolAnalysisContext context, IMethodSymbol ctor, PooledConcurrentSet<IParameterSymbol> referencedParameters)
            {
                foreach (var param in ctor.Parameters)
                {
                    if (referencedParameters.Contains(param))
                    {
                        continue;
                    }

                    ReportUnreferencedParameterDiagnostic(context, param);
                }

                referencedParameters.Free(context.CancellationToken);
            }

            private static bool IsParamMatchFieldName(IParameterSymbol param, IFieldSymbol field)
            {
                if (param.Name.Length != field.Name.Length)
                {
                    return false;
                }

                var paramWords = WordParser.Parse(param.Name, WordParserOptions.SplitCompoundWords);
                var fieldWords = WordParser.Parse(field.Name, WordParserOptions.SplitCompoundWords);

                return paramWords.SequenceEqual(fieldWords, StringComparer.OrdinalIgnoreCase);
            }

            private static bool IsParamMatchPropName(IParameterSymbol param, IPropertySymbol prop)
            {
                if (param.Name.Length != prop.Name.Length)
                {
                    return false;
                }

                var paramWords = WordParser.Parse(param.Name, WordParserOptions.SplitCompoundWords);
                var propWords = WordParser.Parse(prop.Name, WordParserOptions.SplitCompoundWords);

                return paramWords.SequenceEqual(propWords, StringComparer.OrdinalIgnoreCase);
            }

            private bool IsJsonConstructor([NotNullWhen(returnValue: true)] IMethodSymbol? method)
                => method.IsConstructor() &&
                    method.HasAttribute(this._jsonConstructorAttributeInfoType);

            private static IMemberReferenceOperation? TryGetMemberReferenceOperation(IParameterReferenceOperation paramOperation)
            {
                if (paramOperation.Parent is IAssignmentOperation assignmentOperation
                    && assignmentOperation.Target is IMemberReferenceOperation assignmentTarget)
                {
                    return assignmentTarget;
                }

                if (paramOperation.Parent is ITupleOperation sourceTuple
                    && sourceTuple.Parent is IConversionOperation conversion
                    && conversion.Parent is IDeconstructionAssignmentOperation deconstruction
                    && deconstruction.Target is ITupleOperation targetTuple)
                {
                    var paramIndexInTuple = sourceTuple.Elements.IndexOf(paramOperation);

                    return targetTuple.Elements[paramIndexInTuple] as IMemberReferenceOperation;
                }

                return null;
            }

            private static void ReportFieldDiagnostic(OperationAnalysisContext context, DiagnosticDescriptor diagnosticDescriptor, ParameterDiagnosticReason reason, IParameterSymbol param, IFieldSymbol field)
            {
                var properties = ImmutableDictionary<string, string?>.Empty
                    .SetItem(ReferencedFieldOrPropertyName, field.Name)
                    .SetItem(DiagnosticReason, reason.ToString());

                context.ReportDiagnostic(
                    param.CreateDiagnostic(
                        diagnosticDescriptor,
                        properties,
                        param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                        param.Name,
                        field.Name));
            }

            private static void ReportPropertyDiagnostic(OperationAnalysisContext context, DiagnosticDescriptor diagnosticDescriptor, ParameterDiagnosticReason reason, IParameterSymbol param, IPropertySymbol prop)
            {
                var properties = ImmutableDictionary<string, string?>.Empty
                    .SetItem(ReferencedFieldOrPropertyName, prop.Name)
                    .SetItem(DiagnosticReason, reason.ToString());

                context.ReportDiagnostic(
                    param.CreateDiagnostic(
                        diagnosticDescriptor,
                        properties,
                        param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                        param.Name,
                        prop.Name));
            }

            private static void ReportUnreferencedParameterDiagnostic(SymbolAnalysisContext context, IParameterSymbol param)
            {
                var properties = ImmutableDictionary<string, string?>.Empty
                    .SetItem(UnreferencedParameterName, param.Name)
                    .SetItem(DiagnosticReason, ParameterDiagnosticReason.UnreferencedParameter.ToString());

                context.ReportDiagnostic(
                    param.CreateDiagnostic(
                        UnreferencedParameterRule,
                        properties,
                        param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                        param.Name));
            }
        }
    }
}