// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Test.Utilities;
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
        public void CA1036ClassGenerateAllCSharp()
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
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (ReferenceEquals(left, null))
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(A left, A right)
    {
        return !(left == right);
    }

    public static bool operator <(A left, A right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(A left, A right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(A left, A right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(A left, A right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
");
        }

        [Fact]
        public void CA1036StructGenerateAllCSharp()
        {
            VerifyCSharpFix(@"
using System;

public struct A : IComparable
{    
    public int CompareTo(object obj)
    {
        return 1;
    }
}
", @"
using System;

public struct A : IComparable
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
        return left.Equals(right);
    }

    public static bool operator !=(A left, A right)
    {
        return !(left == right);
    }

    public static bool operator <(A left, A right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(A left, A right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(A left, A right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(A left, A right)
    {
        return left.CompareTo(right) >= 0;
    }
}
");
        }

        [Fact]
        public void CA1036ClassGenerateSomeCSharp()
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
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (ReferenceEquals(left, null))
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator <(A left, A right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(A left, A right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(A left, A right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(A left, A right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
", 
            validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CA1036StructGenerateSomeCSharp()
        {
            VerifyCSharpFix(@"
using System;

public struct A : IComparable
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

public struct A : IComparable
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
        return left.Equals(right);
    }

    public static bool operator <(A left, A right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(A left, A right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(A left, A right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(A left, A right)
    {
        return left.CompareTo(right) >= 0;
    }
}
", 
            validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CA1036ClassGenerateAllVisualBasic()
        {
            VerifyBasicFix(@"
Imports System

Public Class A : Implements IComparable

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

End Class
", @"
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
        If ReferenceEquals(left, right) Then
            Return True
        End If

        If ReferenceEquals(left, Nothing) Then
            Return False
        End If

        Return left.Equals(right)
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Return Not left = right
    End Operator

    Public Shared Operator <(left As A, right As A) As Boolean
        Return If(ReferenceEquals(left, Nothing), Not ReferenceEquals(right, Nothing), left.CompareTo(right) < 0)
    End Operator

    Public Shared Operator <=(left As A, right As A) As Boolean
        Return ReferenceEquals(left, Nothing) OrElse left.CompareTo(right) <= 0
    End Operator

    Public Shared Operator >(left As A, right As A) As Boolean
        Return Not ReferenceEquals(left, Nothing) AndAlso left.CompareTo(right) > 0
    End Operator

    Public Shared Operator >=(left As A, right As A) As Boolean
        Return If(ReferenceEquals(left, Nothing), ReferenceEquals(right, Nothing), left.CompareTo(right) >= 0)
    End Operator
End Class
");
        }

        [Fact]
        public void CA1036StructGenerateAllVisualBasic()
        {
            VerifyBasicFix(@"
Imports System

Public Structure A : Implements IComparable

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

End Structure
", @"
Imports System

Public Structure A : Implements IComparable

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
        Return left.Equals(right)
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Return Not left = right
    End Operator

    Public Shared Operator <(left As A, right As A) As Boolean
        Return left.CompareTo(right) < 0
    End Operator

    Public Shared Operator <=(left As A, right As A) As Boolean
        Return left.CompareTo(right) <= 0
    End Operator

    Public Shared Operator >(left As A, right As A) As Boolean
        Return left.CompareTo(right) > 0
    End Operator

    Public Shared Operator >=(left As A, right As A) As Boolean
        Return left.CompareTo(right) >= 0
    End Operator
End Structure
");
        }

        [Fact]
        public void CA1036ClassGenerateSomeVisualBasic()
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

End Class
", @"
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
        If ReferenceEquals(left, right) Then
            Return True
        End If

        If ReferenceEquals(left, Nothing) Then
            Return False
        End If

        Return left.Equals(right)
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Return Not left = right
    End Operator

    Public Shared Operator <=(left As A, right As A) As Boolean
        Return ReferenceEquals(left, Nothing) OrElse left.CompareTo(right) <= 0
    End Operator

    Public Shared Operator >(left As A, right As A) As Boolean
        Return Not ReferenceEquals(left, Nothing) AndAlso left.CompareTo(right) > 0
    End Operator

    Public Shared Operator >=(left As A, right As A) As Boolean
        Return If(ReferenceEquals(left, Nothing), ReferenceEquals(right, Nothing), left.CompareTo(right) >= 0)
    End Operator
End Class
", 
            validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CA1036StructGenerateSomeVisualBasic()
        {
            VerifyBasicFix(@"
Imports System

Public Structure A : Implements IComparable

    Public Overrides Function GetHashCode() As Integer
        Return 1234
    End Function

    Public Shared Operator <(objLeft As A, objRight As A) As Boolean   ' error BC33033: Matching '>' operator is required
        Return True
    End Operator

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return 1
    End Function

End Structure
", @"
Imports System

Public Structure A : Implements IComparable

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
        Return left.Equals(right)
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Return Not left = right
    End Operator

    Public Shared Operator <=(left As A, right As A) As Boolean
        Return left.CompareTo(right) <= 0
    End Operator

    Public Shared Operator >(left As A, right As A) As Boolean
        Return left.CompareTo(right) > 0
    End Operator

    Public Shared Operator >=(left As A, right As A) As Boolean
        Return left.CompareTo(right) >= 0
    End Operator
End Structure
", 
            validationMode: TestValidationMode.AllowCompileErrors);
        }
    }
}
