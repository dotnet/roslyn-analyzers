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
    /// <summary>
    /// 
    /// 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseNameofInPlaceOfStringAnalyzer : DiagnosticAnalyzer

    {
        // TODO: need a RuleId
        internal const string RuleId = "NAMEOFANALYZER";

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
            // TODO correct setting?
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
        }

        private void AnalyzeArgument(OperationAnalysisContext context)
        {

            var argument = (IArgumentOperation)context.Operation;

            if (argument.Value.Type.SpecialType != SpecialType.System_String)
            {
                return;
            }
            var stringText = argument.Value.ConstantValue.Value.ToString();

            var matchingParameter = (IParameterSymbol)argument.Parameter;
            switch (matchingParameter.Name)
            {
                case "paramName":
                    var parametersInScope = GetParametersInScope(context);
                    if (HasAMatchInScope(stringText, parametersInScope))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleWithSuggestion, argument.Syntax.GetLocation()));
                    }
                    return;
                case "propertyName":
                    var propertiesInScope = GetPropertiesInScope(context);
                    if (HasAMatchInScope(stringText, propertiesInScope))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleWithSuggestion, argument.Syntax.GetLocation()));
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