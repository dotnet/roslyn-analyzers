// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferDictionaryTryAddValueOverGuardedAddFixer : CodeFixProvider
    {
        protected const string TryAdd = nameof(TryAdd);

        protected static string CodeFixTitle => MicrosoftNetCoreAnalyzersResources.PreferDictionaryTryAddValueCodeFixTitle;

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(PreferDictionaryTryAddValueOverGuardedAddAnalyzer.RuleId);
    }
}