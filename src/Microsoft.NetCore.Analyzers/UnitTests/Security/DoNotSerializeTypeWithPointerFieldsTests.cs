// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotSerializeTypeWithPointerFields,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotSerializeTypeWithPointerFieldsTests
    {
        [Fact]
        public void TestChildPointerToStructureDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private TestStructB* pointer;
}

[Serializable()]
struct TestStructB
{
}",
            GetCSharpResultAt(7, 26, "pointer"));
        }

        [Fact]
        public void TestChildPointerToIntegerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private int* pointer;
}",
            GetCSharpResultAt(7, 18, "pointer"));
        }

        [Fact]
        public void TestChildPointerToBooleanDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private bool* pointer;
}",
            GetCSharpResultAt(7, 19, "pointer"));
        }

        [Fact]
        public void TestChildPointerToPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private int** pointer;
}",
            GetCSharpResultAt(7, 19, "pointer"));
        }

        [Fact]
        public void TestChildPointerPropertyToPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private int** pointer { get; set; }
}",
            GetCSharpResultAt(7, 19, "pointer"));
        }

        [Fact]
        public void TestChildPointerInArrayDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private int*[] pointers;
}",
            GetCSharpResultAt(7, 20, "pointers"));
        }

        [Fact]
        public void TestChildArrayOfChildPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private TestClassB[] testClassBs;
}

[Serializable()]
unsafe class TestClassB
{
    private int* pointer;
}",
            GetCSharpResultAt(13, 18, "pointer"));
        }

        [Fact]
        public void TestChildListOfChildPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;
using System.Collections.Generic;

[Serializable()]
unsafe class TestClassA
{
    private List<TestClassB> testClassBs;
}

[Serializable()]
unsafe class TestClassB
{
    private int* pointer;
}",
            GetCSharpResultAt(14, 18, "pointer"));
        }

        [Fact]
        public void TestChildListOfListOfChildPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;
using System.Collections.Generic;

[Serializable()]
unsafe class TestClassA
{
    private List<List<TestClassB>> testClassBs;
}

[Serializable()]
unsafe class TestClassB
{
    private int* pointer;
}",
            GetCSharpResultAt(14, 18, "pointer"));
        }

        [Fact]
        public void TestGrandchildPointerToIntegerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private TestClassB testClassB;
}

[Serializable()]
unsafe class TestClassB
{
    public int* pointer;
}",
            GetCSharpResultAt(13, 17, "pointer"));
        }

        [Fact]
        public void TestGrandchildPointerInArrayDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private TestClassB testClassB;
}

[Serializable()]
unsafe class TestClassB
{
    private int*[] pointers;
}",
            GetCSharpResultAt(13, 20, "pointers"));
        }

        [Fact]
        public void TestChildPointerAndGrandchildPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private TestStructB* pointer1;
}

[Serializable()]
unsafe struct TestStructB
{
    public int* pointer2;
}",
            GetCSharpResultAt(7, 26, "pointer1"),
            GetCSharpResultAt(13, 17, "pointer2"));
        }

        [Fact]
        public void TestMultiChildPointersDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private int* pointer1;
    
    private int* pointer2;
}",
            GetCSharpResultAt(7, 18, "pointer1"),
            GetCSharpResultAt(9, 18, "pointer2"));
        }

        [Fact]
        public void TestChildPointerToSelfDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe struct TestStructA
{
    private TestStructA* pointer;
}",
            GetCSharpResultAt(7, 26, "pointer"));
        }

        [Fact]
        public void TestGrandchildPointerToSelfDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    public TestStructB testStructB;
}

[Serializable()]
unsafe struct TestStructB
{
    public TestStructB* pointer;
}",
            GetCSharpResultAt(13, 25, "pointer"));
        }

        [Fact]
        public void TestSubclassWithPointerFieldsDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    protected TestStructB* pointer1;
}

[Serializable()]
struct TestStructB
{
}

[Serializable()]
unsafe class TestClassC : TestClassA
{
    private int* pointer2;
}",
            GetCSharpResultAt(7, 28, "pointer1"),
            GetCSharpResultAt(18, 18, "pointer2"));
        }

        [Fact]
        public void TestGenericTypeWithPointerFieldDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA<T>
{
    private T[] normalField;

    private int* pointer;
}",
            GetCSharpResultAt(9, 18, "pointer"));
        }

        [Fact]
        public void TestGenericTypeWithoutPointerFieldNoDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
class TestClassA<T>
{
    private T[] normalField;
}");
        }

        [Fact]
        public void TestWithoutPointerFieldNoDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private int normalField;
}");
        }

        [Fact]
        public void TestWithoutSerializableAttributeNoDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

unsafe class TestClassA
{
    private int* pointer;
}");
        }

        [Fact]
        public void TestChildPointerWithNonSerializedNoDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    [NonSerialized]
    private int* pointer;
}");
        }

        [Fact]
        public void TestChildPointerWithStaticNoDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private static int* pointer;
}");
        }

        private void VerifyCSharpUnsafeCode(string code, params DiagnosticResult[] expected)
        {
            // TODO: Amaury - Fix this code
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
