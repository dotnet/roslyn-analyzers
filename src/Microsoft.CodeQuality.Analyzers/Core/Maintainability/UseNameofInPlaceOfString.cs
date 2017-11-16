// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public abstract class UseNameofInPlaceOfStringAnalyzer<TSyntaxKind> : DiagnosticAnalyzer
        where TSyntaxKind : struct
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

            analysisContext.RegisterSyntaxNodeAction(AnalyzeArgument, ArgumentSyntaxKind);
        }

        protected abstract TSyntaxKind ArgumentSyntaxKind { get;  }

        private void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {

            var argumentSyntaxNode = context.Node;

            // iF argument has a NameColon but it is not equal to paramName or propertyName
            if (TryGetNamedArgument(argumentSyntaxNode, out var argumentName) && argumentName != "paramName" && argumentName != "propertyName")
            {
                return;
            }

            // expression must be a string literal and a valid identifier name
            if (!TryGetStringLiteralOfExpression(argumentSyntaxNode, out var stringLiteral, out var stringText))
            {
                return;
            }

            if (!IsValidIdentifier(stringText))
            {
                return;
            }

            var argumentList = GetArgumentListSyntax(argumentSyntaxNode);
            if (argumentList == null)
            {
                return;
            }

            var argumentExpression = GetArgumentExpression(argumentList);
            if (argumentExpression == null)
            {
                return;
            }

            var parametersInScope = GetParametersInScope(argumentSyntaxNode);
            var propertiesInScope = GetPropertiesInScope(argumentSyntaxNode);

            if (!parametersInScope.Any() && !propertiesInScope.Any())
            {
                return;
            }
            
            var matchesParameterInScope = CheckForMatching(stringText, parametersInScope);
            var matchesPropertyInScope = CheckForMatching(stringText, propertiesInScope);
            if (!matchesParameterInScope && !matchesPropertyInScope)
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var methodOrProperty = semanticModel.GetSymbolInfo(argumentExpression).Symbol;
            if (methodOrProperty == null)
            {
                return;
            }

            var methodOrPropertyParameters = methodOrProperty.GetParameters();
            if (methodOrPropertyParameters.Length == 0)
            {
                return;
            }

            IParameterSymbol matchingParameter = null;
            if (argumentName != null) // named argument
            {
                matchingParameter = methodOrPropertyParameters.Single(p => p.Name == argumentName);
            }
            else // positional arguments
            {
                var index = GetIndexOfArgument(argumentList, argumentSyntaxNode);
                if ((index < 0) || (index >= methodOrPropertyParameters.Length))
                {
                    return;
                }
                else
                {
                    matchingParameter = methodOrPropertyParameters[index];
                }
            }

            if ((matchingParameter.Name == "paramName" && matchesParameterInScope) || (matchingParameter.Name == "propertyName" && matchesPropertyInScope))
                {
                    context.ReportDiagnostic(Diagnostic.Create(RuleWithSuggestion, stringLiteral.GetLocation()));
                }
        }

        internal abstract int GetIndexOfArgument(SyntaxNode argumentList, SyntaxNode argumentSyntaxNode);
        internal abstract SyntaxNode GetArgumentExpression(SyntaxNode argumentList);
        internal abstract SyntaxNode GetArgumentListSyntax(SyntaxNode node);
        internal abstract bool IsValidIdentifier(string stringLiteral);
        internal abstract bool TryGetStringLiteralOfExpression(SyntaxNode argument, out SyntaxNode stringLiteral, out string stringText);
        internal abstract bool TryGetNamedArgument(SyntaxNode argumentSyntaxNode, out string argumentName);
        internal abstract IEnumerable<string> GetParametersInScope(SyntaxNode node);
        internal abstract IEnumerable<string> GetPropertiesInScope(SyntaxNode argument);

        private static bool CheckForMatching(string stringText, IEnumerable<string> searchCollection )
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