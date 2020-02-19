// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.IdentifiersShouldNotMatchKeywordsAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpIdentifiersShouldNotMatchKeywordsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.IdentifiersShouldNotMatchKeywordsAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines.BasicIdentifiersShouldNotMatchKeywordsFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    /// <summary>
    /// Contains those unit tests for the IdentifiersShouldNotMatchKeywords analyzer that
    /// pertain to the MemberParameterRule, which applies to the names of type member parameters.
    /// </summary>
    public class IdentifiersShouldNotMatchKeywordsMemberParameterRuleTests
    {
        [Fact]
        public async Task CSharpNoDiagnosticForCaseSensitiveKeywordNamedParameterOfPublicVirtualMethodInPublicClassWithDifferentCasing()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public virtual void F(int @iNt) {}
}");
        }

        [Fact]
        public async Task BasicNoDiagnosticForCaseSensitiveKeywordNamedParameterOfPublicVirtualMethodInPublicClassWithDifferentCasing()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public Overridable Sub F([iNt] As Integer)
    End Sub
End Class");
        }

        [Fact]
        public async Task CSharpNoDiagnosticForKeywordNamedParameterOfInternalVirtualMethodInPublicClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    internal virtual void F(int @int) {}
}");
        }

        [Fact]
        public async Task BasicNoDiagnosticForKeywordNamedParameterOfInternalVirtualMethodInPublicClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Friend Overridable Sub F([int] As Integer)
    End Sub
End Class");
        }

        [Fact]
        public async Task CSharpNoDiagnosticForParameterOfPublicNonVirtualMethodInPublicClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public void F(int @int) {}
}");
        }

        [Fact]
        public async Task BasicNoDiagnosticForKeywordNamedParameterOfPublicNonVirtualMethodInPublicClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public Sub F([int] As Integer)
    End Sub
End Class");
        }

        [Fact]
        public async Task CSharpNoDiagnosticForNonKeywordNamedParameterOfPublicVirtualMethodInPublicClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public void F(int int2) {}
}");
        }

        [Fact]
        public async Task BasicNoDiagnosticForNonKeywordNamedParameterOfPublicVirtualMethodInPublicClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public Overridable Sub F([int2] As Integer)
    End Sub
End Class");
        }

        [Fact]
        public async Task CSharpNoDiagnosticForKeywordNamedParameterOfPublicVirtualMethodInInternalClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class C
{
    public void F(int @int) {}
}");
        }

        [Fact]
        public async Task BasicNoDiagnosticForKeywordNamedParameterOfPublicVirtualMethodInInternalClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Friend Class C
    Public Overridable Sub F([int] As Integer)
    End Sub
End Class");
        }

        [Fact]
        public async Task CSharpNoDiagnosticForKeywordNamedParameterOfMethodInInternalInterface()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal interface I
{
    void F(int @int);
}");
        }

        [Fact]
        public async Task BasicNoDiagnosticForKeywordNamedParameterOfMethodInInternalInterface()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Friend Interface I
    Sub F([int] As Integer)
End Interface");
        }

        [Fact]
        public async Task CA1710_ParameterNamedStep_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public virtual void S(object step) {}
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public Overridable Sub S([step] As Object)
    End Sub
End Class");
        }
    }
}
