// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotDeclareVisibleInstanceFieldsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDeclareVisibleInstanceFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDeclareVisibleInstanceFieldsAnalyzer();
        }

        [Fact]
        public void PublicVariableCS()
        {
            VerifyCSharp(@"
class A
{
    public string field; 
}", GetCSharpResultAt(4, 19, DoNotDeclareVisibleInstanceFieldsAnalyzer.RuleId, DoNotDeclareVisibleInstanceFieldsAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void PublicVariableVB()
        {
            VerifyBasic(@"
Class A
    Public field As System.String
End Class", GetBasicResultAt(3, 12, DoNotDeclareVisibleInstanceFieldsAnalyzer.RuleId, DoNotDeclareVisibleInstanceFieldsAnalyzer.Rule.MessageFormat.ToString()));
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
        public void PublicStaticVariableCS()
        {
            VerifyCSharp(@"
class A
{
    public static string field; 
}");
        }

        [Fact]
        public void PublicStaticVariableVB()
        {
            VerifyBasic(@"
Class A
    Public Shared field as System.String
End Class");
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