// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.QualityGuidelines.Analyzers.UnitTests
{
    public class MarkMembersAsStaticTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicMarkMembersAsStaticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpMarkMembersAsStaticAnalyzer();
        }

        [Fact]
        public void CSharpSimpleMembers()
        {
            VerifyCSharp(@"
public class MembersTests
{
    internal static int s_field;
    public const int Zero = 0;

    public int Method1(string name)
    {
        return name.Length;
    }

    public void Method2() { }

    public void Method3()
    {
        s_field = 4;
    }

    public int Method4()
    {
        return Zero;
    }
    
    public int Property
    {
        get { return 5; }
    }

    public int Property2
    {
        set { s_field = value; }
    }
}",
                GetCSharpResultAt(7, 16, "Method1"),
                GetCSharpResultAt(12, 17, "Method2"),
                GetCSharpResultAt(14, 17, "Method3"),
                GetCSharpResultAt(19, 16, "Method4"),
                GetCSharpResultAt(24, 16, "Property"),
                GetCSharpResultAt(29, 16, "Property2"));
        }

        [Fact]
        public void BasicSimpleMembers()
        {
            VerifyBasic(@"
Public Class MembersTests
    Shared s_field As Integer
    Public Const Zero As Integer = 0

    Public Function Method1(name As String) As Integer
        Return name.Length
    End Function

    Public Sub Method2()
    End Sub

    Public Sub Method3()
        s_field = 4
    End Sub

    Public Function Method4() As Integer
        Return Zero
    End Function

    Public ReadOnly Property Property1 As Integer
        Get
            Return 5
        End Get
    End Property

    Public WriteOnly Property Property2 As Integer
        Set
            s_field = Value
        End Set
    End Property
End Class
",
                GetBasicResultAt(7, 16, "Method1"),
                GetBasicResultAt(12, 17, "Method2"),
                GetBasicResultAt(14, 17, "Method3"),
                GetBasicResultAt(19, 16, "Method4"),
                GetBasicResultAt(24, 16, "Property1"),
                GetBasicResultAt(29, 16, "Property2"));
        }

        [Fact]
        public void CSharpSimpleMembers_NoDiagnostic()
        {
            VerifyCSharp(@"
public class MembersTests
{
    int x; 

    public int Method1(string name)
    {
        return x;
    }

    public event System.EventHandler<System.EventArgs> customEvent { add {} remove {} }
}");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string symbolName)
        {
            return GetCSharpResultAt(line, column, MarkMembersAsStaticAnalyzer.Rule, symbolName);
        }

        private DiagnosticResult GetBasicResultAt(int line, int column, string symbolName)
        {
            return GetBasicResultAt(line, column, MarkMembersAsStaticAnalyzer.Rule, symbolName);
        }
    }
}