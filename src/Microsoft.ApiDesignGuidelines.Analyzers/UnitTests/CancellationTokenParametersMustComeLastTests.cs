// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class CancellationTokenParametersMustComeLast : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void NoDiagnosticInEmptyFile()
        {
            var test = @"";

            VerifyCSharp(test);
        }

        [Fact]
        public void DiagnosticForMethod()
        {
            var source = @"
using System.Threading;
class T
{
    void M(CancellationToken t, int i)
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = CancellationTokenParametersMustComeLastAnalyzer.RuleId,
                Message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.CancellationTokenParametersMustComeLastMessage, "T.M(System.Threading.CancellationToken, int)"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 5, 10)
                }
            };

            VerifyCSharp(source, expected);
        }

        [Fact]
        public void NoDiagnosticWhenLastParam()
        {
            var test = @"
using System.Threading;
class T
{
    void M(int i, CancellationToken t)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenOnlyParam()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenParamsComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, params object[] args)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenOutComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, out int i)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenRefComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, ref int x, ref int y)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenOptionalParameterComesAfterNonOptionalCancellationToken()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, int x = 0)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticOnOverride()
        {
            var test = @"
using System.Threading;
class B
{
    protected virtual void M(CancellationToken t, int i) { }
}

class T : B
{
    protected override void M(CancellationToken t, int i) { }
}";

            // One diagnostic for the virtual, but none for the override.
            var expected = new DiagnosticResult
            {
                Id = CancellationTokenParametersMustComeLastAnalyzer.RuleId,
                Message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.CancellationTokenParametersMustComeLastMessage, "B.M(System.Threading.CancellationToken, int)"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 5, 28)
                }
            };

            VerifyCSharp(test, expected);
        }

        [Fact]
        public void NoDiagnosticOnImplicitInterfaceImplementation()
        {
            var test = @"
using System.Threading;
interface I
{
    void M(CancellationToken t, int i);
}

class T : I
{
    public void M(CancellationToken t, int i) { }
}";

            // One diagnostic for the interface, but none for the implementation.
            var expected = new DiagnosticResult
            {
                Id = CancellationTokenParametersMustComeLastAnalyzer.RuleId,
                Message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.CancellationTokenParametersMustComeLastMessage, "I.M(System.Threading.CancellationToken, int)"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 5, 10)
                }
            };

            VerifyCSharp(test, expected);
        }

        [Fact]
        public void NoDiagnosticOnExplicitInterfaceImplementation()
        {
            var test = @"
using System.Threading;
interface I
{
    void M(CancellationToken t, int i);
}

class T : I
{
    void I.M(CancellationToken t, int i) { }
}";

            // One diagnostic for the interface, but none for the implementation.
            var expected = new DiagnosticResult
            {
                Id = CancellationTokenParametersMustComeLastAnalyzer.RuleId,
                Message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.CancellationTokenParametersMustComeLastMessage, "I.M(System.Threading.CancellationToken, int)"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 5, 10)
                }
            };

            VerifyCSharp(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CancellationTokenParametersMustComeLastAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new CancellationTokenParametersMustComeLastAnalyzer();
        }
    }
}
