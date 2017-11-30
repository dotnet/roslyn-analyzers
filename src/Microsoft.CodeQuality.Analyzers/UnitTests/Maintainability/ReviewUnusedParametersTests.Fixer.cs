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
        UnusedParamMethod(0);
        UnusedParamStaticMethod(0);
        UnusedDefaultParamMethod(0);
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
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
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
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void BaseScenario_Basic()
        {
            var code = @"
Class C
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
End Class
";
            var fix = @"
Class C
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
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }
    }
}