// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotSerializeTypeWithPointerFieldsTests : DiagnosticAnalyzerTestBase
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
            GetCSharpResultAt(7, 26, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(7, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(7, 19, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(7, 19, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(7, 19, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(7, 20, DoNotSerializeTypeWithPointerFields.Rule, "pointers"));
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
            GetCSharpResultAt(13, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(14, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(14, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(13, 17, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(13, 20, DoNotSerializeTypeWithPointerFields.Rule, "pointers"));
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
            GetCSharpResultAt(7, 26, DoNotSerializeTypeWithPointerFields.Rule, "pointer1"),
            GetCSharpResultAt(13, 17, DoNotSerializeTypeWithPointerFields.Rule, "pointer2"));
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
            GetCSharpResultAt(7, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer1"),
            GetCSharpResultAt(9, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer2"));
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
            GetCSharpResultAt(7, 26, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(13, 25, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
            GetCSharpResultAt(7, 28, DoNotSerializeTypeWithPointerFields.Rule, "pointer1"),
            GetCSharpResultAt(18, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer2"));
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
            GetCSharpResultAt(9, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotSerializeTypeWithPointerFields();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotSerializeTypeWithPointerFields();
        }
    }
}
