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

        [Fact(Skip = "TODO: Readonly Field Analysis")]
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
        str = a?.X;     // Need PointsTo analysis to detect constant, NYI
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
        GetCSharpResultAt(111, 13, "Adapter1.Adapter1(string cmd, string parameter2)", "M1"),
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
        str = a?.X                  ' Need PointsTo analysis to detect constant, NYI
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
            GetBasicResultAt(146, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"),
            GetBasicResultAt(150, 13, "Sub Adapter1.New(cmd As String, parameter2 As String)", "M1"));
        }

        [Fact(Skip = "TODO:File Bug")]
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

        [Fact(Skip = "TODO:File Bug")]
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

        [Fact(Skip = "TODO:File Bug")]
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

        [Fact(Skip = "TODO:File Bug")]
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
    }
}
