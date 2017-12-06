// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class AvoidZeroLengthArrayAllocationsAnalyzerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() { return new CSharpAvoidZeroLengthArrayAllocationsAnalyzer(); }
        protected override CodeFixProvider GetCSharpCodeFixProvider() { return new AvoidZeroLengthArrayAllocationsFixer(); }
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer() { return new BasicAvoidZeroLengthArrayAllocationsAnalyzer(); }
        protected override CodeFixProvider GetBasicCodeFixProvider() { return new AvoidZeroLengthArrayAllocationsFixer(); }

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
        public void EmptyArrayCSharp()
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

            VerifyCSharpUnsafeCode(badSource + arrayEmptySource, new[]
            {
                GetCSharpResultAt(8, 22, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(9, 23, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<byte>()"),
                GetCSharpResultAt(10, 20, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<C>()"),
                GetCSharpResultAt(14, 24, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int[]>()"),
                GetCSharpResultAt(15, 28, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int[][][]>()"),
                GetCSharpResultAt(17, 26, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int[,]>()")
            });
            VerifyCSharpUnsafeCodeFix(
                arrayEmptySource + badSource,
                arrayEmptySource + fixedSource,
                allowNewCompilerDiagnostics: true);
            VerifyCSharpUnsafeCodeFix(
                "using System;\r\n" + arrayEmptySource + badSource,
                "using System;\r\n" + arrayEmptySource + fixedSource.Replace("System.Array.Empty", "Array.Empty"),
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void EmptyArrayCSharpError()
        {
            const string badSource = @"
// This is a compile error but we want to ensure analyzer doesn't complain for it.
[System.Runtime.CompilerServices.Dynamic(new bool[0])]
";

            VerifyCSharp(badSource, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void EmptyArrayVisualBasic()
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

            VerifyBasic(badSource + arrayEmptySource, new[]
            {
                GetBasicResultAt(7, 33, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty(Of Integer)()"),
                GetBasicResultAt(8, 30, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty(Of Byte)()"),
                GetBasicResultAt(9, 27, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty(Of C)()"),
                GetBasicResultAt(13, 35, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty(Of Integer())()"),
                GetBasicResultAt(14, 39, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty(Of Integer()()())()"),
                GetBasicResultAt(16, 37, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty(Of Integer(,))()")
            });
            VerifyBasicFix(
                arrayEmptySource + badSource,
                arrayEmptySource + fixedSource,
                allowNewCompilerDiagnostics: true);
            VerifyBasicFix(
                "Imports System\r\n" + arrayEmptySource + badSource,
                "Imports System\r\n" + arrayEmptySource + fixedSource.Replace("System.Array.Empty", "Array.Empty"),
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void EmptyArrayCSharp_DifferentTypeKind()
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
            string arrayEmptySource = GetArrayEmptySourceCSharp();

            VerifyCSharp(badSource + arrayEmptySource, new[]
            {
                GetCSharpResultAt(6, 22, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(7, 25, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<double>()")
            });

            VerifyCSharpFix(
                arrayEmptySource + badSource,
                arrayEmptySource + fixedSource,
                allowNewCompilerDiagnostics: true);
            VerifyCSharpFix(
                "using System;\r\n" + arrayEmptySource + badSource,
                "using System;\r\n" + arrayEmptySource + fixedSource.Replace("System.Array.Empty", "Array.Empty"),
                allowNewCompilerDiagnostics: true);
        }

        [WorkItem(10214, "https://github.com/dotnet/roslyn/issues/10214")]
        [Fact]
        public void EmptyArrayVisualBasic_CompilerGeneratedArrayCreation()
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
            VerifyBasic(source + arrayEmptySource);
        }

        [WorkItem(1209, "https://github.com/dotnet/roslyn-analyzers/issues/1209")]
        [Fact]
        public void EmptyArrayCSharp_CompilerGeneratedArrayCreationInObjectCreation()
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
            VerifyCSharp(source + arrayEmptySource, addLanguageSpecificCodeAnalysisReference: true);
        }
        
        [WorkItem(1209, "https://github.com/dotnet/roslyn-analyzers/issues/1209")]
        [Fact]
        public void EmptyArrayCSharp_CompilerGeneratedArrayCreationInIndexerAccess()
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
            VerifyCSharp(source + arrayEmptySource, addLanguageSpecificCodeAnalysisReference: true);
        }

        [Fact]
        public void EmptyArrayCSharp_UsedInAttribute_NoDiagnostics()
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
            VerifyCSharp(source);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public void EmptyArrayCSharp_FieldOrPropertyInitializer()
        {
            const string badSource = @"
using System;

class C
{
    public int[] f1 = new int[] { };
    public int[] p1 { get; set; } = new int[] { };
}
";

            string arrayEmptySource = GetArrayEmptySourceCSharp();

            VerifyCSharp(badSource + arrayEmptySource, new[]
            {
                GetCSharpResultAt(6, 23, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(7, 37, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()")
            });

            const string fixedSource = @"
using System;

class C
{
    public int[] f1 = Array.Empty<int>();
    public int[] p1 { get; set; } = Array.Empty<int>();
}
";

            VerifyCSharpFix(badSource, fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public void EmptyArrayCSharp_UsedInAssignment()
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
            VerifyCSharp(badSource, new DiagnosticResult[]
            {
                GetCSharpResultAt(9, 14, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(10, 14, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()")
            });

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
            VerifyCSharpFix(badSource, fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public void EmptyArrayCSharp_DeclarationTypeDoesNotMatch_NotArray()
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
            VerifyCSharp(badSource, new DiagnosticResult[]
            {
                GetCSharpResultAt(9, 34, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(10, 34, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(11, 42, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(12, 28, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(13, 36, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(15, 29, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(16, 29, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()"),
                GetCSharpResultAt(17, 23, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<int>()")
            });

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
            VerifyCSharpFix(badSource, fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public void EmptyArrayCSharp_DeclarationTypeDoesNotMatch_DifferentElementType()
        {
            const string badSource = @"
using System;

class C
{
    public object[] f1 = new string[0];
}
";
            VerifyCSharp(badSource, new DiagnosticResult[]
            {
                GetCSharpResultAt(6, 26, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<string>()")
            });

            const string fixedSource = @"
using System;

class C
{
    public object[] f1 = Array.Empty<string>();
}
";
            VerifyCSharpFix(badSource, fixedSource);
        }

        [WorkItem(1298, "https://github.com/dotnet/roslyn-analyzers/issues/1298")]
        [Fact]
        public void EmptyArrayCSharp_UsedAsExpression()
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
            VerifyCSharp(badSource, new DiagnosticResult[]
            {
                GetCSharpResultAt(17, 12, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<object>()"),
                GetCSharpResultAt(18, 12, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<object>()"),
                GetCSharpResultAt(21, 20, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<object>()"),
                GetCSharpResultAt(25, 16, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<object>()"),
            });

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
            VerifyCSharpFix(badSource, fixedSource);
        }

        [Fact]
        public void EmptyArrayCSharp_SystemNotImported()
        {
            const string badSource = @"
class C
{
    public object[] f1 = new object[0];
}
";
            VerifyCSharp(badSource, new DiagnosticResult[]
            {
                GetCSharpResultAt(4, 26, AvoidZeroLengthArrayAllocationsAnalyzer.UseArrayEmptyDescriptor, "Array.Empty<object>()")
            });

            const string fixedSource = @"
class C
{
    public object[] f1 = System.Array.Empty<object>();
}
";
            VerifyCSharpFix(badSource, fixedSource);
        }
    }
}
