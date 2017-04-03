// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class PropertyNamesShouldNotMatchGetMethodsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new PropertyNamesShouldNotMatchGetMethodsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PropertyNamesShouldNotMatchGetMethodsAnalyzer();
        }

        [Fact]
        public void CSharp_CA1721_PropertyNameDoesNotMatchGetMethodName_Exposed_NoDiagnostic()
        {
            const string Test = @"
using System;

public class Test
{
    public string PropA { get; }
    public string GetPropB()
    {
        return string.Empty;
    }
}";

            VerifyCSharp(Test);
        }

        [Theory] 
        [InlineData("public", "public")]
        [InlineData("public", "protected")]
        [InlineData("public", "protected internal")]
        [InlineData("protected", "public")]
        [InlineData("protected", "protected")]
        [InlineData("protected", "protected internal")]
        [InlineData("protected internal", "public")]
        [InlineData("protected internal", "protected")]
        [InlineData("protected internal", "protected internal")]
#pragma warning disable CA1801 // Review unused parameters
        public void CSharp_CA1721_PropertyNamesMatchGetMethodNames_Exposed_Diagnostics(string propertyAccessibility, string methodAccessibility)
#pragma warning restore CA1801 // Review unused parameters
        {
            var test = $@"
using System;

public class Test
{{
    {propertyAccessibility} string Prop {{ get; }}
    {methodAccessibility} string GetProp()
    {{
        return string.Empty;
    }}
}}";

            VerifyCSharp(
                test,
                GetCA1721CSharpResultAt(
                    line: 6, 
                    column: $"    {propertyAccessibility} string ".Length + 1,
                    identifierName: "Prop", 
                    otherIdentifierName: "GetProp"));
        }

        [Theory]
        [InlineData("private", "private")]
        [InlineData("private", "internal")]
        [InlineData("internal", "private")]
        [InlineData("internal", "internal")]
        [InlineData("", "")]
#pragma warning disable CA1801 // Review unused parameters
        public void CSharp_CA1721_PropertyNamesMatchGetMethodNames_Unexposed_NoDiagnostics(string propertyAccessibility, string methodAccessibility)
#pragma warning restore CA1801 // Review unused parameters
        {
            var test = $@"
using System;

public class Test
{{
    {propertyAccessibility} string Prop {{ get; }}
    {methodAccessibility} string GetProp()
    {{
        return string.Empty;
    }}
}}";

            VerifyCSharp(test);
        }

        [Theory]
        [InlineData("public", "private")]
        [InlineData("protected", "private")]
        [InlineData("protected internal", "private")]
        [InlineData("public", "internal")]
        [InlineData("protected", "internal")]
        [InlineData("protected internal", "internal")]
        [InlineData("public", "")]
        [InlineData("protected", "")]
        [InlineData("protected internal", "")]
        [InlineData("private", "public")]
        [InlineData("private", "protected")]
        [InlineData("private", "protected internal")]
        [InlineData("internal", "public")]
        [InlineData("internal", "protected")]
        [InlineData("internal", "protected internal")]
        [InlineData("", "public")]
        [InlineData("", "protected")]
        [InlineData("", "protected internal")]
#pragma warning disable CA1801 // Review unused parameters
        public void CSharp_CA1721_PropertyNamesMatchGetMethodNames_MixedExposure_NoDiagnostics(string propertyAccessibility, string methodAccessibility)
#pragma warning restore CA1801 // Review unused parameters
        {
            var test = $@"
using System;

public class Test
{{
    {propertyAccessibility} string Prop {{ get; }}
    {methodAccessibility} string GetProp()
    {{
        return string.Empty;
    }}
}}";

            VerifyCSharp(test);
        }

        [Fact]
        public void CSharp_CA1721_PropertyNameMatchesBaseClassGetMethodName_Exposed_Diagnostic()
        {
            const string Test = @"
using System;

public class Foo
{
    public string GetDate()
    {
        return DateTime.Today.ToString();
    }
}

public class Bar : Foo
{
    public DateTime Date
    {
        get { return DateTime.Today; }
    }         
}";

            VerifyCSharp(
                Test,
                GetCA1721CSharpResultAt(line: 14, column: 21, identifierName: "Date", otherIdentifierName: "GetDate"));
        }


        [Fact]
        public void CSharp_CA1721_GetMethodNameMatchesBaseClassPropertyName_Exposed_Diagnostic()
        {
            const string Test = @"
using System;

public class Foo
{
    public DateTime Date
    {
        get { return DateTime.Today; }
    }         
}

public class Bar : Foo
{
    public string GetDate()
    {
        return DateTime.Today.ToString();
    }
}";
            VerifyCSharp(
                Test,
                GetCA1721CSharpResultAt(line: 14, column: 19, identifierName: "Date", otherIdentifierName: "GetDate"));
        }

        [Fact]
        public void Basic_CA1721_PropertyNameDoesNotMatchGetMethodName_Exposed_NoDiagnostic()
        {
            const string Test = @"
Imports System

Public Class Test
    Public ReadOnly Property [Date]() As DateTime
        Get
            Return DateTime.Today
        End Get
    End Property
    Public Function GetTime() As String
        Return Me.Date.ToString()
    End Function 
End Class";

            VerifyBasic(Test);
        }

        [Fact]
        public void Basic_CA1721_PropertyNameMatchesGetMethodName_Exposed_Diagnostic()
        {
            const string Test = @"
Imports System

Public Class Test
    Public ReadOnly Property [Date]() As DateTime
        Get
            Return DateTime.Today
        End Get
    End Property
    Public Function GetDate() As String
        Return Me.Date.ToString()
    End Function 
End Class";

            VerifyBasic(
                Test,
                GetCA1721BasicResultAt(line: 5, column: 30, identifierName: "Date", otherIdentifierName: "GetDate"));
        }


        [Fact]
        public void Basic_CA1721_PropertyNameMatchesBaseClassGetMethodName_Exposed_Diagnostic()
        {
            const string Test = @"
Imports System

Public Class Foo
    Public Function GetDate() As String
        Return DateTime.Today.ToString()
    End Function
End Class

Public Class Bar 
    Inherits Foo
    Public ReadOnly Property [Date]() As DateTime
        Get
            Return DateTime.Today
        End Get
    End Property
End Class";

            VerifyBasic(
                Test,
                GetCA1721BasicResultAt(line: 12, column: 30, identifierName: "Date", otherIdentifierName: "GetDate"));
        }


        [Fact]
        public void Basic_CA1721_GetMethodNameMatchesBaseClassPropertyName_Exposed_Diagnostic()
        {
            const string Test = @"
Imports System

Public Class Foo
    Public ReadOnly Property [Date]() As DateTime
        Get
            Return DateTime.Today
        End Get
    End Property
End Class
Public Class Bar 
    Inherits Foo
    Public Function GetDate() As String
        Return DateTime.Today.ToString()
    End Function
End Class";

            VerifyBasic(
                Test,
                GetCA1721BasicResultAt(line: 13, column: 21, identifierName: "Date", otherIdentifierName: "GetDate"));
        }

        #region Helpers

        private static DiagnosticResult GetCA1721CSharpResultAt(int line, int column, string identifierName, string otherIdentifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage, identifierName, otherIdentifierName);
            return GetCSharpResultAt(line, column, PropertyNamesShouldNotMatchGetMethodsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1721BasicResultAt(int line, int column, string identifierName, string otherIdentifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage, identifierName, otherIdentifierName);
            return GetBasicResultAt(line, column, PropertyNamesShouldNotMatchGetMethodsAnalyzer.RuleId, message);
        }

        #endregion
    }
}