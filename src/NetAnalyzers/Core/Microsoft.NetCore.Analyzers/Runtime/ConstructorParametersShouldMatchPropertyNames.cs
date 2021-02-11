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
    public sealed class ConstructorParametersShouldMatchPropertyNamesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1071";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageProperty = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchPropertyName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageField = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParameterShouldMatchFieldName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyNamesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor PropertyRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageProperty,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);
        internal static DiagnosticDescriptor FieldRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageField,
                                                                             DiagnosticCategory.Design,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyRule, FieldRule);

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
                    context.ReportDiagnostic(
                        param.CreateDiagnostic(
                            FieldRule,
                            param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                            param.Name,
                            field.Name));
                }

                if (IsSupportedProp(prop) && !IsParamMatchPropName(param, prop))
                {
                    context.ReportDiagnostic(
                        param.CreateDiagnostic(
                            PropertyRule,
                            param.ContainingType.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                            param.Name,
                            prop.Name));
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

            public bool ShouldAnalyzeMethod(IMethodSymbol method)
            {
                // We only care about constructors with parameters.
                if (method.Parameters.IsEmpty)
                {
                    return false;
                }

                // We only care about constructors that are marked with JsonConstructor attribute.
                if (!method.IsJsonConstructor(_jsonConstructorAttributeInfoType))
                {
                    return false;
                }

                return true;
            }
        }
    }
}