// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureDeserializerJsonNetWithoutBinderTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor DefinitelyRule =
            DoNotUseInsecureDeserializerJsonNetWithoutBinder.DefinitelyInsecureSerializer;
        private static readonly DiagnosticDescriptor MaybeRule =
            DoNotUseInsecureDeserializerJsonNetWithoutBinder.MaybeInsecureSerializer;

        [Fact]
        public void Insecure_JsonSerializer_Deserialize_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    object Method(JsonReader jr)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.All;
        return serializer.Deserialize(jr);
    }
}",
                GetCSharpResultAt(10, 16, DefinitelyRule));
        }

        [Fact]
        public void ExplicitlyNone_JsonSerializer_Deserialize_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    object Method(JsonReader jr)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.None;
        return serializer.Deserialize(jr);
    }
}");
        }

        [Fact]
        public void AllAndBinder_JsonSerializer_Deserialize_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    private Func<ISerializationBinder> SbGetter;

    object Method(JsonReader jr)
    {
        ISerializationBinder sb = SbGetter();
        if (sb != null)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;
            serializer.SerializationBinder = sb;
            return serializer.Deserialize(jr);
        }
        else
        {
            return null;
        }
    }
}");
        }

        [Fact]
        public void InitializeField_JsonSerializer_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    JsonSerializer MyJsonSerializer;

    void Init()
    {
        this.MyJsonSerializer = new JsonSerializer();
        this.MyJsonSerializer.TypeNameHandling = TypeNameHandling.All;
    }
}",
                GetCSharpResultAt(10, 9, DefinitelyRule));
        }

        [Fact]
        public void Insecure_JsonSerializer_Populate_MaybeDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    private Func<ISerializationBinder> SbGetter;

    object Method(JsonReader jr)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.All;
        serializer.SerializationBinder = SbGetter();
        object o = new object();
        serializer.Populate(jr, o);
        return o;
    }
}",
                GetCSharpResultAt(15, 9, MaybeRule));
        }

        [Fact]
        public void Insecure_JsonSerializer_DeserializeGeneric_MaybeDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    private Func<ISerializationBinder> SbGetter;

    T Method<T>(JsonReader jr)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.All;
        serializer.SerializationBinder = SbGetter();
        return serializer.Deserialize<T>(jr);
    }
}",
                GetCSharpResultAt(14, 16, MaybeRule));
        }

        // Ideally, we'd transfer the JsonSerializerSettings' TypeNameHandling's state to the JsonSerializer's TypeNameHandling's state.
        [Fact]
        public void Insecure_JsonSerializer_FromInsecureSettings_DeserializeGeneric_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    private Func<ISerializationBinder> SbGetter;

    T Method<T>(JsonReader jr)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Arrays,
        };
        JsonSerializer serializer = JsonSerializer.Create(settings);
        return serializer.Deserialize<T>(jr);
    }
}");
        }

        [Fact]
        public void TypeNameHandlingNoneBinderNonNull_JsonSerializer_Populate_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

class Blah
{
    private Func<SerializationBinder> SbGetter;

    object Method(JsonReader jr)
    {
        SerializationBinder sb = SbGetter();
        if (sb != null)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Binder = sb;
            object o = new object();
            serializer.Populate(jr, o);
            return o;
        }
        else
        {
            return null;
        }
    }
}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.excluded_symbol_names = Method")]
        [InlineData(@"dotnet_code_quality.CA2329.excluded_symbol_names = Method
                      dotnet_code_quality.CA2330.excluded_symbol_names = Method")]
        [InlineData("dotnet_code_quality.dataflow.excluded_symbol_names = Method")]
        public void EditorConfigConfiguration_ExcludedSymbolNamesOption(string editorConfigText)
        {
            DiagnosticResult[] expected = Array.Empty<DiagnosticResult>();
            if (editorConfigText.Length == 0)
            {
                expected = new DiagnosticResult[]
                {
                    GetCSharpResultAt(10, 16, DefinitelyRule)
                };
            }

            VerifyCSharpAcrossTwoAssemblies(
                NewtonsoftJsonNetApis.CSharp,
                @"
using Newtonsoft.Json;

class Blah
{
    object Method(JsonReader jr)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.All;
        return serializer.Deserialize(jr);
    }
}",
                GetEditorConfigAdditionalFile(editorConfigText),
                expected);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerJsonNetWithoutBinder();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerJsonNetWithoutBinder();
        }

        private void VerifyCSharpWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            this.VerifyCSharpAcrossTwoAssemblies(NewtonsoftJsonNetApis.CSharp, source, expected);
        }
    }
}
