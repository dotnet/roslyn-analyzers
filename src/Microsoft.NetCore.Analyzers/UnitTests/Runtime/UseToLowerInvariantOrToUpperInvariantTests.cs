// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class UseToLowerInvariantOrToUpperInvariantTests : DiagnosticAnalyzerTestBase
    {
        #region Helper methods

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpUseToLowerInvariantOrToUpperInvariantAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicUseToLowerInvariantOrToUpperInvariantAnalyzer();
        }

        private static DiagnosticResult CSharpResult(int line, int column)
        {
            return GetCSharpResultAt(line, column, UseToLowerInvariantOrToUpperInvariantAnalyzer.RuleId, SystemRuntimeAnalyzersResources.UseToLowerInvariantOrToUpperInvariantTitle);
        }

        private static DiagnosticResult BasicResult(int line, int column)
        {
            return GetBasicResultAt(line, column, UseToLowerInvariantOrToUpperInvariantAnalyzer.RuleId, SystemRuntimeAnalyzersResources.UseToLowerInvariantOrToUpperInvariantTitle);
        }

        #endregion

        #region Diagnostic tests

        [Fact]
        public void CANotYetKnown_ToLowerTest_CSharp()
        {
            VerifyCSharp(@"
class C
{
    void Method()
    {
        string a = ""test"";
        a.ToLower();
        a?.ToLower();
    }
}
",
        CSharpResult(7, 11),
        CSharpResult(8, 11)
);
        }

        [Fact]
        public void CANotYetKnown_ToLowerTest_Basic()
        {
            VerifyBasic(@"
Class C
    Sub Method()
        Dim a As String = ""test""
        a.ToLower()
        a?.ToLower()
    End Sub
End Class
",
        BasicResult(5, 11),
        BasicResult(6, 12)
);
        }

        [Fact]
        public void CANotYetKnown_ToUpperTest_CSharp()
        {
            VerifyCSharp(@"
class C
{
    void Method()
    {
        string a = ""test"";
        a.ToUpper();
        a?.ToUpper();
    }
}
",
        CSharpResult(7, 11),
        CSharpResult(8, 11)
);
        }

        [Fact]
        public void CANotYetKnown_ToUpperTest_Basic()
        {
            VerifyBasic(@"
Class C
    Sub Method()
        Dim a As String = ""test""
        a.ToUpper()
        a?.ToUpper()
    End Sub
End Class
",
        BasicResult(5, 11),
        BasicResult(6, 12)
);
        }

        #endregion
    }
}
