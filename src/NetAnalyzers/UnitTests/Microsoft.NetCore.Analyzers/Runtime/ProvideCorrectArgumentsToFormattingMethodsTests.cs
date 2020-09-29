// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ProvideCorrectArgumentsToFormattingMethodsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ProvideCorrectArgumentsToFormattingMethodsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ProvideCorrectArgumentsToFormattingMethodsTests
    {
        [Theory]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_MoreArgsThanFormatItem_VoidMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"("""", 1)|};
        {|#1:" + invocation + @"(""{0}"", 1, 2)|};
        {|#2:" + invocation + @"(""{0} {1}"", 1, 2, 3)|};
        {|#3:" + invocation + @"(""{0} {1} {2}"", 1, 2, 3, 4)|};
        {|#4:" + invocation + @"(""{0} {0}"", 1, 2)|};
    }
}
",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2),
            VerifyCS.Diagnostic().WithLocation(3),
            VerifyCS.Diagnostic().WithLocation(4));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"("""", 1)|}
        {|#1:" + invocation + @"(""{0}"", 1, 2)|}
        {|#2:" + invocation + @"(""{0} {1}"", 1, 2, 3)|}
        {|#3:" + invocation + @"(""{0} {1} {2}"", 1, 2, 3, 4)|}
        {|#4:" + invocation + @"(""{0} {0}"", 1, 2)|}
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2),
            VerifyVB.Diagnostic().WithLocation(3),
            VerifyVB.Diagnostic().WithLocation(4));
        }

        [Fact]
        public async Task CA2241_MoreArgsThanFormatItem_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider p)
    {
        var a = {|#0:String.Format("""", 1)|};
        var b = {|#1:String.Format(""{0}"", 1, 2)|};
        var c = {|#2:String.Format(""{0} {1}"", 1, 2, 3)|};
        var d = {|#3:String.Format(""{0} {1} {2}"", 1, 2, 3, 4)|};
        var e = {|#4:string.Format(""{0} {0}"", 1, 2)|};

        var f = {|#5:String.Format(p, """", 1)|};
        var g = {|#6:String.Format(p, ""{0}"", 1, 2)|};
        var h = {|#7:String.Format(p, ""{0} {1}"", 1, 2, 3)|};
        var i = {|#8:String.Format(p, ""{0} {1} {2}"", 1, 2, 3, 4)|};
        var j = {|#9:String.Format(p, ""{0} {0}"", 1, 2)|};
    }
}
",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2),
            VerifyCS.Diagnostic().WithLocation(3),
            VerifyCS.Diagnostic().WithLocation(4),
            VerifyCS.Diagnostic().WithLocation(5),
            VerifyCS.Diagnostic().WithLocation(6),
            VerifyCS.Diagnostic().WithLocation(7),
            VerifyCS.Diagnostic().WithLocation(8),
            VerifyCS.Diagnostic().WithLocation(9));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(p As IFormatProvider)
        Dim a = {|#0:String.Format("""", 1)|}
        Dim b = {|#1:String.Format(""{0}"", 1, 2)|}
        Dim c = {|#2:String.Format(""{0} {1}"", 1, 2, 3)|}
        Dim d = {|#3:String.Format(""{0} {1} {2}"", 1, 2, 3, 4)|}
        Dim e = {|#4:string.Format(""{0} {0}"", 1, 2)|}

        Dim f = {|#5:String.Format(p, """", 1)|}
        Dim g = {|#6:String.Format(p, ""{0}"", 1, 2)|}
        Dim h = {|#7:String.Format(p, ""{0} {1}"", 1, 2, 3)|}
        Dim i = {|#8:String.Format(p, ""{0} {1} {2}"", 1, 2, 3, 4)|}
        Dim j = {|#9:String.Format(p, ""{0} {0}"", 1, 2)|}
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2),
            VerifyVB.Diagnostic().WithLocation(3),
            VerifyVB.Diagnostic().WithLocation(4),
            VerifyVB.Diagnostic().WithLocation(5),
            VerifyVB.Diagnostic().WithLocation(6),
            VerifyVB.Diagnostic().WithLocation(7),
            VerifyVB.Diagnostic().WithLocation(8),
            VerifyVB.Diagnostic().WithLocation(9));
        }

        [Theory]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_LessArgsThanFormatItem_VoidMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"(""{0} {1}"", 1)|};
        {|#1:" + invocation + @"(""{0} {1} {2}"", 1)|};
        {|#2:" + invocation + @"(""{0} {1} {2}"", 1, 2)|};

        // These cases are not handled as we cannot find a parameter named format
        " + invocation + @"(""{0}"");
        " + invocation + @"(""{0} {1}"");
    }
}
",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"(""{0} {1}"", 1)|}
        {|#1:" + invocation + @"(""{0} {1} {2}"", 1)|}
        {|#2:" + invocation + @"(""{0} {1} {2}"", 1, 2)|}

        ' These cases are not handled as we cannot find a parameter named format
        " + invocation + @"(""{0}"")
        " + invocation + @"(""{0} {1}"")
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2));
        }

        [Fact]
        public async Task CA2241_LessArgsThanFormatItem_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider p)
    {
        var a = {|#0:String.Format(""{0}"")|};
        var b = {|#1:String.Format(""{0} {1}"", 1)|};
        var c = {|#2:String.Format(""{0} {1} {2}"", 1)|};
        var d = {|#3:String.Format(""{0} {1} {2}"", 1, 2)|};
        var e = {|#4:String.Format(""{0} {0}"")|};

        var f = {|#5:String.Format(p, ""{0}"")|};
        var g = {|#6:String.Format(p, ""{0} {1}"", 1)|};
        var h = {|#7:String.Format(p, ""{0} {1} {2}"", 1)|};
        var i = {|#8:String.Format(p, ""{0} {1} {2}"", 1, 2)|};
        var j = {|#9:String.Format(p, ""{0} {0}"")|};
    }
}
",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2),
            VerifyCS.Diagnostic().WithLocation(3),
            VerifyCS.Diagnostic().WithLocation(4),
            VerifyCS.Diagnostic().WithLocation(5),
            VerifyCS.Diagnostic().WithLocation(6),
            VerifyCS.Diagnostic().WithLocation(7),
            VerifyCS.Diagnostic().WithLocation(8),
            VerifyCS.Diagnostic().WithLocation(9));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(p As IFormatProvider)
        Dim a = {|#0:String.Format(""{0}"")|}
        Dim b = {|#1:String.Format(""{0} {1}"", 1)|}
        Dim c = {|#2:String.Format(""{0} {1} {2}"", 1)|}
        Dim d = {|#3:String.Format(""{0} {1} {2}"", 1, 2)|}
        Dim e = {|#4:String.Format(""{0} {0}"")|}

        Dim f = {|#5:String.Format(p, ""{0}"")|}
        Dim g = {|#6:String.Format(p, ""{0} {1}"", 1)|}
        Dim h = {|#7:String.Format(p, ""{0} {1} {2}"", 1)|}
        Dim i = {|#8:String.Format(p, ""{0} {1} {2}"", 1, 2)|}
        Dim j = {|#9:String.Format(p, ""{0} {0}"")|}
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2),
            VerifyVB.Diagnostic().WithLocation(3),
            VerifyVB.Diagnostic().WithLocation(4),
            VerifyVB.Diagnostic().WithLocation(5),
            VerifyVB.Diagnostic().WithLocation(6),
            VerifyVB.Diagnostic().WithLocation(7),
            VerifyVB.Diagnostic().WithLocation(8),
            VerifyVB.Diagnostic().WithLocation(9));
        }

        [Fact]
        public async Task CA2241_ValidInvocation_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        var a = String.Format(""{0}"", 1);
        var b = String.Format(""{0} {1}"", 1, 2);
        var c = String.Format(""{0} {1} {2}"", 1, 2, 3);
        var d = String.Format(""{0} {1} {2} {3}"", 1, 2, 3, 4);
        var e = String.Format(""{0} {1} {2} {0}"", 1, 2, 3);
        var f = String.Format(""abc"");

        Console.Write(""{0}"", 1);
        Console.Write(""{0} {1}"", 1, 2);
        Console.Write(""{0} {1} {2}"", 1, 2, 3);
        Console.Write(""{0} {1} {2} {3}"", 1, 2, 3, 4);
        Console.Write(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, 5);
        Console.Write(""{0} {1} {2} {3} {0}"", 1, 2, 3, 4);

        Console.WriteLine(""{0}"", 1);
        Console.WriteLine(""{0} {1}"", 1, 2);
        Console.WriteLine(""{0} {1} {2}"", 1, 2, 3);
        Console.WriteLine(""{0} {1} {2} {3}"", 1, 2, 3, 4);
        Console.WriteLine(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, 5);
        Console.WriteLine(""{0} {1} {2} {3} {0}"", 1, 2, 3, 4);
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        Dim a = String.Format(""{0}"", 1)
        Dim b = String.Format(""{0} {1}"", 1, 2)
        Dim c = String.Format(""{0} {1} {2}"", 1, 2, 3)
        Dim d = String.Format(""{0} {1} {2} {3}"", 1, 2, 3, 4)
        Dim e = String.Format(""{0} {1} {2} {0}"", 1, 2, 3)
        Dim f = String.Format(""abc"")

        Console.Write(""{0}"", 1)
        Console.Write(""{0} {1}"", 1, 2)
        Console.Write(""{0} {1} {2}"", 1, 2, 3)
        Console.Write(""{0} {1} {2} {3}"", 1, 2, 3, 4)
        Console.Write(""{0} {1} {2} {0}"", 1, 2, 3)

        Console.WriteLine(""{0}"", 1)
        Console.WriteLine(""{0} {1}"", 1, 2)
        Console.WriteLine(""{0} {1} {2}"", 1, 2, 3)
        Console.WriteLine(""{0} {1} {2} {3}"", 1, 2, 3, 4)
        Console.WriteLine(""{0} {1} {2} {0}"", 1, 2, 3)
    End Sub
End Class");
        }

        [Fact]
        public async Task CA2241_ExplicitObjectArray()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Diag()
    {
        var s = {|#0:String.Format(""{0} {1} {2}"", new object[] { 1, 2, 3, 4 })|};
        {|#1:Console.Write(""{0} {1} {2}"", new object[] { 1, 2, 3, 4 })|};
        {|#2:Console.WriteLine(""{0} {1} {2}"", new object[] { 1, 2, 3, 4 })|};
    }

    void NoDiag()
    {
        var s = String.Format(""{0} {1} {2}"", new object[] { 1, 2, 3 });
        Console.Write(""{0} {1} {2}"", new object[] { 1, 2, 3 });
        Console.WriteLine(""{0} {1} {2}"", new object[] { 1, 2, 3 });
    }
}
",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Diag()
        Dim s = {|#0:String.Format(""{0} {1} {2}"", New Object() { 1, 2, 3, 4 })|}
        {|#1:Console.Write(""{0} {1} {2}"", New Object() { 1, 2, 3, 4 })|}
        {|#2:Console.WriteLine(""{0} {1} {2}"", New Object() { 1, 2, 3, 4 })|}
    End Sub

    Sub NoDiag()
        Dim s = String.Format(""{0} {1} {2}"", New Object() { 1, 2, 3 })
        Console.Write(""{0} {1} {2}"", New Object() { 1, 2, 3 })
        Console.WriteLine(""{0} {1} {2}"", New Object() { 1, 2, 3 })
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2));
        }

        [Fact]
        public async Task CA2241_VarArgsNotSupported()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default,
                TestCode = @"
using System;

public class C
{
    void Method()
    {
        Console.Write(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, __arglist(5));
        Console.WriteLine(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, __arglist(5));
    }
}
",
            }.RunAsync();
        }

        [Fact]
        public async Task CA2241_FormatStringParser_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        var a = String.Format(""{0,-4 :xd}"", 1);
        var b = String.Format(""{0   ,    5 : d} {1}"", 1, 2);
        var c = String.Format(""{0:d} {1} {2}"", 1, 2, 3);
        var d = String.Format(""{0, 5} {1} {2} {3}"", 1, 2, 3, 4);

        Console.Write(""{0,1}"", 1);
        Console.Write(""{0:   x} {1}"", 1, 2);
        Console.Write(""{{escape}}{0} {1} {2}"", 1, 2, 3);
        Console.Write(""{0: {{escape}} x} {1} {2} {3}"", 1, 2, 3, 4);
        Console.Write(""{0 , -10  :   {{escape}}  y} {1} {2} {3} {4}"", 1, 2, 3, 4, 5);
    }
}
");
        }

        [Theory, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_MissingStringFormatItem_VoidMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"(""{0} {2}"", 1)|};
        {|#1:" + invocation + @"(""{0} {2}"", 1, 2)|};
        {|#2:" + invocation + @"(""{0} {2}"", 1, 2, 3)|};
        {|#3:" + invocation + @"(""{2} {0}"", 1)|};
        {|#4:" + invocation + @"(""{0} {2} {0} {2}"", 1)|};
        {|#5:" + invocation + @"(""{0} {2} {4} {5}"", 1)|};
    }
}",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2),
            VerifyCS.Diagnostic().WithLocation(3),
            VerifyCS.Diagnostic().WithLocation(4),
            VerifyCS.Diagnostic().WithLocation(5));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"(""{0} {2}"", 1)|}
        {|#1:" + invocation + @"(""{0} {2}"", 1, 2)|}
        {|#2:" + invocation + @"(""{0} {2}"", 1, 2, 3)|}
        {|#3:" + invocation + @"(""{2} {0}"", 1)|}
        {|#4:" + invocation + @"(""{0} {2} {0} {2}"", 1)|}
        {|#5:" + invocation + @"(""{0} {2} {4} {5}"", 1)|}
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2),
            VerifyVB.Diagnostic().WithLocation(3),
            VerifyVB.Diagnostic().WithLocation(4),
            VerifyVB.Diagnostic().WithLocation(5));
        }

        [Fact, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        public async Task CA2241_MissingStringFormatItem_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider p)
    {
        {|#0:string.Format(""{0} {2}"", 1)|};
        {|#1:string.Format(""{0} {2}"", 1, 2)|};
        {|#2:string.Format(""{0} {2}"", 1, 2, 3)|};
        {|#3:string.Format(""{2} {0}"", 1)|};
        {|#4:string.Format(""{0} {2} {0} {2}"", 1)|};
        {|#5:string.Format(""{0} {2} {4} {5}"", 1)|};

        {|#6:string.Format(p, ""{0} {2}"", 1)|};
        {|#7:string.Format(p, ""{0} {2}"", 1, 2)|};
        {|#8:string.Format(p, ""{0} {2}"", 1, 2, 3)|};
        {|#9:string.Format(p, ""{2} {0}"", 1)|};
        {|#10:string.Format(p, ""{0} {2} {0} {2}"", 1)|};
        {|#11:string.Format(p, ""{0} {2} {4} {5}"", 1)|};
    }
}",
            VerifyCS.Diagnostic().WithLocation(0),
            VerifyCS.Diagnostic().WithLocation(1),
            VerifyCS.Diagnostic().WithLocation(2),
            VerifyCS.Diagnostic().WithLocation(3),
            VerifyCS.Diagnostic().WithLocation(4),
            VerifyCS.Diagnostic().WithLocation(5),
            VerifyCS.Diagnostic().WithLocation(6),
            VerifyCS.Diagnostic().WithLocation(7),
            VerifyCS.Diagnostic().WithLocation(8),
            VerifyCS.Diagnostic().WithLocation(9),
            VerifyCS.Diagnostic().WithLocation(10),
            VerifyCS.Diagnostic().WithLocation(11));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(p As IFormatProvider)
        {|#0:string.Format(""{0} {2}"", 1)|}
        {|#1:string.Format(""{0} {2}"", 1, 2)|}
        {|#2:string.Format(""{0} {2}"", 1, 2, 3)|}
        {|#3:string.Format(""{2} {0}"", 1)|}
        {|#4:string.Format(""{0} {2} {0} {2}"", 1)|}
        {|#5:string.Format(""{0} {2} {4} {5}"", 1)|}

        {|#6:string.Format(p, ""{0} {2}"", 1)|}
        {|#7:string.Format(p, ""{0} {2}"", 1, 2)|}
        {|#8:string.Format(p, ""{0} {2}"", 1, 2, 3)|}
        {|#9:string.Format(p, ""{2} {0}"", 1)|}
        {|#10:string.Format(p, ""{0} {2} {0} {2}"", 1)|}
        {|#11:string.Format(p, ""{0} {2} {4} {5}"", 1)|}
    End Sub
End Class",
            VerifyVB.Diagnostic().WithLocation(0),
            VerifyVB.Diagnostic().WithLocation(1),
            VerifyVB.Diagnostic().WithLocation(2),
            VerifyVB.Diagnostic().WithLocation(3),
            VerifyVB.Diagnostic().WithLocation(4),
            VerifyVB.Diagnostic().WithLocation(5),
            VerifyVB.Diagnostic().WithLocation(6),
            VerifyVB.Diagnostic().WithLocation(7),
            VerifyVB.Diagnostic().WithLocation(8),
            VerifyVB.Diagnostic().WithLocation(9),
            VerifyVB.Diagnostic().WithLocation(10),
            VerifyVB.Diagnostic().WithLocation(11));
        }

        [Fact]
        public async Task CA2241_VarArgsNotSupported_ButStillCatchesMissingStringFormatItems()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default,
                TestCode = @"
using System;

public class C
{
    void Method()
    {
        {|#0:Console.Write(""{0} {1} {2} {2} {4}"", 1, 2, 3, 4, __arglist(5))|};
        {|#1:Console.WriteLine(""{0} {1} {2} {2} {4}"", 1, 2, 3, 4, __arglist(5))|};
    }
}
",
                ExpectedDiagnostics =
                {
                    VerifyCS.Diagnostic().WithLocation(0),
                    VerifyCS.Diagnostic().WithLocation(1),
                },
            }.RunAsync();
        }

        [Theory]
        [WorkItem(2799, "https://github.com/dotnet/roslyn-analyzers/issues/2799")]
        // No configuration - validate no diagnostics in default configuration
        [InlineData(null)]
        // Configured but disabled
        [InlineData(false)]
        // Configured and enabled
        [InlineData(true)]
        public async Task CA2241_EditorConfigConfiguration_HeuristicAdditionalStringFormattingMethods(bool? editorConfig)
        {
            string editorConfigText = editorConfig == null ? string.Empty :
                "dotnet_code_quality.try_determine_additional_string_formatting_methods_automatically = " + editorConfig.Value;

            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
class Test
{
    public static string MyFormat(string format, params object[] args) => format;

    void M1(string param)
    {
        var a = MyFormat("""", 1);
    }
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfig == true)
            {
                csharpTest.ExpectedDiagnostics.Add(
                    // Test0.cs(8,17): warning CA2241: Provide correct arguments to formatting methods
                    VerifyCS.Diagnostic().WithLocation(8, 17));
            }

            await csharpTest.RunAsync();

            var basicTest = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Class Test
    Public Shared Function MyFormat(format As String, ParamArray args As Object()) As String
        Return format
    End Function

    Private Sub M1(ByVal param As String)
        Dim a = MyFormat("""", 1)
    End Sub
End Class"
},
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfig == true)
            {
                basicTest.ExpectedDiagnostics.Add(
                    // Test0.vb(8,17): warning CA2241: Provide correct arguments to formatting methods
                    VerifyVB.Diagnostic().WithLocation(8, 17));
            }

            await basicTest.RunAsync();
        }

        [Theory]
        [WorkItem(2799, "https://github.com/dotnet/roslyn-analyzers/issues/2799")]
        // No configuration - validate no diagnostics in default configuration
        [InlineData("")]
        // Match by method name
        [InlineData("dotnet_code_quality.additional_string_formatting_methods = MyFormat")]
        // Setting only for Rule ID
        [InlineData("dotnet_code_quality." + ProvideCorrectArgumentsToFormattingMethodsAnalyzer.RuleId + ".additional_string_formatting_methods = MyFormat")]
        // Match by documentation ID without "M:" prefix
        [InlineData("dotnet_code_quality.additional_string_formatting_methods = Test.MyFormat(System.String,System.Object[])~System.String")]
        // Match by documentation ID with "M:" prefix
        [InlineData("dotnet_code_quality.additional_string_formatting_methods = M:Test.MyFormat(System.String,System.Object[])~System.String")]
        public async Task CA2241_EditorConfigConfiguration_AdditionalStringFormattingMethods(string editorConfigText)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
class Test
{
    public static string MyFormat(string format, params object[] args) => format;

    void M1(string param)
    {
        var a = MyFormat("""", 1);
    }
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfigText.Length > 0)
            {
                csharpTest.ExpectedDiagnostics.Add(
                    // Test0.cs(8,17): warning CA2241: Provide correct arguments to formatting methods
                    VerifyCS.Diagnostic().WithLocation(8, 17));
            }

            await csharpTest.RunAsync();

            var basicTest = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Class Test
    Public Shared Function MyFormat(format As String, ParamArray args As Object()) As String
        Return format
    End Function

    Private Sub M1(ByVal param As String)
        Dim a = MyFormat("""", 1)
    End Sub
End Class"
},
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfigText.Length > 0)
            {
                basicTest.ExpectedDiagnostics.Add(
                    // Test0.vb(8,17): warning CA2241: Provide correct arguments to formatting methods
                    VerifyVB.Diagnostic().WithLocation(8, 17));
            }

            await basicTest.RunAsync();
        }
    }
}