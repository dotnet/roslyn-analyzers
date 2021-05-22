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
        [Fact]
        public async Task CSharpBailOutNoFixCase()
        {
            string csInput = @"
using System;
using System.Security.Cryptography;

public class Test
{
    public static void TestMethod(SHA256 sha256)
    {
        var buffer = new byte[1024];
        int aboveLine = 20;
        byte[] digest = sha256.ComputeHash(buffer);
        int belowLine = 10;
    }
}
";
            await TestCS(csInput);
        }

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
        public async Task BasicCreateHelperUsingBlockCase2()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim digest As Byte()
        {|#2:Using sha256 As SHA256 = SHA256.Create()|}
            Dim aboveLine = 20
            digest = {|#0:sha256.ComputeHash({|#1:buffer|})|}
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
        Dim digest As Byte()
        Dim aboveLine = 20
        digest = SHA256.HashData(buffer)
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
        public async Task BasicCreateHelperUsingBlockCastedCaseNoFix()
        {
            // unable to get type info for SHA256
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        Dim digest As Byte()
        Using sha256 As HashAlgorithm = SHA256.Create()
            Dim aboveLine = 20
            digest = sha256.ComputeHash(buffer)
            Dim belowLine = 10
        End Using
    End Sub
End Class
";

            await TestVB(vbInput);
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

        [Fact]
        public async Task BasicObjectCreationUsingBlockCastedCase()
        {
            string vbInput = @"
Imports System
Imports System.Security.Cryptography

Public Class Test
    Public Shared Sub TestMethod()
        Dim buffer = New Byte(1023) {}
        {|#2:Using sha256 As HashAlgorithm = New SHA256Managed()|}
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

        private static async Task TestCSWithVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyCS.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1)
                .WithLocation(2);

            var test = GetTestCS(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
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

        private static async Task TestVBWithVariable(string source, string corrected, string hashAlgorithmTypeName)
        {
            var expected = VerifyVB.Diagnostic(PreferHashDataOverComputeHashAnalyzer.StringRule)
                .WithArguments(hashAlgorithmTypeName)
                .WithLocation(0)
                .WithLocation(1)
                .WithLocation(2);
            var test = GetTestVB(source, corrected, ReferenceAssemblies.Net.Net50);
            test.ExpectedDiagnostics.Add(expected);
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
