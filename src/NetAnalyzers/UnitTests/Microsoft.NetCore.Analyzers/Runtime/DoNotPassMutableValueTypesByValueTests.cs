// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotPassMutableValueTypesByValueAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDoNotPassMutableValueTypesByValueFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotPassMutableValueTypesByValueAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicDoNotPassMutableValueTypesByValueFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotPassMutableValueTypesByValueTests
    {
        public static IEnumerable<object[]> CS_KnownProblematicTypeNames
        {
            get
            {
                yield return new[] { "System.Threading.SpinLock" };
                yield return new[] { "System.Text.Json.Utf8JsonReader" };
            }
        }

        public static IEnumerable<object[]> VB_KnownProblematicTypeNames
        {
            get
            {
                yield return new[] { "System.Threading.SpinLock" };
            }
        }

        [Theory]
        [MemberData(nameof(CS_KnownProblematicTypeNames))]
        public Task KnownProblematicTypes_ByValue_Diagnostic_CS(string knownTypeName)
        {
            string source = $@"
public class Testopolis
{{
    public void ByValue({{|#0:{knownTypeName} x|}}) {{ }}
}}";
            string fixedSource = $@"
public class Testopolis
{{
    public void ByValue(ref {knownTypeName} x) {{ }}
}}";
            var diagnostics = VerifyCS.Diagnostic(Rule).WithLocation(0).WithArguments(knownTypeName);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(VB_KnownProblematicTypeNames))]
        public Task KnownProblematicTypes_ByValue_Diagnostic_VB(string knownTypeName)
        {
            string source = $@"
Public Class Testopolis
    Public Sub ByValue({{|#0:x As {knownTypeName}|}})
    End Sub
End Class";
            string fixedSource = $@"
Public Class Testopolis
    Public Sub ByValue(ByRef x As {knownTypeName})
    End Sub
End Class";
            var diagnostics = VerifyVB.Diagnostic(Rule).WithLocation(0).WithArguments(knownTypeName);

            return VerifyVB.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(CS_KnownProblematicTypeNames))]
        public Task KnownProblematicTypes_ByReferenceReadOnly_Diagnostic_CS(string knownTypeName)
        {
            string source = $@"
public class Testopolis
{{
    public void ByReferenceReadOnly({{|#0:in {knownTypeName} x|}}) {{ }}
}}";
            string fixedSource = $@"
public class Testopolis
{{
    public void ByReferenceReadOnly(ref {knownTypeName} x) {{ }}
}}";
            var diagnostics = VerifyCS.Diagnostic(Rule).WithLocation(0).WithArguments(knownTypeName);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(CS_KnownProblematicTypeNames))]
        public Task KnownProblematicTypes_ByReference_NoDiagnostic_CS(string knownTypeName)
        {
            string source = $@"
public class Testopolis
{{
    public void ByReference(ref {knownTypeName} x) {{ }}
    public void ByOutReference(out {knownTypeName} x) => x = default;
}}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(VB_KnownProblematicTypeNames))]
        public Task KnownProblematicTypes_ByReference_NoDiagnostic_VB(string knownTypeName)
        {
            string source = $@"
Public Class Testopolis
    Public Sub ByReference(ByRef x As {knownTypeName})
    End Sub
End Class";

            return VerifyVB.VerifyAnalyzerAsync(source);
        }

        private static DiagnosticDescriptor Rule => DoNotPassMutableValueTypesByValueAnalyzer.Rule;
    }
}
