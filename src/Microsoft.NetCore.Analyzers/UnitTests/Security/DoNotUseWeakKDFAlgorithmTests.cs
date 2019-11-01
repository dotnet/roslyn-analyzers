// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseWeakKDFAlgorithmTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestMD5Diagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.MD5);
    }
}",
            GetCSharpResultAt(8, 34, DoNotUseWeakKDFAlgorithm.Rule, "Rfc2898DeriveBytes"));
        }

        [Fact]
        public void TestSHA1Diagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA1);
    }
}",
            GetCSharpResultAt(8, 34, DoNotUseWeakKDFAlgorithm.Rule, "Rfc2898DeriveBytes"));
        }

        [Fact]
        public void TestNoHashAlgorithmNameDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
    }
}",
            GetCSharpResultAt(8, 34, DoNotUseWeakKDFAlgorithm.Rule, "Rfc2898DeriveBytes"));
        }

        [Fact]
        public void TestDerivedClassOfRfc2898DeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class DerivedClass : Rfc2898DeriveBytes
{
    public DerivedClass (byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm) : base(password, salt, iterations, hashAlgorithm)
    {
    }
}

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var derivedClass = new DerivedClass(password, salt, iterations, HashAlgorithmName.MD5);
    }
}",
            GetCSharpResultAt(15, 28, DoNotUseWeakKDFAlgorithm.Rule, "DerivedClass"));
        }

        [Fact]
        public void TestDerivedClassOfRfc2898DeriveBytesNewPropertyDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class DerivedClass : Rfc2898DeriveBytes
{
    public DerivedClass (byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm) : base(password, salt, iterations, hashAlgorithm)
    {
    }

    public HashAlgorithmName HashAlgorithm { get; set;}
}

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var derivedClass = new DerivedClass(password, salt, iterations, HashAlgorithmName.MD5);
        derivedClass.HashAlgorithm = HashAlgorithmName.SHA256;
    }
}",
            GetCSharpResultAt(17, 28, DoNotUseWeakKDFAlgorithm.Rule, "DerivedClass"));
        }

        [Fact]
        public void TestNormalClassNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public TestClass (byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
    }

    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var subClass = new TestClass(password, salt, iterations, HashAlgorithmName.MD5);
    }
}");
        }

        [Fact]
        public void TestSHA256NoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
    }
}");
        }

        [Fact]
        public void TestDerivedClassOfRfc2898DeriveBytesNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class DerivedClass : Rfc2898DeriveBytes
{
    public DerivedClass (byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm) : base(password, salt, iterations, hashAlgorithm)
    {
    }
}

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var derivedClass = new DerivedClass(password, salt, iterations, HashAlgorithmName.SHA256);
    }
}");
        }

        [Fact]
        public void TestDerivedClassOfRfc2898DeriveBytesNewPropertyNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class DerivedClass : Rfc2898DeriveBytes
{
    public DerivedClass (byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm) : base(password, salt, iterations, hashAlgorithm)
    {
    }

    public HashAlgorithmName HashAlgorithm { get; set;}
}

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
    {
        var derivedClass = new DerivedClass(password, salt, iterations, HashAlgorithmName.SHA256);
        derivedClass.HashAlgorithm = HashAlgorithmName.MD5;
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseWeakKDFAlgorithm();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseWeakKDFAlgorithm();
        }
    }
}
