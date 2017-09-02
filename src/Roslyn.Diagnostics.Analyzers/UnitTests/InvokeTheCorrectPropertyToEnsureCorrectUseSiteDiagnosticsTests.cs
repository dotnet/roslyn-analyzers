// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Diagnostics.CSharp.Analyzers;
using Roslyn.Diagnostics.VisualBasic.Analyzers;
using Test.Utilities;
using Xunit;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class InvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicInvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpInvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsAnalyzer();
        }

        [Fact]
        public void TestTypeArgumentsInBasic()
        {
            var source = @"
Namespace Microsoft.CodeAnalysis.VisualBasic.Symbols
    Public Class NamedTypeSymbol
        Public ReadOnly Property TypeArguments As Integer
            Get
                Return 1
            End Get
        End Property
    End Class
End Namespace

Class C
    Sub M(c As Microsoft.CodeAnalysis.VisualBasic.Symbols.NamedTypeSymbol)
        Dim x = c.TypeArguments
        System.Console.Write(x)
    End Sub
End Class
";

            VerifyBasic(source, GetRS0004BasicResultAt(14, 19));
        }

        [Fact]
        public void TestTypeArgumentsInCSharp()
        {
            var source = @"
namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    public class NamedTypeSymbol
    {
        public int TypeArguments
        {
            get => 1;
        }
    }
}
class C
{
    void M(Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol c)
    {
        var x = c.TypeArguments;
        System.Console.Write(x);
    }
}
";

            VerifyCSharp(source, GetRS0004CSharpResultAt(16, 19));
        }

        private static DiagnosticResult GetRS0004CSharpResultAt(int line, int column, params string[] args)
        {
            return GetCSharpResultAt(line, column, CSharpInvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsAnalyzer.Rule, args);
        }

        private static DiagnosticResult GetRS0004BasicResultAt(int line, int column, params string[] args)
        {
            return GetBasicResultAt(line, column, BasicInvokeTheCorrectPropertyToEnsureCorrectUseSiteDiagnosticsAnalyzer.Rule, args);
        }
    }
}