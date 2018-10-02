// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.UnitTests.Security
{
    public class DoNotUseInsecureDeserializerBinaryFormatterBannedMethodsTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor BannedMethodRule = DoNotUseInsecureDeserializerBinaryFormatterBannedMethods.RealBannedMethodDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatterBannedMethods();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatterBannedMethods();
        }

        [Fact]
        public void UnsafeDeserialize_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.UnsafeDeserialize(new MemoryStream(bytes), null);
        }
    }
}",
                GetCSharpResultAt(12, 20, BannedMethodRule, "object BinaryFormatter.UnsafeDeserialize(Stream serializationStream, HeaderHandler handler)"));
        }

        [Fact]
        public void UnsafeDeserializeMethodResponse_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.UnsafeDeserializeMethodResponse(new MemoryStream(bytes), null, null);
        }
    }
}",
                GetCSharpResultAt(12, 20, BannedMethodRule, "object BinaryFormatter.UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)"));
        }

        [Fact]
        public void Deserialize_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object D(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}",
                GetCSharpResultAt(12, 20, BannedMethodRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
        }

        [Fact]
        public void Deserialize_HeaderHandler_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object D(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(new MemoryStream(bytes), null);
        }
    }
}",
                GetCSharpResultAt(12, 20, BannedMethodRule, "object BinaryFormatter.Deserialize(Stream serializationStream, HeaderHandler handler)"));
        }

        [Fact]
        public void DeserializeMethodResponse_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object D(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.DeserializeMethodResponse(new MemoryStream(bytes), null, null);
        }
    }
}",
                GetCSharpResultAt(12, 20, BannedMethodRule, "object BinaryFormatter.DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)"));
        }

        [Fact]
        public void Serialize_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public byte[] S(object o)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, o);
            return ms.ToArray();
        }
    }
}");
        }
    }
}
