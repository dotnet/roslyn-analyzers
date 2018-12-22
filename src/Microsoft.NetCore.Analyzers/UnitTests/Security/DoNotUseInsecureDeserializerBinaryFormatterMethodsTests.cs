// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureDeserializerBinaryFormatterMethodsTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor InvocationRule = DoNotUseInsecureDeserializerBinaryFormatterMethods.RealInvocationDescriptor;
        private static readonly DiagnosticDescriptor ReferenceRule = DoNotUseInsecureDeserializerBinaryFormatterMethods.RealReferenceDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatterMethods();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatterMethods();
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
                GetCSharpResultAt(12, 20, InvocationRule, "object BinaryFormatter.UnsafeDeserialize(Stream serializationStream, HeaderHandler handler)"));
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
                GetCSharpResultAt(12, 20, InvocationRule, "object BinaryFormatter.UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)"));
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
                GetCSharpResultAt(12, 20, InvocationRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
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
                GetCSharpResultAt(12, 20, InvocationRule, "object BinaryFormatter.Deserialize(Stream serializationStream, HeaderHandler handler)"));
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
                GetCSharpResultAt(12, 20, InvocationRule, "object BinaryFormatter.DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)"));
        }

        [Fact]
        public void Deserialize_Reference_Diagnostic()
        {
            VerifyCSharp(@"
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
                GetCSharpResultAt(13, 20, ReferenceRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
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

        [Fact]
        public void Serialize_Reference_NoDiagnostic()
        {
            VerifyCSharp(@"
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
    }
}
