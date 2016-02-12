// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OverrideGetHashCodeOnOverridingEqualsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicOverrideGetHashCodeOnOverridingEqualsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            throw new NotSupportedException("CA2218 is not applied to C# since it already reports CS0661");
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicOverrideGetHashCodeOnOverridingEqualsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpOverrideGetHashCodeOnOverridingEqualsFixer();
        }

        [Fact]
        public void CS0660()
        {
            VerifyFix(
                LanguageNames.CSharp,
                DummyCS0661Analyzer.Instance,
                GetCSharpCodeFixProvider(),
                @"
class C
{
    public override bool Equals(object obj) => true;
}
",
                @"
class C
{
    public override bool Equals(object obj) => true;

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}
",
                codeFixIndex: null,
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CS0660_Simplified()
        {
            VerifyFix(
                LanguageNames.CSharp,
                DummyCS0661Analyzer.Instance,
                GetCSharpCodeFixProvider(),
                @"
using System;

class C
{
    public override bool Equals(object obj) => true;
}
",
                @"
using System;

class C
{
    public override bool Equals(object obj) => true;

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
",
                codeFixIndex: null,
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CA2224()
        {
            VerifyBasicFix(@"
Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function
End Class
",
@"
Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function

    Public Overrides Function GetHashCode() As Integer
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        [Fact]
        public void CA2224_Simplified()
        {
            VerifyBasicFix(@"
Imports System

Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function
End Class
",
@"
Imports System

Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function

    Public Overrides Function GetHashCode() As Integer
        Throw New NotImplementedException()
    End Function
End Class
");
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private sealed class DummyCS0661Analyzer : DiagnosticAnalyzer
        {
            public static readonly DiagnosticAnalyzer Instance = new DummyCS0661Analyzer();

            private static readonly DiagnosticDescriptor s_descriptor =
                new DiagnosticDescriptor(
                    "CS0660",
                    "title",
                    "message",
                    "category",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

            private DummyCS0661Analyzer() { }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_descriptor);

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSymbolAction(symbolContext =>
                        symbolContext.ReportDiagnostic(Diagnostic.Create(s_descriptor, symbolContext.Symbol.Locations[0])),
                    SymbolKind.NamedType);
            }
        }
    }
}