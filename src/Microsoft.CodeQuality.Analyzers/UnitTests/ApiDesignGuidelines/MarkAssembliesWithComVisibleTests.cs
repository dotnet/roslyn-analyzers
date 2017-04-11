﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class MarkAllAssembliesWithComVisibleTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkAssembliesWithComVisibleAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkAssembliesWithComVisibleAnalyzer();
        }

        [Fact]
        public void NoTypesComVisibleMissing()
        {
            VerifyCSharp("");
        }

        [Fact]
        public void NoTypesComVisibleTrue()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]");
        }

        [Fact]
        public void NoTypesComVisibleFalse()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]");
        }

        [Fact]
        public void PublicTypeComVisibleMissing()
        {
            VerifyCSharp(@"
public class C
{
}",
                GetAddComVisibleFalseResult());
        }

        [Fact]
        public void PublicTypeComVisibleTrue()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]

public class C
{
}",
                GetExposeIndividualTypesResult());
        }

        [Fact]
        public void PublicTypeComVisibleFalse()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

public class C
{
}");
        }

        [Fact]
        public void InternalTypeComVisibleMissing()
        {
            VerifyCSharp(@"
internal class C
{
}");
        }

        [Fact]
        public void InternalTypeComVisibleTrue()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]

internal class C
{
}");
        }

        [Fact]
        public void InternalTypeComVisibleFalse()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

internal class C
{
}");
        }

        private static DiagnosticResult GetExposeIndividualTypesResult()
        {
            return GetGlobalResult(MarkAssembliesWithComVisibleAnalyzer.RuleId, string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ChangeAssemblyLevelComVisibleToFalse, "TestProject"));
        }

        private static DiagnosticResult GetAddComVisibleFalseResult()
        {
            return GetGlobalResult(MarkAssembliesWithComVisibleAnalyzer.RuleId, string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.AddAssemblyLevelComVisibleFalse, "TestProject"));
        }
    }
}
