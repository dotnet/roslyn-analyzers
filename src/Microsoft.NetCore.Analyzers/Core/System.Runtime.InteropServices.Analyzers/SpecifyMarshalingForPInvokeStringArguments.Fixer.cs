// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Analyzer.Utilities;

namespace System.Runtime.InteropServices.Analyzers
{
    public abstract class SpecifyMarshalingForPInvokeStringArgumentsFixer : CodeFixProvider
    {
        protected const string CharSetText = "CharSet";
        protected const string LPWStrText = "LPWStr";
        protected const string UnicodeText = "Unicode";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PInvokeDiagnosticAnalyzer.RuleCA2101Id);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);
            if (node == null)
            {
                return;
            }

            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            INamedTypeSymbol charSetType = model.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.CharSet");
            INamedTypeSymbol dllImportType = model.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");
            INamedTypeSymbol marshalAsType = model.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.MarshalAsAttribute");
            INamedTypeSymbol unmanagedType = model.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedType");
            if (charSetType == null || dllImportType == null || marshalAsType == null || unmanagedType == null)
            {
                return;
            }

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();

            if (IsAttribute(node))
            {
                context.RegisterCodeFix(new MyCodeAction(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsTitle,
                                                         async ct => await FixAttributeArguments(context.Document, node, charSetType, dllImportType, marshalAsType, unmanagedType, ct).ConfigureAwait(false)),
                                        diagnostic);
            }
            else if (IsDeclareStatement(node))
            {
                context.RegisterCodeFix(new MyCodeAction(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsTitle,
                                                         async ct => await FixDeclareStatement(context.Document, node, ct).ConfigureAwait(false)),
                                        diagnostic);
            }
        }

        protected abstract bool IsAttribute(SyntaxNode node);
        protected abstract bool IsDeclareStatement(SyntaxNode node);
        protected abstract Task<Document> FixDeclareStatement(Document document, SyntaxNode node, CancellationToken cancellationToken);
        protected abstract SyntaxNode FindNamedArgument(IReadOnlyList<SyntaxNode> arguments, string argumentName);

        private async Task<Document> FixAttributeArguments(Document document, SyntaxNode attributeDeclaration,
            INamedTypeSymbol charSetType, INamedTypeSymbol dllImportType, INamedTypeSymbol marshalAsType, INamedTypeSymbol unmanagedType, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // could be either a [DllImport] or [MarshalAs] attribute
            ISymbol attributeType = model.GetSymbolInfo(attributeDeclaration, cancellationToken).Symbol;
            IReadOnlyList<SyntaxNode> arguments = generator.GetAttributeArguments(attributeDeclaration);

            if (dllImportType.Equals(attributeType.ContainingType))
            {
                // [DllImport] attribute, add or replace CharSet named parameter
                SyntaxNode argumentValue = generator.MemberAccessExpression(
                                        generator.TypeExpression(charSetType),
                                        generator.IdentifierName(UnicodeText));
                SyntaxNode newCharSetArgument = generator.AttributeArgument(CharSetText, argumentValue);

                SyntaxNode charSetArgument = FindNamedArgument(arguments, CharSetText);
                if (charSetArgument == null)
                {
                    // add the parameter
                    editor.AddAttributeArgument(attributeDeclaration, newCharSetArgument);
                }
                else
                {
                    // replace the parameter
                    editor.ReplaceNode(charSetArgument, newCharSetArgument);
                }
            }
            else if (marshalAsType.Equals(attributeType.ContainingType) && arguments.Count == 1)
            {
                // [MarshalAs] attribute, replace the only argument
                SyntaxNode newArgument = generator.AttributeArgument(
                                        generator.MemberAccessExpression(
                                            generator.TypeExpression(unmanagedType),
                                            generator.IdentifierName(LPWStrText)));

                editor.ReplaceNode(arguments[0], newArgument);
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
