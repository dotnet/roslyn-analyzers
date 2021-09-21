// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class AvoidConstArraysTests
    {
        [Fact]
        public async Task IdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine({|CA1850:new[]{ 1, 2, 3 }|});
    }
}
", @"
using System;

public class A
{
    private static readonly int[] value = new[]{ 1, 2, 3 };

    public void B()
    {
        Console.WriteLine(value);
    }
}
");

            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine({|CA1850:{1, 2, 3}|})
    End Sub
End Class
", @"
Imports System

Public Class A
    Private Shared ReadOnly value As Integer() = {1, 2, 3}
    Public Sub B()
        Console.WriteLine(value)
    End Sub
End Class
");

            // Explicit initialization check
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine({|CA1850:new int[]{ 1, 2, 3 }|});
    }
}
", @"
using System;

public class A
{
    private static readonly int[] value = new int[]{ 1, 2, 3 };

    public void B()
    {
        Console.WriteLine(value);
    }
}
");

            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine({|CA1850:New Integer() {1, 2, 3}|})
    End Sub
End Class
", @"
Imports System

Public Class A
    Private Shared ReadOnly value As Integer() = New Integer() {1, 2, 3}
    Public Sub B()
        Console.WriteLine(value)
    End Sub
End Class
");

            // Nested arguments
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(string.Join("" "", {|CA1850:new[] { ""Cake"", ""is"", ""good"" }|}));
    }
}
", @"
using System;

public class A
{
    private static readonly string[] value = new[] { ""Cake"", ""is"", ""good"" };

    public void B()
    {
        Console.WriteLine(string.Join("" "", value));
    }
}
");

            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(String.Join("" ""c, {|CA1850:{""Cake"", ""is"", ""good""}|}))
    End Sub
End Class
", @"
Imports System

Public Class A
    Private Shared ReadOnly value As String() = {""Cake"", ""is"", ""good""}
    Public Sub B()
        Console.WriteLine(String.Join("" ""c, value))
    End Sub
End Class
");

            // Trivia test, CS only
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(string.Join("" "", {|CA1850:new[] { ""a"", ""b"" }|} /* test comment */));
    }
}
", @"
using System;

public class A
{
    private static readonly string[] value = new[] { ""a"", ""b"" };

    public void B()
    {
        Console.WriteLine(string.Join("" "", value /* test comment */));
    }
}
");

            // Extension method usage
            await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Linq;

public class A
{
    public void B()
    {
        string y = {|CA1850:new[] { ""a"", ""b"", ""c"" }|}.First();
        Console.WriteLine(y);
    }
}
", @"
using System;
using System.Linq;

public class A
{
    private static readonly string[] sourceArray = new[] { ""a"", ""b"", ""c"" };

    public void B()
    {
        string y = sourceArray.First();
        Console.WriteLine(y);
    }
}
");

            await VerifyVB.VerifyCodeFixAsync(@"
Imports System
Imports System.Linq

Public Class A
    Public Sub B()
        Dim y As String = {|CA1850:{""a"", ""b"", ""c""}|}.First()
        Console.WriteLine(y)
    End Sub
End Class
", @"
Imports System
Imports System.Linq

Public Class A
    Private Shared ReadOnly stringArray As String() = {""a"", ""b"", ""c""}
    Public Sub B()
        Dim y As String = stringArray.First()
        Console.WriteLine(y)
    End Sub
End Class
");

            // Member extraction tests
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    private static readonly string value = ""hello"";
    private static readonly int[] valueArray = new[]{ -2, -1, 0 };
    private static readonly bool[] valueArray1 = new[]{ true, false, true };

    private static readonly int x = 1;

    public void B()
    {
        Console.WriteLine({|CA1850:new[]{ 1, 2, 3 }|});
    }
}
", @"
using System;

public class A
{
    private static readonly string value = ""hello"";
    private static readonly int[] valueArray = new[]{ -2, -1, 0 };
    private static readonly bool[] valueArray1 = new[]{ true, false, true };

    private static readonly int x = 1;
    private static readonly int[] valueArray0 = new[]{ 1, 2, 3 };

    public void B()
    {
        Console.WriteLine(valueArray0);
    }
}
");

            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Private Shared ReadOnly value As String = ""hello""
    Private Shared ReadOnly valueArray As Integer() = {-2, -1, 0}
    Private Shared ReadOnly valueArray1 As Boolean() = {True, False, True}
    Private Shared ReadOnly x As Integer = 1

    Public Sub B()
        Console.WriteLine({|CA1850:{1, 2, 3}|})
    End Sub
End Class
", @"
Imports System

Public Class A
    Private Shared ReadOnly value As String = ""hello""
    Private Shared ReadOnly valueArray As Integer() = {-2, -1, 0}
    Private Shared ReadOnly valueArray1 As Boolean() = {True, False, True}
    Private Shared ReadOnly x As Integer = 1
    Private Shared ReadOnly valueArray0 As Integer() = {1, 2, 3}

    Public Sub B()
        Console.WriteLine(valueArray0)
    End Sub
End Class
");
        }

        [Fact]
        public async Task IgnoreOtherArgs_NoDiagnostic()
        {
            // A string
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(""Lorem ipsum"");
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(""Lorem ipsum"")
    End Sub
End Class
");

            // Test another type to be extra safe
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(123);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(123)
    End Sub
End Class
");

            // Non-literal array
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        string str = ""Lorem ipsum"";
        Console.WriteLine(new[] { str });
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Dim str As String = ""Lorem ipsum""
        Console.WriteLine({ str })
    End Sub
End Class
");

            // A ReadOnlySpan, which is already optimized
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        C(new bool[] { true, false });
    }

    private void C(ReadOnlySpan<bool> span)
    {
    }
}
");
        }
    }
}