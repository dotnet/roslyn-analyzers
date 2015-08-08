// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the projecVerifyCSharp(t root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class DoNotUseInsecureCryptographicAlgorithmsTests : DiagnosticAnalyzerTestBase
    {
        #region CA5350 
        [Fact]
		public void UseHMACMD5ShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var md5 = new HMACMD5();
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5DerivedClassShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyHMACMD5 : HMACMD5 {}

    class TestClass
    {
        private static void TestMethod()
        {
            var md5 = new MyHMACMD5();
        }
    }
}",
            GetCSharpResultAt(12, 23, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5WithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public HMACMD5 GetHMACMD5
        {
            get { return new HMACMD5(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5WithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACMD5 privateMd5;
        public HMACMD5 GetHMACMD5
        {
            set
            {
                if (value == null)
                    privateMd5 = new HMACMD5();
                else
                    privateMd5 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5WithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACMD5 privateMd5 = new HMACMD5();
    }
}",
            GetCSharpResultAt(7, 30, CA5350RuleName, CA5350Message));
        }         
        
        [Fact]
		public void UseHMACMD5WithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<HMACMD5> md5List = new List<HMACMD5>() { new HMACMD5() };
    }
}",
            GetCSharpResultAt(8, 55, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5WithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACMD5[] md5List = new HMACMD5[] { new HMACMD5() };
    }
}",
            GetCSharpResultAt(7, 45, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5WithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, HMACMD5> md5List = new Dictionary<int, HMACMD5>() { { 1, new HMACMD5() } };
    }
}",
            GetCSharpResultAt(8, 82, CA5350RuleName, CA5350Message));
        }         
        
        [Fact]
		public void UseHMACMD5InTryBlockShouldGenerateDiagnostic()
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
                var md5 = new HMACMD5();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5InCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var md5 = new HMACMD5(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5InFinallyBlockShouldGenerateDiagnostic()
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
            finally { var md5 = new HMACMD5(); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5350RuleName, CA5350Message));
        }        
        
        [Fact]
		public void UseHMACMD5AwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new HMACMD5(); });
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
		public void UseHMACMD5WithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new HMACMD5(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5350RuleName, CA5350Message));
        }        
        
        #endregion

        #region CA5351
        
        [Fact]
        public void UseDESCreateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var aes = DES.Create();  
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5351RuleName, CA5351Message));
        }

        [Fact]
        public void UseDESCreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass
    {
        public DES GetDES
        {
            get { return DES.Create(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5351RuleName, CA5351Message));
        }

        [Fact]
        public void UseDESCreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        DES privateDES;
        public DES GetDES
        {
            set
            {
                if (value == null)
                    privateDES = DES.Create();
                else
                    privateDES = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5351RuleName, CA5351Message));
        }

        [Fact]
        public void UseDESCreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        DES privateDES = DES.Create();
    }
}",
            GetCSharpResultAt(7, 26, CA5351RuleName, CA5351Message));
        }

        [Fact]
        public void UseDESCreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<DES> DESList = new List<DES>() { DES.Create() };
    }
}",
            GetCSharpResultAt(8, 47, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        DES[] DESList = new DES[] { DES.Create() };
    }
}",
            GetCSharpResultAt(7, 37, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, DES> DESList = new Dictionary<int, DES>() { { 1, DES.Create() } };
    }
}",
            GetCSharpResultAt(8, 74, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateInTryBlockShouldGenerateDiagnostic()
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
                var des = DES.Create();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var des = DES.Create(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var des = DES.Create(); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { DES.Create(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCreateWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { DES.Create(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            DES des = new DESCryptoServiceProvider();
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass
    {
        public DESCryptoServiceProvider GetDES
        {
            get { return new DESCryptoServiceProvider(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        DESCryptoServiceProvider privateDES;
        public DESCryptoServiceProvider GetDES
        {
            set
            {
                if (value == null)
                    privateDES = new DESCryptoServiceProvider();
                else
                    privateDES = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        DESCryptoServiceProvider privateDES = new DESCryptoServiceProvider();
    }
}",
            GetCSharpResultAt(7, 47, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<DESCryptoServiceProvider> DESList = new List<DESCryptoServiceProvider>() { new DESCryptoServiceProvider() };
    }
}",
            GetCSharpResultAt(8, 89, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        DESCryptoServiceProvider[] DESList = new DESCryptoServiceProvider[] { new DESCryptoServiceProvider(); };
    }
}",
            GetCSharpResultAt(7, 79, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, DESCryptoServiceProvider> DESList = new Dictionary<int, DESCryptoServiceProvider>() { { 1, new DESCryptoServiceProvider(); } };
    }
}",
            GetCSharpResultAt(8, 116, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateInTryBlockShouldGenerateDiagnostic()
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
                var des = new DESCryptoServiceProvider();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var des = new DESCryptoServiceProvider(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5351RuleName, CA5351Message));
        }  
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var des = new DESCryptoServiceProvider(); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new DESCryptoServiceProvider(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESCryptoServiceProviderCreateWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new DESCryptoServiceProvider(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseMultipleDESCryptoServiceProvidersShouldGenerateMultipleDiagnostics()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            DES des = new DESCryptoServiceProvider();
            DES des2 = new DESCryptoServiceProvider();   
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5351RuleName, CA5351Message),
            GetCSharpResultAt(11, 24, CA5351RuleName, CA5351Message));
        }
        
        [Fact]
        public void UseDESDerivedClassShouldGenerateDiagnostics()
        {
            VerifyCSharp(new[] {
                //Test0
                @"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            MyDES des = new MyDES();
            des.GenerateKey();
        }
    }
}",
                //Test1
                @"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyDES : DES
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override void GenerateIV()
        {
            throw new NotImplementedException();
        }

        public override void GenerateKey()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 25, CA5351RuleName, CA5351Message),
            GetCSharpResultAt(11, 13, CA5351RuleName, CA5351Message)); 
        }                                          
        
        [Fact]
        public void UseDESCreateInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim desalg As DES = DES.Create()
    End Sub
End Module",
            GetBasicResultAt(6, 29, CA5351RuleName, CA5351Message));
        }
        #endregion

        #region CA5352

        [Fact] 
        public void UseRC2CryptoServiceProviderShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var rc2 = new RC2CryptoServiceProvider();
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5352RuleName, CA5352Message));
        }        
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass
    {
        public RC2CryptoServiceProvider GetRC2
        {
            get { return new RC2CryptoServiceProvider(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5352RuleName, CA5352Message));
        }        
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RC2CryptoServiceProvider privateRC2;
        public RC2CryptoServiceProvider GetRC2
        {
            set
            {
                if (value == null)
                    privateRC2 = new RC2CryptoServiceProvider();
                else
                    privateRC2 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RC2CryptoServiceProvider privateRC2 = new RC2CryptoServiceProvider();
    }
}",
            GetCSharpResultAt(7, 47, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<RC2CryptoServiceProvider> RC2List = new List<RC2CryptoServiceProvider>() { new RC2CryptoServiceProvider() };
    }
}",
            GetCSharpResultAt(8, 89, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RC2CryptoServiceProvider[] RC2List = new RC2CryptoServiceProvider[] { new RC2CryptoServiceProvider(); };
    }
}",
            GetCSharpResultAt(7, 79, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, RC2CryptoServiceProvider> RC2List = new Dictionary<int, RC2CryptoServiceProvider>() { { 1, new RC2CryptoServiceProvider(); } };
    }
}",
            GetCSharpResultAt(8, 116, CA5352RuleName, CA5352Message));
        }
   
        [Fact]
        public void UseRC2CryptoServiceProviderCreateInTryBlockShouldGenerateDiagnostic()
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
                var des = new RC2CryptoServiceProvider();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var des = new RC2CryptoServiceProvider(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var des = new RC2CryptoServiceProvider(); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5352RuleName, CA5352Message));
        } 
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new RC2CryptoServiceProvider(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5352RuleName, CA5352Message));
        } 
        
        [Fact]
        public void UseRC2CryptoServiceProviderCreateWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new RC2CryptoServiceProvider(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2DerivedClassShouldGenerateDiagnostic()
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
            var rc2 = new MyRC2();
        }
    }
}",
//Test1
@"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyRC2 : RC2
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override void GenerateIV()
        {
            throw new NotImplementedException();
        }

        public override void GenerateKey()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 23, CA5352RuleName, CA5352Message));
        }
        
        [Fact]
        public void UseRC2CryptoServiceProviderInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim rc2alg As New RC2CryptoServiceProvider
    End Sub
