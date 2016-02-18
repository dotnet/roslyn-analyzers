// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Editing;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpStaticHolderTypesFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(StaticHolderTypesAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            CodeAnalysis.Text.TextSpan span = context.Span;
            CancellationToken cancellationToken = context.CancellationToken;

            cancellationToken.ThrowIfCancellationRequested();
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            ClassDeclarationSyntax classDeclaration = root.FindToken(span.Start).Parent?.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration != null)
            {
                var codeAction = new MyCodeAction(MicrosoftApiDesignGuidelinesAnalyzersResources.MakeClassStatic,
                                                  async ct => await MakeClassStatic(document, root, classDeclaration, ct).ConfigureAwait(false));
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }

        private async Task<Document> MakeClassStatic(Document document, SyntaxNode root, ClassDeclarationSyntax classDeclaration, CancellationToken ct)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
            DeclarationModifiers modifiers = editor.Generator.GetModifiers(classDeclaration);
            editor.SetModifiers(classDeclaration, modifiers - DeclarationModifiers.Sealed + DeclarationModifiers.Static);

            SyntaxList<MemberDeclarationSyntax> members = classDeclaration.Members;
            MemberDeclarationSyntax defaultConstructor = members.FirstOrDefault(m => m.IsDefaultConstructor());
            if (defaultConstructor != null)
            {
                editor.RemoveNode(defaultConstructor);
            }

            return editor.GetChangedDocument();
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public override string EquivalenceKey => nameof(CSharpStaticHolderTypesFixer);

            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument) :
                base(title, createChangedDocument)
            {
            }
        }
    }

    internal static class CA1052CSharpCodeFixProviderExtensions
    {
        internal static bool IsDefaultConstructor(this MemberDeclarationSyntax member)
        {
            if (member.Kind() != SyntaxKind.ConstructorDeclaration)
            {
                return false;
            }

            var constructor = (ConstructorDeclarationSyntax)member;
            if (constructor.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword))
            {
                return false;
            }

            return constructor.ParameterList.Parameters.Count == 0;
        }
    }
}
