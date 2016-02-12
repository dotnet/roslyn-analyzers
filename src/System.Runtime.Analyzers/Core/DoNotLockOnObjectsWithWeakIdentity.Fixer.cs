// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2002: Do not lock on objects with weak identity
    /// </summary>
    public abstract class DoNotLockOnObjectsWithWeakIdentityFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotLockOnObjectsWithWeakIdentityAnalyzer.RuleId);

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