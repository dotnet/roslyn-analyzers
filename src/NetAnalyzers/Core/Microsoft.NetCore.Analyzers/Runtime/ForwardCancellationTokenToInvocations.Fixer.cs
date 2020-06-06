// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
    public abstract class ForwardCancellationTokenToInvocationsFixer : CodeFixProvider
    {
        // Attempts to retrieve the invocation from the current operation.
        protected abstract bool TryGetInvocation(
            SemanticModel model,
            SyntaxNode node,
            CancellationToken ct,
            [NotNullWhen(returnValue: true)] out IInvocationOperation? invocation);

        // Looks for a ct parameter in the ancestor method or function declaration. If one is found, retrieve the name of the parameter.
        // Returns true if a ct parameter was found and parameterName is not null or empty. Returns false otherwise.
        protected abstract bool TryGetAncestorDeclarationCancellationTokenParameterName(
            SyntaxNode node,
            [NotNullWhen(returnValue: true)] out string? parameterName);

        // Verifies if the specified argument was passed with an explicit name.
        protected abstract bool IsArgumentNamed(IArgumentOperation argumentOperation);

        // Retrieves the invocation expression for a conditional operation, which consists of the dot and the method name.
        protected abstract SyntaxNode GetConditionalOperationInvocationExpression(SyntaxNode invocationNode);

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ForwardCancellationTokenToInvocationsAnalyzer.RuleId);

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

            // The analyzer created the diagnostic on the IdentifierNameSyntax, and the parent is the actual invocation
            if (!TryGetInvocation(model, node, ct, out IInvocationOperation? invocation))
            {
                return;
            }

            if (!TryGetAncestorDeclarationCancellationTokenParameterName(node, out string? parameterName))
            {
                return;
            }

            string title = MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToInvocationsTitle;

            if (!TryGenerateNewDocumentRoot(doc, root, invocation, parameterName, out SyntaxNode? newRoot))
            {
                return;
            }

            Task<Document> createChangedDocument(CancellationToken _) =>
                Task.FromResult(doc.WithSyntaxRoot(newRoot));

            context.RegisterCodeFix(
                new MyCodeAction(
                    title: title,
                    createChangedDocument,
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private bool TryGenerateNewDocumentRoot(
            Document doc,
            SyntaxNode root,
            IInvocationOperation invocation,
            string cancellationTokenParameterName,
            [NotNullWhen(returnValue: true)] out SyntaxNode? newRoot)
        {
            newRoot = null;

            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            // Pass the same arguments and add the ct
            List<SyntaxNode> newArguments = new List<SyntaxNode>();
            bool shouldTokenUseName = false;
            string paramName = string.Empty;

            // In C#, invocation.Arguments contains the arguments in the order passed by the user
            // In VB, invocation.Arguments contains the arguments in the official parameter order
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                IArgumentOperation argument = invocation.Arguments[i];

                // The type name is detected even if using an alias
                if (!argument.Parameter.Type.Name.Equals("CancellationToken", StringComparison.Ordinal))
                {
                    if (!argument.IsImplicit)
                    {
                        SyntaxNode newArg;
                        if (IsArgumentNamed(argument))
                        {
                            newArg = generator.Argument(argument.Parameter.Name, argument.Parameter.RefKind, argument.Value.Syntax);
                            shouldTokenUseName = true;
                        }
                        else
                        {
                            newArg = argument.Syntax;
                        }
                        newArguments.Add(newArg);
                    }
                    else
                    {
                        shouldTokenUseName = true;
                    }
                }
                else
                {
                    // Only reachable if the current method is the one that contains the ct
                    // Won't be reached if it's an overload that contains the paramName
                    paramName = argument.Parameter.Name;
                }
            }

            // Create and append new ct argument to pass to the invocation, using the ancestor method parameter name
            SyntaxNode cancellationTokenIdentifier = generator.IdentifierName(cancellationTokenParameterName);
            SyntaxNode cancellationTokenNode;
            if (shouldTokenUseName)
            {
                // If the paramName is unknown at this point, it's because an overload contains the ct parameter
                // and since it cannot be obtained, no fix will be provided or else CA8323 shows up:
                // CA8323: Named argument 'argName' is used out-of-position but is followed by an unnamed argument
                if (string.IsNullOrEmpty(paramName))
                {
                    return false;
                }

                cancellationTokenNode = generator.Argument(paramName, RefKind.None, cancellationTokenIdentifier);
            }
            else
            {
                cancellationTokenNode = generator.Argument(cancellationTokenIdentifier);
            }
            newArguments.Add(cancellationTokenNode);

            SyntaxNode newInvocation;
            // The instance is null when calling a static method from another type
            if (invocation.Instance == null)
            {
                SyntaxNode staticType = generator.TypeExpressionForStaticMemberAccess(invocation.TargetMethod.ContainingType);
                newInvocation = generator.MemberAccessExpression(staticType, invocation.TargetMethod.Name);
            }
            // The method is being invoked with nullability
            else if (invocation.Instance is IConditionalAccessInstanceOperation)
            {
                newInvocation = GetConditionalOperationInvocationExpression(invocation.Syntax);
            }
            // The instance is implicit when calling a method from the same type, call the method directly
            else if (invocation.Instance.IsImplicit)
            {
                newInvocation = invocation.GetInstanceSyntax()!;
            }
            // Calling a method from an object, we must include the instance variable name
            else
            {
                newInvocation = generator.MemberAccessExpression(invocation.GetInstanceSyntax(), invocation.TargetMethod.Name);
            }
            // Insert the new arguments to the new invocation
            SyntaxNode newInvocationWithArguments = generator.InvocationExpression(newInvocation, newArguments).WithTriviaFrom(invocation.Syntax);

            newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocationWithArguments);

            return true;
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