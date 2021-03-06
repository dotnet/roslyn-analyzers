// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpDoNotGuardDictionaryRemoveByContainsKeyFixer : DoNotGuardDictionaryRemoveByContainsKeyFixer
    {
        protected override bool OperationSupportedByFixer(SyntaxNode conditionalOperation)
        {
            if (conditionalOperation is ConditionalExpressionSyntax conditionalExpressionSyntax)
                return conditionalExpressionSyntax.WhenTrue.ChildNodes().Count() == 1;

            if (conditionalOperation is IfStatementSyntax)
                return true;

            return false;
        }
    }
}
