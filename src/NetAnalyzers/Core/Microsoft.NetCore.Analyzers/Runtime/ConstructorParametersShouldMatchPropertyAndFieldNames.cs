// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        internal const string ReferencedFieldOrPropertyName = "ReferencedFieldOrPropertyName";
        internal const string DiagnosticReason = "DiagnosticReason";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageProperty = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageField = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchFieldName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessagePropertyPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyWithPublicVisibility), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionPropertyPublic = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyWithPublicVisibilityDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor PropertyNameRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageProperty,
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
        internal static DiagnosticDescriptor FieldRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageField,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescription,
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

                    compilationStartContext.RegisterOperationBlockStartAction(
                        (startOperationBlockContext) =>
                        {
                            if (startOperationBlockContext.OwningSymbol is IMethodSymbol method
                                && !paramAnalyzer.ShouldAnalyzeMethod(method))
                            {
                                return;
                            }

                            startOperationBlockContext.RegisterOperationAction(
                                context => ParameterAnalyzer.AnalyzeOperationAndReport(context),
                                OperationKind.ParameterReference);
                        });
                });
        }

        internal enum ParameterDiagnosticReason
        {
            NameMismatch,
            PropertyInappropriateVisibility
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

                if (operation.Parent is not IAssignmentOperation assignment)
                {
                    return;
                }

                IParameterSymbol param = operation.Parameter;
                ISymbol? referencedSymbol = assignment.Target.GetReferencedMemberOrLocalOrParameter();

                if (referencedSymbol == null)
                {
                    return;
                }

                var field = referencedSymbol as IFieldSymbol;
                var prop = referencedSymbol as IPropertySymbol;

                if (field == null && prop == null)
                {
                    return;
                }

                // Only process instance fields and properties
                if (referencedSymbol.IsStatic)
                {
                    return;
                }

                if (IsSupportedField(field) && !IsParamMatchFieldName(param, field))
                {
                    var properties = ImmutableDictionary<string, string?>.Empty.SetItem(ReferencedFieldOrPropertyName, field.Name);

                    context.ReportDiagnostic(
                        param.CreateDiagnostic(
                            FieldRule,
                            properties,
                            param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                            param.Name,
                            field.Name));

                    return;
                }

                if (IsSupportedProp(prop))
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

            private static bool IsSupportedProp([NotNullWhen(true)] IPropertySymbol? prop)
            {
                if (prop == null)
                {
                    return false;
                }

                return true;
            }

            private static bool IsSupportedField([NotNullWhen(true)] IFieldSymbol? field)
            {
                if (field == null)
                {
                    return false;
                }

                return true;
            }

            private static bool IsParamMatchFieldName(IParameterSymbol param, IFieldSymbol field)
            {
                var paramWords = WordParser.Parse(param.Name, WordParserOptions.SplitCompoundWords);
                var fieldWords = WordParser.Parse(field.Name, WordParserOptions.SplitCompoundWords).ToImmutableArray();

                return paramWords.All(x => WordParser.ContainsWord(x, WordParserOptions.SplitCompoundWords, fieldWords));
            }

            private static bool IsParamMatchPropName(IParameterSymbol param, IPropertySymbol prop)
            {
                var paramWords = WordParser.Parse(param.Name, WordParserOptions.SplitCompoundWords);
                var propWords = WordParser.Parse(prop.Name, WordParserOptions.SplitCompoundWords).ToImmutableArray();

                return paramWords.All(x => WordParser.ContainsWord(x, WordParserOptions.SplitCompoundWords, propWords));
            }

            private bool IsJsonConstructor([NotNullWhen(returnValue: true)] IMethodSymbol? method)
            => method.IsConstructor() &&
                method.HasAttribute(this._jsonConstructorAttributeInfoType);

            public bool ShouldAnalyzeMethod(IMethodSymbol method)
            {
                // We only care about constructors with parameters.
                if (method.Parameters.IsEmpty)
                {
                    return false;
                }

                // We only care about constructors that are marked with JsonConstructor attribute.
                if (!this.IsJsonConstructor(method))
                {
                    return false;
                }

                return true;
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
        }
    }
}