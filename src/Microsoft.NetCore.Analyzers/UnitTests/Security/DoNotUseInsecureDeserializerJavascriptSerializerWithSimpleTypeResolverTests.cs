// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureDeserializerJavascriptSerializerWithSimpleTypeResolverTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor DefinitelyRule = DoNotUseInsecureDeserializerJavaScriptSerializerWithSimpleTypeResolver.DefinitelyWithSimpleTypeResolver;
        private static readonly DiagnosticDescriptor MaybeRule = DoNotUseInsecureDeserializerJavaScriptSerializerWithSimpleTypeResolver.MaybeWithSimpleTypeResolver;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerJavaScriptSerializerWithSimpleTypeResolver();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerJavaScriptSerializerWithSimpleTypeResolver();
        }

        [Fact]
        public void Deserialize_Generic_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(string str)
        {
            JavaScriptSerializer s = new JavaScriptSerializer(new SimpleTypeResolver());
            return s.Deserialize<T>(str);
        }
    }
}",
                GetCSharpResultAt(12, 20, DefinitelyRule, "T JavaScriptSerializer.Deserialize<T>(string input)"));
        }

        [Fact]
        public void Deserialize_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(string str)
        {
            JavaScriptSerializer s = new JavaScriptSerializer(new SimpleTypeResolver());
            return (T) s.Deserialize(str, typeof(T));
        }
    }
}",
                GetCSharpResultAt(12, 24, DefinitelyRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void DeserializeObject_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            JavaScriptSerializer s = new JavaScriptSerializer(new SimpleTypeResolver());
            return s.DeserializeObject(str);
        }
    }
}",
                GetCSharpResultAt(12, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_AnyPath_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str, bool flag)
        {
            JavaScriptSerializer s;
            if (flag)
                s = new JavaScriptSerializer(new SimpleTypeResolver());
            else
                s = new JavaScriptSerializer();
            return s.DeserializeObject(str);
        }
    }
}",
                GetCSharpResultAt(16, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void Deserialize_FromArgument_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(JavaScriptSerializer s, string str)
        {
            return (T) s.Deserialize(str, typeof(T));
        }
    }
}",
                GetCSharpResultAt(11, 24, MaybeRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void Deserialize_TypeResolver_Unknown_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(Func<JavaScriptTypeResolver> trFactory, string str)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer(trFactory());
            return (T) jss.Deserialize(str, typeof(T));
        }
    }
}",
                GetCSharpResultAt(13, 24, MaybeRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void Deserialize_TypeResolver_UnknownNotNull_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(Func<JavaScriptTypeResolver> trFactory, string str)
        {
            JavaScriptTypeResolver tr = trFactory();
            JavaScriptSerializer jss = tr != null ? new JavaScriptSerializer(tr) : new JavaScriptSerializer();
            return (T) jss.Deserialize(str, typeof(T));
        }
    }
}",
                GetCSharpResultAt(14, 24, MaybeRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void Deserialize_TypeResolver_UnknownNull_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(Func<JavaScriptTypeResolver> trFactory, string str)
        {
            JavaScriptTypeResolver tr = trFactory();
            JavaScriptSerializer jss = tr == null ? new JavaScriptSerializer(tr) : new JavaScriptSerializer();
            return (T) jss.Deserialize(str, typeof(T));
        }
    }
}");
        }

        [Fact]
        public void Deserialize_FromField_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public JavaScriptSerializer Serializer;

        public T D<T>(string str)
        {
            return (T) this.Serializer.Deserialize(str, typeof(T));
        }
    }
}",
                GetCSharpResultAt(13, 24, MaybeRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void Deserialize_FromStaticField_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public static JavaScriptSerializer Serializer;

        public T D<T>(string str)
        {
            return (T) Program.Serializer.Deserialize(str, typeof(T));
        }
    }
}",
                GetCSharpResultAt(13, 24, MaybeRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void Deserialize_NoTypeResolver_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public T D<T>(string str)
        {
            return (T) new JavaScriptSerializer().Deserialize(str, typeof(T));
        }
    }
}");
        }

        [Fact]
        public void Deserialize_CustomTypeResolver_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class MyTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id)
        {
            throw new NotImplementedException();
        }

        public override string ResolveTypeId(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public T D<T>(string str)
        {
            return (T) new JavaScriptSerializer(new MyTypeResolver()).Deserialize(str, typeof(T));
        }
    }
}");
        }

        [Fact]
        public void DeserializeObject_FromLocalFunction_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            return GetSerializer().DeserializeObject(str);

            JavaScriptSerializer GetSerializer() => new JavaScriptSerializer(new SimpleTypeResolver());
        }
    }
}",
            GetCSharpResultAt(11, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_SimpleTypeResolverFromParameter_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(SimpleTypeResolver str1, string str2)
        {
            return GetSerializer().DeserializeObject(str2);

            JavaScriptSerializer GetSerializer() => new JavaScriptSerializer(str1);
        }
    }
}",
                GetCSharpResultAt(11, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_JavaScriptTypeResolverFromParameter_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(JavaScriptTypeResolver jstr, string str)
        {
            return GetSerializer().DeserializeObject(str);

            JavaScriptSerializer GetSerializer() => new JavaScriptSerializer(jstr);
        }
    }
}",
               GetCSharpResultAt(11, 20, MaybeRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_SimpleTypeResolverFromLocalFunction_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            return new JavaScriptSerializer(GetTypeResolver()).DeserializeObject(str);

            JavaScriptTypeResolver GetTypeResolver() => new SimpleTypeResolver();
        }
    }
}",
               GetCSharpResultAt(11, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }


        [Fact]
        public void Deserialize_InLocalFunction_SimpleTypeResolverFromLocalFunction_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str, Type t)
        {
            return Deserialize();

            JavaScriptTypeResolver GetTypeResolver() => new SimpleTypeResolver();

            object Deserialize() => new JavaScriptSerializer(GetTypeResolver()).Deserialize(str, t);
        }
    }
}",
               GetCSharpResultAt(16, 37, DefinitelyRule, "object JavaScriptSerializer.Deserialize(string input, Type targetType)"));
        }

        [Fact]
        public void DeserializeObject_InLambda_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            Func<string, object> f = (s) => new JavaScriptSerializer(GetTypeResolver()).DeserializeObject(s);
            return f(str);

            JavaScriptTypeResolver GetTypeResolver() => new SimpleTypeResolver();
        }
    }
}",
                  GetCSharpResultAt(12, 45, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_InLambda_CustomTypeResolver_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            Func<string, object> f = (s) => new JavaScriptSerializer(GetTypeResolver()).DeserializeObject(s);
            return f(str);

            JavaScriptTypeResolver GetTypeResolver() => new MyTypeResolver();
        }
    }

    public class MyTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id)
        {
            throw new NotImplementedException();
        }

        public override string ResolveTypeId(Type type)
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [Fact]
        public void DeserializeObject_InOtherMethod_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            return D(GetTypeResolver(), str);

            JavaScriptTypeResolver GetTypeResolver() => new SimpleTypeResolver();
        }

        public object D(JavaScriptTypeResolver tr, string s)
        {
            return new JavaScriptSerializer(tr).DeserializeObject(s);
        }
    }
}",
                  GetCSharpResultAt(19, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_InOtherMethodThrice_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            JavaScriptTypeResolver tr = GetTypeResolver();
            D(tr, str);
            D(tr, str);
            return D(tr, str);

            JavaScriptTypeResolver GetTypeResolver() => new SimpleTypeResolver();
        }

        public object D(JavaScriptTypeResolver tr, string s)
        {
            return new JavaScriptSerializer(tr).DeserializeObject(s);
        }
    }
}",
                  GetCSharpResultAt(22, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }

        [Fact]
        public void DeserializeObject_InOtherMethod_OnceDefinitely_OnceMaybe_DefinitelyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public JavaScriptTypeResolver OtherTypeResolver { get; set; }

        public object D(string str)
        {
            JavaScriptTypeResolver tr = GetTypeResolver();
            D(tr, str);
            return D(OtherTypeResolver, str);

            JavaScriptTypeResolver GetTypeResolver() => new SimpleTypeResolver();
        }

        public object D(JavaScriptTypeResolver tr, string s)
        {
            return new JavaScriptSerializer(tr).DeserializeObject(s);
        }
    }
}",
                  GetCSharpResultAt(23, 20, DefinitelyRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }


        [Fact]
        public void DeserializeObject_InOtherMethod_CustomTypeResolver_MaybeDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Blah
{
    public class Program
    {
        public object D(string str)
        {
            return D(GetTypeResolver(), str);

            JavaScriptTypeResolver GetTypeResolver() => new MyTypeResolver();
        }

        public object D(JavaScriptTypeResolver tr, string s)
        {
            return new JavaScriptSerializer(tr).DeserializeObject(s);
        }
    }

    public class MyTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id)
        {
            throw new NotImplementedException();
        }

        public override string ResolveTypeId(Type type)
        {
            throw new NotImplementedException();
        }
    }
}",
                  GetCSharpResultAt(19, 20, MaybeRule, "object JavaScriptSerializer.DeserializeObject(string input)"));
        }
    }
}
