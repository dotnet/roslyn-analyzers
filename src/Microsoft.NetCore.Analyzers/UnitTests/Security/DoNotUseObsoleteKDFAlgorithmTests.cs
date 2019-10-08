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
    public void TestMethod(PasswordDeriveBytes passwordDeriveBytes)
    {
        passwordDeriveBytes.GetBytes(1);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "PasswordDeriveBytes", "GetBytes"));
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
    public void TestMethod(DerivedClass derivedClass, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        derivedClass.CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
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
    public void TestMethod(Rfc2898DeriveBytes rfc2898DeriveBytes, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        rfc2898DeriveBytes.CryptDeriveKey(algname, alghashname, keySize, rgbIV);
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
    public void TestMethod(DerivedClass derivedClass, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        derivedClass.CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(16, 9, DoNotUseObsoleteKDFAlgorithm.Rule, "Rfc2898DeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public void TestNormalMethodOfRfc2898DeriveBytesNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(Rfc2898DeriveBytes rfc2898DeriveBytes)
    {
        rfc2898DeriveBytes.GetBytes(1);
    }
}");
        }

        [Fact]
        public void TestConstructorOfRfc2898DeriveBytesNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        new Rfc2898DeriveBytes(password, salt);
    }
}");
        }

        [Fact]
        public void TestConstructorOfPasswordDeriveBytesNoDiagnostic()
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
}");
        }

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
}");
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
