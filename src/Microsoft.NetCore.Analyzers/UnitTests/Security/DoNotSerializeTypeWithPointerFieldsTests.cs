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
    private TestClassB* pointer;
}

[Serializable()]
struct TestClassB
{
}",
            GetCSharpResultAt(7, 25, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
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
unsafe struct TestClassB
{
    public int* pointer;
}",
            GetCSharpResultAt(13, 17, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
        }

        [Fact]
        public void TestChildPointerAndGrandchildPointerDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    private TestClassB* pointer1;
}

[Serializable()]
unsafe struct TestClassB
{
    public int* pointer2;
}",
            GetCSharpResultAt(7, 25, DoNotSerializeTypeWithPointerFields.Rule, "pointer1"),
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
unsafe struct TestClassA
{
    private TestClassA* pointer;
}",
            GetCSharpResultAt(7, 25, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
        }

        [Fact]
        public void TestGrandchildPointerToSelfDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe struct TestClassA
{
    public TestClassB testClassB;
}

[Serializable()]
unsafe struct TestClassB
{
    public TestClassB* pointer;
}",
            GetCSharpResultAt(13, 24, DoNotSerializeTypeWithPointerFields.Rule, "pointer"));
        }

        [Fact]
        public void TestSubclassWithPointerFieldsDiagnostic()
        {
            VerifyCSharpUnsafeCode(@"
using System;

[Serializable()]
unsafe class TestClassA
{
    protected TestClassB* pointer1;
}

[Serializable()]
struct TestClassB
{
}

[Serializable()]
unsafe class TestClassC : TestClassA
{
    private int* pointer2;
}",
            GetCSharpResultAt(7, 27, DoNotSerializeTypeWithPointerFields.Rule, "pointer1"),
            GetCSharpResultAt(18, 18, DoNotSerializeTypeWithPointerFields.Rule, "pointer2"));
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
