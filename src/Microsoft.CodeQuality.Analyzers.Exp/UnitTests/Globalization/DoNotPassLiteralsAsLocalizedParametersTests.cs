// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.Exp.Globalization;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Exp.UnitTests.Globalization
{
    using Verify = CSharpCodeFixVerifier<DoNotPassLiteralsAsLocalizedParameters, EmptyCodeFixProvider>;
    using VerifyVB = VisualBasicCodeFixVerifier<DoNotPassLiteralsAsLocalizedParameters, EmptyCodeFixProvider>;

    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.CopyAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.NullAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PointsToAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
    public partial class DoNotPassLiteralsAsLocalizedParametersTests
    {
        private DiagnosticResult GetCSharpResultAt(int line, int column, string containingMethod, string parameterName, string invokedMethod, string literals) =>
            Verify.Diagnostic(DoNotPassLiteralsAsLocalizedParameters.Rule).WithLocation(line, column).WithArguments(containingMethod, parameterName, invokedMethod, literals);

        private DiagnosticResult GetBasicResultAt(int line, int column, string containingMethod, string parameterName, string invokedMethod, string literals) =>
            VerifyVB.Diagnostic(DoNotPassLiteralsAsLocalizedParameters.Rule).WithLocation(line, column).WithArguments(containingMethod, parameterName, invokedMethod, literals);

        [Fact]
        public async Task NonLocalizableParameter_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
public class C
{
    public void M(string param)
    {
    }
}

public class Test
{
    public void M1(C c, string param)
    {
        // Literal argument
        var str = """";
        c.M(str);

        // Non-literal argument
        c.M(param);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public Sub M(param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C, param As String)
        ' Literal argument
        Dim str = """"
        c.M(str)

        ' Non-literal argument
        c.M(param)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_NonLiteralArgument_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c, string param)
    {
        c.M(param);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C, param As String)
        c.M(param)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_EmptyStringLiteralArgument_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = """";
        c.M(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = """"
        c.M(str)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_NonEmptyStringLiteralArgument_AllControlChars_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = new string(new char[] { '\u0058' });
        c.M(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel
Imports Microsoft.VisualBasic

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ChrW(&H0030)
        c.M(str)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_StringLiteralArgument_Method_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.M(str);
    }
}
",
            // Test0.cs(16,13): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(16, 13, "void Test.M1(C c)", "param", "void C.M(string param)", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(12,13): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(12, 13, "Sub Test.M1(c As C)", "param", "Sub C.M(param As String)", "a"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_StringLiteralArgument_Constructor_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public C([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c = new C(str);
    }
}
",
            // Test0.cs(16,19): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'C.C(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(16, 19, "void Test.M1(C c)", "param", "C.C(string param)", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub New(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c = New C(str)
    End Sub
End Class
",
            // Test0.vb(12,19): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.New(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(12, 19, "Sub Test.M1(c As C)", "param", "Sub C.New(param As String)", "a"));
        }

        [Fact]
        public async Task PropertyWithLocalizableAttribute_StringLiteralArgument_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    [LocalizableAttribute(true)]
    public string P { get; set; }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.P = str;
    }
}
",
            // Test0.cs(15,9): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'value' of a call to 'void C.P.set'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(15, 9, "void Test.M1(C c)", "value", "void C.P.set", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    <LocalizableAttribute(True)> _
    Public Property P As String
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.P = str
    End Sub
End Class
",
            // Test0.vb(12,9): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'AutoPropertyValue' of a call to 'Property Set C.P(AutoPropertyValue As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(12, 9, "Sub Test.M1(c As C)", "AutoPropertyValue", "Property Set C.P(AutoPropertyValue As String)", "a"));
        }

        [Fact]
        public async Task PropertySetterWithLocalizableAttribute_StringLiteralArgument_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public string P { get; [LocalizableAttribute(true)]set; }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.P = str;
    }
}
",
            // Test0.cs(14,9): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'value' of a call to 'void C.P.set'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(14, 9, "void Test.M1(C c)", "value", "void C.P.set", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Private _p As String
    Public Property P As String
        Get
            Return _p
        End Get
        <LocalizableAttribute(True)>
        Set(valueCustom As String)
            _p = valueCustom
        End Set
    End Property
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.P = str
    End Sub
