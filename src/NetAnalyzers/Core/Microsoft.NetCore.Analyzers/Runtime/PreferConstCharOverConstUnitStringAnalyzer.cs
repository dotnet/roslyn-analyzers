// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Test for single character strings passed in to String.Append
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PreferConstCharOverConstUnitStringAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "PreferCharOverUnitString";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessage,
                                                                                      DiagnosticCategory.Performance,
                                                                                      RuleLevel.IdeSuggestion,
                                                                                      s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            InvocationExpressionSyntax invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression))
            {
                return;
            }

            // Verify that the current call is to StringBuilder.Append
            if (memberAccessExpression.Name.ToString() != nameof(StringBuilder.Append))
            {
                return;
            }

            // Check that we've called Append on an instance of StringBuilder
            ISymbol memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol;
            if (memberSymbol == null)
            {
                return;
            }

            string memberSymbolContainingTypeString = memberSymbol.ContainingType.ToString();
            if (memberSymbol.ContainingType.ToString() != "System.Text.StringBuilder")
            {
                return;
            }

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;
            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;
            if (arguments.Count < 1)
            {
                return;
            }

            ArgumentSyntax firstArgument = arguments.First();
            ExpressionSyntax expr = firstArgument.Expression;
            SymbolInfo exprSymbolInfo = context.SemanticModel.GetSymbolInfo(expr);
            ISymbol exprSymbol = exprSymbolInfo.Symbol;
            if (exprSymbol == null)
            {
                return;
            }

            ImmutableArray<SyntaxReference> syntaxReferences = exprSymbol.DeclaringSyntaxReferences;
            if (syntaxReferences.IsEmpty)
            {
                return;
            }

            SyntaxReference declaration = syntaxReferences.First();
            if (!(declaration.GetSyntax() is VariableDeclaratorSyntax variableDeclarator))
            {
                return;
            }

            if (!(variableDeclarator.Parent is VariableDeclarationSyntax localDeclaration))
            {
                return;
            }

            SeparatedSyntaxList<VariableDeclaratorSyntax> variables = localDeclaration.Variables;
            foreach (VariableDeclaratorSyntax variable in variables)
            {
                Location declarationLocation = variable.GetLocation();

                Optional<object> constant = context.SemanticModel.GetConstantValue(expr);
                if (constant.HasValue)
                {
                    object value = constant.Value;
                    if (value is string stringValue && stringValue.Length == 1)
                    {
                        // Single char string. 
                        context.ReportDiagnostic(Diagnostic.Create(Rule, declarationLocation, variable.Identifier.ValueText));
                    }
                }
            }
        }
    }
}
