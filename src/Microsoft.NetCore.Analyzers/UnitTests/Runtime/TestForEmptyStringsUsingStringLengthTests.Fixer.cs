// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class TestForEmptyStringsUsingStringLengthFixerTests : CodeFixTestBase
    {
        [Fact]
        public void CA1820_FixTestEmptyStringsUsingIsNullOrEmpty()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return s == string.Empty;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return string.IsNullOrEmpty(s);
    }
}
");
            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return s = String.Empty
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return String.IsNullOrEmpty(s)
    End Function
End Class
");
        }

        [Fact]
        public void CA1820_FixTestEmptyStringsUsingStringLength()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return s == string.Empty;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return s.Length == 0;
    }
}
", 1);

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return s = String.Empty
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return s.Length = 0
    End Function
End Class
", 1);
        }

        [Fact]
        public void CA1820_FixTestEmptyStringsUsingIsNullOrEmptyComparisonOnRight()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return string.Empty == s;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return string.IsNullOrEmpty(s);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return String.Empty = s
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return String.IsNullOrEmpty(s)
    End Function
End Class
");
        }

        [Fact]
        public void CA1820_FixTestEmptyStringsUsingStringLengthComparisonOnRight()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return string.Empty == s;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return 0 == s.Length;
    }
}
", 1);

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return String.Empty = s
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return 0 = s.Length
    End Function
End Class
", 1);
        }

        [Fact]
        public void CA1820_FixInequalityTestEmptyStringsUsingIsNullOrEmpty()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return s != string.Empty;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return !string.IsNullOrEmpty(s);
    }
}
");
            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return s <> String.Empty
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return Not String.IsNullOrEmpty(s)
    End Function
End Class
");
        }

        [Fact]
        public void CA1820_FixInequalityTestEmptyStringsUsingStringLength()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return s != string.Empty;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return s.Length != 0;
    }
}
", 1);

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return s <> String.Empty
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return s.Length <> 0
    End Function
End Class
", 1);
        }

        [Fact]
        public void CA1820_FixInequalityTestEmptyStringsUsingIsNullOrEmptyComparisonOnRight()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return string.Empty != s;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return !string.IsNullOrEmpty(s);
    }
}
");
            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return String.Empty <> s
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return Not String.IsNullOrEmpty(s)
    End Function
End Class
");
        }

        [Fact]
        public void CA1820_FixInequalityTestEmptyStringsUsingStringLengthComparisonOnRight()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(string s)
    {
        return string.Empty != s;
    }
}
", @"
public class A
{
    public bool Compare(string s)
    {
        return 0 != s.Length;
    }
}
", 1);

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return String.Empty <> s
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As String) As Boolean
        Return 0 <> s.Length
    End Function
