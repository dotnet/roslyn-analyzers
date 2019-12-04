// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.SpecifyDiagnosticDescriptorCustomTags,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.SpecifyDiagnosticDescriptorCustomTags,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class SpecifyDiagnosticDescriptorCustomTagsTests
    {
        [Fact]
        public async Task ReportOnMissingCustomTags()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.CodeAnalysis;

public class MyAnalyzer
{
    internal static DiagnosticDescriptor Rule1 = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false);
    internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false);

    public void Foo()
    {
        var diag = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false);
    }
}",
                GetCSharpResultAt(6, 50),
                GetCSharpResultAt(7, 50),
                GetCSharpResultAt(12, 20));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports Microsoft.CodeAnalysis

Public Class MyAnalyzer
    Friend Shared Rule1 As DiagnosticDescriptor = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False)
    Friend Shared Rule2 As DiagnosticDescriptor = New DiagnosticDescriptor("""", New LocalizableResourceString("""", Nothing, Nothing), New LocalizableResourceString("""", Nothing, Nothing), """", DiagnosticSeverity.Warning, False)

    Public Sub Foo()
        Dim diag = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False)
    End Sub
End Class",
                GetBasicResultAt(5, 51),
                GetBasicResultAt(6, 51),
                GetBasicResultAt(9, 20));
        }

        [Fact]
        public async Task DoNotReportOnNamedCustomTags()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.CodeAnalysis;

public class MyAnalyzer
{
    internal static DiagnosticDescriptor Rule1 = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, customTags: """");
    internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false, customTags: """");

    public void Foo()
    {
        var diag = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, customTags: """");
    }
}");

            // Named arguments are incompatible with ParamArray in VB.NET
        }

        [Fact]
        public async Task DoNotReportOnCustomTags()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.CodeAnalysis;

public class MyAnalyzer
{
    internal static DiagnosticDescriptor Rule1 = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, null, null, """");
    internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false, new LocalizableResourceString("""", null, null), """", """");

    internal static DiagnosticDescriptor Rule3 = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, null, null, new[] { """", """" });
    internal static DiagnosticDescriptor Rule4 = new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false, new LocalizableResourceString("""", null, null), """", new[] { """", """" });

    public void Foo()
    {
        var diag = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, null, null, """");
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports Microsoft.CodeAnalysis

Public Class MyAnalyzer
    Friend Shared Rule1 As DiagnosticDescriptor = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False, Nothing, Nothing, """")
    Friend Shared Rule2 As DiagnosticDescriptor = New DiagnosticDescriptor("""", New LocalizableResourceString("""", Nothing, Nothing), New LocalizableResourceString("""", Nothing, Nothing), """", DiagnosticSeverity.Warning, False, New LocalizableResourceString("""", Nothing, Nothing), """", """")
    Friend Shared Rule3 As DiagnosticDescriptor = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False, Nothing, Nothing, { """", """" })
    Friend Shared Rule4 As DiagnosticDescriptor = New DiagnosticDescriptor("""", New LocalizableResourceString("""", Nothing, Nothing), New LocalizableResourceString("""", Nothing, Nothing), """", DiagnosticSeverity.Warning, False, New LocalizableResourceString("""", Nothing, Nothing), """", { """", """" })

    Public Sub Foo()
        Dim diag = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False, Nothing, Nothing, """")
    End Sub
End Class");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);

        private static DiagnosticResult GetBasicResultAt(int line, int column)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column);
    }
}
