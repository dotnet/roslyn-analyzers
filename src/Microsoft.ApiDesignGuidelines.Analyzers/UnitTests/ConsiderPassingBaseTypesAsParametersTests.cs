// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class ConsiderPassingBaseTypesAsParametersTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ConsiderPassingBaseTypesAsParametersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConsiderPassingBaseTypesAsParametersAnalyzer();
        }

        [Fact(Skip = "TDD Todo")]
        public void TestCSCheckForMemberReferences()
        {
            var code = @"
class MyBase { public string Potato; public int Farm = 42; public void Chicken(){} }
class MyDerived: MyBase {  }
class Driver {
    int foo(MyDerived d) {
        return d.Farm;
    }
}
";
            VerifyCSharp(code, GetCA1011CSharpResultAt(2, 23, "C"));
        }

        [Fact(Skip = "TDD Todo")]
        public void TestCSCheckForMemberInvocations()
        {
            var code = @"
class MyBase { public string Potato; public int Farm = 42; public void Chicken(){} }
class MyDerived: MyBase {  }
class Driver {
    int foo(MyDerived d) {
        return d.Farm;
    }
}
";
            VerifyCSharp(code, GetCA1011CSharpResultAt(2, 23, "C"));
        }

        internal static readonly string CA1011Name = "CA1011";
        internal static readonly string CA1011Message = MicrosoftApiDesignGuidelinesAnalyzersResources.ConsiderPassingBaseTypesAsParametersMessage;

        // TODO replace objectName with full paramters
        private static DiagnosticResult GetCA1011CSharpResultAt(int line, int column, string objectName)
        {
            return GetCSharpResultAt(line, column, CA1011Name, string.Format(CA1011Message, objectName));
        }

        private static DiagnosticResult GetCA1011BasicResultAt(int line, int column, string objectName)
        {
            return GetBasicResultAt(line, column, CA1011Name, string.Format(CA1011Message, objectName));
        }
    }
}