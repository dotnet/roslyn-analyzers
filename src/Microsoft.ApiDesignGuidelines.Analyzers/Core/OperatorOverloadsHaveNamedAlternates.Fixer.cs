// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2225: Operator overloads have named alternates
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class OperatorOverloadsHaveNamedAlternatesFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            return Task.Run(() => context.RegisterCodeFix(new MyCodeAction(ct => Fix(context, ct)), context.Diagnostics.First()));
        }

        private static async Task<Document> Fix(CodeFixContext context, CancellationToken cancellationToken)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var generator = context.Document.Project?.LanguageServices?.GetService<SyntaxGenerator>();
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var diagnostic = context.Diagnostics.First();
            switch (diagnostic.Properties[OperatorOverloadsHaveNamedAlternatesAnalyzer.DiagnosticKindText])
            {
                case OperatorOverloadsHaveNamedAlternatesAnalyzer.AddAlternateText:
                    var methodDeclaration = generator.GetDeclaration(node, DeclarationKind.Operator) ?? generator.GetDeclaration(node, DeclarationKind.ConversionOperator);
                    var operatorOverloadSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
                    var typeSymbol = (ITypeSymbol)operatorOverloadSymbol.ContainingSymbol;
                    var typeDeclarationSyntax = typeSymbol.DeclaringSyntaxReferences.First().GetSyntax(context.CancellationToken);
                    var typeDeclaration = generator.GetDeclaration(typeDeclarationSyntax, DeclarationKind.Class);

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
                    return context.Document.WithSyntaxRoot(root.ReplaceNode(typeDeclaration, newTypeDeclaration));
                case OperatorOverloadsHaveNamedAlternatesAnalyzer.FixVisibilityText:
                    var badVisibilityNode = generator.GetDeclaration(node, DeclarationKind.Method) ?? generator.GetDeclaration(node, DeclarationKind.Property);
                    var badVisibilitySymbol = semanticModel.GetDeclaredSymbol(badVisibilityNode, context.CancellationToken);
                    var symbolEditor = SymbolEditor.Create(context.Document);
                    var newSymbol = await symbolEditor.EditOneDeclarationAsync(badVisibilitySymbol,
                        (documentEditor, syntaxNode, ct) => Task.Run(() => documentEditor.SetAccessibility(badVisibilityNode, Accessibility.Public))).ConfigureAwait(false);
                    var newDocument = symbolEditor.GetChangedDocuments().Single();
                    var newRoot = await newDocument.GetSyntaxRootAsync(context.CancellationToken);
                    return context.Document.WithSyntaxRoot(newRoot);
                default:
                    return context.Document;
            }
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
            public override string EquivalenceKey => MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesTitle;

            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesTitle, createChangedDocument)
            {
            }
        }
    }
}