﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public partial class OverloadOperatorEqualsOnOverridingValueTypeEqualsTests : CodeFixTestBase
    {
        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new OverloadOperatorEqualsOnOverridingValueTypeEqualsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OverloadOperatorEqualsOnOverridingValueTypeEqualsFixer();
        }

        [Fact]
        public void CA2231CSharpCodeFixNoEqualsOperator()
        {
            VerifyCSharpFix(@"
using System;

// value type without overriding Equals
public struct A
{    
    public override bool Equals(Object obj)
    {
        return true;
    }
}
",
@"
using System;

// value type without overriding Equals
public struct A
{    
    public override bool Equals(Object obj)
    {
        return true;
    }

    public static bool operator ==(A left, A right)
    {
        throw new NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new NotImplementedException();
    }
}
",

                // This fix introduces the compiler warning:
                // Test0.cs(5,15): warning CS0661: 'A' defines operator == or operator != but does not override Object.GetHashCode()
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CA2231BasicCodeFixNoEqualsOperator()
        {
            VerifyBasicFix(@"
Imports System

Public Structure A
    Public Overloads Overrides Function Equals(obj As Object) As Boolean
        Return True
    End Function
End Structure
",
@"
Imports System

Public Structure A
    Public Overloads Overrides Function Equals(obj As Object) As Boolean
        Return True
    End Function

    Public Shared Operator =(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator <>(left As A, right As A) As Boolean
        Throw New NotImplementedException()
    End Operator
End Structure
");
        }
    }
}
