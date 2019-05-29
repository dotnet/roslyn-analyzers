// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseDSATests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestCreateObjectOfDSADerivedClassWithoutParameterDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var dsaCng = new DSACng();
    }
}",
            GetCSharpResultAt(8, 22, DoNotUseDSA.Rule, "DSACng"));
        }

        [Fact]
        public void TestCreateObjectOfDSADerivedClassWithCngKeyParameterDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(CngKey key)
    {
        var dsaCng = new DSACng(key);
    }
}",
            GetCSharpResultAt(8, 22, DoNotUseDSA.Rule, "DSACng"));
        }

        [Fact]
        public void TestCreateObjectOfDSADerivedClassWithInt32ParameterAssignedKeySizeDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var dsaCng = new DSACng(2048);
    }
}",
            GetCSharpResultAt(8, 22, DoNotUseDSA.Rule, "DSACng"));
        }

        [Fact]
        public void TestCreateObjectOfDSADerivedClassWithInt32ParameterUnassignedKeySizeDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(int keySize)
    {
        var dsaCng = new DSACng(keySize);
    }
}",
            GetCSharpResultAt(8, 22, DoNotUseDSA.Rule, "DSACng"));
        }

        [Fact]
        public void TestReturnObjectOfDSADerivedClassDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public DSA TestMethod(DSA dsa)
    {
        return dsa;
    }
}",
            GetCSharpResultAt(8, 9, DoNotUseDSA.Rule, "DSA"));
        }

        [Fact]
        public void TestReturnObjectOfDSADerivedClassLocalFunctionDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        DSA GetDSA(DSA dsa) => dsa;
    }
}",
            GetCSharpResultAt(8, 32, DoNotUseDSA.Rule, "DSA"));
        }

        [Fact]
        public void TestCreateWithDSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""DSA"");
    }
}",
            GetCSharpResultAt(8, 35, DoNotUseDSA.Rule, "DSA"));
        }

        [Fact]
        public void TestCaseSensitiveDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""dSa"");
    }
}",
            GetCSharpResultAt(8, 35, DoNotUseDSA.Rule, "dSa"));
        }

        [Fact]
        public void TestCreateWithSystemSecurityCryptographyDSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""System.Security.Cryptography.DSA"");
    }
}",
            GetCSharpResultAt(8, 35, DoNotUseDSA.Rule, "System.Security.Cryptography.DSA"));
        }

        [Fact]
        public void TestCreateFromNameWithDSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""DSA"");
    }
}",
            GetCSharpResultAt(8, 28, DoNotUseDSA.Rule, "DSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyDSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.DSA"");
    }
}",
            GetCSharpResultAt(8, 28, DoNotUseDSA.Rule, "System.Security.Cryptography.DSA"));
        }

        [Fact]
        public void TestCreateWithECDsaArgNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""ECDsa"");
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithECDsaArgNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""ECDsa"");
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithECDsaAndKeySize1024ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""ECDsa"", 1024);
    }
}");
        }

        [Fact]
        public void TestReturnVoidNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(DSA dsa)
    { 
        return;
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseDSA();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseDSA();
        }
    }
}
