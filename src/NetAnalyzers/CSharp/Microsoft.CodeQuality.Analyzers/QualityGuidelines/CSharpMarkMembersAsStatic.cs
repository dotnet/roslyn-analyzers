// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.QualityGuidelines;

namespace Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpMarkMembersAsStaticAnalyzer : MarkMembersAsStaticAnalyzer
    {
        protected override bool SupportStaticLocalFunctions(ParseOptions parseOptions)
            => ((CSharpParseOptions)parseOptions).LanguageVersion >= LanguageVersion.CSharp8;
    }
}