End Module",
            GetBasicResultAt(6, 23, CA5352RuleName, CA5352Message));
        }
        #endregion

        #region CA5353 
        
        [Fact]
        public void UseTripleDESCreateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var tripleDES = TripleDES.Create(""TripleDES"");  
        }
    }
}",
            GetCSharpResultAt(10, 29, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass
    {
        public TripleDES GetTripleDES
        {
            get { return TripleDES.Create(""TripleDES""); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        TripleDES privateDES;
        public TripleDES GetDES
        {
            set
            {
                if (value == null)
                    privateDES = TripleDES.Create(""TripleDES"");
                else
                    privateDES = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        TripleDES privateDES = TripleDES.Create(""TripleDES"");
    }
}",
            GetCSharpResultAt(7, 32, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<TripleDES> DESList = new List<TripleDES>() { TripleDES.Create(""TripleDES"") };
    }
}",
            GetCSharpResultAt(8, 59, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        TripleDES[] DESList = new TripleDES[] { TripleDES.Create(""TripleDES"") };
    }
}",
            GetCSharpResultAt(7, 49, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, TripleDES> DESList = new Dictionary<int, TripleDES>() { { 1, TripleDES.Create(""TripleDES"") } };
    }
}",
            GetCSharpResultAt(8, 86, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateInTryBlockShouldGenerateDiagnostic()
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
                var des = TripleDES.Create(""TripleDES"");
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var des = TripleDES.Create(""TripleDES""); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var des = TripleDES.Create(""TripleDES""); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { TripleDES.Create(""TripleDES""); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCreateWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { TripleDES.Create(""TripleDES""); };
    }
}",
            GetCSharpResultAt(8, 31, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        }
    }
}",
            GetCSharpResultAt(10, 56, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass
    {
        public TripleDESCryptoServiceProvider GetDES
        {
            get { return new TripleDESCryptoServiceProvider(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        TripleDESCryptoServiceProvider privateDES;
        public TripleDESCryptoServiceProvider GetDES
        {
            set
            {
                if (value == null)
                    privateDES = new TripleDESCryptoServiceProvider();
                else
                    privateDES = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 34, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        TripleDESCryptoServiceProvider privateDES = new TripleDESCryptoServiceProvider();
    }
}",
            GetCSharpResultAt(7, 53, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<TripleDESCryptoServiceProvider> DESList = new List<TripleDESCryptoServiceProvider>() { new TripleDESCryptoServiceProvider() };
    }
}",
            GetCSharpResultAt(8, 101, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        TripleDESCryptoServiceProvider[] DESList = new TripleDESCryptoServiceProvider[] { new TripleDESCryptoServiceProvider(); };
    }
}",
            GetCSharpResultAt(7, 91, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, TripleDESCryptoServiceProvider> DESList = new Dictionary<int, TripleDESCryptoServiceProvider>() { { 1, new TripleDESCryptoServiceProvider(); } };
    }
}",
            GetCSharpResultAt(8, 128, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateInTryBlockShouldGenerateDiagnostic()
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
                var des = new TripleDESCryptoServiceProvider();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 27, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var des = new TripleDESCryptoServiceProvider(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 43, CA5353RuleName, CA5353Message));
        }  
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var des = new TripleDESCryptoServiceProvider(); }
        }
    }
}",
            GetCSharpResultAt(12, 33, CA5353RuleName, CA5353Message));
        } 
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new TripleDESCryptoServiceProvider(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5353RuleName, CA5353Message));
        }  
        
        [Fact]
        public void UseTripleDESCryptoServiceProviderCreateWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new TripleDESCryptoServiceProvider(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleDESDerivedClassShouldGenerateDiagnostics()
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
            var my3DES = new My3DES();
            my3DES.GenerateKey();
        }
    }
}",
                //Test1
                @"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class My3DES : TripleDES
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override void GenerateIV()
        {
            throw new NotImplementedException();
        }

        public override void GenerateKey()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 26, CA5353RuleName, CA5353Message),
            GetCSharpResultAt(11, 13, CA5353RuleName, CA5353Message));
        }
        
        [Fact]
        public void UseTripleESCryptoServiceProviderInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim tDESalg As New TripleDESCryptoServiceProvider
    End Sub
