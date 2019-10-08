// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseInsecureDeserializerLosFormatterTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor Rule = DoNotUseInsecureDeserializerLosFormatter.RealMethodUsedDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerLosFormatter();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerLosFormatter();
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
        LosFormatter formatter = new LosFormatter();
        return formatter.Deserialize(new MemoryStream(bytes));
    }
}",
                GetCSharpResultAt(10, 16, Rule, "object LosFormatter.Deserialize(Stream stream)"));
        }

        [Fact]
        public void DocSample1_VB_Violation_Diagnostic()
        {
            VerifyBasic(@"
Imports System.IO
Imports System.Web.UI

Public Class ExampleClass
    Public Function MyDeserialize(bytes As Byte()) As Object
        Dim formatter As LosFormatter = New LosFormatter()
        Return formatter.Deserialize(New MemoryStream(bytes))
    End Function
End Class",
                GetBasicResultAt(8, 16, Rule, "Function LosFormatter.Deserialize(stream As Stream) As Object"));
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
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(12, 20, Rule, "object LosFormatter.Deserialize(Stream stream)"));
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
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize(input);
        }
    }
}",
            GetCSharpResultAt(12, 20, Rule, "object LosFormatter.Deserialize(string input)"));
        }

        [Fact]
        public void DeserializeTextReader_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.UI;

namespace Blah
{
    public class Program
    {
        public object Deserialize(TextReader tr)
        {
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize(tr);
        }
    }
}",
            GetCSharpResultAt(12, 20, Rule, "object LosFormatter.Deserialize(TextReader input)"));
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
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize;
        }
    }
}",
                GetCSharpResultAt(13, 20, Rule, "object LosFormatter.Deserialize(string input)"));
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
            LosFormatter formatter = new LosFormatter();
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
            LosFormatter formatter = new LosFormatter();
            return formatter.Serialize;
        }
    }
}");
        }
    }
}
