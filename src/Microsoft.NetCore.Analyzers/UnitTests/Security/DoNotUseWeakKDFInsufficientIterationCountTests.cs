// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseWeakKDFInsufficientIterationCountTests : DiagnosticAnalyzerTestBase
    {
        private const int SufficientIterationCount = 100000;

        [Fact]
        public void TestConstructorWithStringAndByteArrayParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestAssignIterationCountDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100;
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(10, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestAssignIterationsParameterMaybeChangedDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var iterations = 100;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            iterations = 100000;
        }

        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(18, 9, DoNotUseWeakKDFInsufficientIterationCount.MaybeUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestAssignIterationCountPropertyMaybeChangedDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            rfc2898DeriveBytes.IterationCount = 100000;
        }

        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(18, 9, DoNotUseWeakKDFInsufficientIterationCount.MaybeUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestPassRfc2898DeriveBytesAsParameterInterproceduralDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100;
        InvokeGetBytes(rfc2898DeriveBytes, cb);
    }

    public void InvokeGetBytes(Rfc2898DeriveBytes rfc2898DeriveBytes, int cb)
    {
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(15, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestReturnRfc2898DeriveBytesInterproceduralDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = GetRfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.GetBytes(cb);
    }

    public Rfc2898DeriveBytes GetRfc2898DeriveBytes(string password, byte[] salt)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100;
    
        return rfc2898DeriveBytes;
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithStringAndIntParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, int saltSize, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltSize);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithStringAndByteArrayAndIntParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithByteArrayAndByteArrayAndIntParametersLowIterationsDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithStringAndIntAndIntParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, int saltSize, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltSize, 100);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithByteArrayAndByteArrayAndIntAndHashAlgorithmNameParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, HashAlgorithmName hashAlgorithm, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100, hashAlgorithm);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithStringAndByteArrayAndIntAndHashAlgorithmNameParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, HashAlgorithmName hashAlgorithm, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100, hashAlgorithm);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestConstructorWithStringAndIntAndIntAndHashAlgorithmNameParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, int saltSize, HashAlgorithmName hashAlgorithm, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltSize, 100, hashAlgorithm);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseWeakKDFInsufficientIterationCount.DefinitelyUseWeakKDFInsufficientIterationCountRule, SufficientIterationCount));
        }

        [Fact]
        public void TestAssignIterationCountNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100000;
        rfc2898DeriveBytes.GetBytes(cb);
    }
}");
        }

        [Fact]
        public void TestPassRfc2898DeriveBytesAsParameterInterproceduralNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100000;
        InvokeGetBytes(rfc2898DeriveBytes, cb);
    }

    public void InvokeGetBytes(Rfc2898DeriveBytes rfc2898DeriveBytes, int cb)
    {
        rfc2898DeriveBytes.GetBytes(cb);
    }
}");
        }

        [Fact]
        public void TestReturnRfc2898DeriveBytesInterproceduralNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        var rfc2898DeriveBytes = GetRfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.GetBytes(cb);
    }

    public Rfc2898DeriveBytes GetRfc2898DeriveBytes(string password, byte[] salt)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
        rfc2898DeriveBytes.IterationCount = 100000;
    
        return rfc2898DeriveBytes;
    }
}");
        }

        [Fact]
        public void TestConstructorWithByteArrayAndByteArrayAndIntParametersUnassignedIterationsNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}");
        }

        [Fact]
        public void TestConstructorWithByteArrayAndByteArrayAndIntParametersHighIterationsNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(byte[] password, byte[] salt, int iterations, int cb)
    {
        var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100000);
        rfc2898DeriveBytes.GetBytes(cb);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseWeakKDFInsufficientIterationCount();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseWeakKDFInsufficientIterationCount();
        }
    }
}
