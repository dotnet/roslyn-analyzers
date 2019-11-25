// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.JsonNetTypeNameHandling,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.JsonNetTypeNameHandling,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class JsonNetTypeNameHandlingTests
    {
        private static readonly DiagnosticDescriptor Rule = JsonNetTypeNameHandling.Rule;

        [Fact]
        public async Task DocSample1_CSharp_Violation_Diagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

public class ExampleClass
{
    public JsonSerializerSettings Settings { get; }

    public ExampleClass()
    {
        Settings = new JsonSerializerSettings();
        Settings.TypeNameHandling = TypeNameHandling.All;    // CA2326 violation.
    }
}",
                GetCSharpResultAt(11, 37, Rule));
        }

        [Fact]
        public async Task DocSample1_VB_Violation_Diagnostic()
        {
            await VerifyBasicWithJsonNet(@"
Imports Newtonsoft.Json

Public Class ExampleClass
    Public ReadOnly Property Settings() As JsonSerializerSettings

    Public Sub New()
        Settings = New JsonSerializerSettings()
        Settings.TypeNameHandling = TypeNameHandling.All    ' CA2326 violation.
    End Sub
End Class",
                GetBasicResultAt(9, 37, Rule));
        }

        [Fact]
        public async Task DocSample1_CSharp_Solution_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

public class ExampleClass
{
    public JsonSerializerSettings Settings { get; }

    public ExampleClass()
    {
        Settings = new JsonSerializerSettings();
        
        // The default value of Settings.TypeNameHandling is TypeNameHandling.None.
    }
}");
        }

        [Fact]
        public async Task DocSample1_VB_Solution_NoDiagnostic()
        {
            await VerifyBasicWithJsonNet(@"
Imports Newtonsoft.Json

Public Class ExampleClass
    Public ReadOnly Property Settings() As JsonSerializerSettings

    Public Sub New()
        Settings = New JsonSerializerSettings()

        ' The default value of Settings.TypeNameHandling is TypeNameHandling.None.
    End Sub
End Class");
        }

        [Fact]
        public async Task Reference_TypeNameHandling_None_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        Console.WriteLine(TypeNameHandling.None);
    }
}");
        }

        [Fact]
        public async Task Reference_TypeNameHandling_All_Diagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        Console.WriteLine(TypeNameHandling.All);
    }
}",
                GetCSharpResultAt(9, 27, Rule));
        }

        [Fact]
        public async Task Reference_AttributeTargets_All_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        Console.WriteLine(AttributeTargets.All);
    }
}");
        }

        [Fact]
        public async Task Assign_TypeNameHandling_Objects_Diagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        TypeNameHandling tnh = TypeNameHandling.Objects;
    }
}",
                GetCSharpResultAt(9, 32, Rule));
        }

        [Fact]
        public async Task Assign_TypeNameHandling_1_Or_Arrays_Diagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        TypeNameHandling tnh = (TypeNameHandling) 1 | TypeNameHandling.Arrays;
    }
}",
                GetCSharpResultAt(9, 55, Rule));
        }

        [Fact]
        public async Task Assign_TypeNameHandling_0_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        TypeNameHandling tnh = (TypeNameHandling) 0;
    }
}");
        }

        [Fact]
        public async Task Assign_TypeNameHandling_None_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static void Main(string[] args)
    {
        TypeNameHandling tnh = TypeNameHandling.None;
    }
}");
        }

        private async Task VerifyCSharpWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                },
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                        var sideProject = solution.AddProject("DependencyProject", "DependencyProject", LanguageNames.CSharp)
                            .AddDocument("Dependency.cs", NewtonsoftJsonNetApis.CSharp).Project
                            .AddMetadataReferences(solution.GetProject(projectId).MetadataReferences)
                            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                        return sideProject.Solution.GetProject(projectId)
                            .AddProjectReference(new ProjectReference(sideProject.Id))
                            .Solution;
                    }
                }
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        private async Task VerifyBasicWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            var vbTest = new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                },
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                        var sideProject = solution.AddProject("DependencyProject", "DependencyProject", LanguageNames.VisualBasic)
                            .AddDocument("Dependency.vb", NewtonsoftJsonNetApis.VisualBasic).Project
                            .AddMetadataReferences(solution.GetProject(projectId).MetadataReferences)
                            .WithCompilationOptions(new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                        return sideProject.Solution.GetProject(projectId)
                            .AddProjectReference(new ProjectReference(sideProject.Id))
                            .Solution;
                    }
                }
            };

            vbTest.ExpectedDiagnostics.AddRange(expected);

            await vbTest.RunAsync();
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule)
           => VerifyCS.Diagnostic(rule)
               .WithLocation(line, column);

        private static DiagnosticResult GetBasicResultAt(int line, int column, DiagnosticDescriptor rule)
           => VerifyVB.Diagnostic(rule)
               .WithLocation(line, column);
    }
}
