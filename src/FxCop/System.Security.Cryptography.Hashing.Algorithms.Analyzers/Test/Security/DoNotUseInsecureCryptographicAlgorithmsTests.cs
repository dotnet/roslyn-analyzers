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
	    public void UseMD5CreateShouldGenerateDiagnostic()
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
        }

        [Fact]
	    public void UseMD5CreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public MD5 GetMD5
        {
            get { return MD5.Create(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5350RuleName, CA5350Message));
        }

        [Fact]
	    public void UseMD5CreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        MD5 privateMd5;
        public MD5 GetMD5
        {
            set
            {
                if (value == null)
                    privateMd5 = MD5.Create();
                else
                    privateMd5 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5350RuleName, CA5350Message));
        }

        [Fact]
		public void UseMD5CreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        MD5 privateMd5 = MD5.Create();
    }
}",
            GetCSharpResultAt(7, 26, CA5350RuleName, CA5350Message));
        }

        [Fact]
		public void UseMD5CreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<MD5> md5List = new List<MD5>() { MD5.Create() };
    }
}",
            GetCSharpResultAt(8, 47, CA5350RuleName, CA5350Message));
        }

        [Fact]
		public void UseMD5CreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        MD5[] md5List = new MD5[] { MD5.Create() };
    }
}",
            GetCSharpResultAt(7, 37, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseMD5CreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, MD5> md5List = new Dictionary<int, MD5>() { { 1, MD5.Create() } };
    }
}",
            GetCSharpResultAt(8, 74, CA5350RuleName, CA5350Message));
        }
        
        [Fact]
		public void UseMD5CreateInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
            try
            {
                var md5 = MD5.Create();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5350RuleName, CA5350Message));
        }

        [Fact]
		public void UseMD5CreateInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { var md5 = MD5.Create(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5350RuleName, CA5350Message));
        }

        [Fact]
		public void UseMD5CreateInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { }
            finally { var md5 = MD5.Create(); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseMD5CreateAwaitShouldGenerateDiagnostic()
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
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5350RuleName, CA5350Message));
        }
        
        [Fact]
		public void UseMD5CreateWithDelegateShouldGenerateDiagnostic()
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
		public void UseMD5DerivedClassShouldGenerateDiagnostic()
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
                
        [Fact]
		public void UseMD5CreateInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestSub()
        Dim md5alg As MD5 = MD5.Create()
    End Sub
End Module",
            GetBasicResultAt(6, 29, CA5350RuleName, CA5350Message));
        }
        #endregion 

        #region CA5354
        
        [Fact]
        public void UseSHA1DerivedClassShouldGenerateDiagnostic()
        {
           VerifyCSharp( new[] {
                //Test0
                @"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var sha1 = new MySHA1();
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
            GetCSharpResultAt(10, 24, CA5354RuleName, CA5354Message));
        } 
        
        [Fact]
        public void UseHMACSHA1ShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var hmaSHA1 = new HMACSHA1();
        }
    }
}",
            GetCSharpResultAt(10, 27, CA5354RuleName, CA5354Message));
        }
        
        [Fact]
        public void UseHMACSHA1WithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public HMACSHA1 GetSHA1
        {
            get { return new HMACSHA1(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5354RuleName, CA5354Message));
        }
        
        [Fact]
        public void UseHMACSHA1WithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACSHA1 privateSHA1;
        public HMACSHA1 GetSHA1
        {
            set
            {
                if (value == null)
                    privateSHA1 = new HMACSHA1();
                else
                    privateSHA1 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 35, CA5354RuleName, CA5354Message));
        }
        
        [Fact]
        public void UseHMACSHA1WithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACSHA1 privateSHA1 = new HMACSHA1();
    }
}",
            GetCSharpResultAt(7, 32, CA5354RuleName, CA5354Message));
        }
        
        [Fact]
        public void UseHMACSHA1WithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<HMACSHA1> SHA1List = new List<HMACSHA1>() { new HMACSHA1() };
    }
}",
            GetCSharpResultAt(8, 58, CA5354RuleName, CA5354Message));
        }
        
        [Fact]
        public void UseHMACSHA1WithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACSHA1[] SHA1List = new HMACSHA1[] { new HMACSHA1() };
    }
}",
            GetCSharpResultAt(7, 48, CA5354RuleName, CA5354Message));
        }   
        
        [Fact]
        public void UseHMACSHA1WithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, HMACSHA1> SHA1List = new Dictionary<int, HMACSHA1>() { { 1, new HMACSHA1() } };
    }
}",
            GetCSharpResultAt(8, 85, CA5354RuleName, CA5354Message));
        }  
        
        [Fact]
        public void UseHMACSHA1InTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
            try
            {
                var sha1 = new HMACSHA1();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 28, CA5354RuleName, CA5354Message));
        }      
        
        [Fact]
        public void UseHMACSHA1InCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { var sha1 = new HMACSHA1(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 44, CA5354RuleName, CA5354Message));
        }       
        
        [Fact]
        public void UseHMACSHA1InFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { }
            finally { var sha1 = new HMACSHA1(); }
        }
    }
}",
            GetCSharpResultAt(12, 34, CA5354RuleName, CA5354Message));
        }
                            
        [Fact]
        public void UseHMACSHA1AwaitShouldGenerateDiagnostic()
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
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5354RuleName, CA5354Message));
        }    
        
        [Fact]
        public void UseHMACSHA1WithDelegateShouldGenerateDiagnostic()
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
        public void UseHMACSHA1DerivedClassShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyHMACSHA1 : HMACSHA1 {}

    class TestClass
    {
        private static void TestMethod()
        {
            var sha1 = new MyHMACSHA1();
        }
    }
}",
            GetCSharpResultAt(12, 24, CA5354RuleName, CA5354Message));
        } 
        
        [Fact]
        public void UseSHA1CryptoServiceProviderInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim SHA1alg As New SHA1CryptoServiceProvider
    End Sub
End Module",
            GetBasicResultAt(6, 24, CA5354RuleName, CA5354Message));
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