End Module",
            GetBasicResultAt(6, 24, CA5353RuleName, CA5353Message));
        }
        #endregion  

        #region CA5355
        
        [Fact]
        public void UseRIPEMD160ManagedShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var md160 = new RIPEMD160Managed();
        }
    }
}",
            GetCSharpResultAt(10, 25, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseRIPEMD160ManagedWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public RIPEMD160Managed GetRIPEMD160
        {
            get { return new RIPEMD160Managed(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RIPEMD160Managed privateRIPEMD160;
        public RIPEMD160Managed GetRIPEMD160
        {
            set
            {
                if (value == null)
                    privateRIPEMD160 = new RIPEMD160Managed();
                else
                    privateRIPEMD160 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 40, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseRIPEMD160ManagedWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RIPEMD160Managed privateRIPEMD160 = new RIPEMD160Managed();
    }
}",
            GetCSharpResultAt(7, 45, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseRIPEMD160ManagedWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<RIPEMD160Managed> RIPEMD160List = new List<RIPEMD160Managed>() { new RIPEMD160Managed() };
    }
}",
            GetCSharpResultAt(8, 79, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseRIPEMD160ManagedWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RIPEMD160Managed[] RIPEMD160List = new RIPEMD160Managed[] { new RIPEMD160Managed() };
    }
}",
            GetCSharpResultAt(7, 69, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, RIPEMD160Managed> RIPEMD160List = new Dictionary<int, RIPEMD160Managed>() { { 1, new RIPEMD160Managed() } };
    }
}",
            GetCSharpResultAt(8, 106, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedInTryBlockShouldGenerateDiagnostic()
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
                var RIPEMD160var = new RIPEMD160Managed();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 36, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var RIPEMD160var = new RIPEMD160Managed(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 52, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var RIPEMD160var = new RIPEMD160Managed(); }
        }
    }
}",
            GetCSharpResultAt(12, 42, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new RIPEMD160Managed(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new RIPEMD160Managed(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            RIPEMD160 md160 = RIPEMD160.Create();
        }
    }
}",
            GetCSharpResultAt(10, 31, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public RIPEMD160 GetRIPEMD160
        {
            get { return RIPEMD160.Create(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RIPEMD160 privateRIPEMD160;
        public RIPEMD160 GetRIPEMD160
        {
            set
            {
                if (value == null)
                    privateRIPEMD160 = RIPEMD160.Create();
                else
                    privateRIPEMD160 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 40, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RIPEMD160 privateRIPEMD160 = RIPEMD160.Create();
    }
}",
            GetCSharpResultAt(7, 38, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<RIPEMD160> RIPEMD160List = new List<RIPEMD160>() { RIPEMD160.Create() };
    }
}",
            GetCSharpResultAt(8, 65, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RIPEMD160[] RIPEMD160List = new RIPEMD160[] { RIPEMD160.Create() };
    }
}",
            GetCSharpResultAt(7, 55, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, RIPEMD160> RIPEMD160List = new Dictionary<int, RIPEMD160>() { { 1, RIPEMD160.Create() } };
    }
}",
            GetCSharpResultAt(8, 92, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CrateInTryBlockShouldGenerateDiagnostic()
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
                var RIPEMD160var = RIPEMD160.Create();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 36, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var RIPEMD160var = RIPEMD160.Create(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 52, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var RIPEMD160var = RIPEMD160.Create(); }
        }
    }
}",
            GetCSharpResultAt(12, 42, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseRIPEMD160CreateAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { RIPEMD160.Create(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160CreateWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { RIPEMD160.Create(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseHMACRIPEMD160ShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var md160 = new HMACRIPEMD160();
        }
    }
}",
            GetCSharpResultAt(10, 25, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseHMACRIPEMD160WithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public HMACRIPEMD160 GetHMARIPEMD160
        {
            get { return new HMACRIPEMD160(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseHMACRIPEMD160WithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACRIPEMD160 privateHMARIPEMD160;
        public HMACRIPEMD160 GetHMARIPEMD160
        {
            set
            {
                if (value == null)
                    privateRIPEMD160 = new HMACRIPEMD160();
                else
                    privateRIPEMD160 = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 40, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseHMACRIPEMD160WithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACRIPEMD160 privateHMARIPEMD160 = new HMACRIPEMD160();
    }
}",
            GetCSharpResultAt(7, 45, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseHMACRIPEMD160WithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<HMACRIPEMD160> RIPEMD160List = new List<HMACRIPEMD160>() { new HMACRIPEMD160() };
    }
}",
            GetCSharpResultAt(8, 73, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseHMACRIPEMD160WithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        HMACRIPEMD160[] RIPEMD160List = new HMACRIPEMD160[] { new HMACRIPEMD160() };
    }
}",
            GetCSharpResultAt(7, 63, CA5355RuleName, CA5355Message));
        }  
        
        [Fact]
        public void UseHMACRIPEMD160WithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, HMACRIPEMD160> RIPEMD160List = new Dictionary<int, HMACRIPEMD160>() { { 1, new HMACRIPEMD160() } };
    }
}",
            GetCSharpResultAt(8, 100, CA5355RuleName, CA5355Message));
        }  
        
        [Fact]
        public void UseHMACRIPEMD160InTryBlockShouldGenerateDiagnostic()
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
                var RIPEMD160var = new HMACRIPEMD160();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 36, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseHMACRIPEMD160InCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var RIPEMD160var = new HMACRIPEMD160(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 52, CA5355RuleName, CA5355Message));
        }     
        
        [Fact]
        public void UseHMACRIPEMD160InFinallyBlockShouldGenerateDiagnostic()
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
            finally { var RIPEMD160var = new HMACRIPEMD160(); }
        }
    }
}",
            GetCSharpResultAt(12, 42, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseHMACRIPEMD160AwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new HMACRIPEMD160(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseHMACRIPEMD160WithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new HMACRIPEMD160(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5355RuleName, CA5355Message));
        }   
        
        [Fact]
        public void UseRIPEMD160DerivedClassShouldGenerateDiagnostic()
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
            var md160 = new MyRIPEMD160();
        }
    }
}",
                //Test1
                @"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyRIPEMD160 : RIPEMD160
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
            GetCSharpResultAt(10, 25, CA5355RuleName, CA5355Message));
        }
        
        [Fact]
        public void UseRIPEMD160ManagedDerivedClassShouldGenerateDiagnostic()
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
            var md160 = new MyRIPEMD160();
        }
    }
}",
                //Test1
                @"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyRIPEMD160 : RIPEMD160Managed
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
            GetCSharpResultAt(10, 25, CA5355RuleName, CA5355Message));
        } 
        
        [Fact]
        public void UseHMACRIPEMD160DerivedClassShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyHMACRIPEMD160 : HMACRIPEMD160 {}

    class TestClass
    {
        private static void TestMethod()
        {
            var md160 = new MyHMACRIPEMD160();
        }
    }
}",
            GetCSharpResultAt(12, 25, CA5355RuleName, CA5355Message));
        }   
        
        [Fact]
        public void UseRIPEMD160ManagedInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim md1601alg As New RIPEMD160Managed
    End Sub
