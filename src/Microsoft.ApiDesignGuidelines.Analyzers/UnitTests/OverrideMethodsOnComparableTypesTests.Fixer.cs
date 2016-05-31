﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public partial class OverrideMethodsOnComparableTypesTests
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OverrideMethodsOnComparableTypesFixer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new OverrideMethodsOnComparableTypesFixer();
        }

        [Fact]
        public void CA1036GenerateAllCSharp()
        {
            VerifyCSharpFix(@"
using System;

public class A : IComparable
{    
    public int CompareTo(object obj)
    {
        return 1;
    }
}
", @"
using System;

public class A : IComparable
{    
    public int CompareTo(object obj)
    {
        return 1;
    }

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(A left, A right)
    {
        throw new NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <(A left, A right)
    {
        throw new NotImplementedException();
    }

    public static bool operator >(A left, A right)
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact]
        public void CA1036GenerateSomeCSharp()
        {
            VerifyCSharpFix(@"
using System;

public class A : IComparable
{    
    public override int GetHashCode()
    {
        return 1234;
    }

    public int CompareTo(object obj)
    {
        return 1;
    }

    public static bool operator !=(A objLeft, A objRight)   // error CS0216: The operator requires a matching operator '==' to also be defined
    {
        return true;
    }
}
", @"
using System;

public class A : IComparable
{    
    public override int GetHashCode()
    {
        return 1234;
    }

    public int CompareTo(object obj)
    {
        return 1;
    }

    public static bool operator !=(A objLeft, A objRight)   // error CS0216: The operator requires a matching operator '==' to also be defined
    {
        return true;
    }

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(A left, A right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <(A left, A right)
    {
        throw new NotImplementedException();
    }

    public static bool operator >(A left, A right)
    {
        throw new NotImplementedException();
    }
}
", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CA1036GenerateAllVisualBasic()
        {
            VerifyBasicFix(@"
Imports System

Public Class A : Implements IComparable

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

End Class", @"
Imports System

Public Class A : Implements IComparable

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function GetHashCode() As Integer
        Throw New NotImplementedException()
    End Function

    Public Shared Operator =(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator <(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator >(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator
End Class");
        }

        [Fact]
        public void CA1036GenerateSomeVisualBasic()
        {
            VerifyBasicFix(@"
Imports System

Public Class A : Implements IComparable

    Public Overrides Function GetHashCode() As Integer
        Return 1234
    End Function

    Public Shared Operator <(objLeft As A, objRight As A) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

End Class", @"
Imports System

Public Class A : Implements IComparable

    Public Overrides Function GetHashCode() As Integer
        Return 1234
    End Function

    Public Shared Operator <(objLeft As A, objRight As A) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Shared Operator =(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator >(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator
End Class", validationMode: TestValidationMode.AllowCompileErrors);
        }
    }
}
