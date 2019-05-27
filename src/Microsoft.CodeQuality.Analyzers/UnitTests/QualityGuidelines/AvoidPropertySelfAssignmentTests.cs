// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.QualityGuidelines;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.QualityGuidelines
{
    public partial class AvoidPropertySelfAssignmentTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidPropertySelfAssignment();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidPropertySelfAssignment();
        }

        [Fact]
        public void AssignmentInConstructorWithNoArguments()
        {
            VerifyCSharp(@"
class C
{
    private string Property { get; set; }
    public C()
    {
        Property = Property;
    }
}
",
                GetCSharpResultAt(7, 9, "="));
        }


        [Fact]
        public void AssignmentInConstructorWithSimilarArgument()
        {
            VerifyCSharp(@"
class C
{
    private string Property { get; set; }
    public C(string property)
    {
        Property = Property;
    }
}
",
                GetCSharpResultAt(7, 9, "="));
        }

        [Fact]
        public void AssignmentInMethodWithoutArguments()
        {
            VerifyCSharp(@"
class C
{
    private string Property { get; set; }
    public void Method()
    {
        Property = Property;
    }
}
",
                GetCSharpResultAt(7, 9, "="));
        }

        [Fact]
        public void AssignmentInMethodWithSimilarArgumentName()
        {
            VerifyCSharp(@"
class C
{
    private string Property { get; set; }
    public void Method(string property)
    {
        Property = Property;
    }
}
",
                GetCSharpResultAt(7, 9, "="));
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string symbolName)
        {
            return GetCSharpResultAt(line, column, AvoidPropertySelfAssignment.Rule, symbolName);
        }
    }
}
