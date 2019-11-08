﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseInsecureDeserializerBinaryFormatterMethods,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseInsecureDeserializerBinaryFormatterMethodsTests
    {
        [Fact]
        public async Task UnsafeDeserialize_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(12, 20, "object BinaryFormatter.UnsafeDeserialize(Stream serializationStream, HeaderHandler handler)"));
        }

        [Fact]
        public async Task UnsafeDeserializeMethodResponse_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(12, 20, "object BinaryFormatter.UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)"));
        }

        [Fact]
        public async Task Deserialize_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(12, 20, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
        }

        [Fact]
        public async Task Deserialize_HeaderHandler_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(12, 20, "object BinaryFormatter.Deserialize(Stream serializationStream, HeaderHandler handler)"));
        }

        [Fact]
        public async Task DeserializeMethodResponse_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(12, 20, "object BinaryFormatter.DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)"));
        }

        [Fact]
        public async Task Deserialize_Reference_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public delegate object Des(Stream s);
        public Des GetDeserializer()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize;
        }
    }
}",
                GetCSharpResultAt(13, 20, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
        }

        [Fact]
        public async Task Serialize_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        [Fact]
        public async Task Serialize_Reference_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public delegate void Ser(Stream s, object o);
        public Ser GetSerializer()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Serialize;
        }
    }
}");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic(DoNotUseInsecureDeserializerBinaryFormatterMethods.RealMethodUsedDescriptor)
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
