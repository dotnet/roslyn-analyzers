// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Exp.UnitTests.Maintainability
{
    public partial class ReviewSQLQueriesForSecurityVulnerabilitiesTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void FlowAnalysis_LocalWithConstantInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = """";
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1()
        Dim str As String = """"
        Dim c As New Command1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_LocalWithConstantAssignment_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str;
        str = """";
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1()
        Dim str As String
        str = """"
        Dim c As New Command1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_ParameterWithConstantAssignment_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string str)
    {{
        str = """";
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(str As String)
        str = """"
        Dim c As New Command1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_LocalWithAllConstantAssignments_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string nonconst, bool flag)
    {{
        string str = """", str2 = """", str3 = ""nonempty"";
        if (flag) {{ str = str2; }}
        else  {{ str = str3; }}
        Command c = new Command1(str, str);
        str = nonconst; // assignment with non-constant value after call should not affect
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(nonconst as String, flag as Boolean)
        Dim str = """", str2 = """", str3 = ""nonempty""
        If flag Then
            str = str2
        Else
            str = str3
        End If
        Dim c As New Command1(str, str)
        str = nonconst ' assignment with non-constant value after call should not affect
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_ParameterWithAllConstantAssignments_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string nonconst, bool flag, string str)
    {{
        string str2 = """", str3 = ""nonempty"";
        str = """";
        if (flag) {{ str = str2; }}
        else  {{ str = str3; }}
        Command c = new Command1(str, str);
        str = nonconst; // assignment with non-constant value after call should not affect
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(nonconst as String, flag as Boolean, str as String)
        Dim str2 = """", str3 = ""nonempty""
        str = """"
        If flag Then
            str = str2
        Else
            str = str3
        End If
        Dim c As New Command1(str, str)
        str = nonconst ' assignment with non-constant value after call should not affect
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_ConstantFieldInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    const string _field = """";
    void M1()
    {{
        string str = _field;
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Const _field As String = """"
    Sub M1()
        Dim str As String = _field
        Dim c As New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_ConversionsInInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        object obj = """";          // Implicit conversion from string to object
        string str = (string)obj;   // Explicit conversion from object to string
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Sub M1()
        Dim obj As Object = """"                        ' Implicit conversion from string to object
        Dim str As String = DirectCast(obj, String)     ' Explicit conversion from object to string
        Dim c As New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_ImplicitUserDefinedConversionsInInitializer_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    Test(string s)
    {{
    }}

    void M1()
    {{
        Test t = """";     // Implicit user defined conversion
        string str = t;    // Implicit user defined conversion
        Command c = new Command1(str, str);
    }}

    public static implicit operator Test(string value)
    {{
        return null;
    }}

    public static implicit operator string(Test value)
    {{
        return null;
    }}
}}
",
            GetCSharpResultAt(103, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test

    Private Sub New(ByVal s As String)
        MyBase.New
    End Sub

    Private Sub M1()
        Dim t As Test = """"    ' Implicit user defined conversion
        Dim str As String = t   ' Implicit user defined conversion
        Dim c As New Command1(str, str)
    End Sub

    Public Shared Widening Operator CType(ByVal value As String) As Test
        Return Nothing
    End Operator

    Public Shared Widening Operator CType(ByVal value As Test) As String
        Return Nothing
    End Operator
End Class",
            GetBasicResultAt(140, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_ExplicitUserDefinedConversionsInInitializer_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    Test(string s)
    {{
    }}

    void M1()
    {{
        Test t = (Test)"""";       // Explicit user defined conversion
        string str = (string)t;    // Explicit user defined conversion
        Command c = new Command1(str, str);
    }}

    public static explicit operator Test(string value)
    {{
        return null;
    }}

    public static explicit operator string(Test value)
    {{
        return null;
    }}
}}
",
            GetCSharpResultAt(103, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
Option Strict On

{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test

    Private Sub New(ByVal s As String)
        MyBase.New
    End Sub

    Private Sub M1()
        Dim t As Test = CType("""", Test)       ' Explicit user defined conversion
        Dim str As String = CType(t, String)    ' Explicit user defined conversion
        Dim c As New Command1(str, str)
    End Sub

    Public Shared Narrowing Operator CType(ByVal value As String) As Test
        Return Nothing
    End Operator

    Public Shared Narrowing Operator CType(ByVal value As Test) As String
        Return Nothing
    End Operator
End Class",
            GetBasicResultAt(142, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_LocalInitializerWithInvocation_Diagnostic()
        {
            // Currently, we do not do any interprocedural or context sensitive flow analysis.
            // So method calls are assumed to always return a MayBe result.
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string command)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = SomeString();
        Adapter c = new Adapter1(str, str);
    }}

    string SomeString() => """";
}}
",
            GetCSharpResultAt(98, 21, "Adapter1.Adapter1(string cmd, string command)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, command As String)
    End Sub
End Class

Module Test
    Sub M1()
        Dim str As String = SomeString()
        Dim c As New Adapter1(str, str)
    End Sub

    Function SomeString()
        Return """"
    End Function
End Module",
            GetBasicResultAt(134, 18, "Sub Adapter1.New(cmd As String, command As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_LocalWithByRefEscape_Diagnostic()
        {
            // Local/parameter passed by ref/out are assumed to be non-constant after the usage.
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str = """";
        M2(ref str);
        Adapter c = new Adapter1(str, str);

        param = """";
        M2(ref param);
        c = new Adapter1(param, param);
    }}

    void M2(ref string str)
    {{
        str = """";
    }}
}}
",
            GetCSharpResultAt(99, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            GetCSharpResultAt(103, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str As String = """"
        M2(str)
        Dim c As New Adapter1(str, str)

        param = """"
        M2(param)
        c = New Adapter1(param, param)
    End Sub

    Sub M2(ByRef str as String)
        str = """"
    End Sub
End Module",
            GetBasicResultAt(135, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(139, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_StringEmptyInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = string.Empty;
        Adapter c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1()
        Dim str As String = String.Empty
        Dim c As New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_NameOfExpression_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str = nameof(param);
        Adapter c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str As String = NameOf(param)
        Dim c As New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_NullOrDefaultValue_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = default(string);
        Adapter c = new Adapter1(str, str);

        str = null;
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1()
        Dim str As String = Nothing
        Dim c As New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_InterpolatedString_Constant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        var local = """";
        string str = $""text_{{""literal""}}_{{local}}"";
        Adapter c = new Adapter1(str, str);

        str = $"""";
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim local = """"
        Dim str As String = $""text_{{""literal""}}_{{local}}""
        Dim c As New Adapter1(str, str)

        str = $""""
        c = New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_InterpolatedString_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        var local = """";
        string str = $""text_{{""literal""}}_{{local}}_{{param}}"";     // param might be non-constant.
        Adapter c = new Adapter1(str, str);
    }}
}}
",
            GetCSharpResultAt(99, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim local = """"
        Dim str As String = $""text_{{""literal""}}_{{local}}_{{param}}""     ' param might be non-constant.
        Dim c As New Adapter1(str, str)
    End Sub
End Module",
            GetBasicResultAt(135, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_BinaryAdd_Constant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str1 = """", str2 = """";
        string str = str1 + str2 + (str1 + str2);
        Adapter c = new Adapter1(str, str);

        str += str1;
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str1 = """", str2 = """"
        Dim str As String = str1 + str2 + (str1 + str2)
        Dim c As New Adapter1(str, str)

        str += str1
        c = New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_BinaryAdd_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str1 = """";
        string str = str1 + param;
        Adapter c = new Adapter1(str, str);

        str = """";
        str += param;
        c = new Adapter1(str, str);
    }}
}}
",
            GetCSharpResultAt(99, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            GetCSharpResultAt(103, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str1 = """"
        Dim str As String = str1 + param
        Dim c As New Adapter1(str, str)

        str1 = """"
        str += param
        c = New Adapter1(str, str)
    End Sub
End Module",
            GetBasicResultAt(135, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(139, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_NullCoalesce_Constant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str1 = """";
        string str = str1 ?? param;
        Adapter c = new Adapter1(str, str);

        str1 = null;
        str = str1 ?? """";
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str1 = """"
        Dim str As String = If(str1, param)
        Dim c As New Adapter1(str, str)

        str1 = Nothing
        str = If(str1, """")
        c = New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_NullCoalesce_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string str1, string param)
    {{
        string str = str1 ?? """";
        Adapter c = new Adapter1(str, str);

        str1 = null;
        str = str1 ?? param;
        c = new Adapter1(str, str);

        str1 = param;
        str = str1 ?? """";
        c = new Adapter1(str, str);
    }}
}}
",
            GetCSharpResultAt(98, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            GetCSharpResultAt(102, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            GetCSharpResultAt(106, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(str1 as String, param As String)
        Dim str As String = If(str1, """")
        Dim c As New Adapter1(str, str)

        str1 = Nothing
        str = If(str1, param)
        c = New Adapter1(str, str)

        str1 = param
        str = If(str1, """")
        c = New Adapter1(str, str)
    End Sub
End Module",
            GetBasicResultAt(134, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(138, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(142, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1569")]
        public void FlowAnalysis_ConditionalAccess_Constant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public readonly string X = """";
}}

class Test
{{
    void M1(A a)
    {{
        string str = a?.X;
        Adapter c = new Adapter1(str, str);

        a = new A();
        str = a?.X;
        c = new Adapter1(str, str);

        a = null;
        str = a?.X;
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public ReadOnly X As String = """"
End Class

Module Test
    Sub M1(a As A)
        Dim str As String = a?.X
        Dim c As New Adapter1(str, str)

        a = new A()
        str = a?.X
        c = New Adapter1(str, str)

        a = Nothing
        str = a?.X
        c = New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_ConditionalAccess_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string X;
}}

class Test
{{
    void M1(A a, string param)
    {{
        string str = a?.X;
        Adapter c = new Adapter1(str, str);

        a = new A();
        str = a?.X;
        c = new Adapter1(str, str);

        a.X = """";
        str = a?.X;
        c = new Adapter1(str, str);

        a.X = param;
        str = a?.X;
        c = new Adapter1(str, str);

        a = null;
        str = a?.X;     // result is always null, so no diagnostic
        c = new Adapter1(str, str);
    }}
}}
",
        GetCSharpResultAt(103, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
        GetCSharpResultAt(107, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
        GetCSharpResultAt(115, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public X As String
End Class

Module Test
    Sub M1(a As A, param As String)
        Dim str As String = a?.X
        Dim c As New Adapter1(str, str)

        a = new A()
        str = a?.X
        c = New Adapter1(str, str)

        a.X = """"
        str = a?.X
        c = New Adapter1(str, str)

        a.X = param
        str = a?.X
        c = New Adapter1(str, str)

        a = Nothing
        str = a?.X                  ' result is always null, so no diagnostic
        c = New Adapter1(str, str)
    End Sub
End Module",
            GetBasicResultAt(138, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(142, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(150, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void FlowAnalysis_WhileLoop_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str = """";
        while (true)
        {{
            Adapter c = new Adapter1(str, str);
            str = param;
        }}
    }}
}}
",
            GetCSharpResultAt(100, 25, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str = """"
        While True
            Dim c As New Adapter1(str, str)
            str = param
        End While
    End Sub
End Module",
            GetBasicResultAt(135, 22, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void FlowAnalysis_ForLoop_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str = """";
        for (int i = 0; i < 10; i++)
        {{
            Adapter c = new Adapter1(str, str);
            str = param;
        }}
    }}
}}
",
            GetCSharpResultAt(100, 25, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str = """"
        For i As Integer = 0 To 10
            Dim c As New Adapter1(str, str)
            str = param
        Next
    End Sub
End Module",
            GetBasicResultAt(135, 22, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void FlowAnalysis_ForEachLoop_NonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str = """";
        foreach (var i in new[] {{ 1, 2, 3 }})
        {{
            Adapter c = new Adapter1(str, str);
            str = param;
        }}
    }}
}}
",
            GetCSharpResultAt(100, 25, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str = """"
        For Each i In New Integer() {{1, 2, 3}}
            Dim c As New Adapter1(str, str)
            str = param
        Next
    End Sub
End Module",
            GetBasicResultAt(135, 22, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void FlowAnalysis_Conditional_Constant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        if (param == """")
        {{
            Adapter c = new Adapter1(param, param);
        }}

        Adapter c2 = param == """" ? new Adapter1(param, param) : null;
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        If param = """" Then
            Dim c As New Adapter1(param, param)
        End If

        Dim c2 As Adapter = If(param = """", New Adapter1(param, param), Nothing)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_LocalFunctionInvocation_EmptyBody_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str;
        str = """";

        void MyLocalFunction()
        {{
        }};

        MyLocalFunction();    // This should not change state of 'str' if we analyzed the local function.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(105, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            // VB has no local functions.
        }

        [Fact]
        public void FlowAnalysis_LocalFunctionInvocation_ChangesCapturedValueToConstant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = param;

        void MyLocalFunction()
        {{
            str = """";
        }};

        MyLocalFunction();    // This should change state of 'str' to be a constant if we analyzed the local function.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            // VB has no local functions.
        }

        [Fact]
        public void FlowAnalysis_LocalFunctionInvocation_ChangesCapturedValueToNonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = """";

        void MyLocalFunction()
        {{
            str = param;
        }};

        MyLocalFunction();    // This should change state of 'str' to be a non-constant.
        Command c = new Command1(str, str);
    }}
}}
",
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            // VB has no local functions.
        }

        [Fact]
        public void FlowAnalysis_LocalFunctionInvocation_ChangesCapturedValueContextSensitive_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = """";

        void MyLocalFunction(string param2)
        {{
            str = param2;
        }};

        MyLocalFunction(str);    // This should change state of 'str' to be a constant if we analyzed the local function in a context sensitive fashion.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            // VB has no local functions.
        }

        [Fact]
        public void FlowAnalysis_LocalFunctionInvocation_ReturnValueContextSensitive_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = """";

        string MyLocalFunction(string param2)
        {{
            return param2;
        }};

        str = MyLocalFunction(str);    // This should change state of 'str' to be a constant if we analyzed the local function in a context sensitive fashion.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            // VB has no local functions.
        }

        [Fact]
        public void FlowAnalysis_LambdaInvocation_EmptyBody_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str;
        str = """";

        System.Action myLambda = () =>
        {{
        }};

        myLambda();    // This should not change state of 'str' if we analyzed the lambda.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(105, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1()
        Dim str As String
        str = """"

        Dim myLambda As System.Action = Sub()
                                        End Sub

        myLambda()      ' This should not change state of 'str' if we analyzed the lambda.
        Dim c As New Command1(str, str)
    End Sub
End Module",
            // Currently we generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            GetBasicResultAt(140, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_LambdaInvocation_ChangesCapturedValueToConstant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = param;

        System.Action myLambda = () =>
        {{
            str = """";
        }};

        myLambda();    // This should change the state of 'str' to be a constant if we analyzed the lambda.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str As String
        str = param

        Dim myLambda As System.Action = Sub()
                                            str = """"
                                        End Sub

        myLambda()      ' This should change the state of 'str' to be a constant if we analyzed the lambda.
        Dim c As New Command1(str, str)
    End Sub
End Module",
            // Currently we generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            GetBasicResultAt(141, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_LambdaInvocation_ChangesCapturedValueToNonConstant_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = """";

        System.Action myLambda = () =>
        {{
            str = param;
        }};

        myLambda();    // This should change state of 'str' to be a non-constant.
        Command c = new Command1(str, str);
    }}
}}
",
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str As String
        str = """"

        Dim myLambda As System.Action = Sub()
                                           str = param 
                                        End Sub

        myLambda()      ' This should change state of 'str' to be a non-constant.
        Dim c As New Command1(str, str)
    End Sub
End Module",
            GetBasicResultAt(141, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_LambdaInvocation_ChangesCapturedValueContextSensitive_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = """";

        System.Action<string> myLambda = (string param2) =>
        {{
            str = param2;
        }};

        myLambda(str);    // This should change state of 'str' to be a constant if we analyzed the local function in a context sensitive fashion.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str As String
        str = """"

        Dim myLambda As System.Action(Of String) =  Sub(param2 As String)
                                                        str = param2 
                                                    End Sub

        myLambda(str)      '  This should change state of 'str' to be a constant if we analyzed the local function in a context sensitive fashion.
        Dim c As New Command1(str, str)
    End Sub
End Module",
            // Currently we generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            GetBasicResultAt(141, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_LambdaInvocation_ReturnValueContextSensitive_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string str;
        str = """";

        System.Func<string, string> myLambda = (string param2) =>
        {{
            return param2;
        }};

        str = myLambda(str);    // This should change state of 'str' to be a constant if we analyzed the local function in a context sensitive fashion.
        Command c = new Command1(str, str);
    }}
}}
",
            // Currently we generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim str As String
        str = """"

        Dim myLambda As System.Func(Of String, String) =    Function (param2 As String)
                                                                Return param2 
                                                            End Function

        str = myLambda(str)      '  This should change state of 'str' to be a constant if we analyzed the local function in a context sensitive fashion.
        Dim c As New Command1(str, str)
    End Sub
End Module",
            // Currently we generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            GetBasicResultAt(141, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsToAnalysis_CopySemanticsForString_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    public string Field;
    void M1(Test t, string param)
    {{
        t.Field = """";
        string str = t.Field;
        t.Field = param; // This should not affect location/value of 'str'.
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Public Field As String
    Sub M1(t As Test, param As String)
        t.Field = """"
        Dim str As String = t.Field
        t.Field = param ' This should not affect location/value of 'str'.
        Dim c As New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeCopy_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    public string Field;
    void M1(Test t)
    {{
        t.Field = """";
        Test t2 = t;
        string str = t2.Field;
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Public Field As String
    Sub M1(t As Test)
        t.Field = """"
        Dim t2 As Test = t
        Dim str As String = t2.Field
        Dim c As New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueTypeCopy_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct Test
{{
    public string Field;
    void M1(Test t)
    {{
        t.Field = """";
        Test t2 = t;
        string str = t2.Field;
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure Test
    Public Field As String
    Sub M1(t As Test)
        t.Field = """"
        Dim t2 As Test = t
        Dim str As String = t2.Field
        Dim c As New Command1(str, str)
    End Sub
End Structure");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeNestingCopy_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        t.a.Field = """";
        Test t2 = t;
        string str = t2.a.Field;
        Command c = new Command1(str, str);

        str = param;
        A a = t.a;
        str = a.Field;
        c = new Command1(str, str);
 
        t.a.Field = param;
        a = t.a;
        A b = a;
        t2.a.Field = """";
        str = b.Field;
        c = new Command1(str, str);
 
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
End Class

Class Test
    Public a As A
    Sub M1(t As Test, param As String)
        t.a.Field = """"
        Dim t2 As Test = t
        Dim str As String = t2.a.Field
        Dim c As New Command1(str, str)

        str = param
        Dim a As A = t.a
        str = a.Field
        c = New Command1(str, str)
 
        t.a.Field = param
        a = t.a
        Dim b As A = a
        t2.a.Field = """"
        str = b.Field
        c = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueTypeNestingCopy_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        t.a.Field = """";
        Test t2 = t;
        string str = t2.a.Field;
        Command c = new Command1(str, str);

        str = param;
        A a = t.a;
        str = a.Field;
        c = new Command1(str, str);
 
        t.a.Field = param;
        a = t.a;
        A b = a;
        t2.a.Field = """";  // 't2.a' and 'b' point to different value type objects.
        str = b.Field;
        c = new Command1(str, str);
    }}
}}
",
        // Test0.cs(118,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
        GetCSharpResultAt(118, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
End Structure

Class Test
    Public a As A
    Sub M1(t As Test, param As String)
        t.a.Field = """"
        Dim t2 As Test = t
        Dim str As String = t2.a.Field
        Dim c As New Command1(str, str)

        str = param
        Dim a As A = t.a
        str = a.Field
        c = New Command1(str, str)
 
        t.a.Field = param
        a = t.a
        Dim b As A = a
        t2.a.Field = """"       ' 't2.a' and 'b' point to different value type objects.
        str = b.Field
        c = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(153,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(153, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]

        public void FlowAnalysis_PointsTo_ValueTypeNestingCopy_02_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        t.a.Field = """";
        Test t2 = new Test();
        t = t2;             // This should clear out all the data about 't'
        string str = t.a.Field;
        Command c = new Command1(str, str);

        t.a.Field = """";
        A a = new A() {{ Field = param }};
        t2 = new Test(){{ a = a }};
        t = t2;             // This should clear out all the data about 't'
        str = t.a.Field;
        c = new Command1(str, str);
    }}
}}
",
        // Test0.cs(107,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
        GetCSharpResultAt(107, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
        // Test0.cs(114,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
        GetCSharpResultAt(114, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
End Structure

Class Test
    Public a As A
    Sub M1(t As Test, param As String) 
        t.a.Field = """"
        Dim t2 As New Test()
        t = t2             ' This should clear out all the data about 't'
        Dim str As String = t.a.Field
        Dim c As New Command1(str, str)

        t.a.Field = """"
        Dim a As New A() With {{ .Field = param }}
        t2 = New Test() With {{ .a = a }}
        t = t2             ' This should clear out all the data about 't'
        str = t.a.Field
        c = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(142,18): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(142, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(149,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(149, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeAllocationAndInitializer_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        t = new Test();
        string str = t.a.Field;         // Unknown value.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(105,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(105, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
    Public Field2 As String
End Class

Class Test

    Public a As A

    Private Sub M1(ByVal t As Test, ByVal param As String)
        t = New Test()
        Dim str As String = t.a.Field       ' Unknown value.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(143,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(143, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeAllocationAndInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        A b = new A();
        b.Field = """";
        t = new Test() {{ a = b }};
        string str = t.a.Field;                 //  'a' and 'b' point to same object.
        Command c = new Command1(str, str);
 
        str = param;
        t = new Test() {{ a = {{ Field = """" }} }};
        str = t.a.Field;
        c = new Command1(str, str); 
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
    Public Field2 As String
End Class

Class Test

    Public a As A

    Private Sub M1(ByVal t As Test, ByVal param As String)
        Dim b As A = New A()
        b.Field = """"
        t = New Test() With {{.a = b}}
        Dim str As String = t.a.Field
        Dim c As Command = New Command1(str, str)

        str = param
        t = New Test() With {{.a = New A() With {{.Field = """", .Field2 = .Field}} }}
        str = t.a.Field2
        c = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueTypeAllocationAndInitializer_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        t = new Test();
        string str = t.a.Field;         // Unknown value.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(105,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(105, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
    Public Field2 As String
End Structure

Class Test

    Public a As A

    Private Sub M1(ByVal t As Test, ByVal param As String)
        t = New Test()
        Dim str As String = t.a.Field       ' Unknown value.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(143,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(143, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueTypeAllocationAndInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        A b = new A();
        b.Field = """";
        t = new Test() {{ a = b }};
        string str = t.a.Field;                 //  'a' and 'b' have the same value.
        Command c = new Command1(str, str);
 
        t.a.Field = param;
        str = param;
        t = new Test() {{ a = {{ Field = """" }} }};
        str = t.a.Field;
        c = new Command1(str, str); 
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
    Public Field2 As String
End Structure

Class Test

    Public a As A

    Private Sub M1(ByVal t As Test, ByVal param As String)
        Dim b As A = New A()
        b.Field = """"
        t = New Test() With {{.a = b}}
        Dim str As String = t.a.Field
        Dim c As Command = New Command1(str, str)

        str = param
        t = New Test() With {{.a = New A() With {{.Field = """", .Field2 = .Field}} }}
        str = t.a.Field2
        c = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueTypeAllocationAndInitializer_02_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    void M1(Test t, string param)
    {{
        A b = new A();
        b.Field = """";
        t = new Test() {{ a = b }};
        Test t2 = t;
        string str = t2.a.Field;                 //  'a' and 'b' have the same value.
        Command1 c = new Command1(str, str);
 
        t.a.Field = param;
        str = param;
        t = new Test() {{ a = {{ Field = """" }} }};
        t2 = t;
        str = t2.a.Field;
        c = new Command1(str, str); 
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
    Public Field2 As String
End Structure

Structure Test

    Public a As A

    Private Sub M1(ByVal t As Test, ByVal param As String)
        Dim b As A = New A()
        b.Field = """"
        t = New Test() With {{.a = b}}
        Dim t2 As Test = t
        Dim str As String = t2.a.Field
        Dim c As Command = New Command1(str, str)

        str = param
        t = New Test() With {{.a = New A() With {{.Field = """", .Field2 = .Field}} }}
        t2 = t
        str = t2.a.Field2
        c = New Command1(str, str)
    End Sub
End Structure");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeCollectionInitializer_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(string param)
    {{
        var list = new System.Collections.Generic.List<string>() {{ """", param }};
        string str = list[1];
        Command c = new Command1(str, str);

        var list2 = new System.Collections.Generic.List<Test>() {{
            new Test() {{ a = {{ Field = """" }} }},
            new Test() {{ a = {{ Field = param }} }}
        }};
        str = list2[1].a.Field;
        c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(105,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(105, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(112,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(112, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
    Public Field2 As String
End Class

Class Test

    Public a As A

    Private Sub M1(ByVal param As String)
        Dim list = New System.Collections.Generic.List(Of String) From {{"""", param}}
        Dim str As String = list(1)
        Dim c As Command = New Command1(str, str)

        Dim list2 = New System.Collections.Generic.List(Of Test) From {{
            New Test() With {{ .a = New A() With {{ .Field = """", .Field2 = .Field }} }},
            New Test() With {{ .a = New A() With {{ .Field = param, .Field2 = .Field }} }}
        }}

        str = list2(1).a.Field2
        c = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(143,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(143, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(151,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(151, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1570")]
        public void FlowAnalysis_PointsTo_ReferenceTypeCollectionInitializer_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(string param)
    {{
        var list = new System.Collections.Generic.List<string>() {{ """", param }};
        string str = list[0];
        Command c = new Command1(str, str);

        var list2 = new System.Collections.Generic.List<Test>() {{
            new Test() {{ a = {{ Field = """" }} }},
            new Test() {{ a = {{ Field = param }} }}
        }};
        str = list2[0].a.Field;
        c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
    Public Field2 As String
End Class

Class Test

    Public a As A

    Private Sub M1(ByVal param As String)
        Dim list = New System.Collections.Generic.List(Of String) From {{"""", param}}
        Dim str As String = list(1)
        Dim c As Command = New Command1(str, str)

        Dim list2 = New System.Collections.Generic.List(Of Test) From {{
            New Test() With {{ .a = New A() With {{ .Field = """", .Field2 = .Field }} }},
            New Test() With {{ .a = New A() With {{ .Field = param, .Field2 = .Field }} }}
        }}

        str = list2(1).a.Field2
        c = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_DynamicObjectCreation_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public Test(int i)
    {{
    }}
    public Test(string s)
    {{
    }}

    void M1(Test t, string param, dynamic d)
    {{
        t = new Test(d);
        string str = t.a.Field;         // Unknown value.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(112,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(112, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_DynamicObjectCreation_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public Test(int i)
    {{
    }}
    public Test(string s)
    {{
    }}

    void M1(Test t, string param, dynamic d)
    {{
        A b = new A();
        b.Field = """";
        t = new Test(d) {{ a = b }};
        string str = t.a.Field;                 //  'a' and 'b' point to same object.
        Command c = new Command1(str, str);
 
        str = param;
        t = new Test(d) {{ a = {{ Field = """" }} }};
        str = t.a.Field;
        c = new Command1(str, str); 
    }}
}}
");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_AnonymousObjectCreation_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        var t = new {{ Field = """", Field2 = param }};
        string str = t.Field2;                  // Unknown value.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(99,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(99, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Private Sub M1(ByVal param As String)
        Dim t As New With {{Key .Field1 = """", .Field2 = param }}
        Dim str As String = t.Field2       ' Unknown value.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(135,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(135, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1568")]
        public void FlowAnalysis_PointsTo_AnonymousObjectCreation_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        var t = new {{ Field = """", Field2 = param }};
        var t2 = new {{ Field = param, Field2 = """" }};

        string str = t.Field;
        Command c = new Command1(str, str);
 
        str = param;
        t = t2;
        str = t.Field2 + t2.Field2;
        c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Private Sub M1(ByVal param As String)
        Dim t As New With {{Key .Field1 = """", .Field2 = .Field1 }}
        Dim t2 As New With {{Key .Field1 = param, .Field2 = """" }}

        Dim str As String = t.Field2
        Dim c As Command = New Command1(str, str)
 
        str = param
        t = t2
        str = t.Field2 + t2.Field2
        c = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_BaseDerived__Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Base
{{
    public string Field;
}}
class Derived : Base
{{
}}

class Test
{{
    public Base B;
    void M1(string param)
    {{
        Test t = new Test();
        Derived d = new Derived();
        d.Field = param;
        t.B = new Base();
        t.B.Field = """";
        t.B = d;                    // t.B now points to d
        string str = t.B.Field;     // d.Field has unknown value.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(113,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(113, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Base
    Public Field As String
End Class

Class Derived
    Inherits Base
End Class

Class Test
    Public b As Base

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        Dim d As New Derived()
        d.Field = param
        t.B = New Base()
        t.B.Field = """"
        t.B = d                             ' t.B now points to d
        Dim str As String = t.B.Field       ' d.Field has unknown value.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(150,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(150, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_BaseDerived_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Base
{{
    public string Field;
}}
class Derived : Base
{{
}}

class Test
{{
    public Base B;
    void M1(string param)
    {{
        Test t = new Test();
        Derived d = new Derived();
        d.Field = """";
        t.B = new Base();
        t.B.Field = param;
        t.B = d;                    // t.B now points to d
        string str = t.B.Field;     // d.Field is empty string.
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Base
    Public Field As String
End Class

Class Derived
    Inherits Base
End Class

Class Test
    Public b As Base

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        Dim d As New Derived()
        d.Field = """"
        t.B = New Base()
        t.B.Field = param
        t.B = d                             ' t.B now points to d
        Dim str As String = t.B.Field       ' d.Field is empty string.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void FlowAnalysis_PointsTo_ReferenceType_BaseDerived_IfStatement_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Base
{{
    public string Field;
}}
class Derived : Base
{{
}}

class Test
{{
    public Base B;
    void M1(string param)
    {{
        Test t = new Test();
        t.B = new Base();
        t.B.Field = param;
        if (param != null)
        {{
            Derived d = new Derived();
            d.Field = param;
            t.B = d;                    // t.B now points to d
        }}
        else 
        {{
            Base b = new Base();
            b.Field = """";
            t.B = b;                    // t.B now points to b       
        }}
        
        string str = t.B.Field;         // t.B now points to either b or d, but d.Field could be an unknown value (param) in one code path.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(123,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(123, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Base
    Public Field As String
End Class

Class Derived
    Inherits Base
End Class

Class Test
    Public b As Base

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        t.B = New Base()
        t.B.Field = param
        If param IsNot Nothing Then
            Dim d As New Derived()
            d.Field = param
            t.B = d                             ' t.B now points to d
        Else
            Dim b As New Base()
            b.Field = """"
            t.B = b                             ' t.B now points to b
        End If
        Dim str As String = t.B.Field           ' t.B now points to either b or d, but d.Field could be an unknown value (param) in one code path.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(156,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(156, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void FlowAnalysis_PointsTo_ReferenceType_BaseDerived_IfStatement_02_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Base
{{
    public string Field;
}}
class Derived : Base
{{
}}

class Test
{{
    public Base B;
    void M1(string param, string param2)
    {{
        Test t = new Test();
        t.B = new Base();
        t.B.Field = param;
        if (param != null)
        {{
            Derived d = new Derived();
            d.Field = """";
            t.B = d;                    // t.B now points to d
            if (param2 != null)
            {{
                d.Field = param;        // t.B.Field is unknown in this code path.
            }}
        }}
        else 
        {{
            Base b = new Base();
            b.Field = """";
            t.B = b;                    // t.B now points to b       
        }}
        
        string str = t.B.Field;         // t.B now points to either b or d, but d.Field could be an unknown value (param) in one code path.
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(127,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(127, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Base
    Public Field As String
End Class

Class Derived
    Inherits Base
End Class

Class Test
    Public b As Base

    Private Sub M1(ByVal param As String, ByVal param2 As String)
        Dim t As New Test()
        t.B = New Base()
        t.B.Field = param
        If param IsNot Nothing Then
            Dim d As New Derived()
            d.Field = """"
            t.B = d                             ' t.B now points to d
            If param2 IsNot Nothing Then
                d.Field = param                 ' t.B.Field is unknown in this code path.
            End If
        Else
            Dim b As New Base()
            b.Field = """"
            t.B = b                             ' t.B now points to b
        End If
        Dim str As String = t.B.Field           ' t.B now points to either b or d, but d.Field could be an unknown value (param) in one code path.
        Dim c As Command = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(159,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(159, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_BaseDerived_IfStatement_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Base
{{
    public string Field;
}}
class Derived : Base
{{
}}

class Test
{{
    public Base B;
    void M1(string param)
    {{
        Test t = new Test();
        t.B = new Base();
        t.B.Field = param;
        if (param != null)
        {{
            Derived d = new Derived();
            d.Field = """";
            t.B = d;                    // t.B now points to d
        }}
        else
        {{
            Base b = new Base();
            b.Field = """";
            t.B = b;                    // t.B now points to b       
        }}
        
        string str = t.B.Field;         // t.B now points to either b or d, both of which have .Field = """"
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Base
    Public Field As String
End Class

Class Derived
    Inherits Base
End Class

Class Test
    Public b As Base

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        t.B = New Base()
        t.B.Field = param
        If param IsNot Nothing Then
            Dim d As New Derived()
            d.Field = """"
            t.B = d                             ' t.B now points to d
        Else
            Dim b As New Base()
            b.Field = """"
            t.B = b                             ' t.B now points to b
        End If
        Dim str As String = t.B.Field           ' t.B now points to either b or d, both of which have .Field = """"
        Dim c As Command = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_ThisInstanceReference_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        string str = this.a.Field;         // Unknown value.
        Command c = new Command1(str, str);

        str = this.Field;           // Unknown value.
        c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(106,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(109,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(109, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
    Public Field2 As String
End Class

Class Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim str As String = Me.a.Field       ' Unknown value.
        Dim c As Command = New Command1(str, str)

        str = Me.Field                       ' Unknown value.
        c = New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(143,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(143, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(146,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(146, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_ThisInstanceReference_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        A b = new A();
        b.Field = """";
        this.a = b;
        string str = this.a.Field;
        Command1 c = new Command1(str, str);

        str = param;
        this.a = new A() {{ Field = """" }};
        str = this.a.Field;
        c = new Command1(str, str);
 
        str = param;
        Field = """";
        str = this.Field;
        c = new Command1(str, str);
     }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
    Public Field2 As String
End Class

Class Test

    Public a As A
    Public Field As String
    Public Field2 As String
 
    Private Sub M1(ByVal param As String)
        Dim b As A = New A()
        b.Field = """"
        Me.a  = b
        Dim str As String = a.Field
        Dim c As New Command1(str, str)

        str = param
        Me.a = New A() With {{.Field = """", .Field2 = .Field}}
        str = a.Field2
        c = New Command1(str, str)

        str = param
        Me.Field = """"
        Field2 = Field
        str = Me.Field2
        c = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueType_ThisInstanceReference_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        string str = this.a.Field;         // Unknown value.
        Command c = new Command1(str, str);

        str = this.Field;           // Unknown value.
        c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(106,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(106, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(109,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(109, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
    Public Field2 As String
End Structure

Structure Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim str As String = Me.a.Field       ' Unknown value.
        Dim c As Command = New Command1(str, str)

        str = Me.Field                       ' Unknown value.
        c = New Command1(str, str)
    End Sub
End Structure",
            // Test0.vb(143,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(143, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(146,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(146, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueType_ThisInstanceReference_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        A b = new A();
        b.Field = """";
        this.a = b;
        string str = this.a.Field;
        Command1 c = new Command1(str, str);

        str = param;
        this.a = new A() {{ Field = """" }};
        str = this.a.Field;
        c = new Command1(str, str);
 
        str = param;
        Field = """";
        str = this.Field;
        c = new Command1(str, str);
     }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
    Public Field2 As String
End Structure

Structure Test

    Public a As A
    Public Field As String
    Public Field2 As String
 
    Private Sub M1(ByVal param As String)
        Dim b As A = New A()
        b.Field = """"
        Me.a  = b
        Dim str As String = a.Field
        Dim c As New Command1(str, str)

        str = param
        Me.a = New A() With {{.Field = """", .Field2 = .Field}}
        str = a.Field2
        c = New Command1(str, str)

        str = param
        Me.Field = """"
        Field2 = Field
        str = Me.Field2
        c = New Command1(str, str)
    End Sub
End Structure");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_InvocationInstanceReceiver_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        this.a.Field = """";
        this.M2();
        string str = this.a.Field;         // Unknown value.
        Command c = new Command1(str, str);

        Test t = new Test();
        t.Field = """";
        t.M2();
        str = t.Field;                     // Unknown value.
        c = new Command1(str, str);
    }}
    
    public void M2()
    {{
    }}
}}
",
            // Test0.cs(108,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(108, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(114,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(114, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
End Class

Class Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Me.a.Field = """"
        Me.M2()
        Dim str As String = Me.a.Field       ' Unknown value.
        Dim c As Command = New Command1(str, str)

        Dim t As New Test()
        t.Field = """"
        t.M2()
        str = Me.Field                       ' Unknown value.
        c = New Command1(str, str)
    End Sub

    Public Sub M2()
    End Sub
End Class",
            // Test0.vb(144,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(144, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(150,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(150, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueType_InvocationInstanceReceiver_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        this.a.Field = """";
        this.M2();
        string str = this.a.Field;         // Unknown value.
        Command c = new Command1(str, str);

        Test t = new Test();
        t.Field = """";
        t.M2();
        str = t.Field;                     // Unknown value.
        c = new Command1(str, str);
    }}
    
    public void M2()
    {{
    }}
}}
",
            // Test0.cs(108,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(108, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(114,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(114, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
End Structure

Structure Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Me.a.Field = """"
        Me.M2()
        Dim str As String = Me.a.Field       ' Unknown value.
        Dim c As Command = New Command1(str, str)

        Dim t As New Test()
        t.Field = """"
        t.M2()
        str = Me.Field                       ' Unknown value.
        c = New Command1(str, str)
    End Sub

    Public Sub M2()
    End Sub
End Structure",
            // Test0.vb(144,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(144, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(150,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(150, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_Argument_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        Test t = new Test();
        t.Field = """";
        M2(t);
        string str = t.Field;               // Unknown value.
        Command c = new Command1(str, str);

        t.a.Field = """";
        this.M2(t);
        str = t.a.Field;                    // Unknown value.
        c = new Command1(str, str);

        t.a.Field = """";
        this.M3(ref t);
        str = t.a.Field;                    // Unknown value.
        c = new Command1(str, str);
    }}
    
    public void M2(Test t)
    {{
    }}
    
    public void M3(ref Test t)
    {{
    }}
}}
",
            // Test0.cs(109,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(109, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(114,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(114, 13, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(119,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(119, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
End Class

Class Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        t.Field = """"
        M2(t)
        Dim str As String = t.Field                       ' Unknown value.
        Dim c As Command = New Command1(str, str)

        t.a.Field = """"
        Me.M2(t)
        str = t.a.Field                                   ' Unknown value.
        c = New Command1(str, str)

        t.a.Field = """"
        Me.M3(t)
        str = t.a.Field                                   ' Unknown value.
        c = New Command1(str, str)
    End Sub

    Public Sub M2(t As Test)
    End Sub

    Public Sub M3(ByRef t as Test)
    End Sub
End Class",
            // Test0.vb(145,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(145, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(150,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(150, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(155,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(155, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceType_ThisArgument_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        Test t = new Test();
        this.a.Field = """";
        t.M2(this);
        string str = a.Field;               // Unknown value.
        Command c = new Command1(str, str);

        this.Field = """";
        t.M2(this);
        str = Field;                        // Unknown value.
        c = new Command1(str, str);
    }}
    
    public void M2(Test t)
    {{
    }}
}}
",
            // Test0.cs(109,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(109, 21, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(114,13): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(114, 13, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
End Class

Class Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim t as New Test()
        Me.a.Field = """"
        t.M2(Me)
        Dim str As String = a.Field                        ' Unknown value.
        Dim c As Command = New Command1(str, str)
        
        Me.Field = """"
        t.M2(Me)
        str = Field                                        ' Unknown value.
        c = New Command1(str, str)
    End Sub

    Public Sub M2(t As Test)
    End Sub
End Class",
            // Test0.vb(145,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(145, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(150,13): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(150, 13, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueType_Argument_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        Test t = new Test();
        t.Field = """";
        M2(t);                              // Passing by value cannot change contents of a value type.
        string str = t.Field;
        Command c = new Command1(str, str);

        t.a.Field = """";
        this.M2(t);                         // Passing by value cannot change contents of a value type.
        str = t.a.Field;
        c = new Command1(str, str);
    }}
    
    public void M2(Test t)
    {{
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
End Structure

Structure Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        t.Field = """"
        M2(t)                                             ' Passing by value cannot change contents of a value type.
        Dim str As String = t.Field
        Dim c As Command = New Command1(str, str)

        t.a.Field = """"
        Me.M2(t)                                          ' Passing by value cannot change contents of a value type.
        str = t.a.Field
        c = New Command1(str, str)
    End Sub

    Public Sub M2(t As Test)
    End Sub

    Public Sub M3(ByRef t as Test)
    End Sub
End Structure");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueType_Argument_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        Test t = new Test();
        t.a.Field = """";
        this.M2(ref t);
        string str = t.a.Field;                    // Passing by ref can change contents of a value type.
        Command c = new Command1(str, str);
    }}
    
    public void M2(ref Test t)
    {{
    }}
}}
",
            // Test0.cs(109,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(109, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
End Structure

Structure Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim t As New Test()
        t.a.Field = """"
        Me.M2(t)                                          ' Passing by ref can change contents of a value type.
        Dim str As String = t.a.Field
        Dim c As Command = New Command1(str, str)
    End Sub

    Public Sub M2(ByRef t as Test)
    End Sub
End Structure",
            // Test0.vb(145,28): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(145, 28, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ValueType_ThisArgument_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string Field;
}}

struct Test
{{
    public A a;
    public string Field;

    void M1(string param)
    {{
        Test t = new Test();
        this.a.Field = """";
        t.M2(this);                              // Passing by value cannot change contents of a value type.
        string str = a.Field;
        Command c = new Command1(str, str);

        this.Field = """";
        t.M2(this);                              // Passing by value cannot change contents of a value type.
        str = Field;
        c = new Command1(str, str);
    }}
    
    public void M2(Test t)
    {{
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public Field As String
End Structure

Structure Test

    Public a As A
    Public Field As String

    Private Sub M1(ByVal param As String)
        Dim t as New Test()
        Me.a.Field = """"
        t.M2(Me)
        Dim str As String = a.Field                        ' Unknown value.
        Dim c As Command = New Command1(str, str)
        
        Me.Field = """"
        t.M2(Me)
        str = Field                                        ' Unknown value.
        c = New Command1(str, str)
    End Sub

    Public Sub M2(t As Test)
    End Sub
End Structure");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ConstantArrayElement_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string[] strArray)
    {{
        strArray[0] = """";
        string str = strArray[0];
        Adapter c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(strArray As String())
        strArray(0) = """"
        Dim str As String = strArray(0)
        Dim c As New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_NonConstantArrayElement_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string[] strArray, string param)
    {{
        strArray[0] = param;
        string str = strArray[0];
        Adapter c = new Adapter1(str, str);
    }}
}}
",
            // Test0.cs(99,21): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(99, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(strArray As String(), param As String)
        strArray(0) = param
        Dim str As String = strArray(0)
        Dim c As New Adapter1(str, str)
    End Sub
End Module",
            // Test0.vb(135,18): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(135, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ArrayInitializer_ConstantArrayElement_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string[] strArray = new string[] {{ """", param }} ;
        string str = strArray[0];
        Adapter c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim strArray As String() = New String() {{ """", param }}
        Dim str As String = strArray(0)
        Dim c As New Adapter1(str, str)
    End Sub
End Module");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ArrayInitializer_NonConstantArrayElement_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1(string param)
    {{
        string[] strArray = new string[] {{ """", param }} ;
        string str = strArray[1];
        Adapter c = new Adapter1(str, str);
    }}
}}
",
            GetCSharpResultAt(99, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Module Test
    Sub M1(param As String)
        Dim strArray As String() = New String() {{ """", param }}
        Dim str As String = strArray(1)
        Dim c As New Adapter1(str, str)
    End Sub
End Module",
            GetBasicResultAt(135, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ConstantArrayElement_ArrayFieldInReferenceType_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string[] StringArrayA;
}}

class Test
{{
    public A a;
    public string[] StringArray;

    void M1(Test t, string[] strArray1, string[] strArray2, string[] strArray3)
    {{
        t.StringArray = strArray1;
        strArray1[0] = """";
        string str = t.StringArray[0];
        Adapter c = new Adapter1(str, str);

        strArray2[1] = """";
        t.StringArray = strArray2;
        str = t.StringArray[1];
        c = new Adapter1(str, str);

        strArray3[1000] = """";
        t.a.StringArrayA = strArray3;
        Test t2 = t;
        str = t2.a.StringArrayA[1000];
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public StringArrayA As String()
End Class

Class Test
    Public a As A
    Public StringArray As String()

    Sub M1(t As Test, strArray1 As String(), strArray2 As String(), strArray3 As String())
        t.StringArray = strArray1
        strArray1(0) = """"
        Dim str As String = t.StringArray(0)
        Dim c As New Adapter1(str, str)

        strArray2(1) = """"
        t.StringArray = strArray2
        str = t.StringArray(1)
        c = New Adapter1(str, str)

        strArray3(1000) = """"
        t.a.StringArrayA = strArray3
        Dim t2 As Test = t
        str = t2.a.StringArrayA(1000)
        c = New Adapter1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_NonConstantArrayElement_ArrayFieldInReferenceType_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string[] StringArrayA;
}}

class Test
{{
    public A a;
    public string[] StringArray;

    void M1(Test t, string[] strArray1, string[] strArray2, string[] strArray3, string param)
    {{
        t.StringArray = strArray1;
        string str = t.StringArray[1];
        Adapter c = new Adapter1(str, str);

        strArray2[1] = """";
        t.StringArray = strArray2;
        strArray2[1] = param;
        str = t.StringArray[1];
        c = new Adapter1(str, str);

        strArray3[1000] = """";
        t.a.StringArrayA = strArray3;
        Test t2 = t;
        string[] strArray4 = strArray3;
        strArray4[1000] = param;
        str = t2.a.StringArrayA[1000];
        c = new Adapter1(str, str);
    }}
}}
",
            // Test0.cs(107,21): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(107, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            // Test0.cs(113,13): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(113, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            // Test0.cs(121,13): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(121, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public StringArrayA As String()
End Class

Class Test
    Public a As A
    Public StringArray As String()

    Sub M1(t As Test, strArray1 As String(), strArray2 As String(), strArray3 As String(), param As String)
        t.StringArray = strArray1
        Dim str As String = t.StringArray(0)
        Dim c As New Adapter1(str, str)

        strArray2(1) = """"
        t.StringArray = strArray2
        strArray2(1) = param
        str = t.StringArray(1)
        c = New Adapter1(str, str)

        strArray3(1000) = """"
        t.a.StringArrayA = strArray3
        Dim t2 As Test = t
        Dim strArray4 As String() = strArray3
        strArray4(1000) = param
        str = t2.a.StringArrayA(1000)
        c = New Adapter1(str, str)
    End Sub
End Class",
            // Test0.vb(142,18): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(142, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(148,13): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(148, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(156,13): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(156, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ConstantArrayElement_ArrayFieldInValueType_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string[] StringArrayA;
}}

struct Test
{{
    public A a;
    public string[] StringArray;

    void M1(Test t, string[] strArray1, string[] strArray2, string[] strArray3)
    {{
        t.StringArray = strArray1;
        strArray1[0] = """";
        string str = t.StringArray[0];
        Adapter c = new Adapter1(str, str);

        strArray2[1] = """";
        t.StringArray = strArray2;
        str = t.StringArray[1];
        c = new Adapter1(str, str);

        strArray3[1000] = """";
        t.a.StringArrayA = strArray3;
        Test t2 = t;
        str = t2.a.StringArrayA[1000];
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public StringArrayA As String()
End Structure

Structure Test
    Public a As A
    Public StringArray As String()

    Sub M1(t As Test, strArray1 As String(), strArray2 As String(), strArray3 As String())
        t.StringArray = strArray1
        strArray1(0) = """"
        Dim str As String = t.StringArray(0)
        Dim c As New Adapter1(str, str)

        strArray2(1) = """"
        t.StringArray = strArray2
        str = t.StringArray(1)
        c = New Adapter1(str, str)

        strArray3(1000) = """"
        t.a.StringArrayA = strArray3
        Dim t2 As Test = t
        str = t2.a.StringArrayA(1000)
        c = New Adapter1(str, str)
    End Sub
End Structure");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_NonConstantArrayElement_ArrayFieldInValueType_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

struct A
{{
    public string[] StringArrayA;
}}

struct Test
{{
    public A a;
    public string[] StringArray;

    void M1(Test t, string[] strArray1, string[] strArray2, string[] strArray3, string param)
    {{
        t.StringArray = strArray1;
        string str = t.StringArray[1];
        Adapter c = new Adapter1(str, str);

        strArray2[1] = """";
        t.StringArray = strArray2;
        strArray2[1] = param;
        str = t.StringArray[1];
        c = new Adapter1(str, str);

        strArray3[1000] = """";
        t.a.StringArrayA = strArray3;
        Test t2 = t;
        string[] strArray4 = strArray3;
        strArray4[1000] = param;
        str = t2.a.StringArrayA[1000];
        c = new Adapter1(str, str);
    }}
}}
",
            // Test0.cs(107,21): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(107, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            // Test0.cs(113,13): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(113, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            // Test0.cs(121,13): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(121, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Structure A
    Public StringArrayA As String()
End Structure

Structure Test
    Public a As A
    Public StringArray As String()

    Sub M1(t As Test, strArray1 As String(), strArray2 As String(), strArray3 As String(), param As String)
        t.StringArray = strArray1
        Dim str As String = t.StringArray(0)
        Dim c As New Adapter1(str, str)

        strArray2(1) = """"
        t.StringArray = strArray2
        strArray2(1) = param
        str = t.StringArray(1)
        c = New Adapter1(str, str)

        strArray3(1000) = """"
        t.a.StringArrayA = strArray3
        Dim t2 As Test = t
        Dim strArray4 As String() = strArray3
        strArray4(1000) = param
        str = t2.a.StringArrayA(1000)
        c = New Adapter1(str, str)
    End Sub
End Structure",
            // Test0.vb(142,18): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(142, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(148,13): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(148, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(156,13): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(156, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ConstantArrayElement_IndexerFieldInReferenceType_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    private string[] _stringArray;
    public string this[int i]
    {{
        get => _stringArray[i];
        set => _stringArray[i] = value;
    }}

}}

class Test
{{
    public A a;

    private string[] _stringArray;
    public string this[int i]
    {{
        get => _stringArray[i];
        set => _stringArray[i] = value;
    }}

    void M1(Test t, string[] strArray1)
    {{
        strArray1[0] = """";
        t[0] = strArray1[0];
        string str = t[0];
        Adapter c = new Adapter1(str, str);

        A a = new A();
        t.a = a;
        Test t2 = t;
        a[1000] = """";
        str = t2.a[1000];
        c = new Adapter1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Private _stringArray As String()
    Public Property StringArray(i As Integer) As String
        Get
            Return _stringArray(i)
        End Get
        Set(value As String)
            _stringArray(i) = value
        End Set
    End Property
End Class

Class Test
    Public a As A
    
    Private _stringArray As String()
    Public Property StringArray(i As Integer) As String
        Get
            Return _stringArray(i)
        End Get
        Set(value As String)
            _stringArray(i) = value
        End Set
    End Property

    Sub M1(t As Test, strArray1 As String())
        strArray1(0) = """"
        t.StringArray(0) = strArray1(0)
        Dim str As String = t.StringArray(0)
        Dim c As New Adapter1(str, str)

        Dim a As new A()
        t.a = a
        Dim t2 As Test = t
        a.StringArray(1000) = """"
        str = t2.a.StringArray(1000)
        c = New Adapter1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_NonConstantArrayElement_IndexerFieldInReferenceType_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Adapter1 : Adapter
{{
    public Adapter1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    private string[] _stringArray;
    public string this[int i]
    {{
        get => _stringArray[i];
        set => _stringArray[i] = value;
    }}

}}

class Test
{{
    public A a;

    private string[] _stringArray;
    public string this[int i]
    {{
        get => _stringArray[i];
        set => _stringArray[i] = value;
    }}

    void M1(Test t, string[] strArray1, string param)
    {{
        t[0] = """";
        string str = t[1];
        Adapter c = new Adapter1(str, str);

        A a = new A();
        t.a = a;
        Test t2 = t;
        a[1000] = param;
        str = t2.a[1000];
        c = new Adapter1(str, str);
    }}
}}
",
            // Test0.cs(119,21): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(119, 21, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
            // Test0.cs(126,13): warning CA2100: Review if the query string passed to 'Adapter1.Adapter1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(126, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Adapter1
    Inherits Adapter

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Private _stringArray As String()
    Public Property StringArray(i As Integer) As String
        Get
            Return _stringArray(i)
        End Get
        Set(value As String)
            _stringArray(i) = value
        End Set
    End Property
End Class

Class Test
    Public a As A
    
    Private _stringArray As String()
    Public Property StringArray(i As Integer) As String
        Get
            Return _stringArray(i)
        End Get
        Set(value As String)
            _stringArray(i) = value
        End Set
    End Property

    Sub M1(t As Test, strArray1 As String(), param As String)
        t.StringArray(0) = """"
        Dim str As String = t.StringArray(1)
        Dim c As New Adapter1(str, str)

        Dim a As new A()
        t.a = a
        Dim t2 As Test = t
        a.StringArray(1000) = param
        str = t2.a.StringArray(1000)
        c = New Adapter1(str, str)
    End Sub
End Class",
            // Test0.vb(159,18): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(159, 18, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(166,13): warning CA2100: Review if the query string passed to 'Sub Adapter1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(166, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeArray_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(string param)
    {{
        A b = new A() {{ Field = """" }};
        A c = new A() {{ Field = param }};
        Test t = new Test() {{ a = c }};
        Test[] testArray = new Test[] {{ new Test() {{ a = b }}, t }};
        string str = testArray[1].a.Field;         // testArray[1].a points to c.
        Command cmd = new Command1(str, str);

        b.Field = param;
        str = testArray[0].a.Field;         // testArray[0].a points to b.
        cmd = new Command1(str, str);        
    }}
}}
",
            // Test0.cs(108,23): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(108, 23, "Command1.Command1(string cmd, string parameter2)", "M1"),
            // Test0.cs(112,15): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(112, 15, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
End Class

Class Test
    Public a As A

    Private Sub M1(ByVal param As String)
        Dim b As A = New A() With {{.Field = """"}}
        Dim c As A = New A() With {{.Field = param}}
        Dim t As Test = New Test() With {{.a = c}}
        Dim testArray As Test() = New Test() {{New Test() With {{.a = b}}, t}}
        Dim str As String = testArray(1).a.Field         ' testArray[1].a points to c.
        Dim cmd As Command = New Command1(str, str)

        b.Field = param
        str = testArray(0).a.Field         ' testArray[0].a points to b.
        cmd = New Command1(str, str)
    End Sub
End Class
",
            // Test0.vb(144,30): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(144, 30, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"),
            // Test0.vb(148,15): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(148, 15, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact]
        public void FlowAnalysis_PointsTo_ReferenceTypeArray_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class A
{{
    public string Field;
}}

class Test
{{
    public A a;
    void M1(string param)
    {{
        A b = new A() {{ Field = """" }};
        A c = new A() {{ Field = param }};
        Test t = new Test() {{ a = c }};
        Test[] testArray = new Test[] {{ new Test() {{ a = b }}, t }};
        string str = testArray[0].a.Field;         // testArray[0].a points to b.
        Command cmd = new Command1(str, str);

        c.Field = b.Field;
        str = testArray[1].a.Field;         // testArray[1].a points to c.
        cmd = new Command1(str, str);        
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class A
    Public Field As String
End Class

Class Test
    Public a As A

    Private Sub M1(ByVal param As String)
        Dim b As A = New A() With {{.Field = """"}}
        Dim c As A = New A() With {{.Field = param}}
        Dim t As Test = New Test() With {{.a = c}}
        Dim testArray As Test() = New Test() {{New Test() With {{.a = b}}, t}}
        Dim str As String = testArray(0).a.Field         ' testArray[0].a points to b.
        Dim cmd As Command = New Command1(str, str)

        c.Field = b.Field
        str = testArray(1).a.Field         ' testArray[1].a points to c.
        cmd = New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_AutoGeneratedProperty_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    public string AutoGeneratedProperty {{ get; set; }}

    void M1()
    {{
        AutoGeneratedProperty = """";
        string str = AutoGeneratedProperty;
        Command c = new Command1(str, str);
    }}
}}
");

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Public Property AutoGeneratedProperty As String
    Sub M1()
        AutoGeneratedProperty = """"
        Dim str As String = AutoGeneratedProperty
        Dim c As New Command1(str, str)
    End Sub
End Class");
        }

        [Fact]
        public void FlowAnalysis_PointsTo_CustomProperty_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    string _value;
    string _value2;
    public string MyProperty {{ get => _value; set => _value = value + _value2; }}

    void M1(string param)
    {{
        _value2 = param;
        MyProperty = """";
        string str = MyProperty;
        Command c = new Command1(str, str);
    }}
}}
",
            // Test0.cs(104,21): warning CA2100: Review if the query string passed to 'Command1.Command1(string cmd, string parameter2)' in 'M1', accepts any user input.
            GetCSharpResultAt(104, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));

            VerifyBasic($@"
{SetupCodeBasic}

Class Command1
    Inherits Command

    Sub New(cmd As String, parameter2 As String)
    End Sub
End Class

Class Test
    Private _value, _value2 As String
    Public Property MyProperty As String
        Get
            Return _value
        End Get
        Set(value As String)
            _value = value + _value2
        End Set
    End Property

    Sub M1(param As String)
        _value2 = param
        MyProperty = """"
        Dim str As String = MyProperty
        Dim c As New Command1(str, str)
    End Sub
End Class",
            // Test0.vb(146,18): warning CA2100: Review if the query string passed to 'Sub Command1.New(cmd As String, parameter2 As String)' in 'M1', accepts any user input.
            GetBasicResultAt(146, 18, "Sub Command1.New(cmd As String, parameter2 As String)", "M1"));
        }
    }
}
