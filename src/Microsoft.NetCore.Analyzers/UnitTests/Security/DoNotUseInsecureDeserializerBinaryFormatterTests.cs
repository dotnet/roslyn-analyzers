using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.UnitTests.Security
{
    public class DoNotUseInsecureDeserializerBinaryFormatterTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor BannedMethodRule = DoNotUseInsecureDeserializerBinaryFormatter.RealBannedMethodDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatter();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatter();
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
            GetCSharpResultAt(12, 20, BannedMethodRule, "UnsafeDeserialize"));
        }
    }
}
