// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// </summary>
    public abstract class ForwardCancellationTokenToAsyncMethodsFixer : CodeFixProvider
    {
        protected const string CancellationTokenName = "CancellationToken";

        // Looks for a ct parameter in the ancestor method or function declaration. If one is found, retrieve the name of the parameter.
        // Returns true if a ct parameter was found and parameterName is not null or empty. Returns false otherwise.
        protected abstract bool TryGetAncestorDeclarationCancellationTokenParameterName(SemanticModel model, SyntaxNode node, out string? parameterName);

        protected static bool IsCancellationTokenParameter(SemanticModel model, SyntaxNode parameter)
        {
            return model.GetDeclaredSymbol(parameter) is IParameterSymbol parameterSymbol &&
                   parameterSymbol.Type.Name.Equals(CancellationTokenName, StringComparison.Ordinal);
        }

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ForwardCancellationTokenToAsyncMethodsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document doc = context.Document;
            CancellationToken ct = context.CancellationToken;
            SyntaxNode root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

            if (!(root.FindNode(context.Span, getInnermostNodeForTie: true) is SyntaxNode node))
            {
                return;
            }

            SemanticModel model = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false);
            if (!(model.GetOperation(node, ct) is IInvocationOperation invocation))
            {
                return;
            }

            if (!TryGetAncestorDeclarationCancellationTokenParameterName(model, node, out string? parameterName))
            {
                return;
            }

            string title = MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToAsyncMethodsTitle;

            Task<Document> createChangedDocument(CancellationToken _) => FixInvocation(doc, root, invocation, parameterName!);

            context.RegisterCodeFix(
                new MyCodeAction(
                    title: title,
                    createChangedDocument,
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static Task<Document> FixInvocation(Document doc, SyntaxNode root, IInvocationOperation invocation, string cancellationTokenParamenterName)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            // Create a new ct argument to pass to the invocation, using the ancestor method parameter name
            SyntaxNode cancellationTokenIdentifier = generator.IdentifierName(cancellationTokenParamenterName);
            SyntaxNode cancellationTokenNode = generator.Argument(cancellationTokenIdentifier);

            // Pass the same arguments and add the ct
            List<SyntaxNode> newArguments = new List<SyntaxNode>();
            foreach (IArgumentOperation argument in invocation.Arguments)
            {
                if (argument.Parameter.Type.Name != CancellationTokenName || !argument.IsImplicit)
                {
                    newArguments.Add(argument.Syntax);
                }
            }
            newArguments.Add(cancellationTokenNode);


            SyntaxNode newInvocation;
            // The instance is null when calling a static method from another type
            if (invocation.Instance == null)
            {
                SyntaxNode staticType = generator.TypeExpressionForStaticMemberAccess(invocation.TargetMethod.ContainingType);
                newInvocation = generator.MemberAccessExpression(staticType, invocation.TargetMethod.Name);
            }
            // The instance is implicit when calling a method from the same type, call the method directly
            else if (invocation.Instance.IsImplicit)
            {
                newInvocation = invocation.GetInstance(); // GetInstance will return the method name
            }
            // Calling a method from an object, we must include the instance variable name
            else
            {
                newInvocation = generator.MemberAccessExpression(invocation.GetInstance(), invocation.TargetMethod.Name);
            }
            // Insert the new arguments to the new invocation
            SyntaxNode newInvocationWithArguments = generator.InvocationExpression(newInvocation, newArguments);

            SyntaxNode newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocationWithArguments);
            return Task.FromResult(doc.WithSyntaxRoot(newRoot));
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192) 
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}