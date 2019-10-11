// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseCreateEncryptorWithNonDefaultIVTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void Test_CreateEncryptorWithoutParameter_NonDefaultIV_Diagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] rgbIV)
    {
        var aesCng  = new AesCng();
        aesCng.IV = rgbIV;
        aesCng.CreateEncryptor();
    }
}",
            GetCSharpResultAt(10, 9, DoNotUseCreateEncryptorWithNonDefaultIV.DefinitelyUseCreateEncryptorWithNonDefaultIVRule, "CreateEncryptor"));
        }

        [Fact]
        public void Test_CreateEncryptorWithoutParameter_NonDefaultIV_DefinitelyNotNull_Diagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        byte[] rgbIV = new byte[] { 1, 2, 3};
        var aesCng  = new AesCng();
        aesCng.IV = rgbIV;
        aesCng.CreateEncryptor();
    }
}",
            GetCSharpResultAt(11, 9, DoNotUseCreateEncryptorWithNonDefaultIV.DefinitelyUseCreateEncryptorWithNonDefaultIVRule, "CreateEncryptor"));
        }

        [Fact]
        public void Test_CreateEncryptorWithoutParameter_MaybeNonDefaultIV_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] rgbIV)
    {
        var aesCng  = new AesCng();
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            aesCng.IV = rgbIV;
        }

        aesCng.CreateEncryptor();
    }
}",
            GetCSharpResultAt(17, 9, DoNotUseCreateEncryptorWithNonDefaultIV.MaybeUseCreateEncryptorWithNonDefaultIVRule, "CreateEncryptor"));
        }

        [Fact]
        public void Test_CreateEncryptorWithByteArrayAndByteArrayParameters_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] rgbKey, byte[] rgbIV)
    {
        var aesCng  = new AesCng();
        aesCng.CreateEncryptor(rgbKey, rgbIV);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseCreateEncryptorWithNonDefaultIV.DefinitelyUseCreateEncryptorWithNonDefaultIVRule, "CreateEncryptor"));
        }
        [Fact]
        public void Test_CreateEncryptorWithoutParameter_DefaultIV_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var aesCng  = new AesCng();
        aesCng.CreateEncryptor();
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseCreateEncryptorWithNonDefaultIV();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseCreateEncryptorWithNonDefaultIV();
        }
    }
}
