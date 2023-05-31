// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonAnalyzer,
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class RecommendCaseInsensitiveStringComparison_VisualBasic_Tests : RecommendCaseInsensitiveStringComparison_Base_Tests
    {
        [Fact]
        public Task Diagnostic_Contains_ToLower()
        {
            string originalCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.ToLower().Contains(b)
    End Sub
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.Contains(b, StringComparison.CurrentCultureIgnoreCase)
    End Sub
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 9, 6, 32, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_Contains_ToLowerInvariant()
        {
            string originalCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.ToLowerInvariant().Contains(b)
    End Sub
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.Contains(b, StringComparison.InvariantCultureIgnoreCase)
    End Sub
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 9, 6, 41, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_Contains_ToUpper()
        {
            string originalCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.ToUpper().Contains(b)
    End Sub
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.Contains(b, StringComparison.CurrentCultureIgnoreCase)
    End Sub
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 9, 6, 32, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_Contains_ToUpperInvariant()
        {
            string originalCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.ToUpperInvariant().Contains(b)
    End Sub
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Sub M()
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        a.Contains(b, StringComparison.InvariantCultureIgnoreCase)
    End Sub
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 9, 6, 41, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToLower_Return()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.ToLower().IndexOf(b)
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.IndexOf(b, StringComparison.CurrentCultureIgnoreCase)
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 16, 6, 38, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToLowerInvariant_Return()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.ToLowerInvariant().IndexOf(b)
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.IndexOf(b, StringComparison.InvariantCultureIgnoreCase)
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 16, 6, 47, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToUpper_Return()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.ToUpper().IndexOf(b)
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.IndexOf(b, StringComparison.CurrentCultureIgnoreCase)
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 16, 6, 38, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToUpperInvariant_Return()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.ToUpperInvariant().IndexOf(b)
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Return a.IndexOf(b, StringComparison.InvariantCultureIgnoreCase)
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 16, 6, 47, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToLower_If()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.ToLower().StartsWith(b) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.StartsWith(b, StringComparison.CurrentCultureIgnoreCase) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 12, 6, 37, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToLowerInvariant_If()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.ToLowerInvariant().StartsWith(b) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 12, 6, 46, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToUpper_If()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.ToUpper().StartsWith(b) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.StartsWith(b, StringComparison.CurrentCultureIgnoreCase) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 12, 6, 37, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToUpperInvariant_If()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.ToUpperInvariant().StartsWith(b) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        If a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase) Then
            Return 5
        End If
        Return 4
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 12, 6, 46, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToLower_Assign()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = a.ToLower().CompareTo(b)
        Return r
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = StringComparer.CurrentCultureIgnoreCase.Compare(a, b)
        Return r
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 28, 6, 52, StringComparerRule, CompareToName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToLowerInvariant_Assign()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = a.ToLowerInvariant().CompareTo(b)
        Return r
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = StringComparer.InvariantCultureIgnoreCase.Compare(a, b)
        Return r
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 28, 6, 61, StringComparerRule, CompareToName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToUpper_Assign()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = a.ToUpper().CompareTo(b)
        Return r
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = StringComparer.CurrentCultureIgnoreCase.Compare(a, b)
        Return r
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 28, 6, 52, StringComparerRule, CompareToName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToUpperInvariant_Assign()
        {
            string originalCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = a.ToUpperInvariant().CompareTo(b)
        Return r
    End Function
End Class
";
            string fixedCode = @"Imports System
Class C
    Private Function M() As Integer
        Dim a As String = ""aBc""
        Dim b As String = ""bc""
        Dim r As Integer = StringComparer.InvariantCultureIgnoreCase.Compare(a, b)
        Return r
    End Function
End Class
";
            return VerifyVisualBasicAsync(originalCode, fixedCode, 6, 28, 6, 61, StringComparerRule, CompareToName);
        }

        private Task VerifyVisualBasicAsync(string originalSource, string fixedSource,
            int startLine, int startColumn, int endLine, int endColumn, DiagnosticDescriptor rule, string argument)
        {
            DiagnosticResult diagnosticResult = VerifyVB
                .Diagnostic(rule)
                .WithArguments(argument)
                .WithSpan(startLine, startColumn, endLine, endColumn);

            VerifyVB.Test test = new()
            {
                TestCode = originalSource,
                FixedCode = fixedSource
            };
            test.ExpectedDiagnostics.Add(diagnosticResult);
            return test.RunAsync();
        }
    }
}