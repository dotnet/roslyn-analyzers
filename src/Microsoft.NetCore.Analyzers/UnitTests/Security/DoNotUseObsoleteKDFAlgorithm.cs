// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseObsoleteKDFAlgorithmTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestNormalMethodOfPasswordDeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        new PasswordDeriveBytes(password, salt).GetBytes(1);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "GetBytes"),
            GetCSharpResultAt(9, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "PasswordDeriveBytes"));
        }

        [Fact]
        public void TestConstructorOfPasswordDeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        new PasswordDeriveBytes(password, salt);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "PasswordDeriveBytes"));
        }

        [Fact]
        public void TestCryptDeriveKeyOfClassDerivedFromPasswordDeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class DerivedClass : PasswordDeriveBytes
{
    public DerivedClass(string password, byte[] salt) : base(password, salt)
    {
    }
}

class TestClass
{
    public void TestMethod(string password, byte[] salt, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        new DerivedClass(password, salt).CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(7, 55, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "PasswordDeriveBytes"),
            GetCSharpResultAt(16, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public void TestCryptDeriveKeyOfRfc2898DeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        new Rfc2898DeriveBytes(password, salt).CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "Rfc2898DeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public void TestCryptDeriveKeyOfClassDerivedFromRfc2898DeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class DerivedClass : Rfc2898DeriveBytes
{
    public DerivedClass(string password, byte[] salt) : base(password, salt)
    {
    }
}

class TestClass
{
    public void TestMethod(string password, byte[] salt, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        new DerivedClass(password, salt).CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(16, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "Rfc2898DeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public void TestNormalMethodOfRfc2898DeriveBytesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        new Rfc2898DeriveBytes(password, salt).GetBytes(1);
    }
}");
        }

        // This diagnositc is produced by base(password, salt) not DerivedClass.GetBytes(cb)
        [Fact]
        public void TestGetBytesOfClassDerivedFromPasswordDeriveBytesNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class DerivedClass : PasswordDeriveBytes
{
    public DerivedClass(string password, byte[] salt) : base(password, salt)
    {
    }

    public override byte[] GetBytes (int cb)
    {
        return null;
    }
}

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        new DerivedClass(password, salt).GetBytes(cb);
    }
}",
            GetCSharpResultAt(7, 55, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "PasswordDeriveBytes"));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseObsoleteKDFAlgorithm();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseObsoleteKDFAlgorithm();
        }
    }
}
