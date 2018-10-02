// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.UnitTests.Security
{
    public class DoNotUseInsecureDeserializerBinaryFormatterWithoutBinderTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor BinderNotSetRule = DoNotUseInsecureDeserializerBinaryFormatterWithoutBinder.RealBinderDefinitelyNotSetDescriptor;

        private static readonly DiagnosticDescriptor BinderMaybeNotSetRule = DoNotUseInsecureDeserializerBinaryFormatterWithoutBinder.RealBinderMaybeNotSetDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatterWithoutBinder();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerBinaryFormatterWithoutBinder();
        }

        protected void VerifyCSharpWithMyBinderDefined(string source, params DiagnosticResult[] expected)
        {
            string myBinderCSharpSourceCode = @"
            using System;
            using System.Runtime.Serialization;

            namespace Blah
            {
                public class MyBinder : SerializationBinder
                {
                    public override Type BindToType(string assemblyName, string typeName)
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            ";

            this.VerifyCSharp(
                new[] { source, myBinderCSharpSourceCode }.ToFileAndSource(), 
                expected);
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
            GetCSharpResultAt(12, 20, BinderNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
        }

        // Ideally, we'd detect that formatter.Binder is always null.
        [Fact]
        public void DeserializeWithInstanceField_Diagnostic_NotIdeal()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        BinaryFormatter formatter = new BinaryFormatter();

        public object BfUnsafeDeserialize(byte[] bytes)
        {
            return this.formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(13, 20, BinderMaybeNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
        }

        [Fact(Skip = " Ideally, we'd detect that this.formatter.Binder is set.")]
        public void DeserializeWithInstanceField_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        BinaryFormatter formatter = new BinaryFormatter() { Binder = new MyBinder() };

        public object BfUnsafeDeserialize(byte[] bytes)
        {
            return this.formatter.Deserialize(new MemoryStream(bytes));
        }
    }
}");
        }

        [Fact]
        public void Deserialize_BinderMaybeSet_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
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
            GetCSharpResultAt(18, 20, BinderMaybeNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
        }

        [Fact]
        public void Deserialize_BinderSet_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
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

        [Fact]
        public void TwoSettersOneBinderOnFirst_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes1, byte[] bytes2)
        {
            if (Environment.GetEnvironmentVariable(""USEFIRST"") == ""1"")
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new MyBinder();
                return bf.Deserialize(new MemoryStream(bytes1));
            }
            else
            {
                return new BinaryFormatter().Deserialize(new MemoryStream(bytes2));
            }
        }
    }
}",
                GetCSharpResultAt(20, 24, BinderNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));
                
        }

        [Fact]
        public void TwoSettersOneBinderOnSecond_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes1, byte[] bytes2)
        {
            if (Environment.GetEnvironmentVariable(""USEFIRST"") == ""1"")
            {
                return new BinaryFormatter().Deserialize(new MemoryStream(bytes1));
            }
            else
            {
                return (new BinaryFormatter() { Binder = new MyBinder() }).Deserialize(new MemoryStream(bytes2));
            }
        }
    }
}",
                GetCSharpResultAt(14, 24, BinderNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));

        }

        [Fact]
        public void TwoSettersNoBinder_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes1, byte[] bytes2)
        {
            if (Environment.GetEnvironmentVariable(""USEFIRST"") == ""1"")
            {
                return new BinaryFormatter().Deserialize(new MemoryStream(bytes1));
            }
            else
            {
                return new BinaryFormatter().Deserialize(new MemoryStream(bytes2));
            }
        }
    }
}",
                GetCSharpResultAt(14, 24, BinderNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"),
                GetCSharpResultAt(18, 24, BinderNotSetRule, "object BinaryFormatter.Deserialize(Stream serializationStream)"));

        }


        [Fact]
        public void BinderSetInline_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blah
{
    public class Program
    {
        public object BfUnsafeDeserialize(byte[] bytes)
        {
            return (new BinaryFormatter() { Binder = new MyBinder() }).Deserialize(new MemoryStream(bytes));
        }
    }
}");
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
    }
}
