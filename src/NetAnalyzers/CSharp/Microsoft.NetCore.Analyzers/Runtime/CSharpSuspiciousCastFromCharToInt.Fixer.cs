// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NetCore.Analyzers;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpSuspiciousCastFromCharToIntFixer : SuspiciousCastFromCharToIntFixer
    {
        internal override SyntaxNode GetMemberAccessExpressionSyntax(SyntaxNode invocationExpressionSyntax)
        {
            return ((InvocationExpressionSyntax)invocationExpressionSyntax).Expression;
        }

        internal override SyntaxNode GetDefaultValueExpression(SyntaxNode parameterSyntax) => ((ParameterSyntax)parameterSyntax).Default.Value;
    }
}
