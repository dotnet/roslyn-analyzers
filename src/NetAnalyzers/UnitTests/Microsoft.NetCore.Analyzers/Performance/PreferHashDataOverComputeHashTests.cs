// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
        public async Task CSharpBailOutNoFixCase()
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
        byte[] digest = hash.ComputeHash(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSAsync(csInput);
            }
        }

        [Fact]
        public async Task BasicBailOutNoFixCase()
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
        Dim digest As Byte() = sha256.ComputeHash(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBAsync(vbInput);
            }
        }

        [Fact]
        public async Task CSharpCreateHelperUnknownMethodBailOutNoFixCase()
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
    public static void UnknownMethod(HashAlgorithm hasher)
    {{
    }}
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        var hasher = {hashType}.Create();
        UnknownMethod(hasher);
        int aboveLine = 20;
        byte[] digest = hasher.ComputeHash(buffer);
        int belowLine = 10;
    }}
}}
";
                await TestCSAsync(csInput);
            }
        }

        [Fact]
        public async Task BasicCreateHelperUnknownMethodBailOutNoFixCase()
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
    Public Shared Sub UnknownMethod(hasher As HashAlgorithm)
    End Sub
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim hasher As {hashType} = {hashType}.Create()
        UnknownMethod(hasher)
        Dim aboveLine = 20
        Dim digest As Byte() = hasher.ComputeHash(buffer)
        Dim belowLine = 10
    End Sub
