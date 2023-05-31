// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public abstract class RecommendCaseInsensitiveStringComparison_Base_Tests
    {
        protected static readonly DiagnosticDescriptor StringComparisonRule = RecommendCaseInsensitiveStringComparisonAnalyzer.RecommendCaseInsensitiveStringComparisonRule;
        protected static readonly DiagnosticDescriptor StringComparerRule = RecommendCaseInsensitiveStringComparisonAnalyzer.RecommendCaseInsensitiveStringComparerRule;

        protected const string ContainsName = RecommendCaseInsensitiveStringComparisonAnalyzer.StringContainsMethodName;
        protected const string IndexOfName = RecommendCaseInsensitiveStringComparisonAnalyzer.StringIndexOfMethodName;
        protected const string StartsWithName = RecommendCaseInsensitiveStringComparisonAnalyzer.StringStartsWithMethodName;
        protected const string CompareToName = RecommendCaseInsensitiveStringComparisonAnalyzer.StringCompareToMethodName;

    }
}