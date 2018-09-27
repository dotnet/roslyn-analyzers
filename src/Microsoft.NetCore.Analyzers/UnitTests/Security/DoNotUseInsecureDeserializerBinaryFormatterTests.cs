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

        private static readonly DiagnosticDescriptor BinderNotSetRule = DoNotUseInsecureDeserializerBinaryFormatter.BinderDefinitelyNotSetDescriptor;

        private static readonly DiagnosticDescriptor BinderMaybeNotSetRule = DoNotUseInsecureDeserializerBinaryFormatter.BinderMaybeNotSetDescriptor;

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
        public object BfUnsafeDeserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(12, 20, BinderNotSetRule));
        }

        [Fact]
        public void Deserialize_BinderMaybeSet_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class MyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (Environment.MachineName.StartsWith(""a""))
            {
                formatter.Binder = new MyBinder();
            }

            return formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(27, 20, BinderMaybeNotSetRule));
        }

        [Fact]
        public void Deserialize_BinderSet_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class MyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new MyBinder();
            return formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}");
        }
    }
}
