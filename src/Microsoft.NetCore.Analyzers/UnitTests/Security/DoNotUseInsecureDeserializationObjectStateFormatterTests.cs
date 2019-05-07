// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseInsecureDeserializerObjectStateFormatterTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor Rule = DoNotUseInsecureDeserializerObjectStateFormatter.RealMethodUsedDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerObjectStateFormatter();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerObjectStateFormatter();
        }

        [Fact]
        public void DocSample1_CSharp_Violation_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

public class ExampleClass
{
    public object MyDeserialize(byte[] bytes)
    {
        ObjectStateFormatter formatter = new ObjectStateFormatter();
        return formatter.Deserialize(new MemoryStream(bytes));
    }
}",
                GetCSharpResultAt(10, 16, Rule, "object ObjectStateFormatter.Deserialize(Stream inputStream)"));
        }

        [Fact]
        public void DocSample1_VB_Violation_Diagnostic()
        {
            VerifyBasic(@"
Imports System.IO
Imports System.Web.UI

Public Class ExampleClass
    Public Function MyDeserialize(bytes As Byte()) As Object
        Dim formatter As ObjectStateFormatter = New ObjectStateFormatter()
        Return formatter.Deserialize(New MemoryStream(bytes))
    End Function
End Class",
                GetBasicResultAt(8, 16, Rule, "Function ObjectStateFormatter.Deserialize(inputStream As Stream) As Object"));
        }

        [Fact]
        public void DeserializeStream_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

namespace Blah
{
    public class Program
    {
        public object Deserialize(byte[] bytes)
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            return formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(12, 20, Rule, "object ObjectStateFormatter.Deserialize(Stream inputStream)"));
        }

        [Fact]
        public void DeserializeString_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

namespace Blah
{
    public class Program
    {
        public object Deserialize(string input)
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            return formatter.Deserialize(input);
        }
    }
}",
            GetCSharpResultAt(12, 20, Rule, "object ObjectStateFormatter.Deserialize(string inputString)"));
        }

        [Fact]
        public void Deserialize_Reference_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

namespace Blah
{
    public class Program
    {
        public delegate object Des(string s);
        public Des GetDeserializer()
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            return formatter.Deserialize;
        }
    }
}",
                GetCSharpResultAt(13, 20, Rule, "object ObjectStateFormatter.Deserialize(string inputString)"));
        }

        [Fact]
        public void Serialize_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

namespace Blah
{
    public class Program
    {
        public byte[] Serialize(object o)
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, o);
            return stream.ToArray();
        }
    }
}");
        }

        [Fact]
        public void Serialize_Reference_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

namespace Blah
{
    public class Program
    {
        public delegate void Ser(Stream s, object o);
        public Ser GetSerializer()
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            return formatter.Serialize;
        }
    }
}");
        }
    }
}
