// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.Analyzers.Performance;
using Xunit;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferHashDataOverComputeHashAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicPreferHashDataOverComputeHashFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Performance
{
    public class PreferHashDataOverComputeHashVBTests
    {
        [Fact]
        public async Task BasicBailOutNoFixCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod(sha256 As SHA256)
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = sha256.ComputeHash(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVB(vbInput);
        }
        [Fact]
        public async Task BasicCreateHelperBailOutNoFixCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim sha256 As SHA256 = SHA256.Create()
        Dim aboveLine = 20
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVB(vbInput);
        }

        [Fact]
        public async Task BasicCreateHelperChainCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = {|#0:SHA256.Create().ComputeHash({|#1:buffer|})|}
        Dim belowLine = 10
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = SHA256.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVBWithoutVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task BasicCreateHelperNoUsingBlockBailOutNoFixCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim sha256 As SHA256 = SHA256.Create()
        Dim aboveLine = 20
        Dim digest As Byte() = sha256.ComputeHash(buffer)
        Dim belowLine = 10
        Dim digest2 As Byte() = sha256.ComputeHash(buffer)
    End Sub
End Class
";
            await TestVB(vbInput);
        }

        [Fact]
        public async Task BasicCreateHelperNoUsingBlockCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        {|#2:Dim sha256 As SHA256 = SHA256.Create()|}
        Dim aboveLine = 20
        Dim digest As Byte() = {|#0:sha256.ComputeHash({|#1:buffer|})|}
        Dim belowLine = 10
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = SHA256.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVBWithVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlockBailOutNoFixCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Using sha256 As SHA256 = SHA256.Create()
            Dim aboveLine = 20
            Dim digest As Byte() = sha256.ComputeHash(buffer)
            Dim belowLine = 10
            Dim digest2 As Byte() = sha256.ComputeHash(buffer)
        End Using
    End Sub
End Class
";
            await TestVB(vbInput);
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlockCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        {|#2:Using sha256 As SHA256 = SHA256.Create()|}
            Dim aboveLine = 20
            Dim digest As Byte() = {|#0:sha256.ComputeHash({|#1:buffer|})|}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = SHA256.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVBWithVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlocks2Case()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Using {|#2:sha256 As SHA256 = SHA256.Create()|}, sha2562 As SHA256 = SHA256.Create()
            Dim aboveLine = 20
            Dim digest As Byte() = {|#0:sha256.ComputeHash({|#1:buffer|})|}
            Dim belowLine = 10
            Dim digest2 As Byte() = sha2562.ComputeHash(sha2562.ComputeHash(buffer))
        End Using
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Using sha2562 As SHA256 = SHA256.Create()
            Dim aboveLine = 20
            Dim digest As Byte() = SHA256.HashData(buffer)
            Dim belowLine = 10
            Dim digest2 As Byte() = sha2562.ComputeHash(sha2562.ComputeHash(buffer))
        End Using
    End Sub
End Class
";
            await TestVBWithVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task BasicObjectCreationChainCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = {|#0:New SHA256Managed().ComputeHash({|#1:buffer|})|}
        Dim belowLine = 10
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = SHA256.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVBWithoutVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }
        [Fact]
        public async Task BasicObjectCreationChainInArgumentCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Test2({|#0:New SHA256Managed().ComputeHash({|#1:buffer|})|})
        Dim belowLine = 10
    End Sub
    Public Shared Sub Test2(buffer As Byte())
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Test2(SHA256.HashData(buffer))
        Dim belowLine = 10
    End Sub
    Public Shared Sub Test2(buffer As Byte())
    End Sub
End Class
";
            await TestVBWithoutVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }

        [Fact]
        public async Task BasicObjectCreationUsingBlockBailOutNoFixCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Using sha256 As New SHA256Managed()
            Dim aboveLine = 20
            Dim digest As Byte() = sha256.ComputeHash(buffer)
            Dim belowLine = 10
            Dim digest2 As Byte() = sha256.ComputeHash(buffer)
        End Using
    End Sub
End Class
";
            await TestVB(vbInput);
        }

        [Fact]
        public async Task BasicObjectCreationUsingBlockCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        {|#2:Using sha256 As New SHA256Managed()|}
            Dim aboveLine = 20
            Dim digest As Byte() = {|#0:sha256.ComputeHash({|#1:buffer|})|}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

            string vbFix = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim aboveLine = 20
        Dim digest As Byte() = SHA256.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
            await TestVBWithVariable(
                vbInput,
                vbFix,
                "System.Security.Cryptography.SHA256");
        }

        private static VerifyVB.Test GetTest(string source, string corrected, ReferenceAssemblies referenceAssemblies)
        {
            var test = new VerifyVB.Test
            {
                TestCode = source,
                ReferenceAssemblies = referenceAssemblies,
                LanguageVersion = VisualBasic.LanguageVersion.Latest,
                FixedCode = corrected,
            };

            return test;
        }
        private static async Task TestVB(string source)
        {
            await GetTest(source, source, ReferenceAssemblies.Net.Net50).RunAsync();
            await GetTest(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }
        private static async Task TestVBWithVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1)
                .WithLocation(2);
            var test = GetTest(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
            await test.RunAsync();
            await GetTest(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }
        private static async Task TestVBWithoutVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
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
