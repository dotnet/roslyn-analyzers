// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseInsecureDeserializerNetDataContractSerializerMethodsTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor Rule = DoNotUseInsecureDeserializerNetDataContractSerializerMethods.RealMethodUsedDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerNetDataContractSerializerMethods();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerNetDataContractSerializerMethods();
        }

        [Fact]
        public void DocSample1_CSharp_Violation_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

public class ExampleClass
{
    public object MyDeserialize(byte[] bytes)
    {
        NetDataContractSerializer serializer = new NetDataContractSerializer();
        return serializer.Deserialize(new MemoryStream(bytes));
    }
}",
                GetCSharpResultAt(10, 16, Rule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void DocSample1_VB_Violation_Diagnostic()
        {
            VerifyBasic(@"
Imports System.IO
Imports System.Runtime.Serialization

Public Class ExampleClass
    Public Function MyDeserialize(bytes As Byte()) As Object
        Dim serializer As NetDataContractSerializer = New NetDataContractSerializer()
        Return serializer.Deserialize(New MemoryStream(bytes))
    End Function
End Class",
                GetBasicResultAt(8, 16, Rule, "Function NetDataContractSerializer.Deserialize(stream As Stream) As Object"));
        }

        [Fact]
        public void Deserialize_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.Deserialize(new MemoryStream(bytes));
        }
    }
}",
                GetCSharpResultAt(12, 20, Rule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void Deserialize_Reference_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public delegate object Des(Stream s);
        public Des GetDeserializer()
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.Deserialize;
        }
    }
}",
                GetCSharpResultAt(13, 20, Rule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void ReadObject_Stream_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.ReadObject(new MemoryStream(bytes));
        }
    }
}",
                GetCSharpResultAt(12, 20, Rule, "object XmlObjectSerializer.ReadObject(Stream stream)"));
        }

        [Fact]
        public void ReadObject_Stream_Reference_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public delegate object Des(Stream s);
        public Des D()
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.ReadObject;
        }
    }
}",
                GetCSharpResultAt(13, 20, Rule, "object XmlObjectSerializer.ReadObject(Stream stream)"));
        }

        [Fact]
        public void ReadObject_XmlReader_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Blah
{
    public class Program
    {
        public object D(XmlReader xmlReader)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.ReadObject(xmlReader);
        }
    }
}",
                GetCSharpResultAt(13, 20, Rule, "object NetDataContractSerializer.ReadObject(XmlReader reader)"));
        }

        [Fact]
        public void ReadObject_XmlReader_Reference_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Blah
{
    public class Program
    {
        public delegate object Des(XmlReader r);
        public Des D()
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.ReadObject;
        }
    }
}",
                GetCSharpResultAt(14, 20, Rule, "object NetDataContractSerializer.ReadObject(XmlReader reader)"));
        }

        [Fact]
        public void Serialize_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public byte[] S(object o)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, o);
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
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public delegate void Ser(Stream s, object o);
        public Ser GetSerializer()
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.Serialize;
        }
    }
}");
        }
    }
}
