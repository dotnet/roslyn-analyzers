// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonAnalyzer,
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class RecommendCaseInsensitiveStringComparison_VisualBasic_Tests : RecommendCaseInsensitiveStringComparison_Base_Tests
    {
        [Theory]
        [MemberData(nameof(DiagnosedAndFixedData))]
        public Task Diagnostic_Assign(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = [|{diagnosedLine}|]
        Return r
    End Function
End Class
";
            string fixedCode = $@"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = {fixedLine}
        Return r
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedData))]
        public Task Diagnostic_Return(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return [|{diagnosedLine}|]
    End Function
End Class
";
            string fixedCode = $@"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return {fixedLine}
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedWithEqualsToData))]
        public Task Diagnostic_If(string diagnosedLine, string fixedLine, string equalsTo)
        {
            if (equalsTo == " == -1")
            {
                equalsTo = " = -1"; // VB syntax
            }

            string originalCode = $@"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If [|{diagnosedLine}|]{equalsTo} Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            string fixedCode = $@"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If {fixedLine}{equalsTo} Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedData))]
        public Task Diagnostic_IgnoreResult(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        [|{diagnosedLine}|]
    End Sub
End Class
";
            string fixedCode = $@"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        {fixedLine}
    End Sub
End Class
"; ;
            return VerifyVisualBasicAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedStringLiteralsData))]
        public Task Diagnostic_StringLiterals_Return(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"Imports System
Class C
    Private Function M() As Integer
        Return [|{diagnosedLine}|]
    End Function
End Class
";
            string fixedCode = $@"Imports System
Class C
    Private Function M() As Integer
        Return {fixedLine}
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedStringReturningMethodsData))]
        public Task Diagnostic_StringReturningMethods_Discard(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"Imports System
Class C
    Public Function GetStringA() As String
        Return ""aBc""
    End Function
    Public Function GetStringB() As String
        Return ""DeF""
    End Function
    Public Sub M()
        [|{diagnosedLine}|]
    End Sub
End Class
";
            string fixedCode = $@"Imports System
Class C
    Public Function GetStringA() As String
        Return ""aBc""
    End Function
    Public Function GetStringB() As String
        Return ""DeF""
    End Function
    Public Sub M()
        {fixedLine}
    End Sub
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode);
        }

        private Task VerifyVisualBasicAsync(string originalSource, string fixedSource)
        {
            VerifyVB.Test test = new()
            {
                TestCode = originalSource,
                FixedCode = fixedSource,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            };

            return test.RunAsync();
        }
    }
}