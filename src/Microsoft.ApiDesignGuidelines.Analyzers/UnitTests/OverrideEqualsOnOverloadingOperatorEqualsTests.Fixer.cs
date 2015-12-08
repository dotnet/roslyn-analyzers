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
    public class OverrideEqualsOnOverloadingOperatorEqualsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicOverrideEqualsOnOverloadingOperatorEqualsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            throw new NotSupportedException("CA2224 is not applied to C# since it already reports CS0660");
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicOverrideEqualsOnOverloadingOperatorEqualsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpOverrideEqualsOnOverloadingOperatorEqualsFixer();
        }

        [Fact]
        public void CS0660()
        {
            VerifyFix(
                LanguageNames.CSharp, 
                DummyCS0660Analyzer.Instance, 
                GetCSharpCodeFixProvider(),
                @"
class C
{
    public static bool operator ==(C c1, C c2) => true;
    public static bool operator !=(C c1, C c2) => false;
}
",
                @"
class C
{
    public static bool operator ==(C c1, C c2) => true;
    public static bool operator !=(C c1, C c2) => false;

    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
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
    Public Shared Operator =(c1 As C, c2 As C) As Boolean
        Return True
    End Operator

    Public Shared Operator <>(c1 As C, c2 As C) As Boolean
        Return False
    End Operator
End Class
",
@"
Class C
    Public Shared Operator =(c1 As C, c2 As C) As Boolean
        Return True
    End Operator

    Public Shared Operator <>(c1 As C, c2 As C) As Boolean
        Return False
    End Operator

    Public Overrides Function Equals(obj As Object) As Boolean
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        private sealed class DummyCS0660Analyzer : DiagnosticAnalyzer
        {
            public static readonly DiagnosticAnalyzer Instance = new DummyCS0660Analyzer();

            private DummyCS0660Analyzer() { }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = 
                ImmutableArray.Create(
                    new DiagnosticDescriptor("CS0660", "title", "message", "category", DiagnosticSeverity.Warning, isEnabledByDefault: true));


            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSymbolAction(symbolContext =>
                {
                    symbolContext.ReportDiagnostic(
                        Diagnostic.Create(
                            "CS0660", 
                            "category", 
                            "message", 
                            DiagnosticSeverity.Warning, 
                            DiagnosticSeverity.Warning, 
                            isEnabledByDefault: true, 
                            warningLevel: 1)); 
                },
                SymbolKind.NamedType);
            }
        }
    }
}