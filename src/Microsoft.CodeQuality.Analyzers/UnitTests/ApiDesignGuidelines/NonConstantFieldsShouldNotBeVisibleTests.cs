// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class NonConstantFieldsShouldNotBeVisibleTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new NonConstantFieldsShouldNotBeVisibleAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NonConstantFieldsShouldNotBeVisibleAnalyzer();
        }

        [Fact]
        public void DefaultVisibilityCS()
        {
            VerifyCSharp(@"
class A
{
    string field; 
}");
        }

        [Fact]
        public void DefaultVisibilityVB()
        {
            VerifyBasic(@"
Class A
    Dim field As System.String
End Class");
        }

        [Fact]
        public void PublicVariableCS()
        {
            VerifyCSharp(@"
class A
{
    public string field; 
}");
        }

        [Fact]
        public void PublicVariableVB()
        {
            VerifyBasic(@"
Class A
    Public field As System.String
End Class");
        }

        [Fact]
        public void PublicStaticVariableCS()
        {
            VerifyCSharp(@"
class A
{
    public static string field; 
}", GetCSharpResultAt(4, 26, NonConstantFieldsShouldNotBeVisibleAnalyzer.RuleId, NonConstantFieldsShouldNotBeVisibleAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void PublicStaticVariableVB()
        {
            VerifyBasic(@"
Class A
    Public Shared field as System.String
End Class", GetBasicResultAt(3, 19, NonConstantFieldsShouldNotBeVisibleAnalyzer.RuleId, NonConstantFieldsShouldNotBeVisibleAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void PublicStaticReadonlyVariableCS()
        {
            VerifyCSharp(@"
class A
{
    public static readonly string field; 
}");
        }

        [Fact]
        public void PublicStaticReadonlyVariableVB()
        {
            VerifyBasic(@"
Class A
    Public Shared ReadOnly field as System.String
End Class");
        }

        [Fact]
        public void PublicConstVariableCS()
        {
            VerifyCSharp(@"
class A
{
    public const string field = ""X""; 
}");
        }

        [Fact]
        public void PublicConstVariableVB()
        {
            VerifyBasic(@"
Class A
    Public Const field as System.String = ""X""
End Class");
        }
    }
}
