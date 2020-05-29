// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities.Extensions
{
    internal static class SyntaxTokenExtensions
    {
        public static SyntaxNode? GetAncestor(this SyntaxToken token, Func<SyntaxNode, bool>? predicate)
            => token.GetAncestor<SyntaxNode>(predicate);

        public static T? GetAncestor<T>(this SyntaxToken token, Func<T, bool>? predicate = null)
            where T : SyntaxNode
            => token.Parent?.FirstAncestorOrSelf(predicate);
    }
}
