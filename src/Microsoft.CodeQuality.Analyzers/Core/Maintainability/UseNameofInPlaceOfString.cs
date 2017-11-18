// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    // TODO summary
    /// <summary>
    /// 
    /// 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseNameofInPlaceOfStringAnalyzer : DiagnosticAnalyzer

    {
        // TODO: RuleId 
        internal const string RuleId = "NAMEOFANALYZER";
        private const string ParamName = "paramName";
        private const string PropertyName = "propertyName";

        // TODO: need final wording for feature
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor RuleWithSuggestion = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Maintainability,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: true,
                                                                         description: "Use nameof",
                                                                         // TODO: add MSDN url
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182181.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleWithSuggestion);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
        }

        private void AnalyzeArgument(OperationAnalysisContext context)
        {

            var argument = (IArgumentOperation)context.Operation;

            if (argument.Value.Kind != OperationKind.Literal)
            {
                return;
            }
            // TODO better way to get the string?
            var stringText = argument.Value.ConstantValue.Value.ToString();
            var properties = ImmutableDictionary<string, string>.Empty.Add("StringText", stringText);

            // TODO when showing diagnostic on a named argument, should just squiggle the string and not the argument name
            var matchingParameter = (IParameterSymbol)argument.Parameter;
            switch (matchingParameter.Name)
            {
                case ParamName:
                    var parametersInScope = GetParametersInScope(context);
                    // TODO if argument doesn't match any parameters, give a warning
                    if (HasAMatchInScope(stringText, parametersInScope))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleWithSuggestion, argument.Syntax.GetLocation(), properties: properties));
                    }
                    return;
                case PropertyName:
                    var propertiesInScope = GetPropertiesInScope(context);
                    if (HasAMatchInScope(stringText, propertiesInScope))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleWithSuggestion, argument.Syntax.GetLocation(), properties: properties));
                    }
                    return;
                default:
                    return;
            }
        }

        private IEnumerable<string> GetPropertiesInScope(OperationAnalysisContext context)
        {
            var containingType = context.ContainingSymbol.ContainingType;
            if (containingType != null)
            {
                foreach (var property in containingType.GetMembers().OfType<IPropertySymbol>())
                {
                    yield return property.Name;
                }
            }
        }

        internal IEnumerable<string> GetParametersInScope(OperationAnalysisContext context)
        {
            foreach (var parameter in context.ContainingSymbol.GetParameters())
            {
                yield return parameter.Name;
            }

            var parentOperation = context.Operation.Parent;
            while (parentOperation != null)
            {
                if (parentOperation.Kind == OperationKind.AnonymousFunction)
                {
                    IMethodSymbol lambdaSymbol = ((IAnonymousFunctionOperation)parentOperation).Symbol;
                    if (lambdaSymbol != null)
                    {
                        foreach (var lambdaParameter in lambdaSymbol.Parameters)
                        {
                            yield return lambdaParameter.Name;
                        }
                    }
                }

                parentOperation = parentOperation.Parent;
            }
        }

        private static bool HasAMatchInScope(string stringText, IEnumerable<string> searchCollection)
        {
            foreach (var name in searchCollection)
            {
                if (stringText == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}