// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class ProvideCorrectArgumentsToFormattingMethodsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ProvideCorrectArgumentsToFormattingMethodsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ProvideCorrectArgumentsToFormattingMethodsAnalyzer();
        }

        #region Diagnostic Tests

        [Fact]
        public void CA2241CSharpDiagnosticProviderTestFired()
        {
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = String.Format(""{0}"", null);
    }
}
",
            GetCA2241CSharpResultAt(6, 18),
            GetCA2241CSharpResultAt(10, 19));
        }

        [Fact]
        public void CA1813CSharpDiagnosticProviderTestFiredWithScope()
        {
            VerifyCSharp(@"
using System;

[|public class AttributeClass: Attribute
{
}|]

private class AttributeClass2: Attribute
{
}
",
            GetCA2241CSharpResultAt(4, 14));
        }

        [Fact]
        public void CA1813CSharpDiagnosticProviderTestNotFired()
        {
            VerifyCSharp(@"
using System;

public sealed class AttributeClass: Attribute
{
    private abstract class AttributeClass2: Attribute
    {
        public abstract void F();
    }
}");
        }

        [Fact]
        public void CA1813VisualBasicDiagnosticProviderTestFired()
        {
            VerifyBasic(@"
Imports System

Public Class AttributeClass
    Inherits Attribute
End Class

Private Class AttributeClass2
    Inherits Attribute
End Class
",
            GetCA2241BasicResultAt(4, 14),
            GetCA2241BasicResultAt(8, 15));
        }

        [Fact]
        public void CA1813VisualBasicDiagnosticProviderTestFiredwithScope()
        {
            VerifyBasic(@"
Imports System

Public Class AttributeClass
    Inherits Attribute
End Class

[|Private Class AttributeClass2
    Inherits Attribute
End Class|]
",
            GetCA2241BasicResultAt(8, 15));
        }

        [Fact]
        public void CA1813VisualBasicDiagnosticProviderTestNotFired()
        {
            VerifyBasic(@"
Imports System

Public NotInheritable Class AttributeClass
    Inherits Attribute

    Private MustInherit Class AttributeClass2
        Inherits Attribute
        MustOverride Sub F()
    End Class
End Class
");
        }

        #endregion

        internal static string CA2241Name = "CA2241";

        private static DiagnosticResult GetCA2241CSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA2241Name, SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsMessage);
        }

        private static DiagnosticResult GetCA2241BasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA2241Name, SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsMessage);
        }
    }
}