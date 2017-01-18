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
        public void CSharp_CA1721_PropertyNameDoesNotMatchGetMethod_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class Test
{
    public string PropA { get; }
    public string GetPropB()
    {
        return string.Empty;
    }
}
");
        }

        [Fact]
        public void CSharp_CA1721_PropertyNamesMatchGetMethods_Accessible_Diagnostics()
        {
            VerifyCSharp(@"
using System;

public class Test
{
    // Public property, public method
    public string PropA { get; }
    public string GetPropA()
    {
        return string.Empty;
    }

    // Public property, protected method
    public string PropB { get; }
    protected string GetPropB()
    {
        return string.Empty;
    }

    // Protected property, public method
    protected string PropC { get; }
    public string GetPropC()
    {
        return string.Empty;
    }

    // Protected property, protected method
    protected string PropD { get; }
    protected string GetPropD()
    {
        return string.Empty;
    }
}
",
            GetCA1721CSharpResultAt(line: 7, column: 19, identifierName: "PropA", otherIdentifierName: "GetPropA"),
            GetCA1721CSharpResultAt(line: 14, column: 19, identifierName: "PropB", otherIdentifierName: "GetPropB"),
            GetCA1721CSharpResultAt(line: 21, column: 22, identifierName: "PropC", otherIdentifierName: "GetPropC"),
            GetCA1721CSharpResultAt(line: 28, column: 22, identifierName: "PropD", otherIdentifierName: "GetPropD"));
        }

        [Fact]
        public void CSharp_CA1721_PropertyNamesMatchGetMethods_NotAccessible_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class Test
{
    // Private property, private method
    private string PropA { get; }
    private string GetPropA()
    {
        return string.Empty;
    }

    // Private property, internal method
    private string PropB { get; }
    internal string GetPropB()
    {
        return string.Empty;
    }

    // Internal property, private method
    internal string PropC { get; }
    private string GetPropC()
    {
        return string.Empty;
    }

    // Internal property, internal method
    internal string PropD { get; }
    internal string GetPropD()
    {
        return string.Empty;
    }

    // Implicitly private property/method
    string PropE { get; }
    string GetPropE() 
    {
        return string.Empty;
    }
}
");
        }

        [Fact]
        public void CSharp_CA1721_PropertyNamesMatchGetMethods_MixedAccessiblity_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class Test
{
    // Public property, private method
    public string PropA { get; }
    private string GetPropA()
    {
        return string.Empty;
    }

    // Private property, public method
    private string PropB { get; }
    public string GetPropB()
    {
        return string.Empty;
    }
}
");
        }

        [Fact]
        public void CSharp_CA1721_PropertyNameMatchesBaseClassGetMethod_Diagnostic()
        {
            VerifyCSharp(@"
using System;

public class Foo
{
    public class Ray
    {
        public string GetDate()
        {
            return DateTime.Today.ToString();
        }
    }
    public class Bar : Ray
    {
        public DateTime Date
        {
            get { return DateTime.Today; }
        }         
    }
}
",
            GetCA1721CSharpResultAt(line: 15, column: 25, identifierName: "Date", otherIdentifierName: "GetDate"));
        }


        [Fact]
        public void CSharp_CA1721_GetMethodMatchesBaseClassPropertyName_Diagnostic()
        {
            VerifyCSharp(@"
using System;

public class Foo
{
    public class Ray
    {
        public DateTime Date
        {
            get { return DateTime.Today; }
        }         
    }
    public class Bar : Ray
    {
        public string GetDate()
        {
            return DateTime.Today.ToString();
        }
    }
}
",
            GetCA1721CSharpResultAt(line: 15, column: 23, identifierName: "Date", otherIdentifierName: "GetDate"));
        }

        [Fact]
        public void Basic_CA1721_PropertyNameDoesNotMatchGetMethod_NoDiagnostic()
        {
            VerifyBasic(@"
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
End Class
");
        }

        [Fact]
        public void Basic_CA1721_PropertyNameMatchesGetMethod_Diagnostic()
        {
            VerifyBasic(@"
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
End Class
",
            GetCA1721BasicResultAt(line: 5, column: 30, identifierName: "Date", otherIdentifierName: "GetDate"));
        }


        [Fact]
        public void Basic_CA1721_PropertyNameMatchesBaseClassGetMethod_Diagnostic()
        {
            VerifyBasic(@"
Imports System

Public Class Foo
    Public Class Ray
        Public Function GetDate() As String
            Return DateTime.Today.ToString()
        End Function
    End Class
    Public Class Bar 
        Inherits Ray
        Public ReadOnly Property [Date]() As DateTime
            Get
                Return DateTime.Today
            End Get
        End Property
    End Class
End Class
",
            GetCA1721BasicResultAt(line: 12, column: 34, identifierName: "Date", otherIdentifierName: "GetDate"));
        }


        [Fact]
        public void Basic_CA1721_GetMethodMatchesBaseClassPropertyName_Diagnostic()
        {
            VerifyBasic(@"
Imports System

Public Class Foo
    Public Class Ray
        Public ReadOnly Property [Date]() As DateTime
            Get
                Return DateTime.Today
            End Get
        End Property
    End Class
    Public Class Bar 
        Inherits Ray
        Public Function GetDate() As String
            Return DateTime.Today.ToString()
        End Function
    End Class
End Class
",
            GetCA1721BasicResultAt(line: 14, column: 25, identifierName: "Date", otherIdentifierName: "GetDate"));
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