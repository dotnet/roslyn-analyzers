// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
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
        public void CSharp_CA1721_NoDiagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public DateTime Today
    {
        get { return DateTime.Today; }
    }
    public string GetDate()
    {
        return this.Today.ToString();
    }
}
");
        }

        [Fact]
        public void CSharp_CA1721_SomeDiagnostic1()
        {
            VerifyCSharp(@"
public class Test
{
    public DateTime Date
    {
        get { return DateTime.Today; }
    }         
    public string GetDate()
    {
        return this.Date.ToString();
    }
}
",
            GetCA1721CSharpDeclaringTypeResultAt(line: 4, column: 21, identifierName: "Date", typeName: "Test"));
        }


        [Fact]
        public void CSharp_CA1721_SomeDiagnostic2()
        {
            VerifyCSharp(@"
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
            GetCA1721CSharpBaseTypeResultAt(line: 13, column: 25, identifierName: "Date", typeName: "Ray"));
        }


        [Fact]
        public void CSharp_CA1721_SomeDiagnostic3()
        {
            VerifyCSharp(@"
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
            GetCA1721CSharpBaseTypeResultAt(line: 13, column: 23, identifierName: "GetDate", typeName: "Ray"));
        }

        [Fact]
        public void Basic_CA1721_NoDiagnostic()
        {
            VerifyBasic(@"
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
        public void Basic_CA1721_SomeDiagnostic1()
        {
            VerifyBasic(@"
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
            GetCA1721BasicDeclaringTypeResultAt(line: 3, column: 30, identifierName: "Date", typeName: "Test"));
        }


        [Fact]
        public void Basic_CA1721_SomeDiagnostic2()
        {
            VerifyBasic(@"
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
            GetCA1721BasicBaseTypeResultAt(line: 10, column: 34, identifierName: "Date", typeName: "Ray"));
        }


        [Fact]
        public void Basic_CA1721_SomeDiagnostic3()
        {
            VerifyBasic(@"
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
            GetCA1721BasicBaseTypeResultAt(line: 12, column: 25, identifierName: "GetDate", typeName: "Ray"));
        }
        #region Helpers

        private static DiagnosticResult GetCA1721CSharpDeclaringTypeResultAt(int line, int column, string identifierName, string typeName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage, identifierName, typeName);
            return GetCSharpResultAt(line, column, PropertyNamesShouldNotMatchGetMethodsAnalyzer.RuleId, message);
        }
        private static DiagnosticResult GetCA1721CSharpBaseTypeResultAt(int line, int column, string identifierName, string typeName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage, identifierName, typeName);
            return GetCSharpResultAt(line, column, PropertyNamesShouldNotMatchGetMethodsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1721BasicDeclaringTypeResultAt(int line, int column, string identifierName, string typeName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage, identifierName, typeName);
            return GetBasicResultAt(line, column, PropertyNamesShouldNotMatchGetMethodsAnalyzer.RuleId, message);
        }
        private static DiagnosticResult GetCA1721BasicBaseTypeResultAt(int line, int column, string identifierName, string typeName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage, identifierName, typeName);
            return GetBasicResultAt(line, column, PropertyNamesShouldNotMatchGetMethodsAnalyzer.RuleId, message);
        }
        #endregion
    }
}