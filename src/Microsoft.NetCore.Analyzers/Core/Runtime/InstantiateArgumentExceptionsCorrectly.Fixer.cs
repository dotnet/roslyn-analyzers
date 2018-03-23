// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.NetCore.Analyzers.Runtime.InstantiateArgumentExceptionsCorrectlyAnalyzer;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    public abstract class InstantiateArgumentExceptionsCorrectlyFixer : CodeFixProvider
    {
        private class SemanticParameterComparer : IEqualityComparer<IParameterSymbol>
        {
            public static readonly SemanticParameterComparer Instance = new SemanticParameterComparer();

            private SemanticParameterComparer()
            {
            }

            public bool Equals(IParameterSymbol x, IParameterSymbol y)
            {
                return x.Name == y.Name && x.Type == y.Type;
            }

            public int GetHashCode(IParameterSymbol obj)
            {
                return unchecked(obj.Name.GetHashCode() * 33 + obj.Type.GetHashCode());
            }
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxNode creationSyntax = root.FindNode(context.Span, getInnermostNodeForTie: true);
            var diagnostic = context.Diagnostics.First();
            var problemKind = GetProblemKind(diagnostic);

            switch (problemKind)
            {
                case ProblemKind.IncorrectMessage:
                    await RegisterIncorrectMessageCodeFixesAsync().ConfigureAwait(false);
                    break;
                case ProblemKind.IncorrectParameterName:
                    await RegisterIncorrectParameterNameCodeFixesAsync().ConfigureAwait(false);
                    break;
                case ProblemKind.NoArguments:
                    await RegisterNoArgumentsCodeFixesAsync().ConfigureAwait(false);
                    break;
                case ProblemKind.SwappedMessageAndParameterName:
                    await RegisterSwappedMessageAndParameterNameCodeFixesAsync().ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException($"Unrecognized {nameof(ProblemKind)} value: {problemKind}");
            }

            Task RegisterSwappedMessageAndParameterNameCodeFixesAsync()
            {
                var creationOperation = (IObjectCreationOperation)semanticModel.GetOperation(creationSyntax, context.CancellationToken);
                IArgumentOperation messageArgument = creationOperation.Arguments.FirstOrDefault(arg => IsMessage(arg.Parameter));
                IArgumentOperation parameterNameArgument = creationOperation.Arguments.FirstOrDefault(arg => IsParameterName(arg.Parameter));

                string title = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyCodeFixSwappedMessageAndParameterName;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedDocument: async ct => await SwapNodesAsync(document, messageArgument.Syntax, parameterNameArgument.Syntax, ct).ConfigureAwait(false),
                        equivalenceKey: title),
                    context.Diagnostics);

                return Task.CompletedTask;
            }

            async Task RegisterIncorrectMessageCodeFixesAsync()
            {
                // This is triggered for expressions like 'new ArgumentException(nameof(param))'.
                // 'nameof(param)' is the "bad argument" because it represents a parameter name, but is being passed to the 'message' argument.
                // The codefix replaces the expression with 'new ArgumentException("message", nameof(param))'.
                // We switch from calling .ctor(message) => .ctor(message, paramName).
                await RegisterIncorrectMessageOrParameterNameCodeFixesAsync(
                    isBadArgument: arg => IsMessage(arg.Parameter),
                    isReplacementConstructor: IsMatchingConstructorWithParameterNameAfterMessage,
                    title: SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyCodeFixIncorrectMessage
                    ).ConfigureAwait(false);
            }

            async Task RegisterIncorrectParameterNameCodeFixesAsync()
            {
                // This is triggered for expressions like 'new ArgumentNullException("message")'.
                // '"message"' is the "bad argument" because it represents a message, but is being passed to the 'paramName' argument.
                // The codefix replaces the expression with 'new ArgumentException("paramName", "message")'.
                // We switch from calling .ctor(paramName) => .ctor(paramName, message).
                await RegisterIncorrectMessageOrParameterNameCodeFixesAsync(
                    isBadArgument: arg => IsParameterName(arg.Parameter),
                    isReplacementConstructor: IsMatchingConstructorWithMessageAfterParameterName,
                    title: SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName
                    ).ConfigureAwait(false);
            }

            Task RegisterIncorrectMessageOrParameterNameCodeFixesAsync(
                Func<IArgumentOperation, bool> isBadArgument,
                Func<IMethodSymbol, IMethodSymbol, bool> isReplacementConstructor,
                string title)
            {
                var creationOperation = (IObjectCreationOperation)semanticModel.GetOperation(creationSyntax, context.CancellationToken);
                IArgumentOperation badArgument = creationOperation.Arguments.FirstOrDefault(isBadArgument);
                if (badArgument == null)
                {
                    return Task.CompletedTask;
                }

                var constructor = creationOperation.Constructor;
                var overloads = constructor.GetOverloads();

                var replacementConstructor = overloads.FirstOrDefault(ctor => isReplacementConstructor(ctor, constructor));
                if (replacementConstructor == null)
                {
                    return Task.CompletedTask;
                }

                var replacementCreationSyntax = MoveArgumentToNextParameter(
                    creationSyntax,
                    argument: badArgument.Syntax,
                    newArgument: badArgument.Parameter?.Name);
                
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedDocument: async ct => await ReplaceNodeAsync(document, root, creationSyntax, replacementCreationSyntax, ct).ConfigureAwait(false),
                        equivalenceKey: title),
                    context.Diagnostics);

                return Task.CompletedTask;
            }

            Task RegisterNoArgumentsCodeFixesAsync()
            {
                IEnumerable<IMethodSymbol> constructors = GetCandidateReplacementConstructors(creationSyntax, semanticModel);
                IEnumerable<IParameterSymbol> parametersForNameOf = GetCandidateParametersForNameOf(creationSyntax, semanticModel, context.CancellationToken);
                IEnumerable<string> parameterNames = parametersForNameOf.Select(p => p.Name);

                foreach (IMethodSymbol constructor in constructors)
                {
                    string title = string.Format(
                        SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyCodeFixNoArguments,
                        constructor.ToMinimalDisplayString(semanticModel, creationSyntax.SpanStart));

                    if (!constructor.Parameters.Any(IsParameterName) || !parameterNames.Any())
                    {
                        // - If this constructor overload does not have a 'paramName' parameter, register a single codefix.
                        // - If this constructor overload does have a 'paramName' parameter but there are no eligible parameters
                        //   for nameof, pass "paramName" instead of a nameof expression.
                        RegisterCodeFix(title, constructor, parameterNameOpt: null);
                    }
                    else
                    {
                        // If this constructor overload does have a 'paramName' parameter and there are eligible parameters
                        // for nameof, register multiple codefixes, one for each eligible parameter.
                        foreach (string parameterName in parameterNames)
                        {
                            RegisterCodeFix(title, constructor, parameterName);
                        }
                    }
                }

                return Task.CompletedTask;

                void RegisterCodeFix(string title, IMethodSymbol constructor, string parameterNameOpt)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title,
                            async ct => await PopulateConstructorInvocationAsync(document, creationSyntax, constructor, parameterNameOpt, ct).ConfigureAwait(false),
                            // Codefixes that call the same constructor but with different parameters passed to nameof will have the same title.
                            equivalenceKey: title + parameterNameOpt),
                        context.Diagnostics);
                }
            }
        }

        private static ProblemKind GetProblemKind(Diagnostic diagnostic)
        {
            return EnumHelpers.Parse<ProblemKind>(diagnostic.Properties[ProblemKindProperty]);
        }

        #region Incorrect message / parameter name

        protected abstract SyntaxNode MoveArgumentToNextParameter(
            SyntaxNode creation,
            SyntaxNode argument,
            string newArgument);

        /// <summary>
        /// Returns <c>true</c> if <paramref name="candidate"/> has the same method signature as <paramref name="original"/>,
        /// except the former has an extra 'paramName' parameter after the 'message' parameter.
        /// </summary>
        private static bool IsMatchingConstructorWithParameterNameAfterMessage(
            IMethodSymbol candidate,
            IMethodSymbol original)
        {
            return IsMatchingConstructorWithYAfterX(
                candidate,
                original,
                xPredicate: IsMessage,
                yPredicate: IsParameterName);
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="candidate"/> has the same method signature as <paramref name="original"/>,
        /// except the former has an extra 'message' parameter after the 'paramName' parameter.
        /// </summary>
        private static bool IsMatchingConstructorWithMessageAfterParameterName(
            IMethodSymbol candidate,
            IMethodSymbol original)
        {
            return IsMatchingConstructorWithYAfterX(
                candidate,
                original,
                xPredicate: IsParameterName,
                yPredicate: IsMessage);
        }

        private static bool IsMatchingConstructorWithYAfterX(
            IMethodSymbol candidate,
            IMethodSymbol original,
            Func<IParameterSymbol, bool> xPredicate,
            Func<IParameterSymbol, bool> yPredicate)
        {
            var originalParameters = original.Parameters;
            var candidateParameters = candidate.Parameters;

            if (candidateParameters.Length != originalParameters.Length + 1)
            {
                return false;
            }

            IParameterSymbol originalX = originalParameters.FirstOrDefault(xPredicate);
            IParameterSymbol candidateX = candidateParameters.FirstOrDefault(xPredicate);
            if (originalX == null || candidateX == null)
            {
                return false;
            }

            int originalXIndex = originalX.Ordinal;
            int candidateXIndex = candidateX.Ordinal;

            int yIndex = candidateXIndex + 1;
            bool isYAfterX =
                yIndex < candidateParameters.Length &&
                yPredicate(candidateParameters[yIndex]);

            return isYAfterX && candidateParameters.RemoveAt(yIndex).SequenceEqual(originalParameters, SemanticParameterComparer.Instance);
        }

        private static Task<Document> ReplaceNodeAsync(Document document, SyntaxNode root, SyntaxNode oldNode, SyntaxNode newNode, CancellationToken cancellationToken)
        {
            SyntaxNode newRoot = root.ReplaceNode(oldNode, newNode);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        private static async Task<Document> SwapNodesAsync(Document document, SyntaxNode first, SyntaxNode second, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(first, second);
            editor.ReplaceNode(second, first);
            return editor.GetChangedDocument();
        }

        #endregion

        #region No arguments

        protected virtual SyntaxNode GetNameOfExpression(SyntaxGenerator generator, string identifierNameArgument)
            => generator.NameOfExpression(generator.IdentifierName(identifierNameArgument));

        protected abstract SyntaxNode GetParameterUsageAnalysisScope(SyntaxNode creation);

        private IEnumerable<IParameterSymbol> GetCandidateParametersForNameOf(
            SyntaxNode creation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            IMethodSymbol containingMethod = creation.GetContainingMethodSymbol(semanticModel, cancellationToken);

            SyntaxNode analysisScope = GetParameterUsageAnalysisScope(creation);
            if (analysisScope == null)
            {
                return Enumerable.Empty<IParameterSymbol>();
            }

            DataFlowAnalysis analysis = semanticModel.AnalyzeDataFlow(analysisScope);
            if (!analysis.Succeeded)
            {
                return Enumerable.Empty<IParameterSymbol>();
            }

            var usedParameters = analysis.ReadInside.OfType<IParameterSymbol>();
            return usedParameters.Intersect(containingMethod.Parameters);
        }

        private static IEnumerable<IMethodSymbol> GetCandidateReplacementConstructors(
            SyntaxNode creation,
            SemanticModel semanticModel)
        {
            var emptyConstructor = (IMethodSymbol)semanticModel.GetSymbolInfo(creation).Symbol;
            Debug.Assert(emptyConstructor.Parameters.IsEmpty);

            INamedTypeSymbol exceptionType = emptyConstructor.ContainingType;
            foreach (IMethodSymbol constructor in exceptionType.Constructors)
            {
                if (constructor == emptyConstructor)
                {
                    continue;
                }

                var parameters = constructor.Parameters;
                switch (parameters.Length)
                {
                    case 1:
                        if (IsMessage(parameters[0]) || IsParameterName(parameters[0]))
                        {
                            yield return constructor;
                        }
                        break;
                    case 2:
                        if ((IsMessage(parameters[0]) && IsParameterName(parameters[1])) ||
                            (IsParameterName(parameters[0]) && IsMessage(parameters[1])))
                        {
                            yield return constructor;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task<Document> PopulateConstructorInvocationAsync(
            Document document,
            SyntaxNode creation,
            IMethodSymbol replacementConstructor,
            string parameterNameOpt,
            CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator g = editor.Generator;

            SyntaxNode newCreation = g.ObjectCreationExpression(
                replacementConstructor.ContainingType,
                GetReplacementConstructorArguments()).WithTriviaFrom(creation);

            editor.ReplaceNode(creation, newCreation);
            return editor.GetChangedDocument();

            IEnumerable<SyntaxNode> GetReplacementConstructorArguments()
            {
                foreach (IParameterSymbol parameter in replacementConstructor.Parameters)
                {
                    if (parameterNameOpt != null && IsParameterName(parameter))
                    {
                        // Pass in nameof([parameter]) for the 'paramName' parameter when there are eligible parameters for nameof.
                        yield return GetNameOfExpression(g, parameterNameOpt);
                    }
                    else
                    {
                        // Pass in "message" or "paramName" for the 'message' or 'paramName' parameters, respectively.
                        yield return g.LiteralExpression(parameter.Name);
                    }
                }
            }
        }

        #endregion
    }
}