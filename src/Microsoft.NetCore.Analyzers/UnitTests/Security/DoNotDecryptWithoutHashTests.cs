using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotDecryptWithoutHashTests : TaintedDataAnalyzerTestBase
    {
        public DoNotDecryptWithoutHashTests(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override DiagnosticDescriptor Rule => DoNotDecryptWithoutHash.Rule;

        [Fact]
        public void Test_DecryptByteArray_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] buffer, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(buffer, offset, count);
    }
}",
            GetCSharpResultAt(10, 9, 6, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(byte[] buffer, int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] buffer", "void TestClass.TestMethod(byte[] buffer, int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_LocalVariableByteArray_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var byteArray = new byte[count];
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(byteArray, offset, count);
    }
}",
            GetCSharpResultAt(11, 9, 8, 13, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] byteArray", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_OutByteArray_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void GetArray(int count, out byte[] byteArray)
    {
        byteArray = new byte[count];
    }
    
    public void TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        GetArray(count, out byte[] byteArray);
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(byteArray, offset, count);
    }
}",
            GetCSharpResultAt(16, 9, 13, 36, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] byteArray", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_GetByteArrayMethod_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public byte[] GetArray(int count)
    {
        return new byte[count];
    }
    
    public void TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var byteArray = GetArray(count);
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(byteArray, offset, count);
    }
}",
            GetCSharpResultAt(16, 9, 13, 13, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] byteArray", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_ReadArrayMethod_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void SomethingReads(byte[] byteArray, int startIndex, int length)
    {
    }
    
    public void TestMethod(byte[] byteArray, int startIndex, int length, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        SomethingReads(byteArray, startIndex, length);
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(byteArray, offset, count);
    }
}",
            GetCSharpResultAt(15, 9, 10, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(byte[] byteArray, int startIndex, int length, int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] byteArray", "void TestClass.TestMethod(byte[] byteArray, int startIndex, int length, int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_MemoryStreamToArray_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var memStream = new MemoryStream(100);
        byteArray = memStream.ToArray();
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(byteArray, offset, count);
    }
}",
        GetCSharpResultAt(12, 9, 6, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] byteArray", "void TestClass.TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_MemoryStreamGetBuffer_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var memStream = new MemoryStream(100);
        byteArray = memStream.GetBuffer();
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(byteArray, offset, count);
    }
}",
        GetCSharpResultAt(12, 9, 6, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] byteArray", "void TestClass.TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_PassArrayWithAMethod_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public byte[] GetArray(byte[] byteArray)
    {
        return byteArray;
    }
    
    public void TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(GetArray(byteArray), offset, count);
    }
}");
        }

        [Fact]
        public void Test_ComputeHash_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] buffer, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        HashAlgorithm sha = new SHA1CryptoServiceProvider();
        byte[] result = sha.ComputeHash(buffer);
        cryptoStream.Write(buffer, offset, count);
    }
}");
        }

        [Fact]
        public void Test_PassByteArrayCreationDirectly_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(new byte[10], offset, count);
    }
}");
        }

        [Fact]
        public void Test_PassGetByteArrayMethodDirectly_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public byte[] GetArray(int count)
    {
        return new byte[count];
    }
    
    public void TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(GetArray(count), offset, count);
    }
}");
        }

        [Fact]
        public void Test_EncryptByteArray_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] buffer, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var encryptor = new AesCng().CreateEncryptor(); 
        var cryptoStream = new CryptoStream(stream, encryptor, mode);
        cryptoStream.Write(buffer, offset, count);
    }
}");
        }

        [Fact]
        public void Test_ReadByteArray_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] buffer, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Read(buffer, offset, count);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDecryptWithoutHash();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDecryptWithoutHash();
        }
    }
}
