// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
    /// CA1001: Types that own disposable fields should be disposable
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class TypesThatOwnDisposableFieldsShouldBeDisposableFixer : CodeFixProvider
    {
        private const string NotImplementedExceptionName = "System.NotImplementedException";
        private const string IDisposableName = "System.IDisposable";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer<SyntaxNode>.RuleId);

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(context.Document);
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxNode declaration = root.FindNode(context.Span);
            declaration = generator.GetDeclaration(declaration);

            if (declaration == null)
            {
                return;
            }

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(new MyCodeAction(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableInterface,
                                                     async ct => await ImplementIDisposable(context.Document, declaration, ct).ConfigureAwait(false)),
                                    diagnostic);
        }

        private async Task<Document> ImplementIDisposable(Document document, SyntaxNode declaration, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            SemanticModel model = editor.SemanticModel;

            // Add the interface to the baselist.
            SyntaxNode interfaceType = generator.TypeExpression(WellKnownTypes.IDisposable(model.Compilation));
            editor.AddInterfaceType(declaration, interfaceType);

            // Find a Dispose method. If one exists make that implement IDisposable, else generate a new method.
            var typeSymbol = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
            IMethodSymbol disposeMethod = (typeSymbol?.GetMembers("Dispose"))?.OfType<IMethodSymbol>()?.Where(m => m.Parameters.Length == 0).FirstOrDefault();
            if (disposeMethod != null && disposeMethod.DeclaringSyntaxReferences.Length == 1)
            {
                SyntaxNode memberPartNode = await disposeMethod.DeclaringSyntaxReferences.Single().GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
                memberPartNode = generator.GetDeclaration(memberPartNode);
                editor.ReplaceNode(memberPartNode, generator.AsPublicInterfaceImplementation(memberPartNode, interfaceType));
            }
            else
            {
                SyntaxNode throwStatement = generator.ThrowStatement(generator.ObjectCreationExpression(WellKnownTypes.NotImplementedException(model.Compilation)));
                SyntaxNode member = generator.MethodDeclaration(TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer<SyntaxNode>.Dispose, statements: new[] { throwStatement });
                member = generator.AsPublicInterfaceImplementation(member, interfaceType);
                editor.AddMember(declaration, member);
            }

            return editor.GetChangedDocument();
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }
        }
    }
}
