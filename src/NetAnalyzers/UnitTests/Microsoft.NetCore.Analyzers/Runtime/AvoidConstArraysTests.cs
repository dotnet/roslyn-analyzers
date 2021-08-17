// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        #region C# Tests

        [Fact]
        public async Task CA1849_CSharp_IdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine({|CA1849:new[]{ 1, 2, 3 }|});
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

            // Explicit initialization check
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine({|CA1849:new int[]{ 1, 2, 3 }|});
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

            // Nested arguments
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(string.Join("" "", {|CA1849:new[] { ""Cake"", ""is"", ""good"" }|}));
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

            // Extension method usage
            await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Linq;

public class A
{
    public void B()
    {
        string y = {|CA1849:new[] { ""a"", ""b"", ""c"" }|}.First();
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
        }

        [Fact]
        public async Task CA1849_CSharp_NoDiagnostic_IgnoreOtherArgs()
        {
            // All code fix tests in this method result in no changes, as the analyzer will ignore these

            // A string
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(""Lorem ipsum"");
    }
}
", @"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(""Lorem ipsum"");
    }
}
");

            // Test another type to be extra safe
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(123);
    }
}
", @"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(123);
    }
}
");

            // Non-literal array
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        string str = ""Lorem ipsum"";
        Console.WriteLine(new[] { str });
    }
}
", @"
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
        }

        #endregion

        #region Visual Basic Tests

        [Fact]
        public async Task CA1849_VisualBasic_IdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine({|CA1849:{1, 2, 3}|})
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
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine({|CA1849:New Integer() {1, 2, 3}|})
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
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(String.Join("" ""c, {|CA1849:{""Cake"", ""is"", ""good""}|}))
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
        }

        [Fact]
        public async Task CA1849_VisualBasic_NoDiagnostic_IgnoreOtherArgs()
        {
            // All code fix tests in this method result in no changes, as the analyzer will ignore these

            // A string
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(""Lorem ipsum"")
    End Sub
End Class
", @"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(""Lorem ipsum"")
    End Sub
End Class
");

            // Test another type to be extra safe
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(123)
    End Sub
End Class
", @"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(123)
    End Sub
End Class
");

            // Non-literal array
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Dim str As String = ""Lorem ipsum""
        Console.WriteLine({ str })
    End Sub
End Class
", @"
Imports System

Public Class A
    Public Sub B()
        Dim str As String = ""Lorem ipsum""
        Console.WriteLine({ str })
    End Sub
End Class
");
        }

        #endregion
    }
}