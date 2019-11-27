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
        public void Test_PassArrayWithAMethod_Diagnostic()
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
}",
            GetCSharpResultAt(15, 9, 15, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] TestClass.GetArray(byte[] byteArray)", "void TestClass.TestMethod(byte[] byteArray, int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_PassByteArrayCreationDirectly_Diagnostic()
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
}",
            GetCSharpResultAt(10, 9, 10, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[]", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_PassGetByteArrayMethodDirectly_Diagnostic()
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
}",
            GetCSharpResultAt(15, 9, 15, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] TestClass.GetArray(int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_InterProcedural1_Diagnostic()
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
}",
            GetCSharpResultAt(15, 9, 15, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] TestClass.GetArray(int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_InterProcedural2_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] bytes1, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        DoDecryption(bytes1, cryptoStream);
    }

    public void DoDecryption(byte[] bytes2, CryptoStream cryptoStream)
    {
        cryptoStream.Write(bytes2, 0, bytes2.Length);
    }
}",
            GetCSharpResultAt(15, 9, 13, 30, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.DoDecryption(byte[] bytes2, CryptoStream cryptoStream)", "byte[] bytes2", "void TestClass.DoDecryption(byte[] bytes2, CryptoStream cryptoStream)"));
        }

        [Fact]
        public void Test_InterProcedural3_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public byte[] GetArray(int count)
    {
        var bytes = new byte[count];
        HashAlgorithm sha = new SHA1CryptoServiceProvider();
        sha.ComputeHash(bytes);
        return bytes;
    }
    
    public void TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        cryptoStream.Write(GetArray(count), offset, count);
    }
}",
            GetCSharpResultAt(18, 9, 18, 28, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)", "byte[] TestClass.GetArray(int count)", "void TestClass.TestMethod(int offset, int count, Stream stream, CryptoStreamMode mode)"));
        }

        [Fact]
        public void Test_InterProcedural4_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Security.Cryptography;
class TestClass
{
    public void TestMethod(byte[] bytes1, int offset, int count, Stream stream, CryptoStreamMode mode)
    {
        var decryptor = new AesCng().CreateDecryptor(); 
        var cryptoStream = new CryptoStream(stream, decryptor, mode);
        HashAlgorithm sha = new SHA1CryptoServiceProvider();
        byte[] result = sha.ComputeHash(bytes1);
        DoDecryption(bytes1, cryptoStream);
    }

    public void DoDecryption(byte[] bytes2, CryptoStream cryptoStream)
    {
        cryptoStream.Write(bytes2, 0, bytes2.Length);
    }
}",
            GetCSharpResultAt(17, 9, 15, 30, "void CryptoStream.Write(byte[] buffer, int offset, int count)", "void TestClass.DoDecryption(byte[] bytes2, CryptoStream cryptoStream)", "byte[] bytes2", "void TestClass.DoDecryption(byte[] bytes2, CryptoStream cryptoStream)"));
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
