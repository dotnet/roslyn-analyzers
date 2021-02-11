// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotGuardDictionaryRemoveByContainsKey,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotGuardDictionaryRemoveByContainsKey,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class DoNotGuardDictionaryRemoveByContainsKeyTests
    {
        #region Reports Diagnostic
        [Fact]
        public async Task RemoveIsTheOnlyStatement_ReportsDiagnostic_CS()
        {
            string code = @"
" + CSUsings + @"
namespace Testopolis
{
    public class MyClass
    {
        private readonly Dictionary<string, string> MyDictionary = new Dictionary<string, string>();

        public MyClass()
        {
            if ({|#0:MyDictionary.ContainsKey(""Key"")|})
                MyDictionary.Remove(""Key"");
        }
    }
}";

            var diagnostic = VerifyCS.Diagnostic(Rule).WithLocation(0);
            await new VerifyCS.Test
            {
                TestCode = code,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                ExpectedDiagnostics = { diagnostic }
            }.RunAsync();
        }

        [Fact]
        public async Task AdditionalStatements_NoDiagnostic_CS()
        {
            string code = @"
" + CSUsings + @"
namespace Testopolis
{
    public class MyClass
    {
        private readonly Dictionary<string, string> MyDictionary = new Dictionary<string, string>();

        public MyClass()
        {
            if (MyDictionary.ContainsKey(""Key""))
            {
                MyDictionary.Remove(""Key"");
                Console.WriteLine();
            }
        }
    }
}";

            await new VerifyCS.Test
            {
                TestCode = code,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                ExpectedDiagnostics = { }
            }.RunAsync();
        }

        [Fact]
        public async Task RemoveIsTheOnlyStatement_ReportsDiagnostic_VB()
        {
            string code = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MyDictionary As New Dictionary(Of String, String)()

        Public Sub New()
            If {|#0:MyDictionary.ContainsKey(""Key"")|} Then
                MyDictionary.Remove(""Key"")
            End If
        End Sub
    End Class
End Namespace";

            var diagnostic = VerifyVB.Diagnostic(Rule).WithLocation(0);
            await new VerifyVB.Test
            {
                TestCode = code,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                ExpectedDiagnostics = { diagnostic }
            }.RunAsync();
        }

        [Fact]
        public async Task AdditionalStatements_NoDiagnostic_VB()
        {
            string code = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MyDictionary As New Dictionary(Of String, String)()

        Public Sub New()
            If MyDictionary.ContainsKey(""Key"") Then
                MyDictionary.Remove(""Key"")
                Console.WriteLine()
            End If
        End Sub
    End Class
End Namespace";

            await new VerifyVB.Test
            {
                TestCode = code,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                ExpectedDiagnostics = { }
            }.RunAsync();
        }
        #endregion

        #region Helpers
        private const string CSUsings = @"using System;
using System.Collections.Generic;";

        private const string VBUsings = @"Imports System
Imports System.Collections.Generic";

        private static DiagnosticDescriptor Rule => DoNotGuardDictionaryRemoveByContainsKey.Rule;
        #endregion
    }
}
