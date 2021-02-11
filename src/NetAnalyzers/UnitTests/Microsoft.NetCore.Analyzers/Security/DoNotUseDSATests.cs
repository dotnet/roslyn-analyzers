﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseDSA,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseDSATests
    {
        [Fact]
        public async Task TestCreateObjectOfDSADerivedClassWithoutParameterDiagnostic()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net462.Default,
                TestState =
                {
                    Sources =
                    {
                        @"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var dsaCng = new DSACng();
    }
}",
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(8, 22, "DSACng"),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestCreateObjectOfDSADerivedClassWithCngKeyParameterDiagnostic()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net462.Default,
                TestState =
                {
                    Sources =
                    {
                        @"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(CngKey key)
    {
        var dsaCng = new DSACng(key);
    }
}",
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(8, 22, "DSACng"),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestCreateObjectOfDSADerivedClassWithInt32ParameterAssignedKeySizeDiagnostic()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net462.Default,
                TestState =
                {
                    Sources =
                    {
                        @"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var dsaCng = new DSACng(2048);
    }
}",
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(8, 22, "DSACng"),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestCreateObjectOfDSADerivedClassWithInt32ParameterUnassignedKeySizeDiagnostic()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net462.Default,
                TestState =
                {
                    Sources =
                    {
                        @"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(int keySize)
    {
        var dsaCng = new DSACng(keySize);
    }
}",
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(8, 22, "DSACng"),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestReturnObjectOfDSADerivedClassDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public DSA TestMethod(DSA dsa)
    {
        return dsa;
    }
}",
            GetCSharpResultAt(8, 9, "DSA"));
        }

        [Fact]
        public async Task TestReturnObjectOfDSADerivedClassLocalFunctionDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        DSA GetDSA(DSA dsa) => dsa;
    }
}",
            GetCSharpResultAt(8, 32, "DSA"));
        }

        [Fact]
        public async Task TestCreateWithDSAArgDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""DSA"");
    }
}",
            GetCSharpResultAt(8, 35, "DSA"));
        }

        [Fact]
        public async Task TestCaseSensitiveDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""dSa"");
    }
}",
            GetCSharpResultAt(8, 35, "dSa"));
        }

        [Fact]
        public async Task TestCreateWithSystemSecurityCryptographyDSAArgDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var asymmetricAlgorithm = AsymmetricAlgorithm.Create(""System.Security.Cryptography.DSA"");
    }
}",
            GetCSharpResultAt(8, 35, "System.Security.Cryptography.DSA"));
        }

        [Fact]
        public async Task TestCreateFromNameWithDSAArgDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""DSA"");
    }
}",
            GetCSharpResultAt(8, 28, "DSA"));
        }

        [Fact]
        public async Task TestCreateFromNameWithSystemSecurityCryptographyDSAArgDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod()
    {
        var cryptoConfig = CryptoConfig.CreateFromName(""System.Security.Cryptography.DSA"");
    }
}",
            GetCSharpResultAt(8, 28, "System.Security.Cryptography.DSA"));
        }

        [Fact]
        public async Task TestCreateWithECDsaArgNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestCreateFromNameWithECDsaArgNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestCreateFromNameWithECDsaAndKeySize1024ArgsNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestReturnVoidNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(DSA dsa)
    { 
        return;
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);
    }
}
