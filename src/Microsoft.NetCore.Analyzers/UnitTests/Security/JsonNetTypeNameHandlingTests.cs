// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class JsonNetTypeNameHandlingTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor Rule = JsonNetTypeNameHandling.Rule;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new JsonNetTypeNameHandling();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new JsonNetTypeNameHandling();
        }

        [Fact]
        public void DocSample1_CSharp_Violation_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void DocSample1_VB_Violation_Diagnostic()
        {
            this.VerifyBasicWithJsonNet(@"
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
        public void DocSample1_CSharp_Solution_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void DocSample1_VB_Solution_NoDiagnostic()
        {
            this.VerifyBasicWithJsonNet(@"
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
        public void Reference_TypeNameHandling_None_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Reference_TypeNameHandling_All_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Reference_AttributeTargets_All_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Assign_TypeNameHandling_Objects_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Assign_TypeNameHandling_1_Or_Arrays_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Assign_TypeNameHandling_0_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Assign_TypeNameHandling_None_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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

        private void VerifyCSharpWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            this.VerifyCSharpAcrossTwoAssemblies(NewtonsoftJsonNetApis.CSharp, source, expected);
        }

        private void VerifyBasicWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            this.VerifyBasicAcrossTwoAssemblies(NewtonsoftJsonNetApis.VisualBasic, source, expected);
        }
    }
}