End Module",
            GetBasicResultAt(6, 26, CA5355RuleName, CA5355Message));
        }
        #endregion

        #region CA5356 
        
        [Fact]
        public void UseDSACreateSignatureShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(DSA dsa, byte[] inBytes)
        {
            var sig = dsa.CreateSignature(inBytes);
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5356RuleName, CA5356Message)); 
        } 
        
        [Fact]
        public void CA5356UseDSACreateSignatureWithListShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;

class TestClass
{
    private void TestMethod(DSA dsa, byte[] inBytes)
    {
        List<byte[]> dsaList = new List<byte[]> { dsa.CreateSignature(inBytes) };
    }
}",
            GetCSharpResultAt(9, 51, CA5356RuleName, CA5356Message));
        }                 
        
        [Fact]
        public void CA5356UseDSACreateSignatureWithDictionaryShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;

class TestClass
{
    private void TestMethod(DSA dsa, byte[] inBytes)
    {
        Dictionary<int, byte[]> dsaDictionary = new Dictionary<int, byte[]>() { { 1, dsa.CreateSignature(inBytes) } };
    }
}",
            GetCSharpResultAt(9, 86, CA5356RuleName, CA5356Message));
        }  
        
        [Fact]
        public void CA5356UseDSACreateSignatureInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    DSA dsa1 = null;
    public byte[] MyProperty
    {
        get
        {
            byte[] inBytes = null;
            return dsa1.CreateSignature(inBytes);
        }
    }
}",
            GetCSharpResultAt(12, 20, CA5356RuleName, CA5356Message));
        }
        
        [Fact]
        public void UseDSASignatureFormatterCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var sf1 = new DSASignatureFormatter();
            var sf2 = new DSASignatureFormatter(new DSACryptoServiceProvider());
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5356RuleName, CA5356Message),
            GetCSharpResultAt(11, 23, CA5356RuleName, CA5356Message));
        }  
        
        [Fact]
        public void CA5356UseDSACreateSignatureFormatterWithListShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;

