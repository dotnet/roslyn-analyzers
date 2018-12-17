// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ApprovedCipherModeTests : DiagnosticAnalyzerTestBase
    {

        [Fact]
        public void TestECBMode()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.ECB;
    }
}",
            GetCSharpResultAt(9, 22, ApprovedCipherModeAnalyzer.Rule,  "ECB"));

            VerifyBasic(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.ECB
    End Sub
End Module",
            GetBasicResultAt(7, 26, ApprovedCipherModeAnalyzer.Rule,  "ECB"));
        }

        [Fact]
        public void TestOFBMode()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.OFB;
    }
}",
            GetCSharpResultAt(9, 22, ApprovedCipherModeAnalyzer.Rule, "OFB"));

            VerifyBasic(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.OFB
    End Sub
End Module",
            GetBasicResultAt(7, 26, ApprovedCipherModeAnalyzer.Rule,  "OFB"));

        }

        [Fact]
        public void TestCFBMode()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Security.Cryptography;

class TestClass {
    private static void TestMethod () {
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode  = CipherMode.CFB;;
    }
}",
            GetCSharpResultAt(9, 22, ApprovedCipherModeAnalyzer.Rule, "CFB"));

            VerifyBasic(@"
Imports System.Security.Cryptography

Public Module SecurityCenter
    Sub TestSub()
        Dim encripter As System.Security.Cryptography.Aes = System.Security.Cryptography.Aes.Create(""AES"")
        encripter.Mode = CipherMode.CFB
    End Sub
End Module",
            GetBasicResultAt(7, 26, ApprovedCipherModeAnalyzer.Rule, "CFB"));

        }
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ApprovedCipherModeAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ApprovedCipherModeAnalyzer();
        }

    }
}