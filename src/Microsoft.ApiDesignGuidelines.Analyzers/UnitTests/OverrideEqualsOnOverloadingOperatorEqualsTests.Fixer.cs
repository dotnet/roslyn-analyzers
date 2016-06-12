// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OverrideEqualsOnOverloadingOperatorEqualsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicOverrideEqualsOnOverloadingOperatorEqualsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            // Fixer fixes compiler diagnostics.
            return null;
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicOverrideEqualsOnOverloadingOperatorEqualsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpOverrideEqualsOnOverloadingOperatorEqualsFixer();
        }

        [Fact]
        public void CS0660()
        {
            VerifyCSharpFix(@"
class C
{
    public static bool operator ==(C c1, C c2) => true;
    public static bool operator !=(C c1, C c2) => false;
}
",
                @"
class C
{
    public static bool operator ==(C c1, C c2) => true;
    public static bool operator !=(C c1, C c2) => false;

    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }
}
",
                codeFixIndex: null,
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CS0660_Simplified()
        {
            VerifyCSharpFix(@"
using System;

class C
{
    public static bool operator ==(C c1, C c2) => true;
    public static bool operator !=(C c1, C c2) => false;
}
",
                @"
using System;

class C
{
    public static bool operator ==(C c1, C c2) => true;
    public static bool operator !=(C c1, C c2) => false;

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }
}
",
                codeFixIndex: null,
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CA2224()
        {
            VerifyBasicFix(@"
Class C
    Public Shared Operator =(c1 As C, c2 As C) As Boolean
        Return True
    End Operator

    Public Shared Operator <>(c1 As C, c2 As C) As Boolean
        Return False
    End Operator
End Class
",
@"
Class C
    Public Shared Operator =(c1 As C, c2 As C) As Boolean
        Return True
    End Operator

    Public Shared Operator <>(c1 As C, c2 As C) As Boolean
        Return False
    End Operator

    Public Overrides Function Equals(obj As Object) As Boolean
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        [Fact]
        public void CA2224_Simplified()
        {
            VerifyBasicFix(@"
Imports System

Class C
    Public Shared Operator =(c1 As C, c2 As C) As Boolean
        Return True
    End Operator

    Public Shared Operator <>(c1 As C, c2 As C) As Boolean
        Return False
    End Operator
End Class
",
@"
Imports System

Class C
    Public Shared Operator =(c1 As C, c2 As C) As Boolean
        Return True
    End Operator

    Public Shared Operator <>(c1 As C, c2 As C) As Boolean
        Return False
    End Operator

    Public Overrides Function Equals(obj As Object) As Boolean
        Throw New NotImplementedException()
    End Function
End Class
");
        }
    }
}