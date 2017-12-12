// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
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
        public void CSharp_PublicVariable_PublicContainingType()
        {
            VerifyCSharp(@"
public class A
{
    public string field; 
}", GetCSharpResultAt(4, 19, DoNotDeclareVisibleInstanceFieldsAnalyzer.RuleId, DoNotDeclareVisibleInstanceFieldsAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void VisualBasic_PublicVariable_PublicContainingType()
        {
            VerifyBasic(@"
Public Class A
    Public field As System.String
End Class", GetBasicResultAt(3, 12, DoNotDeclareVisibleInstanceFieldsAnalyzer.RuleId, DoNotDeclareVisibleInstanceFieldsAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void CSharp_PublicVariable_InternalContainingType()
        {
            VerifyCSharp(@"
internal class A
{
    public string field; 

    public class B
    {
        public string field; 
    }
}");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void VisualBasic_PublicVariable_InternalContainingType()
        {
            VerifyBasic(@"
Friend Class A
    Public field As System.String

    Public Class B
        Public field As System.String
    End Class
End Class
");
        }

        [Fact]
        public void CSharp_DefaultVisibility()
        {
            VerifyCSharp(@"
public class A
{
    string field; 
}");
        }

        [Fact]
        public void VisualBasic_DefaultVisibility()
        {
            VerifyBasic(@"
Public Class A
    Dim field As System.String
End Class");
        }

        [Fact]
        public void CSharp_PublicStaticVariable()
        {
            VerifyCSharp(@"
public class A
{
    public static string field; 
}");
        }

        [Fact]
        public void VisualBasic_PublicStaticVariable()
        {
            VerifyBasic(@"
Public Class A
    Public Shared field as System.String
End Class");
        }

        [Fact]
        public void CSharp_PublicStaticReadonlyVariable()
        {
            VerifyCSharp(@"
public class A
{
    public static readonly string field; 
}");
        }

        [Fact]
        public void VisualBasic_PublicStaticReadonlyVariable()
        {
            VerifyBasic(@"
Public Class A
    Public Shared ReadOnly field as System.String
End Class");
        }

        [Fact]
        public void CSharp_PublicConstVariable()
        {
            VerifyCSharp(@"
public class A
{
    public const string field = ""X""; 
}");
        }

        [Fact]
        public void VisualBasic_PublicConstVariable()
        {
            VerifyBasic(@"
Public Class A
    Public Const field as System.String = ""X""
End Class");
        }
    }
}