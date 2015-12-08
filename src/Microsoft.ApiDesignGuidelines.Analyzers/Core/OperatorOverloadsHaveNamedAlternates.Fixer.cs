// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using System.Collections.Generic;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2225: Operator overloads have named alternates
    /// </summary>
    public abstract class OperatorOverloadsHaveNamedAlternatesFixer<TOperatorDeclarationSyntax, TConversionOperatorSyntax, TClassDeclarationSyntax, TMethodSyntaxKind, IPropertySyntaxKind> : CodeFixProvider
        where TOperatorDeclarationSyntax : SyntaxNode
        where TConversionOperatorSyntax : SyntaxNode
        where TClassDeclarationSyntax : SyntaxNode
        where TMethodSyntaxKind : SyntaxNode
        where IPropertySyntaxKind : SyntaxNode
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            switch (diagnostic.Properties[OperatorOverloadsHaveNamedAlternatesAnalyzer.DiagnosticKindText])
            {
                case OperatorOverloadsHaveNamedAlternatesAnalyzer.AddAlternateText:
                    AddFriendlyAlternate(context, root, node, semanticModel, diagnostic);
                    break;
                case OperatorOverloadsHaveNamedAlternatesAnalyzer.FixVisibilityText:
                    await FixFriendlyVisibility(context, node, semanticModel, diagnostic);
                    break;
            }
        }

        private static void AddFriendlyAlternate(CodeFixContext context, SyntaxNode root, SyntaxNode node, SemanticModel semanticModel, Diagnostic diagnostic)
        {
            var operatorOverload = (SyntaxNode)node.FirstAncestorOrSelf<TOperatorDeclarationSyntax>() ?? node.FirstAncestorOrSelf<TConversionOperatorSyntax>();
            var operatorOverloadSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(operatorOverload, context.CancellationToken);
            var typeSymbol = (ITypeSymbol)operatorOverloadSymbol.ContainingSymbol;
            var typeDeclaration = typeSymbol.DeclaringSyntaxReferences.First().GetSyntax(context.CancellationToken).FirstAncestorOrSelf<TClassDeclarationSyntax>();
            var generator = context.Document?.Project?.LanguageServices?.GetService<SyntaxGenerator>();
            if (generator != null)
            {
                SyntaxNode addedMember;
                var bodyStatements = ImmutableArray.Create(
                    generator.ThrowStatement(generator.ObjectCreationExpression(semanticModel.Compilation.GetTypeByMetadataName("System.NotImplementedException"))));
                if (OperatorOverloadsHaveNamedAlternatesAnalyzer.IsPropertyExpected(operatorOverloadSymbol.Name))
                {
                    // add a property
                    addedMember = generator.PropertyDeclaration(
                        name: OperatorOverloadsHaveNamedAlternatesAnalyzer.IsTrueText,
                        type: generator.TypeExpression(SpecialType.System_Boolean),
                        accessibility: Accessibility.Public,
                        modifiers: DeclarationModifiers.ReadOnly,
                        getAccessorStatements: bodyStatements);
                }
                else
                {
                    // add a method
                    var expectedSignature = GetExpectedMethodSignature(operatorOverloadSymbol, semanticModel.Compilation);
                    addedMember = generator.MethodDeclaration(
                        name: expectedSignature.Name,
                        parameters: expectedSignature.Parameters.Select(p => generator.ParameterDeclaration(p.Item1, generator.TypeExpression(p.Item2))),
                        returnType: generator.TypeExpression(expectedSignature.ReturnType),
                        accessibility: Accessibility.Public,
                        modifiers: expectedSignature.IsStatic ? DeclarationModifiers.Static : DeclarationModifiers.None,
                        statements: bodyStatements);
                }

                var newTypeDeclaration = generator.AddMembers(typeDeclaration, addedMember);
                context.RegisterCodeFix(new MyCodeAction(ct => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(typeDeclaration, newTypeDeclaration)))),
                    diagnostic);
            }
        }

        private static async Task FixFriendlyVisibility(CodeFixContext context, SyntaxNode node, SemanticModel semanticModel, Diagnostic diagnostic)
        {
            var badVisibilityNode = (SyntaxNode)node.FirstAncestorOrSelf<TMethodSyntaxKind>() ?? node.FirstAncestorOrSelf<IPropertySyntaxKind>();
            var badVisibilitySymbol = semanticModel.GetDeclaredSymbol(badVisibilityNode, context.CancellationToken);
            var symbolEditor = SymbolEditor.Create(context.Document);
            var newSymbol = await symbolEditor.EditOneDeclarationAsync(badVisibilitySymbol,
                (documentEditor, syntaxNode, ct) => Task.Run(() => documentEditor.SetAccessibility(badVisibilityNode, Accessibility.Public))).ConfigureAwait(false);
            var newDocument = symbolEditor.GetChangedDocuments().Single();
            var newRoot = await newDocument.GetSyntaxRootAsync(context.CancellationToken);
            context.RegisterCodeFix(new MyCodeAction(ct => Task.FromResult(context.Document.WithSyntaxRoot(newRoot))), diagnostic);
        }

        private static ExpectedMethodSignature GetExpectedMethodSignature(IMethodSymbol operatorOverloadSymbol, Compilation compilation)
        {
            var containingType = (ITypeSymbol)operatorOverloadSymbol.ContainingType;
            var returnType = operatorOverloadSymbol.ReturnType;
            var expectedName = OperatorOverloadsHaveNamedAlternatesAnalyzer.GetExpectedAlternateMethodGroup(operatorOverloadSymbol.Name, returnType).AlternateMethod1;
            switch (operatorOverloadSymbol.Name)
            {
                case "op_GreaterThan":
                case "op_GreaterThanOrEqual":
                case "op_LessThan":
                case "op_LessThanOrEqual":
                    // e.g., public int CompareTo(MyClass other)
                    var intType = compilation.GetSpecialType(SpecialType.System_Int32);
                    return new ExpectedMethodSignature(expectedName, intType, ImmutableArray.Create(Tuple.Create("other", containingType)), isStatic: false);
                case "op_Decrement":
                case "op_Increment":
                case "op_UnaryNegation":
                case "op_UnaryPlus":
                    // e.g., public MyClass Decrement(MyClass item)
                    return new ExpectedMethodSignature(expectedName, returnType, ImmutableArray.Create(Tuple.Create("item", containingType)));
                case "op_Implicit":
                    // e.g., public int ToInt32()
                    return new ExpectedMethodSignature(expectedName, returnType, ImmutableArray.Create<Tuple<string, ITypeSymbol>>(), isStatic: false);
                default:
                    // e.g., public static MyClass Add(MyClass left, MyClass right)
                    return new ExpectedMethodSignature(expectedName, returnType, ImmutableArray.Create(Tuple.Create("left", containingType), Tuple.Create("right", containingType)));
            }
        }

        private class ExpectedMethodSignature
        {
            public string Name { get; }
            public ITypeSymbol ReturnType { get; }
            public IEnumerable<Tuple<string, ITypeSymbol>> Parameters { get; }
            public bool IsStatic { get; }

            public ExpectedMethodSignature(string name, ITypeSymbol returnType, IEnumerable<Tuple<string, ITypeSymbol>> parameters, bool isStatic = true)
            {
                Name = name;
                ReturnType = returnType;
                Parameters = parameters;
                IsStatic = isStatic;
            }
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesTitle, createChangedDocument)
            {
            }
        }
    }
}