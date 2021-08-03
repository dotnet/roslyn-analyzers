// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class AvoidConstArraysTests
    {
        #region C# Tests

        [Fact]
        public async Task CA1839CSharpIdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(new[]{ 1, 2, 3 });
    }
}
");

            // Explicit initialization check
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(new int[]{ 1, 2, 3 });
    }
}
");
        }

        [Fact]
        public async Task CA1839CSharpIgnoreOtherArgs()
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

            // Non-literal array
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public void B()
    {
        Console.WriteLine(string.Join(' ', new[] { ""Cake"", ""is"", ""good"" }));
    }
}
");
        }

        #endregion

        #region Visual Basic Tests

        [Fact]
        public async Task CA1839VisualBasicIdentifyConstArrays()
        {
            // Implicit initialization check
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine({1, 2, 3})
    End Sub
End Class
");

            // Explicit initialization check
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(New Integer() {1, 2, 3})
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1839VisualBasicIgnoreOtherArgs()
        {
            // A string
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(""Lorem ipsum"")
    End Sub
End Class
");

            // Test another type to be extra safe
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Console.WriteLine(123)
    End Sub
End Class
");

            // Non-literal array
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class A
    Public Sub B()
        Dim str As String = ""Lorem ipsum""
        Console.WriteLine(str.Split("" ""c))
    End Sub
End Class
");

            // Nested arguments
            await VerifyVB.VerifyAnalyzerAsync(@"
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