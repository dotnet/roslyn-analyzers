// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.UnitTests.Security
{
    public class DoNotUseInsecureDeserializerLosFormatterTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor BannedMethodRule = DoNotUseInsecureDeserializerLosFormatter.RealBannedMethodDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerLosFormatter();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerLosFormatter();
        }

        // So we don't have to reference System.Web.dll.
        private const string LosFormatterCSharpSourceCode = @"
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
";

        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            this.VerifyCSharp(
                new[] { source, LosFormatterCSharpSourceCode }.ToFileAndSource(),
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
            GetCSharpResultAt(12, 20, BannedMethodRule, "object LosFormatter.Deserialize(Stream stream)"));
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
            GetCSharpResultAt(12, 20, BannedMethodRule, "object LosFormatter.Deserialize(string input)"));
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
    }
}