class TestClass
{
    private void TestMethod(DSA dsa, byte[] inBytes)
    {
        List<DSASignatureFormatter> dsaList = new List<DSASignatureFormatter> { new DSASignatureFormatter() };
        List<DSASignatureFormatter> dsaList1 = new List<DSASignatureFormatter> { new DSASignatureFormatter(new DSACryptoServiceProvider()) };
    }
}",
            GetCSharpResultAt(9, 81, CA5356RuleName, CA5356Message),
            GetCSharpResultAt(10, 82, CA5356RuleName, CA5356Message));
        }     
        
        [Fact]
        public void CA5356UseDSACreateSignatureFormatterWithDictionaryShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;

class TestClass
{
    private void TestMethod(DSA dsa, byte[] inBytes)
    {
        Dictionary<int, DSASignatureFormatter> dsaDictionary = new Dictionary<int, DSASignatureFormatter>() { { 1, new DSASignatureFormatter() } };
        Dictionary<int, DSASignatureFormatter> dsaDictionar1 = new Dictionary<int, DSASignatureFormatter>() { { 1, new DSASignatureFormatter(new DSACryptoServiceProvider()) } };
    }
}",
            GetCSharpResultAt(9, 116, CA5356RuleName, CA5356Message),
            GetCSharpResultAt(10, 116, CA5356RuleName, CA5356Message));
        } 
        
        [Fact]
        public void CA5356UseDSACreateSignatureFormatterInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

