// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Test for single character strings passed in to String.Append
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferConstCharOverConstUnitStringAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1831";
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(AnalyzeSymbol, OperationKind.Invocation);
        }

        private static void AnalyzeSymbol(OperationAnalysisContext context)
        {
            if (!(context.Operation.Syntax is InvocationExpressionSyntax invocationExpression))
            {
                return;
            }

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression))
            {
                return;
            }

            // Verify that the current call is to Append
            if (memberAccessExpression.Name.Identifier.Text != "Append")
            {
                return;
            }

            // Check that we've called Append on an instance of StringBuilder
            SemanticModel semanticModel = context.Operation.SemanticModel;
            if (semanticModel == null)
            {
                return;
            }

            // Check that the object is a StringBuilder
            if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out INamedTypeSymbol? stringBuilder))
            {
                return;
            }

            ISymbol memberSymbol = semanticModel.GetSymbolInfo(memberAccessExpression).Symbol;
            if (memberSymbol == null)
            {
                return;
            }

            INamedTypeSymbol memberSymbolContainingType = memberSymbol.ContainingType;
            bool? memberObjectExists = stringBuilder?.Equals(memberSymbolContainingType);
            if (memberObjectExists == null || memberObjectExists == false)
            {
                return;
            }

            // Find the argument and eventually it's declaration
            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;
            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;
            if (arguments.Count < 1)
            {
                return;
            }

            ArgumentSyntax firstArgument = arguments.First();
            ExpressionSyntax expr = firstArgument.Expression;
            SymbolInfo exprSymbolInfo = semanticModel.GetSymbolInfo(expr);
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

                Optional<object> constant = semanticModel.GetConstantValue(expr);
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
