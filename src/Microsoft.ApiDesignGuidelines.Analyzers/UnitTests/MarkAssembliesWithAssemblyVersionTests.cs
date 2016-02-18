// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class MarkAssembliesWithAssemblyVersionAttributeTests : DiagnosticAnalyzerTestBase
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
        public void CA1016BasicTestWithNoComplianceAttribute()
        {
            VerifyBasic(
@"
imports System.IO
imports System.Reflection
imports System

< Assembly: CLSCompliant(true)>
    class Program
    
        Sub Main
        End Sub
    End class
",
                s_diagnostic);
        }

        [Fact]
        public void CA1016CSharpTestWithVersionAttributeNotFromBCL()
        {
            VerifyCSharp(
@"
using System;
[assembly:System.CLSCompliantAttribute(true)]
[assembly:AssemblyVersion(""1.2.3.4"")]
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
class AssemblyVersionAttribute : Attribute {
    public AssemblyVersionAttribute(string s) {}
}
",
                s_diagnostic);
        }

        [Fact]
        public void CA1016CSharpTestWithNoVersionAttribute()
        {
            VerifyCSharp(
@"
[assembly:System.CLSCompliantAttribute(true)]

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
",
                s_diagnostic);
        }

        [Fact]
        public void CA1016CSharpTestWithVersionAttribute()
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
        public void CA1016CSharpTestWithTwoFilesWithAttribute()
        {
            VerifyCSharp(new string[]
                {
@"
[assembly:System.CLSCompliantAttribute(true)]

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
",
@"
using System.Reflection;
[assembly: AssemblyVersionAttribute(""1.2.3.4"")]
"
                });
        }

        [Fact]
        public void CA1016CSharpTestWithVersionAttributeTruncated()
        {
            VerifyCSharp(
@"
using System.Reflection;
[assembly:AssemblyVersion(""1.2.3.4"")]
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
        public void CA1016CSharpTestWithVersionAttributeFullyQualified()
        {
            VerifyCSharp(
@"
[assembly:System.CLSCompliantAttribute(true)]

[assembly:System.Reflection.AssemblyVersion(""1.2.3.4"")]
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
");
        }

        private static readonly string s_number = "CA1016";
        private static readonly string s_message = MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAssembliesWithAssemblyVersionMessage;

        private static readonly DiagnosticResult s_diagnostic = new DiagnosticResult
        {
            Id = s_number,
            Severity = DiagnosticSeverity.Warning,
            Message = s_message
        };
    }
}
