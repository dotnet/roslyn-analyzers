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
        public async Task CA2241_TooManyArgs_ConsoleWriteMethods_Diagnostic(string invocation)
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
        {|#4:" + invocation + @"(""{0} {1} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|};
        {|#5:" + invocation + @"(""{2} {0} {1}"", 1, 2, 3, 4)|};
        {|#6:" + invocation + @"(""{0} {0}"", 1, 2)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(5),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(6));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"("""", 1)|}
        {|#1:" + invocation + @"(""{0}"", 1, 2)|}
        {|#2:" + invocation + @"(""{0} {1}"", 1, 2, 3)|}
        {|#3:" + invocation + @"(""{0} {1} {2}"", 1, 2, 3, 4)|}
        {|#4:" + invocation + @"(""{0} {1} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|}
        {|#5:" + invocation + @"(""{2} {0} {1}"", 1, 2, 3, 4)|}
        {|#6:" + invocation + @"(""{0} {0}"", 1, 2)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(5),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(6));
        }

        [Theory, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_TooManyArgsMissingFormatIndex_ConsoleWriteMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"(""{1}"", 1, 2, 3)|};
        {|#1:" + invocation + @"(""{2}"", 1, 2, 3, 4)|};
        {|#2:" + invocation + @"(""{0} {2}"", 1, 2, 3, 4)|};
        {|#3:" + invocation + @"(""{0} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|};
        {|#4:" + invocation + @"(""{0} {2} {3} {5}"", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)|};
        {|#5:" + invocation + @"(""{5} {2} {3} {1}"", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(5));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"(""{1}"", 1, 2, 3)|}
        {|#1:" + invocation + @"(""{2}"", 1, 2, 3, 4)|}
        {|#2:" + invocation + @"(""{0} {2}"", 1, 2, 3, 4)|}
        {|#3:" + invocation + @"(""{0} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|}
        {|#4:" + invocation + @"(""{0} {2} {3} {5}"", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)|}
        {|#5:" + invocation + @"(""{5} {2} {3} {1}"", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(5));
        }

        [Fact]
        public async Task CA2241_TooManyArgs_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider formatProvider)
    {
        // Too many args
        var a = {|#0:String.Format("""", 1)|};
        var b = {|#1:String.Format(""{0}"", 1, 2)|};
        var c = {|#2:String.Format(""{0} {1}"", 1, 2, 3)|};
        var d = {|#3:String.Format(""{0} {1} {2}"", 1, 2, 3, 4)|};
        var e = {|#4:String.Format(""{0} {1} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|};
        var f = {|#5:string.Format(""{0} {0}"", 1, 2)|};

        // Too many args with format provider
        var g = {|#6:String.Format(formatProvider, """", 1)|};
        var h = {|#7:String.Format(formatProvider, ""{0}"", 1, 2)|};
        var i = {|#8:String.Format(formatProvider, ""{0} {1}"", 1, 2, 3)|};
        var j = {|#9:String.Format(formatProvider, ""{0} {1} {2}"", 1, 2, 3, 4)|};
        var k = {|#10:String.Format(formatProvider, ""{0} {0}"", 1, 2)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(5),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(6),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(7),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(8),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(9),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(10));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(formatProvider As IFormatProvider)
        ' Too many args
        Dim a = {|#0:String.Format("""", 1)|}
        Dim b = {|#1:String.Format(""{0}"", 1, 2)|}
        Dim c = {|#2:String.Format(""{0} {1}"", 1, 2, 3)|}
        Dim d = {|#3:String.Format(""{0} {1} {2}"", 1, 2, 3, 4)|}
        Dim e = {|#4:String.Format(""{0} {1} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|}
        Dim f = {|#5:string.Format(""{0} {0}"", 1, 2)|}

        ' Too many args with format provider
        Dim g = {|#6:String.Format(formatProvider, """", 1)|}
        Dim h = {|#7:String.Format(formatProvider, ""{0}"", 1, 2)|}
        Dim i = {|#8:String.Format(formatProvider, ""{0} {1}"", 1, 2, 3)|}
        Dim j = {|#9:String.Format(formatProvider, ""{0} {1} {2}"", 1, 2, 3, 4)|}
        Dim k = {|#10:String.Format(formatProvider, ""{0} {0}"", 1, 2)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(5),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(6),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(7),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(8),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(9),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(10));
        }

        [Fact, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        public async Task CA2241_TooManyArgsMissingFormatIndex_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider formatProvider)
    {
        // Too many args + missing format index
        var a = {|#0:string.Format(""{1}"", 1, 2, 3)|};
        var b = {|#1:string.Format(""{2}"", 1, 2, 3, 4)|};
        var c = {|#2:string.Format(""{0} {2}"", 1, 2, 3, 4)|};
        var d = {|#3:string.Format(""{0} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|};
        var e = {|#4:string.Format(""{0} {2} {3} {5}"", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)|};

        // Too many args with format provider + missing format index
        var f = {|#5:string.Format(formatProvider, ""{1}"", 1, 2, 3)|};
        var g = {|#6:string.Format(formatProvider, ""{2}"", 1, 2, 3, 4)|};
        var h = {|#7:string.Format(formatProvider, ""{0} {2}"", 1, 2, 3, 4)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(5),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(6),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(7));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(formatProvider As IFormatProvider)
        ' Too many args + missing format index
        Dim a = {|#0:string.Format(""{1}"", 1, 2, 3)|}
        Dim b = {|#1:string.Format(""{2}"", 1, 2, 3, 4)|}
        Dim c = {|#2:string.Format(""{0} {2}"", 1, 2, 3, 4)|}
        Dim d = {|#3:string.Format(""{0} {2}"", 1, 2, 3, 4, 5, 6, 7, 8)|}
        Dim e = {|#4:string.Format(""{0} {2} {3} {5}"", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)|}

        ' Too many args with format provider + missing format index
        Dim f = {|#5:string.Format(formatProvider, ""{1}"", 1, 2, 3)|}
        Dim g = {|#6:string.Format(formatProvider, ""{2}"", 1, 2, 3, 4)|}
        Dim h = {|#7:string.Format(formatProvider, ""{0} {2}"", 1, 2, 3, 4)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(5),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(6),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(7));
        }

        [Theory]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_NotEnoughArgs_ConsoleWriteMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"(""{0} {1}"", 1)|};
        {|#1:" + invocation + @"(""{0} {1} {2}"", 1, 2)|};
        {|#2:" + invocation + @"(""{0} {1} {2}"", 1)|};
        {|#3:" + invocation + @"(""{2} {0} {1}"", 1)|};
        {|#4:" + invocation + @"(""{1} {0} {1}"", 1)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(4));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"(""{0} {1}"", 1)|}
        {|#1:" + invocation + @"(""{0} {1} {2}"", 1, 2)|}
        {|#2:" + invocation + @"(""{0} {1} {2}"", 1)|}
        {|#3:" + invocation + @"(""{2} {0} {1}"", 1)|}
        {|#4:" + invocation + @"(""{1} {0} {1}"", 1)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(4));
        }

        [Theory, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_NotEnoughArgsMissingFormatIndexRule_ConsoleWriteMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"(""{1}"", 1)|};
        {|#1:" + invocation + @"(""{1} {2}"", 1, 2)|};
        {|#2:" + invocation + @"(""{2} {0}"", 1, 2)|};
        {|#3:" + invocation + @"(""{4} {1} {2}"", 1, 2, 3)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"(""{1}"", 1)|}
        {|#1:" + invocation + @"(""{1} {2}"", 1, 2)|}
        {|#2:" + invocation + @"(""{2} {0}"", 1, 2)|}
        {|#3:" + invocation + @"(""{4} {1} {2}"", 1, 2, 3)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3));
        }

        [Fact]
        public async Task CA2241_NotEnoughArgs_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider formatProvider)
    {
        // Not enough args
        var a = {|#0:String.Format(""{0}"")|};
        var b = {|#1:String.Format(""{0} {1}"", 1)|};
        var c = {|#2:String.Format(""{0} {1} {2}"", 1, 2)|};
        var d = {|#3:String.Format(""{0} {1} {2}"", 1)|};
        var e = {|#4:String.Format(""{2} {0} {1}"", 1)|};
        var f = {|#5:String.Format(""{1} {0} {1}"", 1)|};

        // Not enough args with format provider
        var g = {|#6:String.Format(formatProvider, ""{0}"")|};
        var h = {|#7:String.Format(formatProvider, ""{0} {1}"", 1)|};
        var i = {|#8:String.Format(formatProvider, ""{0} {1} {2}"", 1, 2)|};
        var j = {|#9:String.Format(formatProvider, ""{0} {1} {2}"", 1)|};
        var k = {|#10:String.Format(formatProvider, ""{2} {0} {1}"", 1)|};
        var l = {|#11:String.Format(formatProvider, ""{1} {0} {1}"", 1)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(5),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(6),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(7),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(8),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(9),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(10),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(11));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(formatProvider As IFormatProvider)
        ' Not enough args
        Dim a = {|#0:String.Format(""{0}"")|}
        Dim b = {|#1:String.Format(""{0} {1}"", 1)|}
        Dim c = {|#2:String.Format(""{0} {1} {2}"", 1, 2)|}
        Dim d = {|#3:String.Format(""{0} {1} {2}"", 1)|}
        Dim e = {|#4:String.Format(""{2} {0} {1}"", 1)|}
        Dim f = {|#5:String.Format(""{1} {0} {1}"", 1)|}

        ' Not enough args with format provider
        Dim g = {|#6:String.Format(formatProvider, ""{0}"")|}
        Dim h = {|#7:String.Format(formatProvider, ""{0} {1}"", 1)|}
        Dim i = {|#8:String.Format(formatProvider, ""{0} {1} {2}"", 1, 2)|}
        Dim j = {|#9:String.Format(formatProvider, ""{0} {1} {2}"", 1)|}
        Dim k = {|#10:String.Format(formatProvider, ""{2} {0} {1}"", 1)|}
        Dim l = {|#11:String.Format(formatProvider, ""{1} {0} {1}"", 1)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(5),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(6),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(7),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(8),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(9),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(10),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(11));
        }

        [Fact, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        public async Task CA2241_NotEnoughArgsMissingFormatIndex_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider formatProvider)
    {
        // Not enough args + missing format index
        var a = {|#0:String.Format(""{1}"")|};
        var b = {|#1:String.Format(""{1}"", 1)|};
        var c = {|#2:String.Format(""{2}"", 1, 2)|};
        var d = {|#3:String.Format(""{2} {0}"", 1)|};

        // Not enough args with format provider + missing format index
        var e = {|#4:String.Format(formatProvider, ""{1}"")|};
        var f = {|#5:String.Format(formatProvider, ""{1}"", 1)|};
        var g = {|#6:String.Format(formatProvider, ""{2}"", 1, 2)|};
        var h = {|#7:String.Format(formatProvider, ""{2} {0}"", 1)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(5),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(6),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(7));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(formatProvider As IFormatProvider)
        ' Not enough args + missing format index
        Dim a = {|#0:String.Format(""{1}"")|}
        Dim b = {|#1:String.Format(""{1}"", 1)|}
        Dim c = {|#2:String.Format(""{2}"", 1, 2)|}
        Dim d = {|#3:String.Format(""{2} {0}"", 1)|}

        ' Not enough args with format provider + missing format index
        Dim e = {|#4:String.Format(formatProvider, ""{1}"")|}
        Dim f = {|#5:String.Format(formatProvider, ""{1}"", 1)|}
        Dim g = {|#6:String.Format(formatProvider, ""{2}"", 1, 2)|}
        Dim h = {|#7:String.Format(formatProvider, ""{2} {0}"", 1)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(5),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(6),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(7));
        }

        [Theory, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_EnoughArgsMissingFormatIndex_ConsoleWriteMethods_Diagnostic(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method()
    {
        {|#0:" + invocation + @"(""{1}"", 1, 2)|};
        {|#1:" + invocation + @"(""{1} {2}"", 1, 2, 3)|};
        {|#2:" + invocation + @"(""{2} {0}"", 1, 2, 3)|};
        {|#3:" + invocation + @"(""{4} {1} {2}"", 1, 2, 3, 4, 5)|};
        {|#4:" + invocation + @"(""{0} {2} {0} {2}"", 1, 2, 3)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method()
        {|#0:" + invocation + @"(""{1}"", 1, 2)|}
        {|#1:" + invocation + @"(""{1} {2}"", 1, 2, 3)|}
        {|#2:" + invocation + @"(""{2} {0}"", 1, 2, 3)|}
        {|#3:" + invocation + @"(""{4} {1} {2}"", 1, 2, 3, 4, 5)|}
        {|#4:" + invocation + @"(""{0} {2} {0} {2}"", 1, 2, 3)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4));
        }

        [Fact, WorkItem(1254, "https://github.com/dotnet/roslyn-analyzers/issues/1254")]
        public async Task CA2241_EnoughArgsMissingFormatIndex_StringFormatMethods_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Method(IFormatProvider formatProvider)
    {
        var a = {|#0:string.Format(""{1}"", 1, 2)|};
        var b = {|#1:string.Format(""{1} {2}"", 1, 2, 3)|};
        var c = {|#2:string.Format(""{2} {0}"", 1, 2, 3)|};
        var d = {|#3:string.Format(""{4} {1} {2}"", 1, 2, 3, 4, 5)|};
        var e = {|#4:string.Format(""{0} {2} {0} {2}"", 1, 2, 3)|};

        var f = {|#5:string.Format(formatProvider, ""{1}"", 1, 2)|};
        var g = {|#6:string.Format(formatProvider, ""{1} {2}"", 1, 2, 3)|};
        var h = {|#7:string.Format(formatProvider, ""{2} {0}"", 1, 2, 3)|};
        var i = {|#8:string.Format(formatProvider, ""{4} {1} {2}"", 1, 2, 3, 4, 5)|};
        var j = {|#9:string.Format(formatProvider, ""{0} {2} {0} {2}"", 1, 2, 3)|};
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(5),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(6),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(7),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(8),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(9));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Method(formatProvider As IFormatProvider)
        Dim a = {|#0:string.Format(""{1}"", 1, 2)|}
        Dim b = {|#1:string.Format(""{1} {2}"", 1, 2, 3)|}
        Dim c = {|#2:string.Format(""{2} {0}"", 1, 2, 3)|}
        Dim d = {|#3:string.Format(""{4} {1} {2}"", 1, 2, 3, 4, 5)|}
        Dim e = {|#4:string.Format(""{0} {2} {0} {2}"", 1, 2, 3)|}

        Dim f = {|#5:string.Format(formatProvider, ""{1}"", 1, 2)|}
        Dim g = {|#6:string.Format(formatProvider, ""{1} {2}"", 1, 2, 3)|}
        Dim h = {|#7:string.Format(formatProvider, ""{2} {0}"", 1, 2, 3)|}
        Dim i = {|#8:string.Format(formatProvider, ""{4} {1} {2}"", 1, 2, 3, 4, 5)|}
        Dim j = {|#9:string.Format(formatProvider, ""{0} {2} {0} {2}"", 1, 2, 3)|}
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(5),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(6),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(7),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(8),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(9));
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

        [Theory]
        [InlineData("Console.Write")]
        [InlineData("Console.WriteLine")]
        public async Task CA2241_ExplicitObjectArray_ConsoleWriteMethods(string invocation)
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Diag()
    {
        // Too many args
        {|#0:" + invocation + @"(""{0} {1} {2}"", new object[] { 1, 2, 3, 4 })|};
        // Too many args + missing format index
        {|#1:" + invocation + @"(""{0} {2}"", new object[] { 1, 2, 3, 4 })|};
        // Not enough args
        {|#2:" + invocation + @"(""{0} {1} {2}"", new object[] { 1, 2 })|};
        // Not enough args + missing format index
        {|#3:" + invocation + @"(""{0} {2}"", new object[] { 1, 2 })|};
        // Enough args but missing format index
        {|#4:" + invocation + @"(""{0} {2}"", new object[] { 1, 2, 3 })|};
    }

    void NoDiag()
    {
        " + invocation + @"(""{0} {1} {2}"", new object[] { 1, 2, 3 });
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Diag()
        ' Too many args
        {|#0:" + invocation + @"(""{0} {1} {2}"", New Object() { 1, 2, 3, 4 })|}
        ' Too many args + missing format index
        {|#1:" + invocation + @"(""{0} {2}"", New Object() { 1, 2, 3, 4 })|}
        ' Not enough args
        {|#2:" + invocation + @"(""{0} {1} {2}"", New Object() { 1, 2 })|}
        ' Not enough args + missing format index
        {|#3:" + invocation + @"(""{0} {2}"", New Object() { 1, 2 })|}
        ' Enough args but missing format index
        {|#4:" + invocation + @"(""{0} {2}"", New Object() { 1, 2, 3 })|}
    End Sub

    Sub NoDiag()
        " + invocation + @"(""{0} {1} {2}"", New Object() { 1, 2, 3 })
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4));
        }

        [Fact]
        public async Task CA2241_ExplicitObjectArray_StringFormatMethods()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    void Diag()
    {
        // Too many args
        var a = {|#0:string.Format(""{0} {1} {2}"", new object[] { 1, 2, 3, 4 })|};
        // Too many args + missing format index
        var b = {|#1:string.Format(""{0} {2}"", new object[] { 1, 2, 3, 4 })|};
        // Not enough args
        var c = {|#2:string.Format(""{0} {1} {2}"", new object[] { 1, 2 })|};
        // Not enough args + missing format index
        var d = {|#3:string.Format(""{0} {2}"", new object[] { 1, 2 })|};
        // Enough args but missing format index
        var e = {|#4:string.Format(""{0} {2}"", new object[] { 1, 2, 3 })|};
    }

    void NoDiag()
    {
        var s = String.Format(""{0} {1} {2}"", new object[] { 1, 2, 3 });
    }
}
",
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    Sub Diag()
        ' Too many args
        Dim a = {|#0:string.Format(""{0} {1} {2}"", New Object() { 1, 2, 3, 4 })|}
        ' Too many args + missing format index
        Dim b = {|#1:string.Format(""{0} {2}"", New Object() { 1, 2, 3, 4 })|}
        ' Not enough args
        Dim c = {|#2:string.Format(""{0} {1} {2}"", New Object() { 1, 2 })|}
        ' Not enough args + missing format index
        Dim d = {|#3:string.Format(""{0} {2}"", New Object() { 1, 2 })|}
        ' Enough args but missing format index
        Dim e = {|#4:string.Format(""{0} {2}"", New Object() { 1, 2, 3 })|}
    End Sub

    Sub NoDiag()
        Dim s = String.Format(""{0} {1} {2}"", New Object() { 1, 2, 3 })
    End Sub
End Class",
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(0),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(1),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(2),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(3),
            VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(4));
        }

        [Fact]
        public async Task CA2241_CSharp_VarArgsNotSupported()
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

                Console.Write(""{0} {1}"", 1, 2, 3, 4, __arglist(5));
                Console.WriteLine(""{0} {1}"", 1, 2, 3, 4, __arglist(5));

                Console.Write(""{0} {1} {2} {3} {4} {5} {6}"", 1, 2, 3, 4, __arglist(5));
                Console.WriteLine(""{0} {1} {2} {3} {4} {5} {6}"", 1, 2, 3, 4, __arglist(5));
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
        // Too many args
        var s1 = MyFormat(""{0}"", 1, 2);
        // Too many args and missing format index
        var s2 = MyFormat(""{1}"", 1, 2, 3);

        // Not enough args
        var s3 = MyFormat(""{0} {1}"", 1);
        // Not enough args and missing format index
        var s4 = MyFormat(""{0} {2}"", 1);

        // Enough args and missing format index
        var s5 = MyFormat(""{1}"", 1, 2);
    }
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfig == true)
            {
                csharpTest.ExpectedDiagnostics.AddRange(new[]
                {
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(9, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(11, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(14, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(16, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(19, 18),
                });
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
        ' Too many args
        Dim s1 = MyFormat(""{0}"", 1, 2)
        ' Too many args and missing format index
        Dim s2 = MyFormat(""{1}"", 1, 2, 3)

        ' Not enough args
        Dim s3 = MyFormat(""{0} {1}"", 1)
        ' Not enough args and missing format index
        Dim s4 = MyFormat(""{0} {2}"", 1)

        ' Enough args and missing format index
        Dim s5 = MyFormat(""{1}"", 1, 2)
    End Sub
End Class"
},
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfig == true)
            {
                basicTest.ExpectedDiagnostics.AddRange(new[]
                {
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(9, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(11, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(14, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(16, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(19, 18),
                });
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
        // Too many args
        var s1 = MyFormat(""{0}"", 1, 2);
        // Too many args and missing format index
        var s2 = MyFormat(""{1}"", 1, 2, 3);

        // Not enough args
        var s3 = MyFormat(""{0} {1}"", 1);
        // Not enough args and missing format index
        var s4 = MyFormat(""{0} {2}"", 1);

        // Enough args and missing format index
        var s5 = MyFormat(""{1}"", 1, 2);
    }
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfigText.Length > 0)
            {
                csharpTest.ExpectedDiagnostics.AddRange(new[]
                {
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(9, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(11, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(14, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(16, 18),
                    VerifyCS.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(19, 18),
                });
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
        ' Too many args
        Dim s1 = MyFormat(""{0}"", 1, 2)
        ' Too many args and missing format index
        Dim s2 = MyFormat(""{1}"", 1, 2, 3)

        ' Not enough args
        Dim s3 = MyFormat(""{0} {1}"", 1)
        ' Not enough args and missing format index
        Dim s4 = MyFormat(""{0} {2}"", 1)

        ' Enough args and missing format index
        Dim s5 = MyFormat(""{1}"", 1, 2)
    End Sub
End Class"
},
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (editorConfigText.Length > 0)
            {
                basicTest.ExpectedDiagnostics.AddRange(new[]
                {
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsRule).WithLocation(9, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.TooManyArgsMissingFormatIndexRule).WithLocation(11, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsRule).WithLocation(14, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.NotEnoughArgsMissingFormatIndexRule).WithLocation(16, 18),
                    VerifyVB.Diagnostic(ProvideCorrectArgumentsToFormattingMethodsAnalyzer.EnoughArgsMissingFormatIndexRule).WithLocation(19, 18),
                });
            }

            await basicTest.RunAsync();
        }
    }
}