// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Desktop.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "CA2237 CodeFix provider"), Shared]
    public class CSharpMarkAllNonSerializableFieldsFixer : MarkAllNonSerializableFieldsFixer
    {
        protected override SyntaxNode GetFieldDeclarationNode(SyntaxNode node)
        {
            SyntaxNode fieldNode = node;
            while (fieldNode != null && fieldNode.Kind() != SyntaxKind.FieldDeclaration)
            {
                fieldNode = fieldNode.Parent;
            }

            return fieldNode?.Kind() == SyntaxKind.FieldDeclaration ? fieldNode : null;
        }
    }
}
