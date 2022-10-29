// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferLengthCountIsEmptyOverAnyAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpPreferLengthCountIsEmptyOverAnyFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferLengthCountIsEmptyOverAnyAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicPreferLengthCountIsEmptyOverAnyFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class PreferIsEmptyOverAnyTests
    {
        private const string CSharpTemplate = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {{
    {0}
}}";

        private const string VbTemplate = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    {0}
End Class";

        private static readonly DiagnosticResult ExpectedDiagnostic = new DiagnosticResult(PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyDescriptor).WithLocation(0);

        [Fact]
        public Task TestLocalDeclarationAsync()
        {
            const string code = @"
public void M() {
    var array = ImmutableArray<int>.Empty;
    _ = {|#0:array.Any()|};
}";
            const string fixedCode = @"
public void M() {
    var array = ImmutableArray<int>.Empty;
    _ = !array.IsEmpty;
}";
            return VerifyCS.VerifyCodeFixAsync(string.Format(CSharpTemplate, code), ExpectedDiagnostic, string.Format(CSharpTemplate, fixedCode));
        }

        [Fact]
        public Task VbTestLocalDeclarationAsync()
        {
            const string code = @"
Public Function M()
    Dim array = ImmutableArray(Of Integer).Empty
    Dim x = {|#0:array.Any()|}
End Function";
            const string fixedCode = @"
Public Function M()
    Dim array = ImmutableArray(Of Integer).Empty
    Dim x = Not array.IsEmpty
End Function";

            return VerifyVB.VerifyCodeFixAsync(string.Format(VbTemplate, code), ExpectedDiagnostic, string.Format(VbTemplate, fixedCode));
        }

        [Fact]
        public Task TestParameterDeclarationAsync()
        {
            const string code = @"
public bool HasContents(ImmutableArray<int> array) {
    return {|#0:array.Any()|};
}";
            const string fixedCode = @"
public bool HasContents(ImmutableArray<int> array) {
    return !array.IsEmpty;
}";
            return VerifyCS.VerifyCodeFixAsync(string.Format(CSharpTemplate, code), ExpectedDiagnostic, string.Format(CSharpTemplate, fixedCode));
        }

        [Fact]
        public Task VbTestParameterDeclarationAsync()
        {
            const string code = @"
Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
    Return {|#0:array.Any()|}
End Function";
            const string fixedCode = @"
Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
    Return Not array.IsEmpty
End Function";

            return VerifyVB.VerifyCodeFixAsync(string.Format(VbTemplate, code), ExpectedDiagnostic, string.Format(VbTemplate, fixedCode));
        }

        [Fact]
        public Task TestNegatedAnyAsync()
        {
            const string code = @"
public bool IsEmpty(ImmutableArray<int> array) {
    return !{|#0:array.Any()|};
}";
            const string fixedCode = @"
public bool IsEmpty(ImmutableArray<int> array) {
    return array.IsEmpty;
}";
            return VerifyCS.VerifyCodeFixAsync(string.Format(CSharpTemplate, code), ExpectedDiagnostic, string.Format(CSharpTemplate, fixedCode));
        }

        [Fact]
        public Task VbTestNegatedAnyAsync()
        {
            const string code = @"
Public Function IsEmpty(array As ImmutableArray(Of Integer)) As Boolean
    Return Not {|#0:array.Any()|}
End Function";
            const string fixedCode = @"
Public Function IsEmpty(array As ImmutableArray(Of Integer)) As Boolean
    Return array.IsEmpty
End Function";

            return VerifyVB.VerifyCodeFixAsync(string.Format(VbTemplate, code), ExpectedDiagnostic, string.Format(VbTemplate, fixedCode));
        }

        [Fact]
        public Task DontWarnOnChainedLinqWithAnyAsync()
        {
            const string code = @"
public bool HasContents(ImmutableArray<int> array) {
    return array.Select(x => x).Any();
}";

            return VerifyCS.VerifyAnalyzerAsync(string.Format(CSharpTemplate, code));
        }

        [Fact]
        public Task VbDontWarnOnChainedLinqWithAnyAsync()
        {
            const string code = @"
Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
    Return array.Select(Function(x) x).Any()
End Function";

            return VerifyVB.VerifyAnalyzerAsync(string.Format(VbTemplate, code));
        }

        [Fact]
        public Task DontWarnOnAnyWithPredicateAsync()
        {
            const string code = @"
public bool HasContents(ImmutableArray<int> array) {
    return array.Any(x => x > 5);
}";

            return VerifyCS.VerifyAnalyzerAsync(string.Format(CSharpTemplate, code));
        }

        [Fact]
        public Task VbDontWarnOnAnyWithPredicateAsync()
        {
            const string code = @"
Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
    Return array.Any(Function(x) x > 5)
End Function";

            return VerifyVB.VerifyAnalyzerAsync(string.Format(VbTemplate, code));
        }
    }
}