class TestClass
{
    DSA dsa1 = null;
    public DSASignatureFormatter MyProperty
    {
        get
        {
            DSASignatureFormatter inBytes = null;
            if (inBytes == null) { return new DSASignatureFormatter(); }
            else return new DSASignatureFormatter(new DSACryptoServiceProvider());
        }
    }
}",
            GetCSharpResultAt(12, 43, CA5356RuleName, CA5356Message),
            GetCSharpResultAt(13, 25, CA5356RuleName, CA5356Message));
        }
        
        [Fact]
        public void UseCreateSignatureFromDSADerivedClassShouldGenerateDiagnostic()
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
            var myDsa = new MyDsa();
            myDsa.CreateSignature(inBytes);
        }
    }
}",
                //Test1
                @"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyDsa : DSA
    {
        public override string KeyExchangeAlgorithm
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string SignatureAlgorithm
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override byte[] CreateSignature(byte[] rgbHash)
        {
            throw new NotImplementedException();
        }

        public override DSAParameters ExportParameters(bool includePrivateParameters)
        {
            throw new NotImplementedException();
        }

        public override void ImportParameters(DSAParameters parameters)
        {
            throw new NotImplementedException();
        }

        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(11, 13, CA5356RuleName, CA5356Message));
        } 
        
        [Fact]
        public void UseDSACreateSignatureInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Function TestMethod(ByVal bytes As Byte())
        Dim dsa As New DSACryptoServiceProvider
        Return dsa.CreateSignature(bytes)
    End Function
