// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2234: Pass system uri objects instead of strings
    /// </summary>
    public abstract class PassSystemUriObjectsInsteadOfStringsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PassSystemUriObjectsInsteadOfStringsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // This is to get rid of warning CS1998, please remove when implementing this analyzer
            await new Task(() => { });
            throw new NotImplementedException();
        }
    }
}