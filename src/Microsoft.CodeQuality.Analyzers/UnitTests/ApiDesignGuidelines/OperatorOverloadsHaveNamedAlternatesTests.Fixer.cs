// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class OperatorOverloadsHaveNamedAlternatesFixerTests : CodeFixTestBase
    {
        #region Boilerplate

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OperatorOverloadsHaveNamedAlternatesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OperatorOverloadsHaveNamedAlternatesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new OperatorOverloadsHaveNamedAlternatesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OperatorOverloadsHaveNamedAlternatesFixer();
        }

        #endregion

        #region C# tests

        [Fact]
        public void AddAlternateMethod_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static C operator +(C left, C right) { return new C(); }
}
",
@"
public class C
{
    public static C operator +(C left, C right) { return new C(); }

    public static C Add(C left, C right)
    {
        throw new System.NotImplementedException();
    }
}
");
        }

        [Fact]
        public void AddAlternateOfMultiples_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static C operator %(C left, C right) { return new C(); }
}
",
@"
public class C
{
    public static C operator %(C left, C right) { return new C(); }

    public static C Mod(C left, C right)
    {
        throw new System.NotImplementedException();
    }
}
");
        }

        [Fact]
        public void AddAlternateProperty_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static bool operator true(C item) { return true; }
    public static bool operator false(C item) { return false; }
}
",
@"
public class C
{
    public static bool operator true(C item) { return true; }
    public static bool operator false(C item) { return false; }

    public bool IsTrue
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }
}
");
        }

        [Fact]
        public void AddAlternateForConversion_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static implicit operator int(C item) { return 0; }
}
",
@"
public class C
{
    public static implicit operator int(C item) { return 0; }

    public int ToInt32()
    {
        throw new System.NotImplementedException();
    }
}
");
        }

        [Fact]
        public void AddAlternateForCompare_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static bool operator <(C left, C right) { return true; }   // error CS0216: The operator requires a matching operator '>' to also be defined
}
",
@"
public class C
{
    public static bool operator <(C left, C right) { return true; }   // error CS0216: The operator requires a matching operator '>' to also be defined

    public int CompareTo(C other)
    {
        if (ReferenceEquals(other, null))
        {
            return 1;
        }

        throw new System.NotImplementedException();
    }
}
", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void AddAlternateForStructCompare_CSharp()
        {
            VerifyCSharpFix(@"
public struct C
{
    public static bool operator <(C left, C right) { return true; }   // error CS0216: The operator requires a matching operator '>' to also be defined
}
",
@"
public struct C
{
    public static bool operator <(C left, C right) { return true; }   // error CS0216: The operator requires a matching operator '>' to also be defined

    public int CompareTo(C other)
    {
        throw new System.NotImplementedException();
    }
}
", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void AddAlternateForIncrement_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static C operator ++(C item) { return new C(); }
}
",
@"
public class C
{
    public static C operator ++(C item) { return new C(); }

    public static C Increment(C item)
    {
        throw new System.NotImplementedException();
    }
}
");
        }

        [Fact]
        public void FixImproperMethodVisibility_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static C operator +(C left, C right) { return new C(); }
    protected static C Add(C left, C right) { return new C(); }
}
",
@"
public class C
{
    public static C operator +(C left, C right) { return new C(); }

    public static C Add(C left, C right) { return new C(); }
}
");
        }

        [Fact]
        public void FixImproperPropertyVisibility_CSharp()
        {
            VerifyCSharpFix(@"
public class C
{
    public static bool operator true(C item) { return true; }
    public static bool operator false(C item) { return false; }
    bool IsTrue => true;
}
",
@"
public class C
{
    public static bool operator true(C item) { return true; }
    public static bool operator false(C item) { return false; }

    public bool IsTrue => true;
}
");
        }

        #endregion

        #region VB tests

        [Fact]
        public void AddAlternateMethod_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator
End Class
",
@"
Public Class C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator

    Public Shared Function Add(left As C, right As C) As C
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        [Fact]
        public void AddAlternateOfMultiples_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Operator Mod(left As C, right As C) As C
        Return New C()
    End Operator
End Class
",
@"
Public Class C
    Public Shared Operator Mod(left As C, right As C) As C
        Return New C()
    End Operator

    Public Shared Function [Mod](left As C, right As C) As C
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        [Fact]
        public void AddAlternateProperty_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Operator IsTrue(item As C) As Boolean
        Return True
    End Operator
    Public Shared Operator IsFalse(item As C) As Boolean
        Return False
    End Operator
End Class
",
@"
Public Class C
    Public Shared Operator IsTrue(item As C) As Boolean
        Return True
    End Operator
    Public Shared Operator IsFalse(item As C) As Boolean
        Return False
    End Operator

    Public ReadOnly Property IsTrue As Boolean
        Get
            Throw New System.NotImplementedException()
        End Get
    End Property
End Class
");
        }

        [Fact]
        public void AddAlternateForConversion_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Widening Operator CType(ByVal item As C) As Integer
        Return 0
    End Operator
End Class
",
@"
Public Class C
    Public Shared Widening Operator CType(ByVal item As C) As Integer
        Return 0
    End Operator

    Public Function ToInt32() As Integer
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        [Fact]
        public void AddAlternateForCompare_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Operator <(left As C, right As C) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator
End Class
",
@"
Public Class C
    Public Shared Operator <(left As C, right As C) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator

    Public Function CompareTo(other As C) As Integer
        If ReferenceEquals(other, Nothing) Then
            Return 1
        End If

        Throw New System.NotImplementedException()
    End Function
End Class
", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void AddAlternateForStructCompare_Basic()
        {
            VerifyBasicFix(@"
Public Structure C
    Public Shared Operator <(left As C, right As C) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator
End Structure
",
@"
Public Structure C
    Public Shared Operator <(left As C, right As C) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator

    Public Function CompareTo(other As C) As Integer
        Throw New System.NotImplementedException()
    End Function
End Structure
", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void FixImproperMethodVisibility_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator

    Protected Shared Function Add(left As C, right As C) As C
        Return New C()
    End Function
End Class
",
@"
Public Class C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator

    Public Shared Function Add(left As C, right As C) As C
        Return New C()
    End Function
End Class
");
        }

        [Fact]
        public void FixImproperPropertyVisibility_Basic()
        {
            VerifyBasicFix(@"
Public Class C
    Public Shared Operator IsTrue(item As C) As Boolean
        Return True
    End Operator
    Public Shared Operator IsFalse(item As C) As Boolean
        Return False
    End Operator

    Private ReadOnly Property IsTrue As Boolean
        Get
            Return True
        End Get
    End Property
End Class
",
@"
Public Class C
    Public Shared Operator IsTrue(item As C) As Boolean
        Return True
    End Operator
    Public Shared Operator IsFalse(item As C) As Boolean
        Return False
    End Operator

    Public ReadOnly Property IsTrue As Boolean
        Get
            Return True
        End Get
    End Property
End Class
");
        }

        #endregion
    }
}