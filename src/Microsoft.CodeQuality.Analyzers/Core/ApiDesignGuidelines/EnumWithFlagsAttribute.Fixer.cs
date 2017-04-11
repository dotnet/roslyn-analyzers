// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1027: Mark enums with FlagsAttribute
    /// CA2217: Do not mark enums with FlagsAttribute
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class EnumWithFlagsAttributeFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EnumWithFlagsAttributeAnalyzer.RuleIdMarkEnumsWithFlags,
                                                                                   EnumWithFlagsAttributeAnalyzer.RuleIdDoNotMarkEnumsWithFlags);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            INamedTypeSymbol flagsAttributeType = WellKnownTypes.FlagsAttribute(model.Compilation);
            if (flagsAttributeType == null)
            {
                return;
            }

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();
            string fixTitle = diagnostic.Id == EnumWithFlagsAttributeAnalyzer.RuleIdMarkEnumsWithFlags ?
                                                    MicrosoftApiDesignGuidelinesAnalyzersResources.MarkEnumsWithFlagsCodeFix :
                                                    MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotMarkEnumsWithFlagsCodeFix;
            context.RegisterCodeFix(new MyCodeAction(fixTitle,
                                         async ct => await AddOrRemoveFlagsAttribute(context.Document, context.Span, diagnostic.Id, flagsAttributeType, ct).ConfigureAwait(false)),
                        diagnostic);
        }

        private async Task<Document> AddOrRemoveFlagsAttribute(Document document, TextSpan span, string diagnosticId, INamedTypeSymbol flagsAttributeType, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(span);

            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newEnumBlockSyntax = diagnosticId == EnumWithFlagsAttributeAnalyzer.RuleIdMarkEnumsWithFlags ?
                AddFlagsAttribute(editor.Generator, node, flagsAttributeType) :
                RemoveFlagsAttribute(editor.Generator, model, node, flagsAttributeType, cancellationToken);

            editor.ReplaceNode(node, newEnumBlockSyntax);
            return editor.GetChangedDocument();
        }

        private static SyntaxNode AddFlagsAttribute(SyntaxGenerator generator, SyntaxNode enumTypeSyntax, INamedTypeSymbol flagsAttributeType)
        {
            return generator.AddAttributes(enumTypeSyntax, generator.Attribute(generator.TypeExpression(flagsAttributeType)));
        }

        private static SyntaxNode RemoveFlagsAttribute(SyntaxGenerator generator, SemanticModel model, SyntaxNode enumTypeSyntax, INamedTypeSymbol flagsAttributeType, CancellationToken cancellationToken)
        {
            var enumType = model.GetDeclaredSymbol(enumTypeSyntax, cancellationToken) as INamedTypeSymbol;
            Debug.Assert(enumType != null);

            AttributeData flagsAttribute = enumType.GetAttributes().First(a => a.AttributeClass == flagsAttributeType);
            SyntaxNode attributeNode = flagsAttribute.ApplicationSyntaxReference.GetSyntax(cancellationToken);

            return generator.RemoveNode(enumTypeSyntax, attributeNode);
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
