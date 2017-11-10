using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseNameofInPlaceOfStringAnalyzer : DiagnosticAnalyzer
    {
        // TODO: need a RuleId
        internal const string RuleId = "NAMEOFANALYZER";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Maintainability,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: true,
                                                                         description: s_localizableDescription,
                                                                         // TODO: add MSDN url
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182181.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor RuleWithSuggestion = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Maintainability,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: true,
                                                                         description: "suggest a name here",
                                                                         // TODO: add MSDN url
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182181.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO correct setting?
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSyntaxNodeAction(AnalyzeArgument, SyntaxKind.Argument);
        }

        private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {

            var argument = (ArgumentSyntax)context.Node;

            // iF argument has a NameColon but it is not equal to paramName or propertyName, bail
            if (argument.NameColon != null && !NamedArgumentParamNameOrPropertyName(argument))
            {
                return;
            }

            // expression must be a string literal
            var expression = argument.Expression;
            if (expression == null || !expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return;
            }

            var parametersInScope = GetParametersInScope(argument);
            var propertiesInScope = GetPropertiesInScope(argument);

            // if there aren't any parameters or members in scope, bail 
            if (!parametersInScope.Any() && !propertiesInScope.Any())
            {
                return;
            }

            // if there isn't an argument list (Can this happen?)
            var argumentList = argument.Parent as ArgumentListSyntax;
            if (argumentList == null)
            {
                return;
            }

            var argumentExpression = argumentList.Parent as ExpressionSyntax;
            if (argumentExpression == null)
            {
                return;
            }

            // ******** compilation

            // Get the method or property symbol
            var semanticModel = context.SemanticModel;
            var methodOrProperty = semanticModel.GetSymbolInfo(argumentExpression).Symbol;
            if (methodOrProperty == null)
            {
                return;
            }

            // Get the matching parameter for the argument
            var methodOrPropertyParameters = methodOrProperty.GetParameters();
            if (methodOrPropertyParameters.Length == 0)
            {
                return;
            }

            IParameterSymbol matchingParameter = null;
            if (argument.NameColon != null) // named argument
            {
                matchingParameter = methodOrPropertyParameters.Single(p => p.Name == argument.NameColon.Name.Identifier.ValueText);
            }
            else
            {
                var index = argumentList.Arguments.IndexOf(argument);
                if (index < 0)
                {
                    return;
                }

                if (index < methodOrPropertyParameters.Length)
                {
                    matchingParameter = methodOrPropertyParameters[index];
                }

                // TODO what if the matching parameter is params[]?
                if (index >= methodOrPropertyParameters.Length && methodOrPropertyParameters[methodOrPropertyParameters.Length - 1].IsParams)
                {
                    return;
                }
            }

            // If the parameter is named paramName or propertyName, see if the string literal matches with a parameter or member in scope
            var stringLiteral = (LiteralExpressionSyntax)argument.Expression;
            var stringText = stringLiteral.Token.ValueText;
            bool stringMatches = false;
            switch (matchingParameter.Name)
            {
                case "paramName":
                    stringMatches = CheckForMatching(stringText, parametersInScope);
                    break;
                case "propertyName":
                    stringMatches = CheckForMatching(stringText, propertiesInScope);
                    break;
                default:
                    return;
            }

            Diagnostic diagnostic = null;
            if (stringMatches)
            {
                diagnostic = Diagnostic.Create(RuleWithSuggestion, stringLiteral.GetLocation());
            }
            else
            {
                // If propertyName and ArgumentException or ArgumentNullException then warn
                if (matchingParameter.Name == "paramName")
                diagnostic = Diagnostic.Create(Rule, stringLiteral.GetLocation());
            }

            context.ReportDiagnostic(diagnostic);

        }

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

        /// <summary>
        /// NamedArgumentParamNameOrPropertyName - returns true if it is a named argument called either paramName or propertyName
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static bool NamedArgumentParamNameOrPropertyName(ArgumentSyntax argument)
        {
            return  argument.NameColon.Name.Identifier.ValueText == "paramName" || argument.NameColon.Name.Identifier.ValueText == "propertyName";
        }

        // GetParameterList in Ros code, but not public API http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp.Workspaces/CodeGeneration/CSharpSyntaxGenerator.cs,b2ccfe7fa29b0558
        private static IEnumerable<string> GetParametersInScope(SyntaxNode node)
        {
            {
                foreach (var ancestor in node.AncestorsAndSelf())
                {
                    if (ancestor.IsKind(SyntaxKind.SimpleLambdaExpression))
                    {
                        yield return ((SimpleLambdaExpressionSyntax)ancestor).Parameter.Identifier.ValueText;
                    }
                    else
                    {
                        var parameterList = GetParameterList(ancestor);
                        if (parameterList != null)
                        {
                            foreach (var parameter in parameterList.Parameters)
                            {
                                yield return parameter.Identifier.ValueText;
                            }
                        }
                    }
                }
            }
        }

        // TODO GetPropertiesInScope
        private static IEnumerable<string> GetPropertiesInScope(ArgumentSyntax argument)
        {
            // and struct
            var ancestors = argument.FirstAncestorOrSelf<SyntaxNode>(a => a.IsKind(SyntaxKind.ClassDeclaration))
                .ChildNodes()
                .Where(t => t.IsKind(SyntaxKind.PropertyDeclaration))
                .SelectMany(t => ((PropertyDeclarationSyntax)t).Identifier.ValueText);

            return ancestors.Cast<string>();
        }
        
        private static BaseParameterListSyntax GetParameterList(SyntaxNode ancestor)
        {
            switch (ancestor?.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)ancestor).ParameterList;
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)ancestor).ParameterList;
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)ancestor).ParameterList;
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return ((ParenthesizedLambdaExpressionSyntax)ancestor).ParameterList;
                case SyntaxKind.AnonymousMethodExpression:
                    return ((AnonymousMethodExpressionSyntax)ancestor).ParameterList;
                default:
                    return null;
            }
        }
    }
}