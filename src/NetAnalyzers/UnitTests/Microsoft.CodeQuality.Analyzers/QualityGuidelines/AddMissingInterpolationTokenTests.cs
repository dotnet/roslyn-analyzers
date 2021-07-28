// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpAddMissingInterpolationToken,
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpAddMissingInterpolationTokenFixer>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines.BasicAddMissingInterpolationToken,
    Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines.BasicAddMissingInterpolationTokenFixer>;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.QualityGuidelines
{
    public class AddMissingInterpolationTokenTests
    {
        [Fact]
        public async Task HasValidVariableInScope_Diagnostic()
        {
            var csCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine([|""{x}""|]);
    }
}";

            var csFixedCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine($""{x}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(csCode, csFixedCode);

            var vbCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine([|""{x}""|])
    End Sub
End Class";

            var vbFixedCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine($""{x}"")
    End Sub
End Class";
            await VerifyVB.VerifyCodeFixAsync(vbCode, vbFixedCode);
        }

        [Fact]
        public async Task DoesNotHaveValidVariableInScope_NoDiagnostic()
        {
            var csCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine(""{y}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(csCode, csCode);

            var vbCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine(""{y}"")
    End Sub
End Class";
            await VerifyVB.VerifyCodeFixAsync(vbCode, vbCode);
        }

        [Fact]
        public async Task ContainsOnlyLiterals_NoDiagnostic()
        {
            var csCode = @"
using System;

class Program
{
    public static void Main()
    {
        Console.WriteLine(""{0}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(csCode, csCode);

            var vbCode = @"
Imports System

Class Program
    Sub Main()
        Console.WriteLine(""{0}"")
    End Sub
End Class";
            await VerifyVB.VerifyCodeFixAsync(vbCode, vbCode);
        }

        [Fact]
        public async Task ContainsLiteralAndBindableExpression_Diagnostic()
        {
            var csCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine([|""{x}, {0}""|]);
    }
}";

            var csFixedCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine($""{x}, {0}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(csCode, csFixedCode);

            var vbCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine([|""{x}, {0}""|])
    End Sub
End Class";

            var vbFixedCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine($""{x}, {0}"")
    End Sub
End Class";
            await VerifyVB.VerifyCodeFixAsync(vbCode, vbFixedCode);
        }

        [Fact]
        public async Task ContainsBindableExpression_Diagnostic()
        {
            var csCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine([|""{M(x)}""|]);
    }

    private static string M(int x) => x.ToString();
}";

            var csFixedCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine($""{M(x)}"");
    }

    private static string M(int x) => x.ToString();
}";
            await VerifyCS.VerifyCodeFixAsync(csCode, csFixedCode);

            var vbCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine([|""{M(x)}""|])
    End Sub

    Private Shared Function M(x As Integer) As String
        Return x.ToString()
    End Function
End Class";

            var vbFixedCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine($""{M(x)}"")
    End Sub

    Private Shared Function M(x As Integer) As String
        Return x.ToString()
    End Function
End Class";
            await VerifyVB.VerifyCodeFixAsync(vbCode, vbFixedCode);
        }

        [Fact]
        public async Task ContainsNonBindableExpression_NoDiagnostic()
        {
            var csCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine(""{N(x)}"");
    }

    private static string M(int x) => x.ToString();
}";

            await VerifyCS.VerifyCodeFixAsync(csCode, csCode);

            var vbCode = @"
Imports System

Class Program
    Sub Main()
        Dim x As Integer = 5
        Console.WriteLine(""{N(x)}"")
    End Sub

    Private Shared Function M(x As Integer) As String
        Return x.ToString()
    End Function
End Class";

            await VerifyVB.VerifyCodeFixAsync(vbCode, vbCode);
        }
    }
}
