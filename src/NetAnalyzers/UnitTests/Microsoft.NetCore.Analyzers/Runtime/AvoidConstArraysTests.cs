// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    // Analyzer and code fix tests are both in this file
    // "Do not separate analyzer tests from code fix tests"
    // https://github.com/dotnet/roslyn-analyzers/blob/main/docs/NetCore_GettingStarted.md#definition-of-done
    public class AvoidConstArraysTests
    {
        #region C# Tests

        [Fact]
        public async Task CA1839_CSharp_IdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(new[]{ 1, 2, 3 });
    }
}
", @"
using System;

public class A
{
    private static readonly int[] valueArray = new[]{ 1, 2, 3 };

    public void B()
    {
        Console.WriteLine(valueArray);
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
        Console.WriteLine(new int[]{ 1, 2, 3 });
    }
}
", @"
using System;

public class A
{
    private static readonly int[] valueArray = new int[]{ 1, 2, 3 };

    public void B()
    {
        Console.WriteLine(valueArray);
    }
}
");
        }

        [Fact]
        public async Task CA1839_CSharp_NoDiagnostic_IgnoreOtherArgs()
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
        Console.WriteLine(str.Split(' '));
    }
}
", @"
using System;

public class A
{
    public void B()
    {
        string str = ""Lorem ipsum"";
        Console.WriteLine(str.Split(' '));
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
        Console.WriteLine("" "".Join(new[] { ""Cake"", ""is"", ""good"" }));
    }
}
", @"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine("" "".Join(new[] { ""Cake"", ""is"", ""good"" }));
    }
}
");
        }

        #endregion

        #region Visual Basic Tests

        [Fact]
        public async Task CA1839_VisualBasic_IdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine({1, 2, 3})
    End Sub
End Class
", @"
Imports System

Public Class A
    Private Shared ReadOnly valueArray As Integer() = {1, 2, 3}

    Public Sub B()
        Console.WriteLine(valueArray)
    End Sub
End Class
");

            // Explicit initialization check
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(New Integer() {1, 2, 3})
    End Sub
End Class
", @"
Imports System

Public Class A
    Private Shared ReadOnly valueArray As Integer() = New Integer() {1, 2, 3}

    Public Sub B()
        Console.WriteLine(valueArray)
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1839_VisualBasic_NoDiagnostic_IgnoreOtherArgs()
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
        Console.WriteLine(str.Split("" ""c))
    End Sub
End Class
", @"
Imports System

Public Class A
    Public Sub B()
        Dim str As String = ""Lorem ipsum""
        Console.WriteLine(str.Split("" ""c))
    End Sub
End Class
");

            // Nested arguments
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(String.Join("" ""c, {""Cake"", ""is"", ""good""}))
    End Sub
End Class
", @"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(String.Join("" ""c, {""Cake"", ""is"", ""good""}))
    End Sub
End Class
");
        }

        #endregion
    }
}