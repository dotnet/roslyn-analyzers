// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.Analyzers.Performance;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferHashDataOverComputeHashAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpPreferHashDataOverComputeHashFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Performance
{
    public class PreferHashDataOverComputeHashCSTests
    {
        [Fact]
        public async Task CSharpBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod(SHA256 hashAlgorithm)
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = hashAlgorithm.ComputeHash(buffer);
        int belowLine = 10;
    }
}
";
            await TestCS(csInput);
        }
        [Fact]
        public async Task CSharpCreateHelperBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        var sha256 = SHA256.Create();
        int aboveLine = 20;
        int belowLine = 10;
    }
}
";
            await TestCS(csInput);
        }

        [Fact]
        public async Task CSharpCreateHelperChainCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {|#0:SHA256.Create().ComputeHash({|#1:buffer|})|};
        int belowLine = 10;
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithoutVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpCreateHelperNoUsingStatementBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        var sha256 = SHA256.Create();
        int aboveLine = 20;
        byte[] digest = sha256.ComputeHash(buffer);
        int belowLine = 10;
        byte[] digest2 = sha256.ComputeHash(buffer);
    }
}
";
            await TestCS(csInput);
        }

        [Fact]
        public async Task CSharpCreateHelperNoUsingStatementCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        {|#2:var sha256 = SHA256.Create();|}
        int aboveLine = 20;
        byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
        int belowLine = 10;
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using (var sha256 = SHA256.Create())
        {
            int aboveLine = 20;
            byte[] digest = sha256.ComputeHash(buffer);
            int belowLine = 10;
            byte[] digest2 = sha256.ComputeHash(buffer);
        }
    }
}
";
            await TestCS(csInput);
        }

        [Fact]
        public async Task CSharpCreateHelperUsingDeclarationCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        {|#2:using var sha256 = SHA256.Create();|}
        int aboveLine2 = 30;
        byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
        int belowLine = 10;
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        int aboveLine2 = 30;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using ({|#2:var sha256 = SHA256.Create()|})
        {
            int aboveLine = 20;
            byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
            int belowLine = 10;
        }
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementCase2()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        byte[] digest;
        using ({|#2:SHA256 sha256 = SHA256.Create()|})
        {
            int aboveLine = 20;
            digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
            int belowLine = 10;
        }
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        byte[] digest;
        int aboveLine = 20;
        digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementCastedCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using ({|#2:HashAlgorithm sha256 = SHA256.Create()|})
        {
            int aboveLine = 20;
            byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
            int belowLine = 10;
        }
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatements2Case()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using (SHA256 {|#2:sha256 = SHA256.Create()|}, sha2562 = SHA256.Create())
        {
            int aboveLine = 20;
            byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
            int belowLine = 10;
            byte[] digest2 = sha2562.ComputeHash(sha2562.ComputeHash(digest));
        }
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using (SHA256 sha2562 = SHA256.Create())
        {
            int aboveLine = 20;
            byte[] digest = SHA256.HashData(buffer);
            int belowLine = 10;
            byte[] digest2 = sha2562.ComputeHash(sha2562.ComputeHash(digest));
        }
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpObjectCreationBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        var sha256 = new SHA256Managed();
        int aboveLine = 20;
        int belowLine = 10;
    }
}
";
            await TestCS(csInput);
        }

        [Fact]
        public async Task CSharpObjectCreationChainCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {|#0:new SHA256Managed().ComputeHash({|#1:buffer|})|};
        int belowLine = 10;
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithoutVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpObjectCreationChainInArgumentCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        Test2({|#0:new SHA256Managed().ComputeHash({|#1:buffer|})|});
        int belowLine = 10;
    }
    private static void Test2(byte[] buffer)
    {
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        Test2(SHA256.HashData(buffer));
        int belowLine = 10;
    }
    private static void Test2(byte[] buffer)
    {
    }
}
";
            await TestCSWithoutVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpObjectCreationUsingStatementBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using (var sha256 = new SHA256Managed())
        {
            int aboveLine = 20;
            byte[] digest = sha256.ComputeHash(buffer);
            int belowLine = 10;
            byte[] digest2 = sha256.ComputeHash(buffer);
        }
    }
}
";
            await TestCS(csInput);
        }

        [Fact]
        public async Task CSharpObjectCreationUsingStatementCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using ({|#2:var sha256 = new SHA256Managed()|})
        {
            int aboveLine = 20;
            byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
            int belowLine = 10;
        }
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task CSharpObjectCreationUsingStatementCastedCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        using ({|#2:HashAlgorithm sha256 = new SHA256Managed()|})
        {
            int aboveLine = 20;
            byte[] digest = {|#0:sha256.ComputeHash({|#1:buffer|})|};
            int belowLine = 10;
        }
    }
}
";

            string csFix = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod()
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = SHA256.HashData(buffer);
        int belowLine = 10;
    }
}
";
            await TestCSWithVariable(
                csInput,
                csFix,
                "System.Security.Cryptography.SHA256");
        }

        private static VerifyCS.Test GetTest(string source, string corrected, ReferenceAssemblies referenceAssemblies)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = referenceAssemblies,
                LanguageVersion = LanguageVersion.Preview,
                FixedCode = corrected,
            };
            return test;
        }

        private static async Task TestCS(string source)
        {
            await GetTest(source, source, ReferenceAssemblies.Net.Net50).RunAsync();
            await GetTest(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestCSWithVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1)
                .WithLocation(2);

            var test = GetTest(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
            await test.RunAsync();
            await GetTest(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestCSWithoutVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1);

            var test = GetTest(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
            await test.RunAsync();
            await GetTest(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }
    }
}
