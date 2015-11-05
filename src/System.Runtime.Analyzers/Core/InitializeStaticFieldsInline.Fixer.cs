// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace System.Runtime.Analyzers
{                              
    /// <summary>
    /// CA2207: Initialize value type static fields inline
    /// </summary>
    public abstract class InitializeStaticFieldsInlineFixer<TLanguageKindEnum> : CodeFixProvider
        where TLanguageKindEnum : struct
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(InitializeStaticFieldsInlineAnalyzer<TLanguageKindEnum>.CA1810RuleId, InitializeStaticFieldsInlineAnalyzer<TLanguageKindEnum>.CA2207RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // TODO: Implement the fixer.
            // This is to get rid of warning CS1998, please remove when implementing this analyzer
            await new Task(() => { });
        }
    }
}