// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA2235: Mark all non-serializable fields
    /// </summary>
    public abstract class MarkAllNonSerializableFieldsFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SerializationRulesDiagnosticAnalyzer.RuleCA2235Id);

        protected abstract SyntaxNode GetFieldDeclarationNode(SyntaxNode node);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);
            SyntaxNode fieldNode = GetFieldDeclarationNode(node);
            if (fieldNode == null)
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics.Single();

            // Fix 1: Add a NonSerialized attribute to the field
            context.RegisterCodeFix(new MyCodeAction(DesktopAnalyzersResources.AddNonSerializedAttributeCodeActionTitle,
                                        async ct => await AddNonSerializedAttribute(context.Document, fieldNode, ct).ConfigureAwait(false)),
                                    diagnostic);


            // Fix 2: If the type of the field is defined in source, then add the serializable attribute to the type.
            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var fieldSymbol = model.GetDeclaredSymbol(node, context.CancellationToken) as IFieldSymbol;
            ITypeSymbol type = fieldSymbol?.Type;
            if (type != null && type.Locations.Any(l => l.IsInSource))
            {
                context.RegisterCodeFix(new MyCodeAction(DesktopAnalyzersResources.AddSerializableAttributeCodeActionTitle,
                            async ct => await AddSerializableAttributeToType(context.Document, type, ct).ConfigureAwait(false)),
                        diagnostic);
            }
        }

        private async Task<Document> AddNonSerializedAttribute(Document document, SyntaxNode fieldNode, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxNode attr = editor.Generator.Attribute(editor.Generator.TypeExpression(WellKnownTypes.NonSerializedAttribute(editor.SemanticModel.Compilation)));
            editor.AddAttribute(fieldNode, attr);
            return editor.GetChangedDocument();
        }

        private static async Task<Document> AddSerializableAttributeToType(Document document, ITypeSymbol type, CancellationToken cancellationToken)
        {
            SymbolEditor editor = SymbolEditor.Create(document);
            await editor.EditOneDeclarationAsync(type, (docEditor, declaration) =>
            {
                SyntaxNode serializableAttr = docEditor.Generator.Attribute(docEditor.Generator.TypeExpression(WellKnownTypes.SerializableAttribute(docEditor.SemanticModel.Compilation)));
                docEditor.AddAttribute(declaration, serializableAttr);
            }, cancellationToken).ConfigureAwait(false);

            return editor.GetChangedDocuments().First();
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument) :
                base(title, createChangedDocument)
            {
            }
        }
    }
}
