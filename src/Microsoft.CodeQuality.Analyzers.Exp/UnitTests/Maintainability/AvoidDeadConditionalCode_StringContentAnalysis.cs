// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Exp.UnitTests.Maintainability
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PredicateAnalysis)]
    public partial class AvoidDeadConditionalCodeTests : DiagnosticAnalyzerTestBase
    {
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void SimpleStringCompare_NoDiagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param)
    {
        if (param == """")
        {
        }

        if ("""" == param)
        {
        }
    }
}
");

            VerifyBasic(@"
Module Test
    Sub M1(param As String)
        If param = """" Then
        End If

        If """" = param Then
        End If
    End Sub
End Module");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void SimpleStringCompare_AfterAssignment_Diagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param)
    {
        param = """";
        if (param == """")
        {
        }

        if ("""" != param)
        {
        }
    }
}
",
            // Test0.cs(7,13): warning CA1508: 'param == ""' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(7, 13, @"param == """"", "true"),
            // Test0.cs(11,13): warning CA1508: '"" != param' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(11, 13, @""""" != param", "false"));

            VerifyBasic(@"
Module Test
    Sub M1(param As String)
        param = """"
        If param = """" Then
        End If

        If """" <> param Then
        End If
    End Sub
End Module",
            // Test0.vb(5,12): warning CA1508: 'param = ""' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(5, 12, @"param = """"", "True"),
            // Test0.vb(8,12): warning CA1508: '"" <> param' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(8, 12, @""""" <> param", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void ElseIf_NestedIf_StringCompare_Diagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param)
    {
        string str = """";
        if (param != """")
        {
        }
        else if (param == str)
        {
        }

        if ("""" == param)
        {
            if (param != str)
            {
            }
        }
    }
}
",
            // Test0.cs(10,18): warning CA1508: 'param == str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(10, 18, "param == str", "true"),
            // Test0.cs(16,17): warning CA1508: 'param != str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(16, 17, "param != str", "false"));

            VerifyBasic(@"
Module Test
    Sub M1(param As String)
        Dim str = """"
        If param <> """" Then
        Else If param = str Then
        End If

        If """" = param Then
            If param <> str Then
            End If
        End If
    End Sub
End Module",
            // Test0.vb(6,17): warning CA1508: 'param = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(6, 17, "param = str", "True"),
            // Test0.vb(10,16): warning CA1508: 'param <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(10, 16, "param <> str", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void ConditionaAndOrStringCompare_Diagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param)
    {
        string str = """";
        if (param != """" || param == str)
        {
        }

        if ("""" == param && param != str)
        {
        }
    }
}
",
            // Test0.cs(7,28): warning CA1508: 'param == str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(7, 28, "param == str", "true"),
            // Test0.cs(11,28): warning CA1508: 'param != str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(11, 28, "param != str", "false"));

            VerifyBasic(@"
Module Test
    Sub M1(param As String)
        Dim str = """"
        If param <> """" OrElse param = str Then
        End If

        If """" = param AndAlso param <> str Then
        End If
    End Sub
End Module",
            // Test0.vb(5,31): warning CA1508: 'param = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(5, 31, "param = str", "True"),
            // Test0.vb(8,31): warning CA1508: 'param <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(8, 31, "param <> str", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void ElseIf_NestedIf_StringCompare_DifferentLiteral_Diagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param)
    {
        string str = ""a"";
        if (param != """")
        {
        }
        else if (param == str)
        {
        }

        if ("""" == param)
        {
            if (param != str)
            {
            }
        }
    }
}
",
            // Test0.cs(10,18): warning CA1508: 'param == str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(10, 18, "param == str", "false"),
            // Test0.cs(16,17): warning CA1508: 'param != str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(16, 17, "param != str", "true"));

            VerifyBasic(@"
Module Test
    Sub M1(param As String)
        Dim str = ""a""
        If param <> """" Then
        Else If param = str Then
        End If

        If """" = param Then
            If param <> str Then
            End If
        End If
    End Sub
End Module",
            // Test0.vb(6,17): warning CA1508: 'param = str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(6, 17, "param = str", "False"),
            // Test0.vb(10,16): warning CA1508: 'param <> str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(10, 16, "param <> str", "True"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void ElseIf_NestedIf_StringCompare_DifferentLiterals_NoDiagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param, bool flag)
    {
        string str = flag ? ""a"" : """";
        if (param != """")
        {
        }
        else if (param == str)
        {
        }

        if ("""" == param)
        {
            if (param != str)
            {
            }
        }
    }
}
");

            VerifyBasic(@"
Module Test
    Sub M1(param As String, flag As Boolean)
        Dim str = If(flag, ""a"", """")
        If param <> """" Then
        Else If param = str Then
        End If

        If """" = param Then
            If param <> str Then
            End If
        End If
    End Sub
End Module");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_WhileLoop()
        {
            VerifyCSharp(@"
class Test
{
    void M(string param)
    {
        string str = """";
        while (param == str)
        {
            // param = str here
            if (param == str)
            {
            }
            if (param != str)
            {
            }
        }

        // param is unknown here
        if (str == param)
        {
        }
        if (str != param)
        {
        }
    }
}
",
            // Test0.cs(10,17): warning CA1508: 'param == str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(10, 17, "param == str", "true"),
            // Test0.cs(13,17): warning CA1508: 'param != str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(13, 17, "param != str", "false"));

            VerifyBasic(@"
Module Test
    ' While loop
    Private Sub M1(ByVal param As String)
        Dim str As String = """"
        While param = str
            ' param == str here
            If param = str Then
            End If
            If param <> str Then
            End If
        End While

        ' param is unknown here
        If str = param Then
            End If
        If str <> param Then
        End If
    End Sub
End Module",
            // Test0.vb(8,16): warning CA1508: 'param = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(8, 16, "param = str", "True"),
            // Test0.vb(10,16): warning CA1508: 'param <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(10, 16, "param <> str", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_DoWhileLoop()
        {
            VerifyCSharp(@"
class Test
{
    void M(string param)
    {
        string str = """";
        do
        {
            // param is unknown here
            if (str == param)
            {
            }
            if (str != param)
            {
            }
        }
        while (param != str);

        // param = str here
        if (param == str)
        {
        }
        if (param != str)
        {
        }
    }
}
",
            // Test0.cs(20,13): warning CA1508: 'param == str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(20, 13, "param == str", "true"),
            // Test0.cs(23,13): warning CA1508: 'param != str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(23, 13, "param != str", "false"));

            VerifyBasic(@"
Module Test
    ' Do-While top loop
    Private Sub M(ByVal param As String)
        Dim str As String = """"
        Do While param <> str
            ' param is unknown here
            If str = param Then
                End If
            If str <> param Then
            End If
        Loop

        ' param == str here
        If param = str Then
        End If
        If param <> str Then
        End If
    End Sub

    ' Do-While bottom loop
    Private Sub M2(ByVal param2 As String)
        Dim str As String = """"
        Do
            ' param2 is unknown here
            If str = param2 Then
                End If
            If str <> param2 Then
            End If
        Loop While param2 <> str

        ' param2 == str here
        If param2 = str Then
        End If
        If param2 <> str Then
        End If
    End Sub
End Module",
            // Test0.vb(15,12): warning CA1508: 'param = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(15, 12, "param = str", "True"),
            // Test0.vb(17,12): warning CA1508: 'param <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(17, 12, "param <> str", "False"),
            // Test0.vb(33,12): warning CA1508: 'param2 = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(33, 12, "param2 = str", "True"),
            // Test0.vb(35,12): warning CA1508: 'param2 <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(35, 12, "param2 <> str", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_DoUntilLoop()
        {
            VerifyBasic(@"
Module Test
    ' Do-Until top loop
    Private Sub M(ByVal param As String)
        Dim str As String = """"
        Do Until param <> str
            ' param == str here
            If param = str Then
            End If
            If param <> str Then
            End If
        Loop

        ' param is unknown here
        If str = param Then
            End If
        If str <> param Then
        End If
    End Sub

    ' Do-Until bottom loop
    Private Sub M2(ByVal param2 As String)
        Dim str As String = """"
        Do
            ' param2 is unknown here
            If str = param2 Then
                End If
            If str <> param2 Then
            End If
        Loop Until param2 = str

        ' param2 == str here
        If param2 = str Then
        End If
        If param2 <> str Then
        End If
    End Sub
End Module",
            // Test0.vb(8,16): warning CA1508: 'param = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(8, 16, "param = str", "True"),
            // Test0.vb(10,16): warning CA1508: 'param <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(10, 16, "param <> str", "False"),
            // Test0.vb(33,12): warning CA1508: 'param2 = str' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(33, 12, "param2 = str", "True"),
            // Test0.vb(35,12): warning CA1508: 'param2 <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(35, 12, "param2 <> str", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_ForLoop()
        {
            VerifyCSharp(@"
class Test
{
    void M(string param, string param2)
    {
        string str = """";
        for (param = str; param2 != str;)
        {
            // param = str here
            if (param == str)
            {
            }
            if (param != str)
            {
            }

            // param2 != str here, but we don't track not-contained values so no diagnostic.
            if (param2 == str)
            {
            }
            if (param2 != str)
            {
            }
        }
        
        // param2 == str here
        if (str == param2)
        {
        }
        if (str != param2)
        {
        }
        
        // param == str here
        if (str == param)
        {
        }
        if (str != param)
        {
        }
    }
}
",
            // Test0.cs(10,17): warning CA1508: 'param == str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(10, 17, "param == str", "true"),
            // Test0.cs(13,17): warning CA1508: 'param != str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(13, 17, "param != str", "false"),
            // Test0.cs(27,13): warning CA1508: 'str == param2' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(27, 13, "str == param2", "true"),
            // Test0.cs(30,13): warning CA1508: 'str != param2' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(30, 13, "str != param2", "false"),
            // Test0.cs(35,13): warning CA1508: 'str == param' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(35, 13, "str == param", "true"),
            // Test0.cs(38,13): warning CA1508: 'str != param' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(38, 13, "str != param", "false"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.CopyAnalysis)]
        [Fact]
        public void StringCompare_CopyAnalysis()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param, string param2)
    {
        string str = ""a"";
        if (param == str && param2 == str && param == param2)
        {
        }

        param = param2;
        if (param != str || param2 != str)
        {
        }
    }
}
",
            // Test0.cs(7,46): warning CA1508: 'param == param2' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(7, 46, "param == param2", "true"),
            // Test0.cs(12,29): warning CA1508: 'param2 != str' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(12, 29, "param2 != str", "false"));

            VerifyBasic(@"
Module Test
    Sub M1(param As String, param2 As String)
        Dim str = ""a""
        If param = str AndAlso param2 = str AndAlso param = param2 Then
        End If

        param = param2
        If param <> str OrElse param2 <> str Then
        End If
    End Sub
End Module",
            // Test0.vb(5,53): warning CA1508: 'param = param2' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(5, 53, "param = param2", "True"),
            // Test0.vb(9,32): warning CA1508: 'param2 <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(9, 32, "param2 <> str", "False"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_WithNonLiteral_ConditionalOr_NoDiagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param, string param2, bool flag)
    {
        string str = """";
        string str2 = flag ? ""a"" : ""b"";
        string strMayBeConst = param2;

        if (param == str || param == str2)
        {
        }

        if (str2 != param || param == strMayBeConst)
        {
        }

        if (param == strMayBeConst || str2 != param)
        {
        }
    }
}
");

            VerifyBasic(@"
Module Test
    Sub M1(param As String, param2 As String, flag As Boolean)
        Dim str = """"
        Dim str2 = If(flag, ""a"", ""b"")
        Dim strMayBeConst = param2

        If param = str OrElse param = str2 Then
        End If

        If str2 <> param OrElse param = strMayBeConst Then
        End If

        If param = strMayBeConst OrElse str2 <> param Then
        End If
    End Sub
End Module");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_WithNonLiteral_ConditionalAnd_NoDiagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param, string param2, bool flag)
    {
        string str = """";
        string str2 = flag ? ""a"" : ""b"";
        string strMayBeConst = param2;

        if (param == str && param2 == str2)
        {
        }

        if (param == strMayBeConst && str2 == param)
        {
        }

        if (param != str && param != str2)
        {
        }

        if (param != str && param2 != str2)
        {
        }

        if (str2 != param && param == strMayBeConst)
        {
        }
    }
}
");

            VerifyBasic(@"
Module Test
    Sub M1(param As String, param2 As String, flag As Boolean)
        Dim str = """"
        Dim str2 = If(flag, ""a"", ""b"")
        Dim strMayBeConst = param2

        If param = str AndAlso param2 = str2 Then
        End If

        If param = strMayBeConst AndAlso str2 = param Then
        End If

        If str2 <> param AndAlso param <> str Then
        End If
        
        If str2 <> param AndAlso param2 <> str Then
        End If
        
        If str2 <> param AndAlso param = strMayBeConst Then
        End If
    End Sub
End Module");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_ConditionalAndOrNegation_NoDiagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param, bool flag, string param2)
    {
        string strConst = """";
        string strConst2 = flag ? ""a"" : """";
        string strMayBeNonConst = flag ? ""c"" : param2;

        if (param == strConst || !(strConst2 != param) && param != strMayBeNonConst)
        {
        }

        if (!(strConst2 == param && !(param != strConst)) || param == strMayBeNonConst)
        {
        }

        if (param != strConst && !(strConst2 != param || param != strMayBeNonConst))
        {
        }
    }
}
");

            VerifyBasic(@"
Module Test
    Sub M1(param As String, param2 As String, flag As Boolean)
        Dim strConst As String = """"
        Dim strConst2 As String = If(flag, ""a"", """")
        Dim strMayBeNonConst As String = If(flag, ""c"", param2)

        If param = strConst OrElse Not(strConst2 <> param) AndAlso param <> strMayBeNonConst Then
        End If

        If Not(strConst2 = param AndAlso Not (param <> strConst)) OrElse param <> strMayBeNonConst Then
        End If

        If param <> strConst AndAlso Not(strConst2 <> param OrElse param <> strMayBeNonConst) Then
        End If
    End Sub
End Module");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_ConditionalAndOrNegation_Diagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param, bool flag, string param2, string param3)
    {
        string strConst = """";
        string strConst2 = flag ? ""a"" : """";
        string strMayBeNonConst = flag ? ""c"" : param2;

        // First and last conditions are opposites, so infeasible.
        if (param == strConst && !(strConst2 != param || param != strConst))
        {
        }

        // First and last conditions are identical.
        if (param == strConst && !(strConst2 != param || param == strConst)){
        }

        // Comparing with maybe const, no diagnostic
        if (param3 == strConst && !(strConst2 == param3 || param3 == strMayBeNonConst))
        {
        }

        // We don't track not-equals values, no diagnostic
        if (param3 != strConst && !(strConst2 != param3 || param3 != strConst))
        {
        }
    }
}
",
            // Test0.cs(11,58): warning CA1508: 'param != strConst' is always 'false'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(11, 58, "param != strConst", "false"),
            // Test0.cs(16,58): warning CA1508: 'param == strConst' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(16, 58, "param == strConst", "true"));

            VerifyBasic(@"
Module Test
    Sub M1(param As String, param2 As String, flag As Boolean, param3 As String)
        Dim strConst As String = """"
        Dim strConst2 As String = If(flag, ""a"", """")
        Dim strMayBeNonConst As String = If(flag, ""c"", param2)

        ' First and last conditions are opposites, so infeasible.
        If param = strConst AndAlso Not(strConst2 <> param OrElse param <> strConst) Then
        End If

        ' First and last conditions are identical.
        If param = strConst AndAlso Not(strConst2 <> param OrElse param = strConst) Then
        End If

        ' Comparing with maybe const, no diagnostic
        If param3 = strConst AndAlso Not(strConst2 = param3 OrElse param3 = strMayBeNonConst) Then
        End If

        ' We don't track not-equals values, no diagnostic
        If param3 <> strConst AndAlso Not(strConst2 <> param3 OrElse param3 <> strConst) Then
        End If
    End Sub
End Module",
            // Test0.vb(9,67): warning CA1508: 'param <> strConst' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(9, 67, "param <> strConst", "False"),
            // Test0.vb(13,67): warning CA1508: 'param = strConst' is always 'True'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(13, 67, "param = strConst", "True"));
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_ContractCheck_NoDiagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M1(string param)
    {
        System.Diagnostics.Contracts.Contract.Requires(param != """");
    }

    void M2(string param, string param2)
    {
        param2 = """";
        System.Diagnostics.Contracts.Contract.Requires(param == """" || param2 != param);
    }

    void M3(string param, string param2, string param3)
    {
        System.Diagnostics.Contracts.Contract.Requires(param == param2 && !(param2 != """") || param2 == param3);
    }
}
");

            VerifyBasic(@"
Module Test
    Private Sub M1(ByVal param As String)
        System.Diagnostics.Contracts.Contract.Requires(param <> """")
    End Sub

    Private Sub M2(ByVal param As String, ByVal param2 As String)
        param2 = """"
        System.Diagnostics.Contracts.Contract.Requires(param = """" OrElse param2 <> param)
    End Sub

    Private Sub M3(ByVal param As String, ByVal param2 As String, param3 As String)
        System.Diagnostics.Contracts.Contract.Requires(param = param2 AndAlso Not(param2 <> """") OrElse param2 = param3)
    End Sub
End Module");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.StringContentAnalysis)]
        [Fact]
        public void StringCompare_ContractCheck_Diagnostic()
        {
            VerifyCSharp(@"
class Test
{
    void M(string param)
    {
        var str = """";
        param = """";
        System.Diagnostics.Contracts.Contract.Requires(param == str);
    }
}
",
            // Test0.cs(8,56): warning CA1508: 'param == str' is always 'true'. Remove or refactor the condition(s) to avoid dead code.
            GetCSharpResultAt(8, 56, "param == str", "true"));

            VerifyBasic(@"
Module Test
    Private Sub M(ByVal param As String)
        Dim str = """"
        param = """"
        System.Diagnostics.Contracts.Contract.Requires(param <> str)
    End Sub
End Module",
            // Test0.vb(6,56): warning CA1508: 'param <> str' is always 'False'. Remove or refactor the condition(s) to avoid dead code.
            GetBasicResultAt(6, 56, "param <> str", "False"));
        }
    }
}
