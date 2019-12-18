﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpAvoidZeroLengthArrayAllocationsAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidZeroLengthArrayAllocationsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicAvoidZeroLengthArrayAllocationsAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidZeroLengthArrayAllocationsFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class AvoidZeroLengthArrayAllocationsAnalyzerTests
    {
        /// <summary>
        /// This type isn't defined in all locations where this test runs.  Need to alter the
        /// test code slightly to account for this.
        /// </summary>
        private static bool IsArrayEmptyDefined()
        {
            Assembly assembly = typeof(object).Assembly;
            Type type = assembly.GetType("System.Array");
            return type.GetMethod("Empty", BindingFlags.Public | BindingFlags.Static) != null;
        }

        private static string GetArrayEmptySourceBasic()
        {
            const string arrayEmptySourceRaw = @"
Namespace System
    Public Class Array
       Public Shared Function Empty(Of T)() As T()
           Return Nothing
       End Function
    End Class
End Namespace
";
            return IsArrayEmptyDefined() ? string.Empty : arrayEmptySourceRaw;
        }

        private static string GetArrayEmptySourceCSharp()
        {
            const string arrayEmptySourceRaw = @"
namespace System
{
    public class Array
    {
        public static T[] Empty<T>()
        {
            return null;
        }
    }
}
";
            return IsArrayEmptyDefined() ? string.Empty : arrayEmptySourceRaw;
        }

        [Fact]
        public async Task EmptyArrayCSharp()
        {
            const string badSource = @"
using System.Collections.Generic;

class C
{
    unsafe void M1()
    {
        int[] arr1 = new int[0];                       // yes
        byte[] arr2 = { };                             // yes
        C[] arr3 = new C[] { };                        // yes
        string[] arr4 = new string[] { null };         // no
        double[] arr5 = new double[1];                 // no
        int[] arr6 = new[] { 1 };                      // no
        int[][] arr7 = new int[0][];                   // yes
        int[][][][] arr8 = new int[0][][][];           // yes
        int[,] arr9 = new int[0,0];                    // no
        int[][,] arr10 = new int[0][,];                // yes
        int[][,] arr11 = new int[1][,];                // no
        int[,][] arr12 = new int[0,0][];               // no
        int*[] arr13 = new int*[0];                    // no
        List<int> list1 = new List<int>() { };         // no
    }
}";

            const string fixedSource = @"
using System.Collections.Generic;

class C
{
    unsafe void M1()
    {
        int[] arr1 = System.Array.Empty<int>();                       // yes
        byte[] arr2 = System.Array.Empty<byte>();                             // yes
        C[] arr3 = System.Array.Empty<C>();                        // yes
        string[] arr4 = new string[] { null };         // no
        double[] arr5 = new double[1];                 // no
        int[] arr6 = new[] { 1 };                      // no
        int[][] arr7 = System.Array.Empty<int[]>();                   // yes
        int[][][][] arr8 = System.Array.Empty<int[][][]>();           // yes
        int[,] arr9 = new int[0,0];                    // no
        int[][,] arr10 = System.Array.Empty<int[,]>();                // yes
        int[][,] arr11 = new int[1][,];                // no
        int[,][] arr12 = new int[0,0][];               // no
        int*[] arr13 = new int*[0];                    // no
        List<int> list1 = new List<int>() { };         // no
    }
}";
            string arrayEmptySource = GetArrayEmptySourceCSharp();

            await VerifyCS.VerifyCodeFixAsync(
                badSource + arrayEmptySource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(8, 22).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(9, 23).WithArguments("Array.Empty<byte>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(10, 20).WithArguments("Array.Empty<C>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(14, 24).WithArguments("Array.Empty<int[]>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(15, 28).WithArguments("Array.Empty<int[][][]>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(17, 26).WithArguments("Array.Empty<int[,]>()"),
                },
                fixedSource + arrayEmptySource);

            await VerifyCS.VerifyCodeFixAsync(
                "using System;\r\n" + badSource + arrayEmptySource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(8 + 1, 22).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(9 + 1, 23).WithArguments("Array.Empty<byte>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(10 + 1, 20).WithArguments("Array.Empty<C>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(14 + 1, 24).WithArguments("Array.Empty<int[]>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(15 + 1, 28).WithArguments("Array.Empty<int[][][]>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(17 + 1, 26).WithArguments("Array.Empty<int[,]>()"),
                },
                "using System;\r\n" + fixedSource.Replace("System.Array.Empty", "Array.Empty") + arrayEmptySource);
        }

        [Fact]
        public async Task EmptyArrayCSharpError()
        {
            const string badSource = @"
// This is a compile error but we want to ensure analyzer doesn't complain for it.
[System.Runtime.CompilerServices.Dynamic(new bool[0])]
";

            await VerifyCS.VerifyAnalyzerAsync(
                badSource,
                DiagnosticResult.CompilerError("CS0116").WithSpan(3, 54, 3, 55)/*.WithMessage("A namespace cannot directly contain members such as fields or methods")*/);
        }

        [Fact]
        public async Task EmptyArrayVisualBasic()
        {
            const string badSource = @"
Imports System.Collections.Generic

<System.Runtime.CompilerServices.Dynamic(new Boolean(-1) {})> _
Class C
    Sub M1()
        Dim arr1 As Integer() = New Integer(-1) { }               ' yes
        Dim arr2 As Byte() = { }                                  ' yes
        Dim arr3 As C() = New C(-1) { }                           ' yes
        Dim arr4 As String() = New String() { Nothing }           ' no
        Dim arr5 As Double() = New Double(1) { }                  ' no
        Dim arr6 As Integer() = { -1 }                            ' no
        Dim arr7 as Integer()() = New Integer(-1)() { }           ' yes
        Dim arr8 as Integer()()()() = New Integer(  -1)()()() { } ' yes
        Dim arr9 as Integer(,) = New Integer(-1,-1) { }           ' no
        Dim arr10 as Integer()(,) = New Integer(-1)(,) { }        ' yes
        Dim arr11 as Integer()(,) = New Integer(1)(,) { }         ' no
        Dim arr12 as Integer(,)() = New Integer(-1,-1)() { }      ' no
        Dim arr13 as Integer() = New Integer(0) { }               ' no
        Dim list1 as List(Of Integer) = New List(Of Integer) From { }  ' no
    End Sub
End Class";

            const string fixedSource = @"
Imports System.Collections.Generic

<System.Runtime.CompilerServices.Dynamic(new Boolean(-1) {})> _
Class C
    Sub M1()
        Dim arr1 As Integer() = System.Array.Empty(Of Integer)()               ' yes
        Dim arr2 As Byte() = System.Array.Empty(Of Byte)()                                  ' yes
        Dim arr3 As C() = System.Array.Empty(Of C)()                           ' yes
        Dim arr4 As String() = New String() { Nothing }           ' no
        Dim arr5 As Double() = New Double(1) { }                  ' no
        Dim arr6 As Integer() = { -1 }                            ' no
        Dim arr7 as Integer()() = System.Array.Empty(Of Integer())()           ' yes
        Dim arr8 as Integer()()()() = System.Array.Empty(Of Integer()()())() ' yes
        Dim arr9 as Integer(,) = New Integer(-1,-1) { }           ' no
        Dim arr10 as Integer()(,) = System.Array.Empty(Of Integer(,))()        ' yes
        Dim arr11 as Integer()(,) = New Integer(1)(,) { }         ' no
        Dim arr12 as Integer(,)() = New Integer(-1,-1)() { }      ' no
        Dim arr13 as Integer() = New Integer(0) { }               ' no
        Dim list1 as List(Of Integer) = New List(Of Integer) From { }  ' no
    End Sub
End Class";

            string arrayEmptySource = GetArrayEmptySourceBasic();

            await VerifyVB.VerifyCodeFixAsync(
                badSource + arrayEmptySource,
                new[]
                {
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(7, 33).WithArguments("Array.Empty(Of Integer)()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(8, 30).WithArguments("Array.Empty(Of Byte)()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(9, 27).WithArguments("Array.Empty(Of C)()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(13, 35).WithArguments("Array.Empty(Of Integer())()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(14, 39).WithArguments("Array.Empty(Of Integer()()())()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(16, 37).WithArguments("Array.Empty(Of Integer(,))()"),
                },
                fixedSource + arrayEmptySource);

            await VerifyVB.VerifyCodeFixAsync(
                "Imports System\r\n" + badSource + arrayEmptySource,
                new[]
                {
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(7 + 1, 33).WithArguments("Array.Empty(Of Integer)()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(8 + 1, 30).WithArguments("Array.Empty(Of Byte)()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(9 + 1, 27).WithArguments("Array.Empty(Of C)()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(13 + 1, 35).WithArguments("Array.Empty(Of Integer())()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(14 + 1, 39).WithArguments("Array.Empty(Of Integer()()())()"),
                    VerifyVB.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(16 + 1, 37).WithArguments("Array.Empty(Of Integer(,))()"),
                },
                "Imports System\r\n" + fixedSource.Replace("System.Array.Empty", "Array.Empty") + arrayEmptySource);
        }

        [Fact]
        public async Task EmptyArrayCSharp_DifferentTypeKind()
        {
            const string badSource = @"
class C
{
    void M1()
    {
        int[] arr1 = new int[(long)0];                 // yes
        double[] arr2 = new double[(ulong)0];         // yes
        double[] arr3 = new double[(long)1];         // no
    }
}";

            const string fixedSource = @"
class C
{
    void M1()
    {
        int[] arr1 = System.Array.Empty<int>();                 // yes
        double[] arr2 = System.Array.Empty<double>();         // yes
        double[] arr3 = new double[(long)1];         // no
    }
}";

            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(6, 22).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(7, 25).WithArguments("Array.Empty<double>()"),
                },
                fixedSource);

            await VerifyCS.VerifyCodeFixAsync(
                "using System;\r\n" + badSource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(6 + 1, 22).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(7 + 1, 25).WithArguments("Array.Empty<double>()"),
                },
                "using System;\r\n" + fixedSource.Replace("System.Array.Empty", "Array.Empty"));
        }

        [WorkItem(10214, "https://github.com/dotnet/roslyn/issues/10214")]
        [Fact]
        public async Task EmptyArrayVisualBasic_CompilerGeneratedArrayCreation()
        {
            const string source = @"
Class C
    Private Sub F(ParamArray args As String())
    End Sub

Private Sub G()
        F()     ' Compiler seems to generate a param array with size 0 for the invocation.
    End Sub
End Class
";

            string arrayEmptySource = GetArrayEmptySourceBasic();

            // Should we be flagging diagnostics on compiler generated code?
            // Should the analyzer even be invoked for compiler generated code?
            await VerifyVB.VerifyAnalyzerAsync(source + arrayEmptySource);
        }

        [WorkItem(1209, "https://github.com/dotnet/roslyn-analyzers/issues/1209")]
        [Fact]
        public async Task EmptyArrayCSharp_CompilerGeneratedArrayCreationInObjectCreation()
        {
            const string source = @"
namespace N
{
    using Microsoft.CodeAnalysis;
    class C
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            ""RuleId"",
            ""Title"",
            ""MessageFormat"",
            ""Dummy"",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: ""Description"");
    }
}
";

            string arrayEmptySource = GetArrayEmptySourceCSharp();

            // Should we be flagging diagnostics on compiler generated code?
            // Should the analyzer even be invoked for compiler generated code?
            await VerifyCS.VerifyAnalyzerAsync(source + arrayEmptySource);
        }

        [WorkItem(1209, "https://github.com/dotnet/roslyn-analyzers/issues/1209")]
        [Fact]
        public async Task EmptyArrayCSharp_CompilerGeneratedArrayCreationInIndexerAccess()
        {
            const string source = @"
public abstract class C
{
    protected abstract int this[int p1, params int[] p2] {get; set;}
    public void M()
    {
        var x = this[0];
    }
}
";

            string arrayEmptySource = GetArrayEmptySourceCSharp();

            // Should we be flagging diagnostics on compiler generated code?
            // Should the analyzer even be invoked for compiler generated code?
            await VerifyCS.VerifyAnalyzerAsync(source + arrayEmptySource);
        }

        [Fact]
        public async Task EmptyArrayCSharp_UsedInAttribute_NoDiagnostics()
        {
            const string source = @"
using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]  
class CustomAttribute : Attribute
{
    public CustomAttribute(object o)
    {
    }
}

[Custom(new int[0])]
[Custom(new string[] { })]
class C
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public async Task EmptyArrayCSharp_FieldOrPropertyInitializer()
        {
            const string badSource = @"
using System;

class C
{
    public int[] f1 = new int[] { };
    public int[] p1 { get; set; } = new int[] { };
}
";
            const string fixedSource = @"
using System;

class C
{
    public int[] f1 = Array.Empty<int>();
    public int[] p1 { get; set; } = Array.Empty<int>();
}
";

            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(6, 23).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(7, 37).WithArguments("Array.Empty<int>()"),
                },
                fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public async Task EmptyArrayCSharp_UsedInAssignment()
        {
            const string badSource = @"
using System;

class C
{
    void M()
    {
        int[] l1;
        l1 = new int[0];
        l1 = new int[] { };
    }
}
";
            const string fixedSource = @"
using System;

class C
{
    void M()
    {
        int[] l1;
        l1 = Array.Empty<int>();
        l1 = Array.Empty<int>();
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(9, 14).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(10, 14).WithArguments("Array.Empty<int>()"),
                },
                fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public async Task EmptyArrayCSharp_DeclarationTypeDoesNotMatch_NotArray()
        {
            const string badSource = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

class C
{
    public IEnumerable<int> f1 = new int[0];
    public ICollection<int> f2 = new int[0];
    public IReadOnlyCollection<int> f3 = new int[0];
    public IList<int> f4 = new int[0];
    public IReadOnlyList<int> f5 = new int[0];

    public IEnumerable f6 = new int[0];
    public ICollection f7 = new int[0];
    public IList f8 = new int[0];
}
";
            const string fixedSource = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

class C
{
    public IEnumerable<int> f1 = Array.Empty<int>();
    public ICollection<int> f2 = Array.Empty<int>();
    public IReadOnlyCollection<int> f3 = Array.Empty<int>();
    public IList<int> f4 = Array.Empty<int>();
    public IReadOnlyList<int> f5 = Array.Empty<int>();

    public IEnumerable f6 = Array.Empty<int>();
    public ICollection f7 = Array.Empty<int>();
    public IList f8 = Array.Empty<int>();
}
";
            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(9, 34).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(10, 34).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(11, 42).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(12, 28).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(13, 36).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(15, 29).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(16, 29).WithArguments("Array.Empty<int>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(17, 23).WithArguments("Array.Empty<int>()"),
                },
                fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public async Task EmptyArrayCSharp_DeclarationTypeDoesNotMatch_DifferentElementType()
        {
            const string badSource = @"
using System;

class C
{
    public object[] f1 = new string[0];
}
";
            const string fixedSource = @"
using System;

class C
{
    public object[] f1 = Array.Empty<string>();
}
";

            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(6, 26).WithArguments("Array.Empty<string>()"),
                fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public async Task EmptyArrayCSharp_UsedAsExpression()
        {
            const string badSource = @"
using System;

class C
{
    void M1(object[] array)
    {
    }

    // Tests handling of implicit conversion. Do not change to 'object[] obj'.
    void M2(object obj)
    {
    }

    void M3()
    {
        M1(new object[0]);
        M2(new object[0]);
    }

    object M4() => new object[0];

    object M5()
    {
        return new object[0];
    }
}
";
            const string fixedSource = @"
using System;

class C
{
    void M1(object[] array)
    {
    }

    // Tests handling of implicit conversion. Do not change to 'object[] obj'.
    void M2(object obj)
    {
    }

    void M3()
    {
        M1(Array.Empty<object>());
        M2(Array.Empty<object>());
    }

    object M4() => Array.Empty<object>();

    object M5()
    {
        return Array.Empty<object>();
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                new[]
                {
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(17, 12).WithArguments("Array.Empty<object>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(18, 12).WithArguments("Array.Empty<object>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(21, 20).WithArguments("Array.Empty<object>()"),
                    VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(25, 16).WithArguments("Array.Empty<object>()"),
                },
                fixedSource);
        }

        [Fact]
        public async Task EmptyArrayCSharp_SystemNotImported()
        {
            const string badSource = @"
class C
{
    public object[] f1 = new object[0];
}
";
            const string fixedSource = @"
class C
{
    public object[] f1 = System.Array.Empty<object>();
}
";
            await VerifyCS.VerifyCodeFixAsync(
                badSource,
                VerifyCS.Diagnostic(AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor).WithLocation(4, 26).WithArguments("Array.Empty<object>()"),
                fixedSource);
        }
    }
}
