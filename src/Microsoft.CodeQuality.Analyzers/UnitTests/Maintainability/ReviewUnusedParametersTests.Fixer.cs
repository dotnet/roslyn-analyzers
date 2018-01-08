// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
using Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class ReviewUnusedParametersFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewUnusedParametersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewUnusedParametersAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicReviewUnusedParametersFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpReviewUnusedParametersFixer();
        }

        [Fact]
        public void BaseScenario_CSharp()
        {
            var code = @"
using System;

class C
{
    public int Property1 { get; set; }

    public int Field1;

    public C(int param)
    {
    }

    public void UnusedParamMethod(int param)
    {
    }

    public static void UnusedParamStaticMethod(int param1)
    {
    }

    public void UnusedDefaultParamMethod(int defaultParam = 1)
    {
    }

    public void UnusedParamsArrayParamMethod(params int[] paramsArr)
    {
    }

    public void MultipleUnusedParamsMethod(int param1, int param2)
    {
    }

    private void UnusedRefParamMethod(ref int param1)
    {
    }

    public void UnusedErrorTypeParamMethod(UndefinedType param1) // error CS0246: The type or namespace name 'UndefinedType' could not be found.
    {
    }

    public void Caller()
    {
        var c = new C(0);
        UnusedParamMethod(this.Property1);
        int b = 0;
        UnusedParamMethod(b);
        UnusedParamStaticMethod(1 + 1);
        UnusedDefaultParamMethod(this.Field1);
        UnusedParamsArrayParamMethod(new int[0]);
        MultipleUnusedParamsMethod(0, 1);
        int a = 0;
        UnusedRefParamMethod(ref a);
    }
}
";
            var fix = @"
using System;

class C
{
    public int Property1 { get; set; }

    public int Field1;

    public C()
    {
    }

    public void UnusedParamMethod()
    {
    }

    public static void UnusedParamStaticMethod()
    {
    }

    public void UnusedDefaultParamMethod()
    {
    }

    public void UnusedParamsArrayParamMethod()
    {
    }

    public void MultipleUnusedParamsMethod()
    {
    }

    private void UnusedRefParamMethod()
    {
    }

    public void UnusedErrorTypeParamMethod() // error CS0246: The type or namespace name 'UndefinedType' could not be found.
    {
    }

    public void Caller()
    {
        var c = new C();
        UnusedParamMethod();
        int b = 0;
        UnusedParamMethod();
        UnusedParamStaticMethod();
        UnusedDefaultParamMethod();
        UnusedParamsArrayParamMethod();
        MultipleUnusedParamsMethod();
        int a = 0;
        UnusedRefParamMethod();
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void ExternalFileScenario_CSharp()
        {
            var code = @"
class C
{
    public static void UnusedParamStaticMethod(int param1)
    {
    }
}

class D
{
    public void Caller()
    {
        C.UnusedParamStaticMethod(0);
        E.M(0);
    }
}
";
            var fix = @"
class C
{
    public static void UnusedParamStaticMethod()
    {
    }
}

class D
{
    public void Caller()
    {
        C.UnusedParamStaticMethod();
        E.M();
    }
}
";

            var anotherCode = @"
class E
{
    public static void M(int param1) { }
}
";
            var anotherCodeFix = @"
class E
{
    public static void M() { }
}
";
            VerifyCSharpFix(new[] { code, anotherCode }, new[] { fix, anotherCodeFix });
        }

        [Fact]
        public void CommentsNearParams_CSharp()
        {
            var code = @"
class C
{
    public C(/* comment left */ int /* comment middle */ param /* comment right */)
    {
    }

    public int M(/* comment 1 */ int /* comment 2 */ param1 /* comment 3 */, /* comment 4 */ int /* comment 5 */ param2 /* comment 6 */)
    {   
        return param2;
    }

    public void Caller()
    {
        var c = new C(/* caller comment left */ 0 /* caller comment right */);
        M(/* comment 1 */ 0 /* comment 2 */, /* comment 3 */ 1 /* comment 4 */);
    }
}
";
            var fix = @"
class C
{
    public C(/* comment left */ )
    {
    }

    public int M(/* comment 1 */ int /* comment 5 */ param2 /* comment 6 */)
    {   
        return param2;
    }

    public void Caller()
    {
        var c = new C(/* caller comment left */ );
        M(/* comment 1 */ 1 /* comment 4 */);
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void NamedParams_CSharp()
        {
            var code = @"
class C
{
    public int UnusedParamMethod(int param1, int param2)
    {
        return param1;
    }

    public void Caller()
    {
        UnusedParamMethod(param2: 0, param1: 1);
    }
}
";
            var fix = @"
class C
{
    public int UnusedParamMethod(int param1)
    {
        return param1;
    }

    public void Caller()
    {
        UnusedParamMethod(param1: 1);
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void MultipleNamespaces_CSharp()
        {
            var code = @"
namespace A.B.C.D
{
    public class Test
    {
        public Test(int param1) { }
        
        public static void UnusedParamMethod(int param1) { }
    }
}

namespace E
{
    class CallerClass
    {
        public void Caller()
        {
            var test = new A.B.C.D.Test(0);
            A.B.C.D.Test.UnusedParamMethod(0);
        }
    }
}
";
            var fix = @"
namespace A.B.C.D
{
    public class Test
    {
        public Test() { }
        
        public static void UnusedParamMethod() { }
    }
}

namespace E
{
    class CallerClass
    {
        public void Caller()
        {
            var test = new A.B.C.D.Test();
            A.B.C.D.Test.UnusedParamMethod();
        }
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void IndexerNoFix_CSharp()
        {
            var code = @"
class C
{
    public int this[int i]
    {
        get { return 0; }
        set { }
    }
}
";
            VerifyCSharpFix(code, code);
        }

        [Fact]
        public void PropertyNoFix_CSharp()
        {
            var code = @"
class C
{
    public int Property
    {
        get { return 0; }
        set { }
    }
}
";
            VerifyCSharpFix(code, code);
        }

        [Fact]
        public void CalculationsInParameter_CSharp()
        {
            var code = @"
class C
{
    void M() { }
    
    int N(int x) => x;
    
    void Caller()
    {
        M();
    }
}
";

            VerifyCSharpFix(code, code);
        }

        [Fact]
        public void Conversion_CSharp()
        {
            var code = @"
class C
{
    public static explicit operator int(C value) => 0;

    public void M1(double d) { }

    public void M2(int i) { }

    public void M3(int x) { }

    public void Caller()
    {
        int i = 0;
        M1(i);
        double d = 0;
        M2((int)d);
        var instance = new C();
        M3((int)instance);
    }
}
";
            var fix = @"
class C
{
    public static explicit operator int(C value) => 0;

    public void M1() { }

    public void M2() { }

    public void M3() { }

    public void Caller()
    {
        int i = 0;
        M1();
        double d = 0;
        M2();
        var instance = new C();
        M3();
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void ExtensionMethod_CSharp()
        {
            var code = @"
static class C
{
    static void ExtensionMethod(this int i) { }
    static void ExtensionMethod(this int i, int anotherParam) { }

    static void Caller()
    {
        int i = 0;
        i.ExtensionMethod();
        i.ExtensionMethod(i);
    }
}
";
            var fix = @"
static class C
{
    static void ExtensionMethod(this int i) { }
    static void ExtensionMethod(this int i) { }

    static void Caller()
    {
        int i = 0;
        i.ExtensionMethod();
        i.ExtensionMethod();
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/22449")]
        public void DictionaryConstructor_CSharp()
        {
            var code = @"
using System.Collections.Generic;

class Dict : Dictionary<int, MyValue>
{
    public void Add(int key, int a, int b)
    {
        var val = new MyValue();
        val.A = a;
        this.Add(key, val);
    }
    
    public static Dict Create()
    {
        return new Dict()
        {
            {0, 1, 2}
        };
    }
}

class MyValue
{
    public int A;
}
";

            var fix = @"
using System.Collections.Generic;

class Dict : Dictionary<int, MyValue>
{
    public void Add(int key, int a)
    {
        var val = new MyValue();
        val.A = a;
        this.Add(key, val);
    }
    
    public static Dict Create()
    {
        return new Dict()
        {
            {0, 1}
        };
    }
}

class MyValue
{
    public int A;
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void BaseScenario_Basic()
        {
            var code = @"
Class C
    Public Property Property1 As Integer

    Public Field1 As Integer

    Public Sub New(param As Integer)
    End Sub

    Public Sub UnusedParamMethod(param As Integer)
    End Sub

    Public Shared Sub UnusedParamStaticMethod(param1 As Integer)
    End Sub

    Public Sub UnusedDefaultParamMethod(Optional defaultParam As Integer = 1)
    End Sub

    Public Sub UnusedParamsArrayParamMethod(ParamArray paramsArr As Integer())
    End Sub

    Public Sub MultipleUnusedParamsMethod(param1 As Integer, param2 As Integer)
    End Sub

    Private Sub UnusedRefParamMethod(ByRef param1 As Integer)
    End Sub

    Public Sub UnusedErrorTypeParamMethod(param1 As UndefinedType) ' error BC30002: Type 'UndefinedType' is not defined.
    End Sub

    Public Sub Caller()
        Dim c = New C(0)
        UnusedParamMethod(Property1)
        Dim b As Integer = 0
        UnusedParamMethod(b)
        UnusedParamStaticMethod(1 + 1)
        UnusedDefaultParamMethod(Field1)
        UnusedParamsArrayParamMethod(New Integer() {})
        MultipleUnusedParamsMethod(0, 1)
        Dim a As Integer = 0
        UnusedRefParamMethod(a)
    End Sub
End Class
";
            var fix = @"
Class C
    Public Property Property1 As Integer

    Public Field1 As Integer

    Public Sub New()
    End Sub

    Public Sub UnusedParamMethod()
    End Sub

    Public Shared Sub UnusedParamStaticMethod()
    End Sub

    Public Sub UnusedDefaultParamMethod()
    End Sub

    Public Sub UnusedParamsArrayParamMethod()
    End Sub

    Public Sub MultipleUnusedParamsMethod()
    End Sub

    Private Sub UnusedRefParamMethod()
    End Sub

    Public Sub UnusedErrorTypeParamMethod() ' error BC30002: Type 'UndefinedType' is not defined.
    End Sub

    Public Sub Caller()
        Dim c = New C()
        UnusedParamMethod()
        Dim b As Integer = 0
        UnusedParamMethod()
        UnusedParamStaticMethod()
        UnusedDefaultParamMethod()
        UnusedParamsArrayParamMethod()
        MultipleUnusedParamsMethod()
        Dim a As Integer = 0
        UnusedRefParamMethod()
    End Sub
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void ExternalFileScenario_Basic()
        {
            var code = @"
Class C
    Public Shared Sub UnusedParamStaticMethod(param1 As Integer)
    End Sub
End Class

Class D
    Public Sub Caller()
        C.UnusedParamStaticMethod(0)
        E.M(0)
    End Sub
End Class
";
            var fix = @"
Class C
    Public Shared Sub UnusedParamStaticMethod()
    End Sub
End Class

Class D
    Public Sub Caller()
        C.UnusedParamStaticMethod()
        E.M()
    End Sub
End Class
";

            var anotherCode = @"
Class E
    Public Shared Sub M(param1 As Integer)
    End Sub
End Class
";
            var anotherCodeFix = @"
Class E
    Public Shared Sub M()
    End Sub
End Class
";
            VerifyBasicFix(new[] { code, anotherCode }, new[] { fix, anotherCodeFix });
        }

        [Fact]
        public void NamedParams_Basic()
        {
            var code = @"
Class C
    Public Function UnusedParamMethod(param1 As Integer, param2 As Integer) As Integer
        Return param1
    End Function

    Public Sub Caller()
        UnusedParamMethod(param2:=0, param1:=1)
    End Sub
End Class
";
            var fix = @"
Class C
    Public Function UnusedParamMethod(param1 As Integer) As Integer
        Return param1
    End Function

    Public Sub Caller()
        UnusedParamMethod(param1:=1)
    End Sub
End Class
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void MultipleNamespaces_Basic()
        {
            var code = @"
Namespace A.B.C.D
    Public Class Test
        Public Sub New(param1 As Integer)
        End Sub

        Public Shared Sub UnusedParamMethod(param1 As Integer)
        End Sub
    End Class
End Namespace

Namespace E
    Class CallerClass
        Public Sub Caller()
            Dim test = New A.B.C.D.Test(0)
            A.B.C.D.Test.UnusedParamMethod(0)
        End Sub
    End Class
End Namespace
";
            var fix = @"
Namespace A.B.C.D
    Public Class Test
        Public Sub New()
        End Sub

        Public Shared Sub UnusedParamMethod()
        End Sub
    End Class
End Namespace

Namespace E
    Class CallerClass
        Public Sub Caller()
            Dim test = New A.B.C.D.Test()
            A.B.C.D.Test.UnusedParamMethod()
        End Sub
    End Class
End Namespace
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void CalculationsInParameter_Basic()
        {
            var code = @"
Class C
    Sub M(x As Integer)
    End Sub

    Function N(x As Integer) As Integer
        Return x
    End Function

    Sub Caller()
        M(N(0))
    End Sub
End Class
";

            var fix = @"
Class C
    Sub M()
    End Sub

    Function N(x As Integer) As Integer
        Return x
    End Function

    Sub Caller()
        M()
    End Sub
End Class
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void IndexerNoFix_Basic()
        {
            var code = @"
Class C
    Public Property Item(i As Integer) As Integer
        Get
            Return 0
        End Get

        Set
        End Set
    End Property
End Class
";

            VerifyBasicFix(code, code);
        }

        [Fact]
        public void PropertyNoFix_Basic()
        {
            var code = @"
Class C
    Public Property Property1 As Integer
        Get
            Return 0
        End Get

        Set
        End Set
    End Property
End Class
";

            VerifyBasicFix(code, code);
        }

        [Fact]
        public void Conversion_Basic()
        {
            var code = @"
Class C
    Public Shared Narrowing Operator CType(value As C) As Integer
        Return 0
    End Operator

    Public Sub M1(d As Double)
    End Sub

    Public Sub M2(i As Integer)
    End Sub

    Public Sub M3(x As Integer)
    End Sub

    Public Sub Caller()
        Dim i As Integer = 0
        M1(i)
        Dim d As Double = 0
        M2(CInt(d))
        Dim instance = New C()
        M3(CType(instance, Integer))
    End Sub
End Class
";
            var fix = @"
Class C
    Public Shared Narrowing Operator CType(value As C) As Integer
        Return 0
    End Operator

    Public Sub M1()
    End Sub

    Public Sub M2()
    End Sub

    Public Sub M3()
    End Sub

    Public Sub Caller()
        Dim i As Integer = 0
        M1()
        Dim d As Double = 0
        M2()
        Dim instance = New C()
        M3()
    End Sub
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void ExtensionMethod_Basic()
        {
            var code = @"
Imports System.Runtime.CompilerServices

Module D
    <Extension()> 
    Public Sub ExtensionMethod(s As String)
    End Sub

    <Extension()> 
    Public Sub ExtensionMethod(s As String, i As Integer)
    End Sub

    Sub Caller()
        Dim s as String
        s.ExtensionMethod()
        s.ExtensionMethod(0)
    End Sub
End Module
";
            var fix = @"
Imports System.Runtime.CompilerServices

Module D
    <Extension()> 
    Public Sub ExtensionMethod(s As String)
    End Sub

    <Extension()> 
    Public Sub ExtensionMethod(s As String)
    End Sub

    Sub Caller()
        Dim s as String
        s.ExtensionMethod()
        s.ExtensionMethod()
    End Sub
End Module
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }
    }
}