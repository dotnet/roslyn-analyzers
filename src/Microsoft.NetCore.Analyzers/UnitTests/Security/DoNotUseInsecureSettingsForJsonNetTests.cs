// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureSettingsForJsonNetTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor DefinitelyRule = DoNotUseInsecureSettingsForJsonNet.DefinitelyInsecureSettings;
        private static readonly DiagnosticDescriptor MaybeRule = DoNotUseInsecureSettingsForJsonNet.MaybeInsecureSettings;

        [Fact]
        public void Insecure_JsonConvert_DeserializeObject_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    object Method(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        return JsonConvert.DeserializeObject(s, settings);
    }
}",
                GetCSharpResultAt(10, 16, DoNotUseInsecureSettingsForJsonNet.DefinitelyInsecureSettings));
        }

        [Fact]
        public void Insecure_JsonConvert_DefaultSettings_Lambda_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        JsonConvert.DefaultSettings = () =>
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return settings;
        };
    }
}",
                GetCSharpResultAt(10, 16, DoNotUseInsecureSettingsForJsonNet.DefinitelyInsecureSettings));
        }

        [Fact]
        public void Insecure_Definition_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
}",
                GetCSharpResultAt(10, 16, DoNotUseInsecureSettingsForJsonNet.DefinitelyInsecureSettings));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureSettingsForJsonNet();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureSettingsForJsonNet();
        }

        private void VerifyAcrossTwoAssemblies(string source1, string source2, string language, params DiagnosticResult[] expected)
        {
            Debug.Assert(language == LanguageNames.CSharp || language == LanguageNames.VisualBasic);

            Project project1 = CreateProject(new[] { source1 }, language: language, referenceFlags: ReferenceFlags.RemoveCodeAnalysis);
            Project project2 = CreateProject(new[] { source2 }, language: language, referenceFlags: ReferenceFlags.RemoveCodeAnalysis, addToSolution: project1.Solution)
                           .AddProjectReference(new ProjectReference(project1.Id));

            DiagnosticAnalyzer analyzer = language == LanguageNames.CSharp ? GetCSharpDiagnosticAnalyzer() : GetBasicDiagnosticAnalyzer();
            GetSortedDiagnostics(analyzer, project2.Documents.ToArray()).Verify(analyzer, GetDefaultPath(language), expected);
        }

        private void VerifyCSharpWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            this.VerifyAcrossTwoAssemblies(NewtonsoftJsonNetApis.CSharp, source, LanguageNames.CSharp, expected);
        }
    }
}
