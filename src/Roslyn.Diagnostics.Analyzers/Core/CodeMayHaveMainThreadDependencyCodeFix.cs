// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using ContextDependency = Roslyn.Diagnostics.Analyzers.AbstractThreadDependencyAnalyzer.ContextDependency;

namespace Roslyn.Diagnostics.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(CodeMayHaveMainThreadDependencyCodeFix))]
    public class CodeMayHaveMainThreadDependencyCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RoslynDiagnosticIds.CodeMayHaveMainThreadDependencyRuleId);

        public sealed override FixAllProvider GetFixAllProvider()
            => null;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Properties.TryGetValue(nameof(CodeMayHaveMainThreadDependency.Scenario), out var scenario))
                {
                    continue;
                }

                string title;
                Func<CancellationToken, Task<Solution>> createChangedSolution;
                switch (scenario)
                {
                    case CodeMayHaveMainThreadDependency.Scenario.ContainingMethodShouldCaptureContext:
                        title = RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyCodeFix_ContainingMethodShouldCaptureContext;
                        createChangedSolution = cancellationToken => MarkContainerAsCapturesContextAsync(context.Document, diagnostic.AdditionalLocations[0].SourceSpan, cancellationToken);
                        break;

                    case CodeMayHaveMainThreadDependency.Scenario.ContainingMethodShouldBePerInstance:
                        title = RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyCodeFix_ContainingMethodShouldBePerInstance;
                        createChangedSolution = cancellationToken => MarkContainerAsPerInstanceAsync(context.Document, diagnostic.AdditionalLocations[0].SourceSpan, cancellationToken);
                        break;

                    case CodeMayHaveMainThreadDependency.Scenario.TargetMissingAttribute:
                        title = RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyCodeFix_TargetMissingAttribute;
                        var contextDependency = ContextDependency.None;
                        if (diagnostic.Properties.TryGetValue(nameof(ContextDependency), out var contextDependencyString))
                        {
                            contextDependency = (ContextDependency)Enum.Parse(typeof(ContextDependency), contextDependencyString);
                        }

                        var perInstance = diagnostic.Properties.TryGetValue("PerInstance", out var perInstanceString)
                            && perInstanceString == bool.TrueString;

                        createChangedSolution = cancellationToken => MarkTargetWithMainThreadDependencyAsync(context.Document, diagnostic.Location.SourceSpan, contextDependency, perInstance, cancellationToken);
                        break;

                    default:
                        continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedSolution,
                        equivalenceKey: scenario),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Solution> MarkContainerAsCapturesContextAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var obsoleteAttributeApplication = root.FindNode(sourceSpan, getInnermostNodeForTie: true);

            var generator = SyntaxGenerator.GetGenerator(document);

            var declaration = obsoleteAttributeApplication;
            var declarationKind = generator.GetDeclarationKind(declaration);
            while (declarationKind != DeclarationKind.Attribute)
            {
                declaration = generator.GetDeclaration(declaration.Parent);
                if (declaration is null)
                {
                    return document.Project.Solution;
                }

                declarationKind = generator.GetDeclarationKind(declaration);
            }

            var allArguments = generator.GetAttributeArguments(declaration);
            if (allArguments.Count > 0)
            {
                var capturesContextArgument = GenerateCapturesContextArgument(generator);
                return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, root.ReplaceNode(allArguments[0], capturesContextArgument));
            }
            else
            {
                var capturesContextArgument = GenerateCapturesContextArgument(generator);
                var newDeclaration = generator.AddAttributeArguments(declaration, new[] { capturesContextArgument });
                return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, root.ReplaceNode(declaration, newDeclaration));
            }

            // Local functions
            static SyntaxNode GenerateCapturesContextArgument(SyntaxGenerator generator)
                => generator.AttributeArgument(generator.MemberAccessExpression(generator.IdentifierName("ContextDependency"), "Context"));
        }

        private async Task<Solution> MarkContainerAsPerInstanceAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var obsoleteAttributeApplication = root.FindNode(sourceSpan, getInnermostNodeForTie: true);

            var generator = SyntaxGenerator.GetGenerator(document);

            var declaration = obsoleteAttributeApplication;
            var declarationKind = generator.GetDeclarationKind(declaration);
            while (declarationKind != DeclarationKind.Attribute)
            {
                declaration = generator.GetDeclaration(declaration.Parent);
                if (declaration is null)
                {
                    return document.Project.Solution;
                }

                declarationKind = generator.GetDeclarationKind(declaration);
            }

            var perInstanceArgument = GeneratePerInstanceArgument(generator);
            var newDeclaration = generator.AddAttributeArguments(declaration, new[] { perInstanceArgument });
            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, root.ReplaceNode(declaration, newDeclaration));

            // Local functions
            static SyntaxNode GeneratePerInstanceArgument(SyntaxGenerator generator)
                => generator.AttributeArgument("PerInstance", generator.TrueLiteralExpression());
        }

        private async Task<Solution> MarkTargetWithMainThreadDependencyAsync(Document document, TextSpan sourceSpan, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = root.FindNode(sourceSpan, getInnermostNodeForTie: true);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var operation = semanticModel.GetOperation(syntaxNode, cancellationToken);
            if (operation is null)
            {
                return document.Project.Solution;
            }

            var target = GetOperationTarget(operation);
            if (target is null)
            {
                return document.Project.Solution;
            }

            switch (target)
            {
                case IFieldSymbol field:
                    return await MarkFieldWithMainThreadDependencyAsync(document.Project.Solution, field, contextDependency, perInstance, cancellationToken).ConfigureAwait(false);

                case IPropertySymbol property:
                    return await MarkPropertyWithMainThreadDependencyAsync(document.Project.Solution, property, contextDependency, perInstance, cancellationToken).ConfigureAwait(false);

                case IEventSymbol @event:
                    return await MarkEventWithMainThreadDependencyAsync(document.Project.Solution, @event, contextDependency, perInstance, cancellationToken).ConfigureAwait(false);

                case IMethodSymbol method:
                    return await MarkMethodWithMainThreadDependencyAsync(document.Project.Solution, method, contextDependency, perInstance, cancellationToken).ConfigureAwait(false);

                case ITypeSymbol type:
                    return await MarkTypeWithMainThreadDependencyAsync(document.Project.Solution, type, contextDependency, perInstance, cancellationToken).ConfigureAwait(false);

                case IParameterSymbol parameter:
                    return await MarkParameterWithMainThreadDependencyAsync(document.Project.Solution, parameter, contextDependency, perInstance, cancellationToken).ConfigureAwait(false);

                default:
                    return document.Project.Solution;
            }
        }

        private static async Task<Solution> MarkFieldWithMainThreadDependencyAsync(Solution solution, IFieldSymbol field, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var declarationReference = field.DeclaringSyntaxReferences.FirstOrDefault(reference => reference.SyntaxTree is object);
            if (declarationReference is null)
            {
                return solution;
            }

            var root = await declarationReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = await declarationReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(solution.GetDocument(declarationReference.SyntaxTree));

            syntaxNode = generator.GetContainingDeclaration(syntaxNode, DeclarationKind.Field);
            if (syntaxNode is null)
            {
                return solution;
            }

            var attribute = CreateAttribute(generator, contextDependency, perInstance);
            var newNode = generator.AddAttributes(syntaxNode, attribute);
            var newRoot = root.ReplaceNode(syntaxNode, newNode);

            return solution.WithDocumentSyntaxRoot(solution.GetDocumentId(declarationReference.SyntaxTree), newRoot);
        }

        private static async Task<Solution> MarkPropertyWithMainThreadDependencyAsync(Solution solution, IPropertySymbol property, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var declarationReference = property.DeclaringSyntaxReferences.FirstOrDefault(reference => reference.SyntaxTree is object);
            if (declarationReference is null)
            {
                return solution;
            }

            var root = await declarationReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = await declarationReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(solution.GetDocument(declarationReference.SyntaxTree));

            var attribute = CreateAttribute(generator, contextDependency, perInstance);
            var newNode = generator.AddAttributes(syntaxNode, attribute);
            var newRoot = root.ReplaceNode(syntaxNode, newNode);

            return solution.WithDocumentSyntaxRoot(solution.GetDocumentId(declarationReference.SyntaxTree), newRoot);
        }

        private static async Task<Solution> MarkEventWithMainThreadDependencyAsync(Solution solution, IEventSymbol @event, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var declarationReference = @event.DeclaringSyntaxReferences.FirstOrDefault(reference => reference.SyntaxTree is object);
            if (declarationReference is null)
            {
                return solution;
            }

            var root = await declarationReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = await declarationReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(solution.GetDocument(declarationReference.SyntaxTree));

            var attribute = CreateAttribute(generator, contextDependency, perInstance);
            var newNode = generator.AddAttributes(syntaxNode, attribute);
            var newRoot = root.ReplaceNode(syntaxNode, newNode);

            return solution.WithDocumentSyntaxRoot(solution.GetDocumentId(declarationReference.SyntaxTree), newRoot);
        }

        private static async Task<Solution> MarkMethodWithMainThreadDependencyAsync(Solution solution, IMethodSymbol method, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var declarationReference = method.DeclaringSyntaxReferences.FirstOrDefault(reference => reference.SyntaxTree is object);
            if (declarationReference is null)
            {
                return solution;
            }

            var root = await declarationReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = await declarationReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(solution.GetDocument(declarationReference.SyntaxTree));

            var attribute = CreateAttribute(generator, contextDependency, perInstance);
            var newNode = generator.AddReturnAttributes(syntaxNode, attribute);
            var newRoot = root.ReplaceNode(syntaxNode, newNode);

            return solution.WithDocumentSyntaxRoot(solution.GetDocumentId(declarationReference.SyntaxTree), newRoot);
        }

        private static async Task<Solution> MarkTypeWithMainThreadDependencyAsync(Solution solution, ITypeSymbol type, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var declarationReference = type.DeclaringSyntaxReferences.FirstOrDefault(reference => reference.SyntaxTree is object);
            if (declarationReference is null)
            {
                return solution;
            }

            var root = await declarationReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = await declarationReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(solution.GetDocument(declarationReference.SyntaxTree));

            var attribute = CreateAttribute(generator, contextDependency, perInstance);
            var newNode = generator.AddAttributes(syntaxNode, attribute);
            var newRoot = root.ReplaceNode(syntaxNode, newNode);

            return solution.WithDocumentSyntaxRoot(solution.GetDocumentId(declarationReference.SyntaxTree), newRoot);
        }

        private static async Task<Solution> MarkParameterWithMainThreadDependencyAsync(Solution solution, IParameterSymbol parameter, ContextDependency contextDependency, bool perInstance, CancellationToken cancellationToken)
        {
            var declarationReference = parameter.DeclaringSyntaxReferences.FirstOrDefault(reference => reference.SyntaxTree is object);
            if (declarationReference is null)
            {
                return solution;
            }

            var root = await declarationReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxNode = await declarationReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(solution.GetDocument(declarationReference.SyntaxTree));

            var attribute = CreateAttribute(generator, contextDependency, perInstance);
            var newNode = generator.AddAttributes(syntaxNode, attribute);
            var newRoot = root.ReplaceNode(syntaxNode, newNode);

            return solution.WithDocumentSyntaxRoot(solution.GetDocumentId(declarationReference.SyntaxTree), newRoot);
        }

        private static SyntaxNode CreateAttribute(SyntaxGenerator generator, ContextDependency contextDependency, bool perInstance)
        {
            var attribute = generator.Attribute(
                "Roslyn.Utilities.ThreadDependencyAttribute",
                generator.AttributeArgument(generator.MemberAccessExpression(generator.IdentifierName("ContextDependency"), contextDependency.ToString())),
                generator.AttributeArgument("Verified", generator.FalseLiteralExpression()));

            if (perInstance)
            {
                attribute = generator.InsertAttributeArguments(attribute, 1, new[] { generator.AttributeArgument("PerInstance", generator.TrueLiteralExpression()) });
            }

            return attribute;
        }

        private ISymbol GetOperationTarget(IOperation operation)
        {
            switch (operation)
            {
                case IInvocationOperation invocation:
                    if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                    {
                        return GetOperationTarget(invocation.Instance);
                    }
                    else
                    {
                        return invocation.TargetMethod;
                    }

                case IFieldReferenceOperation fieldReference:
                    return fieldReference.Field;

                case IPropertyReferenceOperation propertyReference:
                    return propertyReference.Property;

                case IParameterReferenceOperation parameterReference:
                    return parameterReference.Parameter;

                case IAwaitOperation awaitOperation:
                    return GetOperationTarget(awaitOperation.Operation);

                case IReturnOperation returnOperation:
                    return GetOperationTarget(returnOperation.ReturnedValue);

                default:
                    return null;
            }
        }
    }
}
