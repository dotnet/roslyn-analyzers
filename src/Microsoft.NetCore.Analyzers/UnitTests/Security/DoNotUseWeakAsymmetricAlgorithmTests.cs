// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseWeakAsymmetricAlgorithmTests : DiagnosticAnalyzerTestBase
    {
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
            GetCSharpResultAt(8, 22, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSACng"));
        }

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
            GetCSharpResultAt(8, 22, DoNotUseWeakAsymmetricAlgorithm.Rule, "DSACng"));
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
            GetCSharpResultAt(9, 22, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSACng"));
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
            GetCSharpResultAt(8, 22, DoNotUseWeakAsymmetricAlgorithm.Rule, "DSACng"));
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
            GetCSharpResultAt(8, 22, DoNotUseWeakAsymmetricAlgorithm.Rule, "DSACng"));
        }

        [Fact]
        public void TestReturnObjectOfRSADerivedClassDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public RSA TestMethod(RSA rsa)
    {
        return rsa;
    }
}",
            GetCSharpResultAt(8, 9, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSA"));
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
            GetCSharpResultAt(8, 9, DoNotUseWeakAsymmetricAlgorithm.Rule, "DSA"));
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
            GetCSharpResultAt(8, 35, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSA"));
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
            GetCSharpResultAt(8, 35, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSA"));
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
            GetCSharpResultAt(8, 35, DoNotUseWeakAsymmetricAlgorithm.Rule, "DSA"));
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
            GetCSharpResultAt(8, 35, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.RSA"));
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
            GetCSharpResultAt(8, 35, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.DSA"));
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
            GetCSharpResultAt(8, 35, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSA"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "DSA"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.RSA"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.DSA"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSA"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.RSA"));
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
            GetCSharpResultAt(8, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
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
            GetCSharpResultAt(9, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "RSA"));
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
            GetCSharpResultAt(9, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.RSA"));
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
            GetCSharpResultAt(9, 28, DoNotUseWeakAsymmetricAlgorithm.Rule, "System.Security.Cryptography.AsymmetricAlgorithm"));
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

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseWeakAsymmetricAlgorithm();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseWeakAsymmetricAlgorithm();
        }
    }
}