End Module",
            GetBasicResultAt(7, 16, CA5356RuleName, CA5356Message));
        }
        #endregion

        #region CA5357 
        
        [Fact]
        public void UseRijndaelManagedShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var rc2 = new RijndaelManaged();
        }
    }
}",
            GetCSharpResultAt(10, 23, CA5357RuleName, CA5357Message));
        }
                                                   
        [Fact]
        public void UseRijndaelManagedWithGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
   class TestClass1
    {
        public RijndaelManaged GetRijndael
        {
            get { return new RijndaelManaged(); }
        }
    }
}",
            GetCSharpResultAt(9, 26, CA5357RuleName, CA5357Message));
        } 
        
        [Fact]
        public void UseRijndaelManagedWithSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RijndaelManaged privateRijndael;
        public RijndaelManaged GetRijndael
        {
            set
            {
                if (value == null)
                    privateRijndael = new RijndaelManaged();
                else
                    privateRijndael = value;
            }
        }
    }
}",
            GetCSharpResultAt(13, 39, CA5357RuleName, CA5357Message));
        }
        
        [Fact]
        public void UseRijndaelManagedWithFieldInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RijndaelManaged privateRijndael = new RijndaelManaged();
    }
}",
            GetCSharpResultAt(7, 43, CA5357RuleName, CA5357Message));
        } 
        
        [Fact]
        public void UseRijndaelManagedWithListCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        List<RijndaelManaged> RijndaelList = new List<RijndaelManaged>() { new RijndaelManaged(); };
    }
}",
            GetCSharpResultAt(8, 76, CA5357RuleName, CA5357Message));
        } 
        
        [Fact]
        public void UseRijndaelManagedWithArrayCollectionInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        RijndaelManaged[] RijndaelList = new RijndaelManaged[] { new RijndaelManaged() };
    }
}",
            GetCSharpResultAt(7, 66, CA5357RuleName, CA5357Message));
        }
        
        [Fact]
        public void UseRijndaelManagedWithDictionaryInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        Dictionary<int, RijndaelManaged> RijndaelList = new Dictionary<int, RijndaelManaged>() { { 1, new RijndaelManaged() } };
    }
}",
            GetCSharpResultAt(8, 103, CA5357RuleName, CA5357Message));
        }
        
        [Fact]
        public void UseRijndaelManagedInTryBlockShouldGenerateDiagnostic()
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
                var rijndael = new RijndaelManaged();
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}",
            GetCSharpResultAt(12, 32, CA5357RuleName, CA5357Message));
        } 
        
        [Fact]
        public void UseRijndaelManagedInCatchBlockShouldGenerateDiagnostic()
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
            catch (Exception) { var rijndael = new RijndaelManaged(); }
            finally { }
        }
    }
}",
            GetCSharpResultAt(11, 48, CA5357RuleName, CA5357Message));
        }
                         
        [Fact]
        public void UseRijndaelManagedInFinallyBlockShouldGenerateDiagnostic()
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
            finally { var rijndael = new RijndaelManaged(); }
        }
    }
}",
            GetCSharpResultAt(12, 38, CA5357RuleName, CA5357Message));
        }  
        
        [Fact]
        public void UseRijndaelManagedAwaitShouldGenerateDiagnostic()
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
            await Task.Run(() => { new RijndaelManaged(); });
        }
        private async void TestMethod2()
        {
            await TestMethod();
        }
    }
}",
            GetCSharpResultAt(10, 36, CA5357RuleName, CA5357Message));
        }  
        
        [Fact]
        public void UseRijndaelManagedWithDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography;
namespace TestNamespace
{
    class TestClass
    {
        delegate void Del();
        Del d = delegate () { new RijndaelManaged(); };
    }
}",
            GetCSharpResultAt(8, 31, CA5357RuleName, CA5357Message));
        } 
        
        [Fact]
        public void UseRijndaelDerivedClassShouldGenerateDiagnostic()
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
            var rc2 = new MyRijndael();
        }
    }
}",
                //Test1
                @"
using System;
using System.Security.Cryptography;

namespace TestNamespace
{
    class MyRijndael : Rijndael
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new NotImplementedException();
        }

        public override void GenerateIV()
        {
            throw new NotImplementedException();
        }

        public override void GenerateKey()
        {
            throw new NotImplementedException();
        }
    }
}" },
            GetCSharpResultAt(10, 23, CA5357RuleName, CA5357Message));
        }

        
        [Fact]
        public void UseRijndaelManagedInVBShouldGenerateDiagnostic()
        {
            VerifyBasic(@"
Imports System.Security.Cryptography

Module TestClass
    Sub TestMethod()
        Dim rijndaelalg As New RijndaelManaged
    End Sub
End Module",
            GetBasicResultAt(6, 28, CA5357RuleName, CA5357Message));
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
        private const string CA5351RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseDESRuleId;
        private const string CA5352RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseRC2RuleId;
        private const string CA5353RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseTripleDESRuleId;
        private const string CA5355RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseRIPEMD160RuleId;
        private const string CA5356RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseDSARuleId;
        private const string CA5357RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseRijndaelRuleId;

        private readonly string CA5350Message = DesktopAnalyzersResources.DoNotUseMD5;
        private readonly string CA5351Message = DesktopAnalyzersResources.DoNotUseDES;
        private readonly string CA5352Message = DesktopAnalyzersResources.DoNotUseRC2;
        private readonly string CA5353Message = DesktopAnalyzersResources.DoNotUseTripleDES;  
        private readonly string CA5355Message = DesktopAnalyzersResources.DoNotUseRIPEMD160;
        private readonly string CA5356Message = DesktopAnalyzersResources.DoNotUseDSA;
        private readonly string CA5357Message = DesktopAnalyzersResources.DoNotUseRijndael;
    }
}