End Class
";
                await TestVBAsync(vbInput);
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
                await TestCSAsync(csInput);
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
                await TestVBAsync(vbInput);
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
        int line1 = 20;
        byte[] digest = {{|#0:{hashType}.Create().ComputeHash({{|#1:buffer|}})|}};
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line2 = 10;
        byte[] digest2 = {{|#2:{hashType}.Create().ComputeHash({{|#3:buffer|}}, {{|#4:0|}}, {{|#5:10|}})|}};
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line3 = 10;
        byte[] digest3 = new byte[1024];
        int line4 = 10;
        if ({{|#6:{hashType}.Create().TryComputeHash({{|#7:buffer|}}, {{|#8:digest3|}}, {{|#9:out var i|}})|}})
        {{
            int line5 = 10;
        }}
        int line6 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line3 = 10;
        byte[] digest3 = new byte[1024];
        int line4 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line5 = 10;
        }}
        int line6 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetChainedCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        Dim line1 = 20
        Dim digest As Byte() = {{|#0:{hashType}.Create().ComputeHash({{|#1:buffer|}})|}}
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line2 = 10
        Dim digest As Byte() = {{|#2:{hashType}.Create().ComputeHash({{|#3:buffer|}}, {{|#4:0|}}, {{|#5:10|}})|}}
        Dim line3 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line3 = 10
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        If {{|#6:{hashType}.Create().TryComputeHash({{|#7:buffer|}}, {{|#8:digest|}}, {{|#9:i|}})|}} Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line2 = 10
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line3 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line3 = 10
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetChainedVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        int line1 = 20;
        byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
        int line2 = 10;
        byte[] digest2 = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}};
        int line3 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        {{|#13:var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest = {{|#5:hasher.ComputeHash({{|#6:buffer|}}, {{|#7:0|}}, {{|#8:10|}})|}};
        int line2 = 10;
        byte[] digest2 = {{|#9:hasher.ComputeHash({{|#10:buffer|}}, {{|#11:0|}}, {{|#12:10|}})|}};
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        {{|#22:var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({{|#14:hasher.TryComputeHash({{|#15:buffer|}}, {{|#16:digest3|}}, {{|#17:out var i|}})|}})
        {{
            int line3 = 10;
        }}
        int line4 = 10;
        if ({{|#18:hasher.TryComputeHash({{|#19:buffer|}}, {{|#20:digest3|}}, {{|#21:out i|}})|}})
        {{
            int line5 = 10;
        }}
        int line6 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer);
        int line3 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out i))
        {{
            int line5 = 10;
        }}
        int line6 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationDoubleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        Dim line1 = 20
        Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
        Dim line2 = 10
        Dim digest2 As Byte() = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}}
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#13:Dim hasher As {hashType} = {hashType}.Create()|}}
        Dim line1 = 20
        Dim digest As Byte() = {{|#5:hasher.ComputeHash({{|#6:buffer|}}, {{|#7:0|}}, {{|#8:10|}})|}}
        Dim line2 = 10
        Dim digest2 As Byte() = {{|#9:hasher.ComputeHash({{|#10:buffer|}}, {{|#11:0|}}, {{|#12:10|}})|}}
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#22:Dim hasher As {hashType} = {hashType}.Create()|}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {{|#14:hasher.TryComputeHash({{|#15:buffer|}}, {{|#16:digest|}}, {{|#17:i|}})|}} Then
            Dim line3 = 10
        End If
        Dim line4 = 10
        If {{|#18:hasher.TryComputeHash({{|#19:buffer|}}, {{|#20:digest|}}, {{|#21:i|}})|}} Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer)
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationDoubleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        int line1 = 20;
        byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        {{|#7:var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}};
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        {{|#12:var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest3|}}, {{|#11:out var i|}})|}})
        {{
            int line3 = 10;
        }}
        int line4 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationSingleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        Dim line1 = 20
        Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#7:Dim hasher As {hashType} = {hashType}.Create()|}}
        Dim line1 = 20
        Dim digest As Byte() = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}}
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#12:Dim hasher As {hashType} = {hashType}.Create()|}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest|}}, {{|#11:i|}})|}} Then
            Dim line3 = 10
        End If
        Dim line4 = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationSingleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            int line1 = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int line2 = 10;
            byte[] digest2 = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}};
            int line3 = 10;
        }}
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({{|#13:var hasher = {hashType}.Create()|}})
        {{
            int line1 = 20;
            byte[] digest = {{|#5:hasher.ComputeHash({{|#6:buffer|}}, {{|#7:0|}}, {{|#8:10|}})|}};
            int line2 = 10;
            byte[] digest2 = {{|#9:hasher.ComputeHash({{|#10:buffer|}}, {{|#11:0|}}, {{|#12:10|}})|}};
            int line3 = 10;
        }}
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        using ({{|#22:var hasher = {hashType}.Create()|}})
        {{
            int line1 = 20;
            byte[] digest3 = new byte[1024];
            int line2 = 10;
            if ({{|#14:hasher.TryComputeHash({{|#15:buffer|}}, {{|#16:digest3|}}, {{|#17:out var i|}})|}})
            {{
                int line3 = 10;
            }}
            int line4 = 10;
            if ({{|#18:hasher.TryComputeHash({{|#19:buffer|}}, {{|#20:digest3|}}, {{|#21:out i|}})|}})
            {{
                int line5 = 10;
            }}
            int line6 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer);
        int line3 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out i))
        {{
            int line5 = 10;
        }}
        int line6 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationDoubleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        {{|#4:Using  hasher As {hashType} = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim line2 = 10
            Dim digest2 As Byte() = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}}
        End Using
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#13:Using  hasher As {hashType} = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#5:hasher.ComputeHash({{|#6:buffer|}}, {{|#7:0|}}, {{|#8:10|}})|}}
            Dim line2 = 10
            Dim digest2 As Byte() = {{|#9:hasher.ComputeHash({{|#10:buffer|}}, {{|#11:0|}}, {{|#12:10|}})|}}
        End Using
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#22:Using  hasher As {hashType} = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest = New Byte(1023) {{}}
            Dim i As Integer
            Dim line2 = 10
            If {{|#14:hasher.TryComputeHash({{|#15:buffer|}}, {{|#16:digest|}}, {{|#17:i|}})|}} Then
                Dim line3 = 10
            End If
            Dim line4 = 10
            If {{|#18:hasher.TryComputeHash({{|#19:buffer|}}, {{|#20:digest|}}, {{|#21:i|}})|}} Then
                Dim line5 = 10
            End If
            Dim line6 = 10
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
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer)
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationDoubleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        {{|#2:using var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        {{|#7:using var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}};
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        {{|#12:using var hasher = {hashType}.Create();|}}
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest3|}}, {{|#11:out var i|}})|}})
        {{
            int line3 = 10;
        }}
        int line4 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationSingleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            int line1 = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({{|#7:var hasher = {hashType}.Create()|}})
        {{
            int line1 = 20;
            byte[] digest = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        using ({{|#12:var hasher = {hashType}.Create()|}})
        {{
            int line1 = 20;
            byte[] digest3 = new byte[1024];
            int line2 = 10;
            if ({{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest3|}}, {{|#11:out var i|}})|}})
            {{
                int line3 = 10;
            }}
            int line4 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationSingleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            Dim line1 = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#7:Using hasher As {hashType} = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#12:Using hasher As {hashType} = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest = New Byte(1023) {{}}
            Dim i As Integer
            Dim line2 = 10
            If {{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest|}}, {{|#11:i|}})|}} Then
                Dim line3 = 10
            End If
            Dim line4 = 10
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
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationSingleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            int line1 = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({{|#7:HashAlgorithm hasher = {hashType}.Create()|}})
        {{
            int line1 = 20;
            byte[] digest = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        using ({{|#12:HashAlgorithm hasher = {hashType}.Create()|}})
        {{
            int line1 = 20;
            byte[] digest3 = new byte[1024];
            int line2 = 10;
            if ({{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest3|}}, {{|#11:out var i|}})|}})
            {{
                int line3 = 10;
            }}
            int line4 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationSingleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        {{|#2:Using hasher As HashAlgorithm = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#7:Using hasher As HashAlgorithm = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#12:Using hasher As HashAlgorithm = {hashType}.Create()|}}
            Dim line1 = 20
            Dim digest = New Byte(1023) {{}}
            Dim i As Integer
            Dim line2 = 10
            If {{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest|}}, {{|#11:i|}})|}} Then
                Dim line3 = 10
            End If
            Dim line4 = 10
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
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationSingleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({hashType} {{|#18:hasher = {hashType}.Create()|}}, {{|#19:hasher2 = {hashType}.Create()|}})
        {{
            int aboveLine = 20;
            byte[] digest = {{|#7:hasher.ComputeHash({{|#8:buffer|}}, {{|#9:0|}}, {{|#10:10|}})|}};
            int belowLine = 10;
            byte[] digest2 = {{|#11:hasher2.ComputeHash({{|#12:hasher2.ComputeHash({{|#13:digest|}}, {{|#14:0|}}, {{|#15:10|}})|}}, {{|#16:0|}}, {{|#17:10|}})|}};
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
    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int belowLine = 10;
        byte[] digest2 = {hashType}.HashData({hashType}.HashData(digest.AsSpan(0, 10)).AsSpan(0, 10));
    }}
}}
";
                var hashAlgorithmTypeName = $"System.Security.Cryptography.{hashType}";
                await TestCSAsync(
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
                        .WithLocation(6),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(7)
                        .WithLocation(8)
                        .WithLocation(9)
                        .WithLocation(10)
                        .WithLocation(18),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(11)
                        .WithLocation(12)
                        .WithLocation(16)
                        .WithLocation(17)
                        .WithLocation(19),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(12)
                        .WithLocation(13)
                        .WithLocation(14)
                        .WithLocation(15)
                        .WithLocation(19)
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
                await TestVBAsync(
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
                await TestCSAsync(csInput);
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
        int line1 = 20;
        byte[] digest = {{|#0:new {hashType}Managed().ComputeHash({{|#1:buffer|}})|}};
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line2 = 10;
        byte[] digest2 = {{|#2:new {hashType}Managed().ComputeHash({{|#3:buffer|}}, {{|#4:0|}}, {{|#5:10|}})|}};
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line3 = 10;
        byte[] digest3 = new byte[1024];
        int line4 = 10;
        if({{|#6:new {hashType}Managed().TryComputeHash({{|#7:buffer|}}, {{|#8:digest3|}}, {{|#9:out var i|}})|}})
        {{
            int line5 = 10;
        }}
        int line6 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line3 = 10;
        byte[] digest3 = new byte[1024];
        int line4 = 10;
        if({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line5 = 10;
        }}
        int line6 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetChainedCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        Dim line1 = 20
        Dim digest As Byte() = {{|#0:New {hashType}Managed().ComputeHash({{|#1:buffer|}})|}}
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line2 = 10
        Dim digest As Byte() = {{|#2:New {hashType}Managed().ComputeHash({{|#3:buffer|}}, {{|#4:0|}}, {{|#5:10|}})|}}
        Dim line3 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line3 = 10
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        If {{|#6:New {hashType}Managed().TryComputeHash({{|#7:buffer|}}, {{|#8:digest|}}, {{|#9:i|}})|}} Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line2 = 10
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line3 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line3 = 10
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetChainedVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
    private static void Test2(byte[] buffer)
    {{
    }}
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        Test2({{|#0:new {hashType}Managed().ComputeHash({{|#1:buffer|}})|}});
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line2 = 10;
        Test2({{|#2:new {hashType}Managed().ComputeHash({{|#3:buffer|}}, {{|#4:0|}}, {{|#5:10|}})|}});
        int line3 = 10;
    }}
}}
";

                string csFix = $@"
using System;
using System.Security.Cryptography;

public class Test
{{
    private static void Test2(byte[] buffer)
    {{
    }}
    public static void TestMethod()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        Test2({hashType}.HashData(buffer));
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line2 = 10;
        Test2({hashType}.HashData(buffer.AsSpan(0, 10)));
        int line3 = 10;
    }}
}}
";
                var hashAlgorithmTypeName = $"System.Security.Cryptography.{hashType}";
                await TestCSAsync(
                    csInput,
                    csFix,
                    new[] {
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(5)
                        });
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
    Public Shared Sub Test2(buffer As Byte())
    End Sub
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Test2({{|#0:New {hashType}Managed().ComputeHash({{|#1:buffer|}})|}})
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line2 = 10
        Test2({{|#2:New {hashType}Managed().ComputeHash({{|#3:buffer|}}, {{|#4:0|}}, {{|#5:10|}})|}})
        Dim line3 = 10
    End Sub
End Class
";

                string vbFix = $@"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub Test2(buffer As Byte())
    End Sub
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Test2({hashType}.HashData(buffer))
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line2 = 10
        Test2({hashType}.HashData(buffer.AsSpan(0, 10)))
        Dim line3 = 10
    End Sub
End Class
";
                var hashAlgorithmTypeName = $"System.Security.Cryptography.{hashType}";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(5));
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
            int line1 = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int line2 = 10;
            byte[] digest2 = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}};
            int line3 = 10;
        }}
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({{|#13:var hasher = new {hashType}Managed()|}})
        {{
            int line1 = 20;
            byte[] digest = {{|#5:hasher.ComputeHash({{|#6:buffer|}}, {{|#7:0|}}, {{|#8:10|}})|}};
            int line2 = 10;
            byte[] digest2 = {{|#9:hasher.ComputeHash({{|#10:buffer|}}, {{|#11:0|}}, {{|#12:10|}})|}};
            int line3 = 10;
        }}
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        using ({{|#22:var hasher = new {hashType}Managed()|}})
        {{
            int line1 = 20;
            byte[] digest3 = new byte[1024];
            int line2 = 10;
            if ({{|#14:hasher.TryComputeHash({{|#15:buffer|}}, {{|#16:digest3|}}, {{|#17:out var i|}})|}})
            {{
                int line3 = 10;
            }}
            int line4 = 10;
            if ({{|#18:hasher.TryComputeHash({{|#19:buffer|}}, {{|#20:digest3|}}, {{|#21:out i|}})|}})
            {{
                int line5 = 10;
            }}
            int line6 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer);
        int line3 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
        byte[] digest2 = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line3 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out i))
        {{
            int line5 = 10;
        }}
        int line6 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationDoubleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
        {{|#4:Using  hasher As New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim line2 = 10
            Dim digest2 As Byte() = {{|#2:hasher.ComputeHash({{|#3:buffer|}})|}}
        End Using
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#13:Using  hasher As New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#5:hasher.ComputeHash({{|#6:buffer|}}, {{|#7:0|}}, {{|#8:10|}})|}}
            Dim line2 = 10
            Dim digest2 As Byte() = {{|#9:hasher.ComputeHash({{|#10:buffer|}}, {{|#11:0|}}, {{|#12:10|}})|}}
        End Using
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#22:Using  hasher As New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest = New Byte(1023) {{}}
            Dim i As Integer
            Dim line2 = 10
            If {{|#14:hasher.TryComputeHash({{|#15:buffer|}}, {{|#16:digest|}}, {{|#17:i|}})|}} Then
                Dim line3 = 10
            End If
            Dim line4 = 10
            If {{|#18:hasher.TryComputeHash({{|#19:buffer|}}, {{|#20:digest|}}, {{|#21:i|}})|}} Then
                Dim line5 = 10
            End If
            Dim line6 = 10
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
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer)
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
        Dim digest2 As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line5 = 10
        End If
        Dim line6 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationDoubleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            int line1 = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({{|#7:var hasher = new {hashType}Managed()|}})
        {{
            int line1 = 20;
            byte[] digest = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        using ({{|#12:var hasher = new {hashType}Managed()|}})
        {{
            int line1 = 20;
            byte[] digest3 = new byte[1024];
            int line2 = 10;
            if ({{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest3|}}, {{|#11:out var i|}})|}})
            {{
                int line3 = 10;
            }}
            int line4 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationSingleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            Dim line1 = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#7:Using hasher As New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#12:Using hasher As New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest = New Byte(1023) {{}}
            Dim i As Integer
            Dim line2 = 10
            If {{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest|}}, {{|#11:i|}})|}} Then
                Dim line3 = 10
            End If
            Dim line4 = 10
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
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationSingleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            int line1 = 20;
            byte[] digest = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        using ({{|#7:HashAlgorithm hasher = new {hashType}Managed()|}})
        {{
            int line1 = 20;
            byte[] digest = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}};
            int line2 = 10;
        }}
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        using ({{|#12:HashAlgorithm hasher = new {hashType}Managed()|}})
        {{
            int line1 = 20;
            byte[] digest3 = new byte[1024];
            int line2 = 10;
            if ({{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest3|}}, {{|#11:out var i|}})|}})
            {{
                int line3 = 10;
            }}
            int line4 = 10;
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
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer);
        int line2 = 10;
    }}

    public static void TestMethod2()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest = {hashType}.HashData(buffer.AsSpan(0, 10));
        int line2 = 10;
    }}

    public static void TestMethod3()
    {{
        var buffer = new byte[1024];
        int line1 = 20;
        byte[] digest3 = new byte[1024];
        int line2 = 10;
        if ({hashType}.TryHashData(buffer, digest3, out var i))
        {{
            int line3 = 10;
        }}
        int line4 = 10;
    }}
}}
";
                await TestCSAsync(
                    csInput,
                    csFix,
                    GetCreationSingleInvokeCSDiagnostics($"System.Security.Cryptography.{hashType}"));
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
            Dim line1 = 20
            Dim digest As Byte() = {{|#0:hasher.ComputeHash({{|#1:buffer|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        {{|#7:Using hasher As HashAlgorithm = New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest As Byte() = {{|#3:hasher.ComputeHash({{|#4:buffer|}}, {{|#5:0|}}, {{|#6:10|}})|}}
            Dim line2 = 10
        End Using
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        {{|#12:Using hasher As HashAlgorithm = New {hashType}Managed()|}}
            Dim line1 = 20
            Dim digest = New Byte(1023) {{}}
            Dim i As Integer
            Dim line2 = 10
            If {{|#8:hasher.TryComputeHash({{|#9:buffer|}}, {{|#10:digest|}}, {{|#11:i|}})|}} Then
                Dim line3 = 10
            End If
            Dim line4 = 10
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
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer)
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod2()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest As Byte() = {hashType}.HashData(buffer.AsSpan(0, 10))
        Dim line2 = 10
    End Sub
    Public Shared Sub TestMethod3()
        Dim buffer = New Byte(1023) {{}}
        Dim line1 = 20
        Dim digest = New Byte(1023) {{}}
        Dim i As Integer
        Dim line2 = 10
        If {hashType}.TryHashData(buffer, digest, i) Then
            Dim line3 = 10
        End If
        Dim line4 = 10
    End Sub
End Class
";
                await TestVBAsync(
                    vbInput,
                    vbFix,
                    GetCreationSingleInvokeVBDiagnostics($"System.Security.Cryptography.{hashType}"));
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

        private static async Task TestCSAsync(string source)
        {
            await GetTestCS(source, source, ReferenceAssemblies.Net.Net50).RunAsync();
            await GetTestCS(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestCSAsync(string source, string corrected, params DiagnosticResult[] diagnosticResults)
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

        private static async Task TestVBAsync(string source)
        {
            await GetTestVB(source, source, ReferenceAssemblies.Net.Net50).RunAsync();
            await GetTestVB(source, source, ReferenceAssemblies.NetCore.NetCoreApp31).RunAsync();
        }

        private static async Task TestVBAsync(string source, string corrected, params DiagnosticResult[] diagnosticResults)
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

        private static DiagnosticResult[] GetChainedCSDiagnostics(string hashAlgorithmTypeName)
        {
            return new[] {
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(5),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(6)
                        .WithLocation(7)
                        .WithLocation(8)
                        .WithLocation(9)
                    };
        }

        private static DiagnosticResult[] GetChainedVBDiagnostics(string hashAlgorithmTypeName)
        {
            return new[] {
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(5),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(6)
                        .WithLocation(7)
                        .WithLocation(8)
                        .WithLocation(9)
                    };
        }

        private static DiagnosticResult[] GetCreationDoubleInvokeCSDiagnostics(string hashAlgorithmTypeName)
        {
            return new[] {
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1)
                        .WithLocation(4),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(4),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(5)
                        .WithLocation(6)
                        .WithLocation(7)
                        .WithLocation(8)
                        .WithLocation(13),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(9)
                        .WithLocation(10)
                        .WithLocation(11)
                        .WithLocation(12)
                        .WithLocation(13),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(14)
                        .WithLocation(15)
                        .WithLocation(16)
                        .WithLocation(17)
                        .WithLocation(22),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(18)
                        .WithLocation(19)
                        .WithLocation(20)
                        .WithLocation(21)
                        .WithLocation(22)
                    };
        }

        private static DiagnosticResult[] GetCreationDoubleInvokeVBDiagnostics(string hashAlgorithmTypeName)
        {
            return new[] {
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1)
                        .WithLocation(4),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(2)
                        .WithLocation(3)
                        .WithLocation(4),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(5)
                        .WithLocation(6)
                        .WithLocation(7)
                        .WithLocation(8)
                        .WithLocation(13),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(9)
                        .WithLocation(10)
                        .WithLocation(11)
                        .WithLocation(12)
                        .WithLocation(13),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(14)
                        .WithLocation(15)
                        .WithLocation(16)
                        .WithLocation(17)
                        .WithLocation(22),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(18)
                        .WithLocation(19)
                        .WithLocation(20)
                        .WithLocation(21)
                        .WithLocation(22)
                    };
        }

        private static DiagnosticResult[] GetCreationSingleInvokeCSDiagnostics(string hashAlgorithmTypeName)
        {
            return new[] {
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1)
                        .WithLocation(2),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(5)
                        .WithLocation(6)
                        .WithLocation(7),
                        VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(8)
                        .WithLocation(9)
                        .WithLocation(10)
                        .WithLocation(11)
                        .WithLocation(12)
                    };
        }

        private static DiagnosticResult[] GetCreationSingleInvokeVBDiagnostics(string hashAlgorithmTypeName)
        {
            return new[] {
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(0)
                        .WithLocation(1)
                        .WithLocation(2),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(3)
                        .WithLocation(4)
                        .WithLocation(5)
                        .WithLocation(6)
                        .WithLocation(7),
                        VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                        .WithArguments(hashAlgorithmTypeName)
                        .WithLocation(8)
                        .WithLocation(9)
                        .WithLocation(10)
                        .WithLocation(11)
                        .WithLocation(12)
                    };
        }
    }
}
