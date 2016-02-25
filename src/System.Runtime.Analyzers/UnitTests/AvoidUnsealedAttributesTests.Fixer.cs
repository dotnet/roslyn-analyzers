// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public partial class AvoidUnsealedAttributeFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidUnsealedAttributesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new AvoidUnsealedAttributesFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidUnsealedAttributesAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AvoidUnsealedAttributesFixer();
        }

        #region CodeFix Tests

        [Fact]
        public void CA1813CSharpCodeFixProviderTestFired()
        {
            VerifyCSharpFix(@"
using System;

public class AttributeClass : Attribute
{
}", @"
using System;

public sealed class AttributeClass : Attribute
{
}");
        }

        [Fact]
        public void CA1813VisualBasicCodeFixProviderTestFired()
        {
            VerifyBasicFix(@"
Imports System

Public Class AttributeClass
    Inherits Attribute
End Class", @"
Imports System

Public NotInheritable Class AttributeClass
    Inherits Attribute
End Class");
        }

        #endregion
    }
}
