// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CSharpConstantExpectedAnalyzer : ConstantExpectedAnalyzer
    {
        private static readonly CSharpDiagnosticHelper s_diagnosticHelper = new();
        private readonly IdentifierNameSyntax _constantExpectedIdentifier = (IdentifierNameSyntax)SyntaxFactory.ParseName(ConstantExpected);
        private readonly IdentifierNameSyntax _constantExpectedAttributeIdentifier = (IdentifierNameSyntax)SyntaxFactory.ParseName(ConstantExpectedAttribute);

        protected override DiagnosticHelper Helper => s_diagnosticHelper;

        protected override void RegisterAttributeSyntax(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(context => OnAttributeNode(context), SyntaxKind.Attribute);
        }
        private void OnAttributeNode(SyntaxNodeAnalysisContext context)
        {
            var attributeSyntax = (AttributeSyntax)context.Node;
            if (!attributeSyntax.Name.IsEquivalentTo(_constantExpectedIdentifier) && !attributeSyntax.Name.IsEquivalentTo(_constantExpectedAttributeIdentifier))
            {
                return;
            }
            var parameter = (ParameterSyntax)attributeSyntax.Parent.Parent;
            var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameter);

            OnParameterWithConstantExpectedAttribute(parameterSymbol, context.ReportDiagnostic);
        }

        private sealed class CSharpDiagnosticHelper : DiagnosticHelper
        {
            private readonly IdentifierNameSyntax _constantExpectedMinIdentifier = (IdentifierNameSyntax)SyntaxFactory.ParseName("Min");
            private readonly IdentifierNameSyntax _constantExpectedMaxIdentifier = (IdentifierNameSyntax)SyntaxFactory.ParseName("Max");

            public override Location? GetMaxLocation(SyntaxNode attributeNode) => GetArgumentLocation(attributeNode, _constantExpectedMaxIdentifier);

            public override Location? GetMinLocation(SyntaxNode attributeNode) => GetArgumentLocation(attributeNode, _constantExpectedMinIdentifier);

            private static Location? GetArgumentLocation(SyntaxNode attributeNode, IdentifierNameSyntax targetNameSyntax)
            {
                var attributeSyntax = (AttributeSyntax)attributeNode;
                if (attributeSyntax.ArgumentList is null)
                {
                    return null;
                }
                var targetArg = attributeSyntax.ArgumentList.Arguments.FirstOrDefault(arg => arg.NameEquals.Name.IsEquivalentTo(targetNameSyntax, true));
                return targetArg?.GetLocation();
            }
        }
    }
}
;