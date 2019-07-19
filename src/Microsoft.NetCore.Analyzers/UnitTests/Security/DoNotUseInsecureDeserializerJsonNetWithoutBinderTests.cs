// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        serializer.TypeNameHandling = TypeNameHandling.All;
    }
}");
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
