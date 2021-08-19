// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferHashDataOverComputeHashAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpPreferHashDataOverComputeHashFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferHashDataOverComputeHashAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicPreferHashDataOverComputeHashFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class PreferHashDataOverComputeHashTests
    {
        private const string HashTypeMD5 = "MD5";
        private const string HashTypeSHA1 = "SHA1";
        private const string HashTypeSHA256 = "SHA256";
        private const string HashTypeSHA384 = "SHA384";
        private const string HashTypeSHA512 = "SHA512";

        [Fact]
        public async Task CSharpParameterReferenceCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;
public class Test
{{
    public static void TestMethod({hashType} hash)
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {{|#0:hash.ComputeHash({{|#1:buffer|}})|}};
        int belowLine = 10;
    }}
}}
";
                string csFix = $@"
using System;
using System.Security.Cryptography;
public class Test
{{
    public static void TestMethod({hashType} hash)
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithoutVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicParameterReferenceCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod(sha256 As {hashType})
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {{|#0:sha256.ComputeHash({{|#1:buffer|}})|}}
        Dim belowLine = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod(sha256 As {hashType})
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithoutVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperBailOutNoFixCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        var hasher = {hashType}.Create();
        int aboveLine = 20;
        int belowLine = 10;
    }}
}}
";
                await TestCS(csInput);
            }
        }

        [Fact]
        public async Task BasicCreateHelperBailOutNoFixCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim sha256 As {hashType} = {hashType}.Create()
        Dim aboveLine = 20
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVB(vbInput);
            }
        }

        [Fact]
        public async Task CSharpCreateHelperChainCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {{|#0:{hashType}.Create().ComputeHash({{|#1:buffer|}})|}};
        int belowLine = 10;
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithoutVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicCreateHelperChainCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {{|#0:{hashType}.Create().ComputeHash({{|#1:buffer|}})|}}
        Dim belowLine = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithoutVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperNoUsingStatement2Case()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        {{|#4:var hasher = {hashType}.Create();|}}
        int aboveLine = 20;
        byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
        int belowLine = 10;
        byte[] digest2 = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}};
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
        byte[] digest2 = {hashType}.HashData(buffer);
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}",
                    expectedDiagnosticCount: 2);
            }
        }

        [Fact]
        public async Task BasicCreateHelperNoUsingBlock2Case()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#4:Dim hasher As {hashType} = {hashType}.Create()|}}
        Dim aboveLine = 20
        Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
        Dim belowLine = 10
        Dim digest2 As Byte() = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}}
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer)
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}",
                    expectedDiagnosticCount: 2);
            }
        }

        [Fact]
        public async Task CSharpCreateHelperNoUsingStatementCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        {{|#2:var hasher = {hashType}.Create();|}}
        int aboveLine = 20;
        byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
        int belowLine = 10;
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicCreateHelperNoUsingBlockCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#2:Dim hasher As {hashType} = {hashType}.Create()|}}
        Dim aboveLine = 20
        Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
        Dim belowLine = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatement2Case()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({{|#4:var hasher = {hashType}.Create()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
            byte[] digest2 = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}};
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
        byte[] digest2 = {hashType}.HashData(buffer);
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}",
                    expectedDiagnosticCount: 2);
            }
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlock2Case()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#4:Using hasher As {hashType} = {hashType}.Create()|}}
            Dim aboveLine = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
            Dim digest2 As Byte() = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}}
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer)
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}",
                    expectedDiagnosticCount: 2);
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUsingDeclarationCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        {{|#2:using var hasher = {hashType}.Create();|}}
        int aboveLine2 = 30;
        byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
        int belowLine = 10;
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        int aboveLine2 = 30;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({{|#2:var hasher = {hashType}.Create()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlockCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#2:Using hasher As {hashType} = {hashType}.Create()|}}
            Dim aboveLine = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementCase2()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        byte[] digest;
        using ({{|#2:{hashType} hasher = {hashType}.Create()|}})
        {{
            int aboveLine = 20;
            digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        byte[] digest;
        int aboveLine = 20;
        digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlockCase2()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim digest As Byte()
        {{|#2:Using hasher As {hashType} = {hashType}.Create()|}}
            Dim aboveLine = 20
            digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim digest As Byte()
        Dim aboveLine = 20
        digest = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatementCastedCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({{|#2:HashAlgorithm hasher = {hashType}.Create()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlockCastedCase()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim digest As Byte()
        {{|#2:Using hasher As HashAlgorithm = {hashType}.Create()|}}
            Dim aboveLine = 20
            digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim digest As Byte()
        Dim aboveLine = 20
        digest = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUsingStatements2Case()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({hashType} {{|#5:hasher = {hashType}.Create()|}}, {{|#6:hasher2 = {hashType}.Create()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
            byte[] digest2 = {{|#2:hasher2.ComputeHash({{|#3:hasher2.ComputeHash({{|#4:digest|}})|}})|}};
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
        byte[] digest2 = {hashType}.HashData({hashType}.HashData(digest));
    }}
}}
";
                var hashAlgorithmTypeName = $"System.Security.Cryptography.{hashType}";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    new[] {
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1)
                        .WithLocation(5),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(6),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(6)
                    });
            }
        }

        [Fact]
        public async Task BasicCreateHelperUsingBlocks2Case()
        {
            await TestWithType(HashTypeMD5);
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Using {{|#5:hasher As {hashType} = {hashType}.Create()|}}, {{|#6:hasher2 As {hashType} = {hashType}.Create()|}}
            Dim aboveLine = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
            Dim digest2 As Byte() = {{|#2:hasher2.ComputeHash({{|#3:hasher2.ComputeHash({{|#4:digest|}})|}})|}}
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
        Dim digest2 As Byte() = {hashType}.HashData({hashType}.HashData(digest))
    End Sub
End Class
";
                var hashAlgorithmTypeName = $"System.Security.Cryptography.{hashType}";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    new[] {
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1)
                        .WithLocation(5),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(6),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(6)
                    });
            }
        }

        [Fact]
        public async Task CSharpObjectCreationBailOutNoFixCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        var sha256 = new {hashType}Managed();
        int aboveLine = 20;
        int belowLine = 10;
    }}
}}
";
                await TestCS(csInput);
            }
        }

        [Fact]
        public async Task CSharpObjectCreationChainCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {{|#0:new {hashType}Managed().ComputeHash({{|#1:buffer|}})|}};
        int belowLine = 10;
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithoutVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicObjectCreationChainCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {{|#0:New {hashType}Managed().ComputeHash({{|#1:buffer|}})|}}
        Dim belowLine = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithoutVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpObjectCreationChainInArgumentCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        Test2({{|#0:new {hashType}Managed().ComputeHash({{|#1:buffer|}})|}});
        int belowLine = 10;
    }}
    private static void Test2(byte[] buffer)
    {{
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        Test2({hashType}.HashData(buffer));
        int belowLine = 10;
    }}
    private static void Test2(byte[] buffer)
    {{
    }}
}}
";
                await TestCSWithoutVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicObjectCreationChainInArgumentCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Test2({{|#0:New {hashType}Managed().ComputeHash({{|#1:buffer|}})|}})
        Dim belowLine = 10
    End Sub
    Public Shared Sub Test2(buffer As Byte())
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Test2({hashType}.HashData(buffer))
        Dim belowLine = 10
    End Sub
    Public Shared Sub Test2(buffer As Byte())
    End Sub
End Class
";
                await TestVBWithoutVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpObjectCreationUsingStatement2Case()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({{|#4:var hasher = new {hashType}Managed()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
            byte[] digest2 = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}};
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
        byte[] digest2 = {hashType}.HashData(buffer);
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}",
                    expectedDiagnosticCount: 2);
            }
        }

        [Fact]
        public async Task BasicObjectCreationUsingBlock2Case()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#4:Using hasher As New {hashType}Managed()|}}
            Dim aboveLine = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
            Dim digest2 As Byte() = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}}
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer)
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}",
                    expectedDiagnosticCount: 2);
            }
        }

        [Fact]
        public async Task CSharpObjectCreationUsingStatementCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({{|#2:var hasher = new {hashType}Managed()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicObjectCreationUsingBlockCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#2:Using hasher As New {hashType}Managed()|}}
            Dim aboveLine = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task CSharpObjectCreationUsingStatementCastedCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string csInput = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        using ({{|#2:HashAlgorithm hasher = new {hashType}Managed()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int belowLine = 10;
        }}
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSWithVariable(
                    csInput,
                    csFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        [Fact]
        public async Task BasicObjectCreationUsingBlockCastedCase()
        {
            await TestWithType(HashTypeSHA1);
            await TestWithType(HashTypeSHA256);
            await TestWithType(HashTypeSHA384);
            await TestWithType(HashTypeSHA512);

            static async Task TestWithType(string hashType)
            {
                string vbInput = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        {{|#2:Using hasher As HashAlgorithm = New {hashType}Managed()|}}
            Dim aboveLine = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim aboveLine = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBWithVariable(
                    vbInput,
                    vbFix,
                    $"System.Security.Cryptography.{hashType}");
            }
        }

        private static VerifyCS.Test GetTestCS(string source, string corrected, ReferenceAssemblies referenceAssemblies)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = referenceAssemblies,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
                FixedCode = corrected,
            };
            return test;
        }

        private static async Task TestCS(string source)
        {
            await GetTestCS(source, source, ReferenceAssemblies.Net.Net50).RunAsync();
            await GetTestCS(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestCSWithVariable(string source, string corrected, string hashAlgorithmTypeName, int expectedDiagnosticCount = 1)
        {
            var test = GetTestCS(source, corrected, ReferenceAssemblies.Net.Net50);

            var lastId = expectedDiagnosticCount * 2;
            for (int i = 0; i < expectedDiagnosticCount; i++)
            {
                var expected = VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                    .WithArguments(hashAlgorithmTypeName)
                    .WithLocation(i * 2)
                    .WithLocation(i * 2 + 1)
                    .WithLocation(lastId);
                test.ExpectedDiagnostics.Add(expected);
            }

            await test.RunAsync();
            await GetTestCS(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestCSWithVariable(string source, string corrected, params DiagnosticResult[] diagnosticResults)
        {
            var test = GetTestCS(source, corrected, ReferenceAssemblies.Net.Net50);

            for (int i = 0; i < diagnosticResults.Length; i++)
            {
                var expected = diagnosticResults[i];
                test.ExpectedDiagnostics.Add(expected);
            }

            await test.RunAsync();
            await GetTestCS(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestCSWithoutVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1);

            var test = GetTestCS(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
            await test.RunAsync();
            await GetTestCS(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static VerifyVB.Test GetTestVB(string source, string corrected, ReferenceAssemblies referenceAssemblies)
        {
            var test = new VerifyVB.Test
            {
                TestCode = source,
                ReferenceAssemblies = referenceAssemblies,
                LanguageVersion = CodeAnalysis.VisualBasic.LanguageVersion.Latest,
                FixedCode = corrected,
            };
            return test;
        }

        private static async Task TestVB(string source)
        {
            await GetTestVB(source, source, ReferenceAssemblies.Net.Net50).RunAsync();
            await GetTestVB(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestVBWithVariable(string source, string corrected, string hashAlgorithmTypeName, int expectedDiagnosticCount = 1)
        {
            var test = GetTestVB(source, corrected, ReferenceAssemblies.Net.Net50);
            var lastId = expectedDiagnosticCount * 2;
            for (int i = 0; i < expectedDiagnosticCount; i++)
            {
                var expected = VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                    .WithArguments(hashAlgorithmTypeName)
                    .WithLocation(i * 2)
                    .WithLocation(i * 2 + 1)
                    .WithLocation(lastId);
                test.ExpectedDiagnostics.Add(expected);
            }
            await test.RunAsync();
            await GetTestVB(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestVBWithVariable(string source, string corrected, params DiagnosticResult[] diagnosticResults)
        {
            var test = GetTestVB(source, corrected, ReferenceAssemblies.Net.Net50);

            for (int i = 0; i < diagnosticResults.Length; i++)
            {
                var expected = diagnosticResults[i];
                test.ExpectedDiagnostics.Add(expected);
            }

            await test.RunAsync();
            await GetTestVB(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestVBWithoutVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1);
            var test = GetTestVB(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
            await test.RunAsync();
            await GetTestVB(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }
    }
}
