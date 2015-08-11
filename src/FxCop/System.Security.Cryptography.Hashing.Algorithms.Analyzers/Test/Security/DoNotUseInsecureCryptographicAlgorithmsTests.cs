// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the projecVerifyCSharp(t root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;                                                        

namespace System.Security.Cryptography.Hashing.Algorithms.Analyzers.UnitTests
{
    public class DoNotUseInsecureCryptographicAlgorithmsTests : DiagnosticAnalyzerTestBase
    {
        #region CA5350 
                
        [Fact]
	    public void CA5350UseMD5CreateInMethodDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var md5 = MD5.Create();
        }
    }
}", 
            GetCSharpResultAt(10, 23, CA5350RuleName, CA5350Message));

            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestSub()
        Dim md5alg As MD5 = MD5.Create()
    End Sub
End Module",
            GetBasicResultAt(6, 29, CA5350RuleName, CA5350Message));
        }

        [Fact]
	    public void CA5350UseMD5CreateInPropertyDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public MD5 GetMD5 => MD5.Create();
    }
}",
            GetCSharpResultAt(7, 30, CA5350RuleName, CA5350Message));
        }

        [Fact]
        public void CA5350UseMD5CreateInGetDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {                                     
        public HashAlgorithm GetAlg
        {
            get { return MD5.Create(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5350RuleName, CA5350Message));
        }

        [Fact]
	    public void CA5350UseMD5CreateInFieldDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass1
    {
        public HashAlgorithm Alg = MD5.Create();  
    }
}",
            GetCSharpResultAt(7, 36, CA5350RuleName, CA5350Message));
        }   
        
        [Fact]
		public void CA5350UseMD5CreateInLambdaExpression()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
using System.Threading.Tasks;
namespace TestNamespace
{
    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => { MD5.Create(); });
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5350RuleName, CA5350Message));
        }
        
        [Fact]
		public void CA5350UseMD5CreateInAnonymousMethodExpression()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { MD5.Create(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void CA5350CreateObjectFromMD5DerivedClass()
        {
            VerifyCSharp( new[] {
//Test0
@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(byte[] inBytes)
        {
            var myMd5 = new MyMD5();
        }
    }
}",
//Test1
@"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyMD5 : MD5
    {
        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            throw new NotImplementedException();
        }

        protected override byte[] HashFinal()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 25, CA5350RuleName, CA5350Message));
        }

        #endregion

        #region CA5354  

        [Fact]
        public void CA5354UseSHA1CreateInMethodDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var sha1 = SHA1.Create();
        }
    }
}",
            GetCSharpResultAt(10, 24, CA5354RuleName, CA5354Message));

            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestSub()
        Dim sha1alg As SHA1 = SHA1.Create()
    End Sub
End Module",
            GetBasicResultAt(6, 31, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354UseSHA1CreateInPropertyDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public SHA1 GetSHA1 => SHA1.Create();
    }
}",
            GetCSharpResultAt(7, 32, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354UseSHA1CreateInGetDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {                                     
        public HashAlgorithm GetAlg
        {
            get { return SHA1.Create(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354UseSHA1CreateInFieldDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass1
    {
        public HashAlgorithm Alg = SHA1.Create();  
    }
}",
            GetCSharpResultAt(7, 36, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354UseSHA1CreateInLambdaExpression()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
using System.Threading.Tasks;
namespace TestNamespace
{
    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => { SHA1.Create(); });
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354UseSHA1CreateInAnonymousMethodExpression()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { SHA1.Create(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateObjectFromSHA1DerivedClass()
        {
            VerifyCSharp(new[] {
//Test0
@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(byte[] inBytes)
        {
            var mySHA1 = new MySHA1();
        }
    }
}",
//Test1
@"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MySHA1 : SHA1
    {
        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            throw new NotImplementedException();
        }

        protected override byte[] HashFinal()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 26, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354UseSHA1CryptoServiceProviderInMethodDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var sha1 = new SHA1CryptoServiceProvider();
        }
    }
}",
            GetCSharpResultAt(10, 24, CA5354RuleName, CA5354Message));

            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim SHA1alg As New SHA1CryptoServiceProvider
    End Sub
End Module",
            GetBasicResultAt(6, 24, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateHMACSHA1ObjectInMethodDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var hmacsha1 = new HMACSHA1();
        }
    }
}",
            GetCSharpResultAt(10, 28, CA5354RuleName, CA5354Message));

            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestSub()
        Dim hmacsha1 As HMACSHA1 = New HMACSHA1()
    End Sub
End Module",
            GetBasicResultAt(6, 36, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateHMACSHA1ObjectInPropertyDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public HMAC GetHMACSHA1 => new HMACSHA1;
    }
}",
            GetCSharpResultAt(7, 36, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateHMACSHA1ObjectInGetDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {                                     
        public HMAC GetAlg
        {
            get { return new HMACSHA1(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateHMACSHA1ObjectInFieldDeclaration()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass1
    {
        public HMAC Alg = new HMACSHA1();  
    }
}",
            GetCSharpResultAt(7, 27, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateHMACSHA1ObjectInLambdaExpression()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
using System.Threading.Tasks;
namespace TestNamespace
{
    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => { new HMACSHA1(); });
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateHMACSHA1ObjectInAnonymousMethodExpression()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new HMACSHA1(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5354RuleName, CA5354Message));
        }

        [Fact]
        public void CA5354CreateObjectFromHMACSHA1DerivedClass()
        {
            VerifyCSharp(new[] {
//Test0
@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(byte[] inBytes)
        {
            var myHMACSHA1 = new MyHMACSHA1();
        }
    }
}",
//Test1
@"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{ 
    class MyHMACSHA1 : HMACSHA1
    {
        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            throw new NotImplementedException();
        }

        protected override byte[] HashFinal()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 30, CA5354RuleName, CA5354Message));
        }
        #endregion 

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotUseInsecureCryptographicAlgorithmsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotUseInsecureCryptographicAlgorithmsAnalyzer();
        }

        private const string CA5350RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseMD5RuleId;
        private const string CA5354RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseSHA1RuleId;

        private readonly string CA5350Message = SystemSecurityCryptographyHashingAlgorithmsAnalyzersResources.DoNotUseMD5;
        private readonly string CA5354Message = SystemSecurityCryptographyHashingAlgorithmsAnalyzersResources.DoNotUseSHA1; 
    }
}