End Class
", 1);

        }
        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInFunctionArgument()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        G(_s == string.Empty);
    }

    public void G(bool comparison) {}
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        G(string.IsNullOrEmpty(_s));
    }

    public void G(bool comparison) {}
}
");

            VerifyBasicFix(@"
Public Class A
    Private _s As String = String.Empty

    Public Sub F()
        G(_s = String.Empty)
    End Sub

    Public Sub G(comparison As Boolean)
    End Sub
End Class
", @"
Public Class A
    Private _s As String = String.Empty

    Public Sub F()
        G(String.IsNullOrEmpty(_s))
    End Sub

    Public Sub G(comparison As Boolean)
    End Sub
End Class
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInTernaryOperator()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public int F()
    {
        return _s == string.Empty ? 1 : 0;
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public int F()
    {
        return string.IsNullOrEmpty(_s) ? 1 : 0;
    }
}
");

            // VB doesn't have the ternary operator, but we add this test for symmetry.
            VerifyBasicFix(@"
Public Class A
    Private _s As String = String.Empty

    Public Function F() As Integer
        Return If(_s = String.Empty, 1, 0)
    End Function
End Class
", @"
Public Class A
    Private _s As String = String.Empty

    Public Function F() As Integer
        Return If(String.IsNullOrEmpty(_s), 1, 0)
    End Function
End Class
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInThrowStatement()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        throw _s != string.Empty ? new System.Exception() : new System.ArgumentException();
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        throw !string.IsNullOrEmpty(_s) ? new System.Exception() : new System.ArgumentException();
    }
}
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInCatchFilterClause()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        try { }
        catch (System.Exception ex) when (_s != string.Empty) { }
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        try { }
        catch (System.Exception ex) when (!string.IsNullOrEmpty(_s)) { }
    }
}
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInYieldReturnStatement()
        {
            VerifyCSharpFix(@"
using System.Collections.Generic;

public class A
{
    string _s = string.Empty;

    public IEnumerable<bool> F()
    {
        yield return _s != string.Empty;
    }
}
", @"
using System.Collections.Generic;

public class A
{
    string _s = string.Empty;

    public IEnumerable<bool> F()
    {
        yield return !string.IsNullOrEmpty(_s);
    }
}
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInSwitchStatement()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        switch (_s != string.Empty)
        {
            default:
                throw new System.NotImplementedException();
        }
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        switch (!string.IsNullOrEmpty(_s))
        {
            default:
                throw new System.NotImplementedException();
        }
    }
}
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInForLoop()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        for (; _s != string.Empty; )
        {
            throw new System.Exception();
        }
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        for (; !string.IsNullOrEmpty(_s); )
        {
            throw new System.Exception();
        }
    }
}
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInWhileLoop()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        while (_s != string.Empty)
        {
        }
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        while (!string.IsNullOrEmpty(_s))
        {
        }
    }
}
");
        }

        [Fact]
        public void CA1820_FixForComparisonWithEmptyStringInDoWhileLoop()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        do
        {
        }
        while (_s != string.Empty);
    }
}
", @"
public class A
{
    string _s = string.Empty;

    public void F()
    {
        do
        {
        }
        while (!string.IsNullOrEmpty(_s));
    }
}
");
        }

        [Fact]
        public void CA1820_MultilineFixTestEmptyStringsUsingIsNullOrEmpty()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;
    public bool Compare(string s)
    {
        return s == string.Empty ||
               s == _s;
    }
}
", @"
public class A
{
    string _s = string.Empty;
    public bool Compare(string s)
    {
        return string.IsNullOrEmpty(s) ||
               s == _s;
    }
}
");
            VerifyBasicFix(@"
Public Class A
    Private _s As String = String.Empty
    Public Function Compare(s As String) As Boolean
        Return s = String.Empty Or
               s = _s
    End Function
End Class
", @"
Public Class A
    Private _s As String = String.Empty
    Public Function Compare(s As String) As Boolean
        Return String.IsNullOrEmpty(s) Or
               s = _s
    End Function
End Class
");
        }

        [Fact]
        public void CA1820_MultilineFixTestEmptyStringsUsingStringLength()
        {
            VerifyCSharpFix(@"
public class A
{
    string _s = string.Empty;
    public bool Compare(string s)
    {
        return s == string.Empty ||
               s == _s;
    }
}
", @"
public class A
{
    string _s = string.Empty;
    public bool Compare(string s)
    {
        return s.Length == 0 ||
               s == _s;
    }
}
", 1);
            VerifyBasicFix(@"
Public Class A
    Private _s As String = String.Empty
    Public Function Compare(s As String) As Boolean
        Return s = String.Empty Or
               s = _s
    End Function
End Class
", @"
Public Class A
    Private _s As String = String.Empty
    Public Function Compare(s As String) As Boolean
        Return s.Length = 0 Or
               s = _s
    End Function
End Class
", 1);
        }
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new TestForEmptyStringsUsingStringLengthAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TestForEmptyStringsUsingStringLengthAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicTestForEmptyStringsUsingStringLengthFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpTestForEmptyStringsUsingStringLengthFixer();
        }
    }
}