End Class
",

            // Test0.vb(20,9): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'valueCustom' of a call to 'Property Set C.P(valueCustom As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(20, 9, "Sub Test.M1(c As C)", "valueCustom", "Property Set C.P(valueCustom As String)", "a"));
        }

        [Fact]
        public async Task PropertySetterParameterWithLocalizableAttribute_StringLiteralArgument_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    private string _p;
    public string P { get => _p; [param: LocalizableAttribute(true)]set => _p = value; }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.P = str;
    }
}
",
            // Test0.cs(15,9): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'value' of a call to 'void C.P.set'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(15, 9, "void Test.M1(C c)", "value", "void C.P.set", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Private _p As String
    Public Property P As String
        Get
            Return _p
        End Get

        Set(<LocalizableAttribute(True)> valueCustom As String)
            _p = valueCustom
        End Set
    End Property
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.P = str
    End Sub
End Class
",

            // Test0.vb(20,9): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'valueCustom' of a call to 'Property Set C.P(valueCustom As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(20, 9, "Sub Test.M1(c As C)", "valueCustom", "Property Set C.P(valueCustom As String)", "a"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_MultipleStringLiteralArguments_Method_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param, string message)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        var message = ""m"";
        c.M(str, message);
    }
}
",
            new[]
            {
                // Test0.cs(17,13): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.M(string param, string message)'. Retrieve the following string(s) from a resource table instead: "a".
                GetCSharpResultAt(17, 13, "void Test.M1(C c)", "param", "void C.M(string param, string message)", "a"),
                // Test0.cs(17,18): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'message' of a call to 'void C.M(string param, string message)'. Retrieve the following string(s) from a resource table instead: "m".
                GetCSharpResultAt(17, 18, "void Test.M1(C c)", "message", "void C.M(string param, string message)", "m"),
            });

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String, message As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        Dim message = ""m""
        c.M(str, message)
    End Sub
End Class
",
            new[]
            {
                // Test0.vb(13,13): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String, message As String)'. Retrieve the following string(s) from a resource table instead: "a".
                GetBasicResultAt(13, 13, "Sub Test.M1(c As C)", "param", "Sub C.M(param As String, message As String)", "a"),
                // Test0.vb(13,18): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'message' of a call to 'Sub C.M(param As String, message As String)'. Retrieve the following string(s) from a resource table instead: "m".
                GetBasicResultAt(13, 18, "Sub Test.M1(c As C)", "message", "Sub C.M(param As String, message As String)", "m"),
            });
        }

        [Fact]
        public async Task ParameterWithLocalizableFalseAttribute_StringLiteralArgument_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(false)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.M(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(False)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ContainingSymbolWithLocalizableTrueAttribute_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

[LocalizableAttribute(true)]
public class C
{
    public void M(string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.M(str);
    }
}
",
            // Test0.cs(17,13): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(17, 13, "void Test.M1(C c)", "param", "void C.M(string param)", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

<LocalizableAttribute(True)> _
Public Class C
    Public Sub M(param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(13,13): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(13, 13, "Sub Test.M1(c As C)", "param", "Sub C.M(param As String)", "a"));
        }

        [Fact]
        public async Task ParameterWithLocalizableFalseAttribute_ContainingSymbolWithLocalizableTrueAttribute_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

