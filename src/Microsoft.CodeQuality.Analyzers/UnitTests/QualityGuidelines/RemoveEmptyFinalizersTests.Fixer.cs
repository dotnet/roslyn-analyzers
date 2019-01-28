// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public partial class RemoveEmptyFinalizersFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRemoveEmptyFinalizersAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new RemoveEmptyFinalizersFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpRemoveEmptyFinalizersAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RemoveEmptyFinalizersFixer();
        }

        [Fact]
        public void CA1821CSharpCodeFixTestRemoveEmptyFinalizers()
        {
            VerifyCSharpFix(@"
public class Class1
{
    // Violation occurs because the finalizer is empty.
    ~Class1()
    {
    }
}
",
@"
public class Class1
{
}
");
        }

        [Fact]
        public void CA1821BasicCodeFixTestRemoveEmptyFinalizers()
        {
            VerifyBasicFix(@"
Imports System.Diagnostics

Public Class Class1
    '  Violation occurs because the finalizer is empty.
    Protected Overrides Sub Finalize()

    End Sub
End Class
",
@"
Imports System.Diagnostics

Public Class Class1
End Class
");
        }
    }
}
