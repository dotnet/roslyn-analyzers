// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeQuality.Analyzers.Maintainability;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{

    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpUseNameofInPlaceOfStringFixer : UseNameOfInPlaceOfStringFixer
    {
        internal override SyntaxNode GetNameOfExpression(string stringText, Document document)
        {
            return InvocationExpression(
                expression: IdentifierName("nameof"),
                argumentList: ArgumentList(
                    arguments: SingletonSeparatedList(Argument(IdentifierName(stringText)))));
        }
    }
}
