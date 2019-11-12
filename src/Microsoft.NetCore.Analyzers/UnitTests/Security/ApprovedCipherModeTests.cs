﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.ApprovedCipherModeAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.ApprovedCipherModeAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ApprovedCipherModeTests
    {
        [Fact]
        public async Task TestECBMode()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.ECB;
    }
}",
            GetCSharpResultAt(9, 22, "ECB"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.ECB
    End Sub
End Module",
            GetBasicResultAt(7, 26, "ECB"));
        }

        [Fact]
        public async Task TestOFBMode()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.OFB;
    }
}",
            GetCSharpResultAt(9, 22, "OFB"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.OFB
    End Sub
End Module",
            GetBasicResultAt(7, 26, "OFB"));
        }

        [Fact]
        public async Task TestCFBMode()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.CFB;;
    }
}",
            GetCSharpResultAt(9, 22, "CFB"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.CFB
    End Sub
End Module",
            GetBasicResultAt(7, 26, "CFB"));
        }

        [Fact]
        public async Task TestCBCMode()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.CBC;
    }
}"
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.CBC
    End Sub
End Module"
            );
        }

        [Fact]
        public async Task TestCTSMode()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.CTS;
    }
}"
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.CTS
    End Sub
End Module"
            );
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);

        private static DiagnosticResult GetBasicResultAt(int line, int column, params string[] arguments)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}