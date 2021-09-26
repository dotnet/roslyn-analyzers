// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NetCore.Analyzers;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpPreferReadOnlySpanPropertiesOverReadOnlyArrayFieldsFixer : PreferReadOnlySpanPropertiesOverReadOnlyArrayFieldsFixer
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var doc = context.Document;
            var root = await doc.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Diagnostics.First().Location.SourceSpan);

            if (node?.Parent?.Parent is not FieldDeclarationSyntax fieldDeclarationSyntax ||
                fieldDeclarationSyntax.Declaration.Type is not ArrayTypeSyntax arrayTypeSyntax)
            {
                return;
            }

            var model = await doc.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (!model.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var readOnlySpanType))
                return;

            var codeAction = CodeAction.Create(
                MicrosoftNetCoreAnalyzersResources.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_CodeFixTitle,
                GetChangedDocumentAsync,
                nameof(MicrosoftNetCoreAnalyzersResources.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_CodeFixTitle));
            context.RegisterCodeFix(codeAction, context.Diagnostics);

            return;

            //  Local functions

            async Task<Document> GetChangedDocumentAsync(CancellationToken token)
            {
                var editor = await DocumentEditor.CreateAsync(doc, token).ConfigureAwait(false);
                var rosNameSyntax = SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(readOnlySpanType.Name),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(arrayTypeSyntax.ElementType)));
                var newModifiers = SyntaxFactory.TokenList(fieldDeclarationSyntax.Modifiers.Where(x => !x.IsKind(SyntaxKind.ReadOnlyKeyword)));
                var arrowExpressionClauseSyntax = SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken).WithTriviaFrom(fieldDeclarationSyntax.Declaration.Variables[0].Initializer.EqualsToken),
                    fieldDeclarationSyntax.Declaration.Variables[0].Initializer.Value);
                var propertyDeclarationSyntax = SyntaxFactory.PropertyDeclaration(rosNameSyntax, fieldDeclarationSyntax.Declaration.Variables[0].Identifier)
                    .WithExpressionBody(arrowExpressionClauseSyntax)
                    .WithModifiers(newModifiers)
                    .WithSemicolonToken(fieldDeclarationSyntax.SemicolonToken)
                    .WithTriviaFrom(fieldDeclarationSyntax);
                editor.ReplaceNode(fieldDeclarationSyntax, propertyDeclarationSyntax);

                //  Replace calls to 'AsSpan' with either the field itself for 'AsSpan()', or 'field.Slice(...)' for 'AsSpan(int)' or 'AsSpan(int, int)'
                var savedSpans = PreferReadOnlySpanPropertiesOverReadOnlyArrayFields.SavedSpanLocation.Deserialize(
                    context.Diagnostics[0].Properties[PreferReadOnlySpanPropertiesOverReadOnlyArrayFields.FixerDataPropertyName]);
                var documentLookup = context.Document.Project.Documents.ToImmutableDictionary(x => x.FilePath);
                foreach (var savedSpan in savedSpans)
                {
                    RoslynDebug.Assert(documentLookup.ContainsKey(savedSpan.SourceFilePath), "Missing FilePath in document dictionary");

                    var doc = documentLookup[savedSpan.SourceFilePath];
                    var root = await doc.GetSyntaxRootAsync(token).ConfigureAwait(false);
                    var model = await doc.GetSemanticModelAsync(token).ConfigureAwait(false);
                    var savedFieldReference = model.GetOperation(root.FindNode(savedSpan.Span, getInnermostNodeForTie: true), token);
                    var invocation = (IInvocationOperation)savedFieldReference.Parent.Parent;

                    //  If we called 'AsSpan(int)' or 'AsSpan(int, int)', replace with a call to the appropriate Slice overload
                    if (invocation.TargetMethod.Parameters.Length > 1)
                    {
                        var asSpanInvocationSyntax = (InvocationExpressionSyntax)invocation.Syntax;
                        var memberAccessSyntax = (MemberAccessExpressionSyntax)asSpanInvocationSyntax.Expression;
                        var sliceMemberNameSyntax = SyntaxFactory.IdentifierName(nameof(ReadOnlySpan<byte>.Slice));

                        //  If 'AsSpan' was not talled via extension method, we need to remove the first argument node from the
                        //  argument list, and replace the expression of the AsSpan member invocation syntax with the array field reference.
                        if (asSpanInvocationSyntax.ArgumentList.Arguments.Count == invocation.Arguments.Length)
                        {
                            var newArgumentList = asSpanInvocationSyntax.ArgumentList.WithArguments(asSpanInvocationSyntax.ArgumentList.Arguments.RemoveAt(0));
                            editor.ReplaceNode(asSpanInvocationSyntax.ArgumentList, newArgumentList);
                            editor.ReplaceNode(memberAccessSyntax.Expression, savedFieldReference.Syntax.WithTriviaFrom(memberAccessSyntax.Expression));
                        }

                        editor.ReplaceNode(memberAccessSyntax.Name, sliceMemberNameSyntax.WithTriviaFrom(memberAccessSyntax.Name));
                    }
                    else
                    {
                        editor.ReplaceNode(invocation.Syntax, savedFieldReference.Syntax.WithTriviaFrom(invocation.Syntax));
                    }
                }

                return editor.GetChangedDocument();
            }
        }
    }
}
