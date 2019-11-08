// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseInsecureDeserializerObjectStateFormatter,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseInsecureDeserializerObjectStateFormatter,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseInsecureDeserializerObjectStateFormatterTests
    {
        [Fact]
        public async Task DocSample1_CSharp_Violation_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(10, 16, "object ObjectStateFormatter.Deserialize(Stream inputStream)"));
        }

        [Fact]
        public async Task DocSample1_VB_Violation_Diagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.IO
Imports System.Web.UI

Public Class ExampleClass
    Public Function MyDeserialize(bytes As Byte()) As Object
        Dim formatter As ObjectStateFormatter = New ObjectStateFormatter()
        Return formatter.Deserialize(New MemoryStream(bytes))
    End Function
End Class",
                GetBasicResultAt(8, 16, "Function ObjectStateFormatter.Deserialize(inputStream As Stream) As Object"));
        }

        [Fact]
        public async Task DeserializeStream_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(12, 20, "object ObjectStateFormatter.Deserialize(Stream inputStream)"));
        }

        [Fact]
        public async Task DeserializeString_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(12, 20, "object ObjectStateFormatter.Deserialize(string inputString)"));
        }

        [Fact]
        public async Task Deserialize_Reference_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                GetCSharpResultAt(13, 20, "object ObjectStateFormatter.Deserialize(string inputString)"));
        }

        [Fact]
        public async Task Serialize_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Serialize_Reference_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        private DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);

        private DiagnosticResult GetBasicResultAt(int line, int column, params string[] arguments)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
