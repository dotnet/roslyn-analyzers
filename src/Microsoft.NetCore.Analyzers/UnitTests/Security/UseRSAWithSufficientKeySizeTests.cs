// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseRSAWithSufficientKeySizeTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void Issue2697()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public RSACryptoServiceProvider TestMethod(string xml)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(xml);
        return rsa;
    }
}");
        }

        [Fact]
        public void TestCreateObjectOfRSADerivedClassWithInt32ParameterDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var rsaCng = new RSACng(1024);
    }
}",
            GetCSharpResultAt(8, 22, UseRSAWithSufficientKeySize.Rule, "RSACng"));
        }

        [Fact]
        public void TestConstantDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        const int keySize = 1024;
        var rsaCng = new RSACng(keySize);
    }
}",
            GetCSharpResultAt(9, 22, UseRSAWithSufficientKeySize.Rule, "RSACng"));
        }

        [Fact]
        public void TestCreateWithoutParameterDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create();
    }
}",
            GetCSharpResultAt(8, 35, UseRSAWithSufficientKeySize.Rule, "RSA"));
        }

        [Fact]
        public void TestCreateWithRSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""RSA"");
    }
}",
            GetCSharpResultAt(8, 35, UseRSAWithSufficientKeySize.Rule, "RSA"));
        }

        [Fact]
        public void TestCreateWithSystemSecurityCryptographyRSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""System.Security.Cryptography.RSA"");
    }
}",
            GetCSharpResultAt(8, 35, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.RSA"));
        }

        [Fact]
        public void TestCreateWithSystemSecurityCryptographyAsymmetricAlgorithmArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""System.Security.Cryptography.AsymmetricAlgorithm"");
    }
}",
            GetCSharpResultAt(8, 35, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
        }

        [Fact]
        public void TestCreateFromNameWithRSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""RSA"");
    }
}",
            GetCSharpResultAt(8, 28, UseRSAWithSufficientKeySize.Rule, "RSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyRSAArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.RSA"");
    }
}",
            GetCSharpResultAt(8, 28, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.RSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyAsymmetricAlgorithmArgDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.AsymmetricAlgorithm"");
    }
}",
            GetCSharpResultAt(8, 28, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
        }

        [Fact]
        public void TestCreateFromNameWithRSAAndKeySize1024ArgsDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""RSA"", 1024);
    }
}",
            GetCSharpResultAt(8, 28, UseRSAWithSufficientKeySize.Rule, "RSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyRSAAndKeySize1024ArgsDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.RSA"", 1024);
    }
}",
            GetCSharpResultAt(8, 28, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.RSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyAsymmetricAlgorithmAndKeySize1024ArgsDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.AsymmetricAlgorithm"", 1024);
    }
}",
            GetCSharpResultAt(8, 28, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
        }

        [Fact]
        public void TestCreateFromNameWithRSAAndObjectArray1024ArgsDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""RSA"", new Object[]{1024});
    }
}",
            GetCSharpResultAt(9, 28, UseRSAWithSufficientKeySize.Rule, "RSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyRSAAndObjectArray1024ArgsDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.RSA"", new Object[]{1024});
    }
}",
            GetCSharpResultAt(9, 28, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.RSA"));
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyAsymmetricAlgorithmAndObjectArray1024ArgsDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.AsymmetricAlgorithm"", new Object[]{1024});
    }
}",
            GetCSharpResultAt(9, 28, UseRSAWithSufficientKeySize.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
        }

        [Fact]
        public void TestCaseSensitiveDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""system.security.cryptography.asymmetricalgorithm"", new Object[]{1024});
    }
}",
            GetCSharpResultAt(9, 28, UseRSAWithSufficientKeySize.Rule, "system.security.cryptography.asymmetricalgorithm"));
        }

        [Fact]
        public void TestReturnObjectOfRSADerivedClassNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public RSA TestMethod(RSA rsa)
    {
        return rsa;
    }
}");
        }

        [Fact]
        public void TestCreateObjectOfRSADerivedClassWithInt32ParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var rsaCng = new RSACng(2048);
    }
}");
        }

        [Fact]
        public void TestCreateObjectOfRSADerivedClassWithoutParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var rsaCng = new RSACng();
    }
}");
        }

        [Fact]
        public void TestCreateObjectOfRSADerivedClassWithCngKeyParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(CngKey key)
    {
        var rsaCng = new RSACng(key);
    }
}");
        }

        [Fact]
        public void TestCreateObjectOfRSADerivedClassWithInt32ParameterUnassignedKeySizeDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(int keySize)
    {
        var rsaCng = new RSACng(keySize);
    }
}");
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
        public void TestCreateFromNameWithRSAAndKeySize2048ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""RSA"", 2048);
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithRSAAndKeySizeArgsUnassignedKeySizeNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(int keySize)
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""RSA"", keySize);
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyRSAAndKeySize2048ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.RSA"", 2048);
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyAsymmetricAlgorithmAndKeySize2048ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.AsymmetricAlgorithm"", 2048);
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithECDsaAndObjectArray1024ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""ECDsa"", new Object[]{1024});
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithRSAAndObjectArray2048ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""RSA"", new Object[]{2048});
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyRSAAndObjectArray2048ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.RSA"", new Object[]{2048});
    }
}");
        }

        [Fact]
        public void TestCreateFromNameWithSystemSecurityCryptographyAsymmetricAlgorithmAndObjectArray2048ArgsNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.AsymmetricAlgorithm"", new Object[]{2048});
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
    public void TestMethod(RSA rsa)
    {
        return;
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseRSAWithSufficientKeySize();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseRSAWithSufficientKeySize();
        }
    }
}
