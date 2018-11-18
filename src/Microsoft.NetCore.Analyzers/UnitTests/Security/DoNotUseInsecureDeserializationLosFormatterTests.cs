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
        private static readonly DiagnosticDescriptor InvocationRule = DoNotUseInsecureDeserializerLosFormatter.RealInvocationDescriptor;
        private static readonly DiagnosticDescriptor ReferenceRule = DoNotUseInsecureDeserializerLosFormatter.RealReferenceDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerLosFormatter();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerLosFormatter();
        }

        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string losFormatterCSharpSourceCode = @"
using System.IO;

namespace System.Web.UI
{
    public sealed class LosFormatter
    {
        public LosFormatter()
        {
        }

        public LosFormatter(bool enabledMac, byte[] macKeyModifier)
        {
        }

        public LosFormatter(bool enabledMac, string macKeyModifier)
        {
        }
        
        public object Deserialize(Stream stream)
        {
            return null;
        }

        public object Deserialize(TextReader input)
        {
            return null;
        }

        public object Deserialize(string input)
        {
            return null;
        }

        public void Serialize(Stream stream, object value)
        {
        }
   
        public void Serialize(TextWriter output, object value)
        {
        }
    }
}
";            this.VerifyCSharp(
                new[] { source, losFormatterCSharpSourceCode }.ToFileAndSource(),
                expected);
        }


        [Fact]
        public void DeserializeStream_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(12, 20, InvocationRule, "object LosFormatter.Deserialize(Stream stream)"));
        }

        [Fact]
        public void DeserializeString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(12, 20, InvocationRule, "object LosFormatter.Deserialize(string input)"));
        }

        [Fact]
        public void Deserialize_Reference_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(13, 20, ReferenceRule, "object LosFormatter.Deserialize(string input)"));
        }

        [Fact]
        public void Serialize_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            VerifyCSharpWithDependencies(@"
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
