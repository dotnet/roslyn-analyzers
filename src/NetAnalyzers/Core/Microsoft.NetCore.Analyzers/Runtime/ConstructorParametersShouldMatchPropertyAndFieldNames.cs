// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
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

        internal const string ReferencedFieldOrPropertyNameKey = "ReferencedFieldOrPropertyName";
        internal const string DiagnosticReasonKey = "DiagnosticReason";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageProperty = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageField = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchFieldName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessagePropertyPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyWithPublicVisibility), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionPropertyPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyWithPublicVisibilityDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFieldPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchFieldWithPublicVisibility), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionFieldPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchFieldWithPublicVisibilityDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyNameRule, FieldRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction((context) =>
            {
                INamedTypeSymbol? jsonConstructorAttributeNamedSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextJsonSerializationJsonConstructorAttribute);
                if (jsonConstructorAttributeNamedSymbol == null)
                {
                    return;
                }

                var paramAnalyzer = new ParameterAnalyzer(jsonConstructorAttributeNamedSymbol);

                context.RegisterSymbolStartAction((context) =>
                {
                    var constructors = ((INamedTypeSymbol)context.Symbol).InstanceConstructors;

                    foreach (var ctor in constructors)
                    {
                        if (paramAnalyzer.ShouldAnalyzeMethod(ctor))
                        {
                            context.RegisterOperationAction(
                                context => ParameterAnalyzer.AnalyzeOperationAndReport(context),
                                OperationKind.ParameterReference);
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

            public static void AnalyzeOperationAndReport(OperationAnalysisContext context)
            {
                var operation = (IParameterReferenceOperation)context.Operation;

                IMemberReferenceOperation? memberReferenceOperation = TryGetMemberReferenceOperation(operation);
                ISymbol? referencedSymbol = memberReferenceOperation?.GetReferencedMemberOrLocalOrParameter();

                // TODO: convert "IsStatic" to a separate diagnostic
                if (referencedSymbol == null || referencedSymbol.IsStatic)
                {
                    return;
                }

                IParameterSymbol param = operation.Parameter;

                if (referencedSymbol is IFieldSymbol field)
                {
                    if (!IsParamMatchesReferencedMemberName(param, field))
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
                    if (!IsParamMatchesReferencedMemberName(param, prop))
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

            private static bool IsParamMatchesReferencedMemberName(IParameterSymbol param, ISymbol referencedMember)
            {
                if (param.Name.Length != referencedMember.Name.Length)
                {
                    return false;
                }

                var paramWords = WordParser.Parse(param.Name, WordParserOptions.SplitCompoundWords);
                var memberWords = WordParser.Parse(referencedMember.Name, WordParserOptions.SplitCompoundWords);

                return paramWords.SequenceEqual(memberWords, StringComparer.OrdinalIgnoreCase);
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
                    .SetItem(ReferencedFieldOrPropertyNameKey, field.Name)
                    .SetItem(DiagnosticReasonKey, reason.ToString());

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
                    .SetItem(ReferencedFieldOrPropertyNameKey, prop.Name)
                    .SetItem(DiagnosticReasonKey, reason.ToString());

                context.ReportDiagnostic(
                    param.CreateDiagnostic(
                        diagnosticDescriptor,
                        properties,
                        param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                        param.Name,
                        prop.Name));
            }
        }
    }
}