[LocalizableAttribute(true)]
public class C
{
    public void M([LocalizableAttribute(false)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.M(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

<LocalizableAttribute(True)>
Public Class C
    Public Sub M(<LocalizableAttribute(False)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableTrueAttribute_ContainingSymbolWithLocalizableFalseAttribute_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

[LocalizableAttribute(false)]
public class C
{
    public void M([LocalizableAttribute(true)]string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        var str = ""a"";
        c.M(str);
    }
}
",
            // Test0.cs(17,13): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(17, 13, "void Test.M1(C c)", "param", "void C.M(string param)", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

<LocalizableAttribute(False)> _
Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(13,13): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(13, 13, "Sub Test.M1(c As C)", "param", "Sub C.M(param As String)", "a"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_OverriddenMethod_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class B
{
    public virtual void NonLocalizableMethod([LocalizableAttribute(false)] string param)
    {
    }

    public virtual void LocalizableMethod([LocalizableAttribute(true)] string param)
    {
    }
}

public class C : B
{
    public override void NonLocalizableMethod(string param)
    {
    }

    public override void LocalizableMethod(string param)
    {
    }
}

public class Test
{
    void M1(C c)
    {
        var str = ""a"";
        c.NonLocalizableMethod(str);
        c.LocalizableMethod(str);
    }
}
",
            // Test0.cs(32,29): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.LocalizableMethod(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(32, 29, "void Test.M1(C c)", "param", "void C.LocalizableMethod(string param)", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class B
    Public Overridable Sub NonLocalizableMethod(<LocalizableAttribute(False)> param As String)
    End Sub

    Public Overridable Sub LocalizableMethod(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class C
    Inherits B
    Public Overrides Sub NonLocalizableMethod(param As String)
    End Sub

    Public Overrides Sub LocalizableMethod(param As String)
    End Sub
End Class

Public Class Test
    Private Sub M1(ByVal c As C)
        Dim str = ""a""
        c.NonLocalizableMethod(str)
        c.LocalizableMethod(str)
    End Sub
End Class
",
            // Test0.vb(25,29): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.LocalizableMethod(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(25, 29, "Sub Test.M1(c As C)", "param", "Sub C.LocalizableMethod(param As String)", "a"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_StringLiteralArgument_MultiplePossibleLiterals_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c, bool flag)
    {
        var str = flag ? ""a"" : ""b"";
        c.M(str);
    }
}
",
            // Test0.cs(16,13): warning CA1303: Method 'void Test.M1(C c, bool flag)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a, b".
            GetCSharpResultAt(16, 13, "void Test.M1(C c, bool flag)", "param", "void C.M(string param)", "a, b"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C, flag As Boolean)
        Dim str = If(flag, ""a"", ""b"")
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(12,13): warning CA1303: Method 'Sub Test.M1(c As C, flag As Boolean)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a, b".
            GetBasicResultAt(12, 13, "Sub Test.M1(c As C, flag As Boolean)", "param", "Sub C.M(param As String)", "a, b"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_StringLiteralArgument_MultiplePossibleLiterals_Ordering_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c, bool flag)
    {
        var str = flag ? ""b"" : ""a"";
        c.M(str);
    }
}
",
            // Test0.cs(16,13): warning CA1303: Method 'void Test.M1(C c, bool flag)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a, b".
            GetCSharpResultAt(16, 13, "void Test.M1(C c, bool flag)", "param", "void C.M(string param)", "a, b"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C, flag As Boolean)
        Dim str = If(flag, ""b"", ""a"")
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(12,13): warning CA1303: Method 'Sub Test.M1(c As C, flag As Boolean)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a, b".
            GetBasicResultAt(12, 13, "Sub Test.M1(c As C, flag As Boolean)", "param", "Sub C.M(param As String)", "a, b"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_StringLiteralArgument_DefaultValue_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c, string param = ""a"")
    {
        c.M(param);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C, Optional param As String = ""a"")
        c.M(param)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_ConstantField_StringLiteralArgument_Method_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    private const string _field = ""a"";
    public void M1(C c)
    {
        c.M(_field);
    }
}
",
            // Test0.cs(16,13): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(16, 13, "void Test.M1(C c)", "param", "void C.M(string param)", "a"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Private Const _field As String = ""a"" 

    Public Sub M1(c As C)
        c.M(_field)
    End Sub
End Class
",
            // Test0.vb(13,13): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(13, 13, "Sub Test.M1(c As C)", "param", "Sub C.M(param As String)", "a"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_XmlStringLiteralArgument_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        string str = ""<a>"";
        c.M(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""<a>""
        c.M(str)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_XmlStringLiteralArgument_Filtering_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(C c, bool flag)
    {
        string str = flag ? ""<a>"" : ""b"";
        c.M(str);
    }
}
",
            // Test0.cs(16,13): warning CA1303: Method 'void Test.M1(C c, bool flag)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "b".
            GetCSharpResultAt(16, 13, "void Test.M1(C c, bool flag)", "param", "void C.M(string param)", "b"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C, flag As Boolean)
        Dim str = If (flag, ""<a>"", ""b"")
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(12,13): warning CA1303: Method 'Sub Test.M1(c As C, flag As Boolean)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "b".
            GetBasicResultAt(12, 13, "Sub Test.M1(c As C, flag As Boolean)", "param", "Sub C.M(param As String)", "b"));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_SpecialCases_ConditionalMethod_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.Diagnostics;

public class C
{
    [Conditional(""DEBUG"")]
    public void M(string message)
    {
    }
}

public class Test
{
    public void M1(C c)
    {
        string str = ""a"";
        c.M(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Diagnostics

Public Class C
    <Conditional(""DEBUG"")>
    Public Sub M(message As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
");
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_SpecialCases_XmlWriterMethod_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;
using System.Xml;

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(XmlWriter writer)
    {
        string str = ""a"";
        writer.WriteString(str);
    }
}
", DiagnosticResult.CompilerError("CS0246").WithLocation(14, 20).WithMessage("The type or namespace name 'XmlWriter' could not be found (are you missing a using directive or an assembly reference?)"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel
Imports System.Xml

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(writer As XmlWriter)
        Dim str = ""a""
        writer.WriteString(str)
    End Sub
End Class
", DiagnosticResult.CompilerError("BC30002").WithLocation(11, 29).WithMessage("Type 'XmlWriter' is not defined."));
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_SpecialCases_SystemConsoleWriteMethods_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

public class Test
{
    public void M1(string param)
    {
        string str = ""a"";
        Console.Write(value: str);
        Console.WriteLine(value: str);
        Console.Write(format: str, arg0: param);
        Console.WriteLine(format: str, arg0: param);
    }
}
",
            new[]
            {
                // Test0.cs(9,23): warning CA1303: Method 'void Test.M1(string param)' passes a literal string as parameter 'value' of a call to 'void Console.Write(string value)'. Retrieve the following string(s) from a resource table instead: "a".
                GetCSharpResultAt(9, 23, "void Test.M1(string param)", "value", "void Console.Write(string value)", "a"),
                // Test0.cs(10,27): warning CA1303: Method 'void Test.M1(string param)' passes a literal string as parameter 'value' of a call to 'void Console.WriteLine(string value)'. Retrieve the following string(s) from a resource table instead: "a".
                GetCSharpResultAt(10, 27, "void Test.M1(string param)", "value", "void Console.WriteLine(string value)", "a"),
                // Test0.cs(11,23): warning CA1303: Method 'void Test.M1(string param)' passes a literal string as parameter 'format' of a call to 'void Console.Write(string format, object arg0)'. Retrieve the following string(s) from a resource table instead: "a".
                GetCSharpResultAt(11, 23, "void Test.M1(string param)", "format", "void Console.Write(string format, object arg0)", "a"),
                // Test0.cs(12,27): warning CA1303: Method 'void Test.M1(string param)' passes a literal string as parameter 'format' of a call to 'void Console.WriteLine(string format, object arg0)'. Retrieve the following string(s) from a resource table instead: "a".
                GetCSharpResultAt(12, 27, "void Test.M1(string param)", "format", "void Console.WriteLine(string format, object arg0)", "a"),
            });

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class Test
    Public Sub M1(param As String)
        Dim str = ""a""
        Console.Write(value:=str)
        Console.WriteLine(value:=str)
        Console.Write(format:=str, arg0:=param)
        Console.WriteLine(format:=str, arg0:=param)
    End Sub
End Class
",
            new[]
            {
                // Test0.vb(7,23): warning CA1303: Method 'Sub Test.M1(param As String)' passes a literal string as parameter 'value' of a call to 'Sub Console.Write(value As String)'. Retrieve the following string(s) from a resource table instead: "a".
                GetBasicResultAt(7, 23, "Sub Test.M1(param As String)", "value", "Sub Console.Write(value As String)", "a"),
                // Test0.vb(8,27): warning CA1303: Method 'Sub Test.M1(param As String)' passes a literal string as parameter 'value' of a call to 'Sub Console.WriteLine(value As String)'. Retrieve the following string(s) from a resource table instead: "a".
                GetBasicResultAt(8, 27, "Sub Test.M1(param As String)", "value", "Sub Console.WriteLine(value As String)", "a"),
                // Test0.vb(9,23): warning CA1303: Method 'Sub Test.M1(param As String)' passes a literal string as parameter 'format' of a call to 'Sub Console.Write(format As String, arg0 As Object)'. Retrieve the following string(s) from a resource table instead: "a".
                GetBasicResultAt(9, 23, "Sub Test.M1(param As String)", "format", "Sub Console.Write(format As String, arg0 As Object)", "a"),
                // Test0.vb(10,27): warning CA1303: Method 'Sub Test.M1(param As String)' passes a literal string as parameter 'format' of a call to 'Sub Console.WriteLine(format As String, arg0 As Object)'. Retrieve the following string(s) from a resource table instead: "a".
                GetBasicResultAt(10, 27, "Sub Test.M1(param As String)", "format", "Sub Console.WriteLine(format As String, arg0 As Object)", "a"),
            });
        }

        [Fact]
        public async Task ParameterWithLocalizableAttribute_SpecialCases_SystemWebUILiteralControl_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI
{
    public class LiteralControl
    {
        public LiteralControl(string text)
        {
        }
    }
}

public class C
{
    public void M([LocalizableAttribute(true)] string param)
    {
    }
}

public class Test
{
    public void M1(LiteralControl control)
    {
        string str = ""a"";
        control = new LiteralControl(str);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.ComponentModel
Imports System.Web.UI

Namespace System.Web.UI
    Public Class LiteralControl
        Public Sub New(text As String)
        End Sub
    End Class
End Namespace

Public Class C
    Public Sub M(<LocalizableAttribute(True)> param As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(control As LiteralControl)
        Dim str = ""a""
        control = New LiteralControl(str)
    End Sub
End Class
");
        }

        [InlineData("Assert")]
        [InlineData("CollectionAssert")]
        [InlineData("StringAssert")]
        [Theory]
        public async Task ParameterWithLocalizableAttribute_SpecialCases_UnitTestApis_NoDiagnostic(string assertClassName)
        {
            await Verify.VerifyAnalyzerAsync($@"
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{{
    public class {assertClassName}
    {{
        public static bool IsTrue(bool condition, string message) => true;
    }}
}}

public class Test
{{
    public void M1()
    {{
        string str = ""a"";
        var result = {assertClassName}.IsTrue(false, str);
    }}
}}
");

            await VerifyVB.VerifyAnalyzerAsync($@"
Imports System.ComponentModel
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Microsoft.VisualStudio.TestTools.UnitTesting
    Public Class {assertClassName}
        Public Shared Function IsTrue(condition As Boolean, message As String) As Boolean
            Return True
        End Function
    End Class
End Namespace

Public Class Test
    Public Sub M1()
        Dim str = ""a""
        Dim result = {assertClassName}.IsTrue(False, str)
    End Sub
End Class
");
        }

        [InlineData("message")]
        [InlineData("text")]
        [InlineData("caption")]
        [InlineData("Message")]
        [InlineData("Text")]
        [InlineData("Caption")]
        [Theory]
        public async Task ParameterWithLocalizableName_StringLiteralArgument_Method_Diagnostic(string parameterName)
        {
            await Verify.VerifyAnalyzerAsync($@"
public class C
{{
    public void M(string {parameterName})
    {{
    }}
}}

public class Test
{{
    public void M1(C c)
    {{
        var str = ""a"";
        c.M(str);
    }}
}}
",
            // Test0.cs(14,13): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'param' of a call to 'void C.M(string param)'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(14, 13, "void Test.M1(C c)", parameterName, $"void C.M(string {parameterName})", "a"));

            await VerifyVB.VerifyAnalyzerAsync($@"
Public Class C
    Public Sub M({parameterName} As String)
    End Sub
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.M(str)
    End Sub
End Class
",
            // Test0.vb(10,13): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'param' of a call to 'Sub C.M(param As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(10, 13, "Sub Test.M1(c As C)", parameterName, $"Sub C.M({parameterName} As String)", "a"));
        }

        [InlineData("message")]
        [InlineData("text")]
        [InlineData("caption")]
        [InlineData("Message")]
        [InlineData("Text")]
        [InlineData("Caption")]
        [Theory]
        public async Task PropertyWithLocalizableName_StringLiteralArgument_Diagnostic(string propertyName)
        {
            await Verify.VerifyAnalyzerAsync($@"
public class C
{{
    public string {propertyName} {{ get; set; }}
}}

public class Test
{{
    public void M1(C c)
    {{
        var str = ""a"";
        c.{propertyName} = str;
    }}
}}
",

            // Test0.cs(12,9): warning CA1303: Method 'void Test.M1(C c)' passes a literal string as parameter 'value' of a call to 'void C.caption.set'. Retrieve the following string(s) from a resource table instead: "a".
            GetCSharpResultAt(12, 9, "void Test.M1(C c)", "value", $"void C.{propertyName}.set", "a"));

            await VerifyVB.VerifyAnalyzerAsync($@"
Public Class C
    Public Property {propertyName} As String
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim str = ""a""
        c.{propertyName} = str
    End Sub
End Class
",

            // Test0.vb(9,9): warning CA1303: Method 'Sub Test.M1(c As C)' passes a literal string as parameter 'AutoPropertyValue' of a call to 'Property Set C.caption(AutoPropertyValue As String)'. Retrieve the following string(s) from a resource table instead: "a".
            GetBasicResultAt(9, 9, "Sub Test.M1(c As C)", "AutoPropertyValue", $"Property Set C.{propertyName}(AutoPropertyValue As String)", "a"));
        }
    }
}
