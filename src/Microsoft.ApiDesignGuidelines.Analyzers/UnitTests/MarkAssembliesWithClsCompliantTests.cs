﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class MarkAssembliesWithCLSCompliantAttributeTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkAssembliesWithAttributesDiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkAssembliesWithAttributesDiagnosticAnalyzer();
        }

        [Fact]
        public void CA1014CA1016BasicTestWithCLSCompliantAttributeNone()
        {
            VerifyBasic(
@"
imports System.Reflection

    class Program
    
        Sub Main
        End Sub
    End class
",
            s_diagnosticCA1016, s_diagnosticCA1014);
        }

        [Fact]
        public void CA1014BasicTestWithNoVersionAttribute()
        {
            VerifyBasic(
@"
imports System.Reflection

< Assembly: AssemblyVersionAttribute(""1.1.3.4"")>
    class Program
    
        Sub Main
        End Sub
    End class
",
                s_diagnosticCA1014);
        }

        [Fact]
        public void CA1014CSharpTestWithComplianceAttributeNotFromBCL()
        {
            VerifyCSharp(
@"
using System;
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]

[assembly:CLSCompliant(true)]
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
class CLSCompliantAttribute : Attribute {
    public CLSCompliantAttribute(bool s) {}
}
",
                s_diagnosticCA1014);
        }

        [Fact]
        public void CA1014CSharpTestWithNoCLSComplianceAttribute()
        {
            VerifyCSharp(
@"
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]

class Program
{
    static void Main(string[] args)
    {
    }
}
",
                s_diagnosticCA1014);
        }

        [Fact]
        public void CA1014CSharpTestWithCLSCompliantAttribute()
        {
            VerifyCSharp(
@"
using System;
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]

[assembly:CLSCompliantAttribute(true)]
class Program
{
    static void Main(string[] args)
    {
    }
}
");
        }

        [Fact]
        public void CA1014CSharpTestWithTwoFilesWithAttribute()
        {
            VerifyCSharp(new[]
                {
@"
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]

class Program
{
    static void Main(string[] args)
    {
    }
}
",
@"
using System;
[assembly:CLSCompliantAttribute(true)]
"
                });
        }

        [Fact]
        public void CA1014CSharpTestWithCLSCompliantAttributeTruncated()
        {
            VerifyCSharp(
@"
using System;
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]

[assembly:CLSCompliant(true)]
class Program
{
    static void Main(string[] args)
    {
    }
}
");
        }

        [Fact]
        public void CA1014CSharpTestWithCLSCompliantAttributeFullyQualified()
        {
            VerifyCSharp(
@"
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]
[assembly:System.CLSCompliantAttribute(true)]
class Program
{
    static void Main(string[] args)
    {
    }
}
");
        }

        [Fact]
        public void CA1014CSharpTestWithCLSCompliantAttributeNone()
        {
            VerifyCSharp(
@"
using System.Reflection;
class Program
{
    static void Main(string[] args)
    {
    }
}
",
            s_diagnosticCA1016, s_diagnosticCA1014);
        }

        private static readonly DiagnosticResult s_diagnosticCA1014 = new DiagnosticResult
        {
            Id = MarkAssembliesWithAttributesDiagnosticAnalyzer.CA1014RuleId,
            Severity = DiagnosticHelpers.DefaultDiagnosticSeverity,
            Message = MarkAssembliesWithAttributesDiagnosticAnalyzer.CA1014Rule.MessageFormat.ToString()
        };

        private static readonly DiagnosticResult s_diagnosticCA1016 = new DiagnosticResult
        {
            Id = MarkAssembliesWithAttributesDiagnosticAnalyzer.CA1016RuleId,
            Severity = DiagnosticHelpers.DefaultDiagnosticSeverity,
            Message = MarkAssembliesWithAttributesDiagnosticAnalyzer.CA1016Rule.MessageFormat.ToString()
        };
    }
}
