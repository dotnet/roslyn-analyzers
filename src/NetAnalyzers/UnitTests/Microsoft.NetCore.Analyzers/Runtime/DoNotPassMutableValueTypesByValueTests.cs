// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
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

        public static IEnumerable<object[]> EnumeratorTypeNames
        {
            get
            {
                yield return new[] { "Enumerator" };
                yield return new[] { "FrobbingEnumerator" };
            }
        }

        public static IEnumerable<object[]> EditorConfigText
        {
            get
            {
                yield return new[] { $"dotnet_code_quality.{EditorConfigOptionNames.AdditionalMutableValueTypes} = MyMutableStruct" };
                yield return new[] { $"dotnet_code_quality.{DoNotPassMutableValueTypesByValueAnalyzer.RuleId}.{EditorConfigOptionNames.AdditionalMutableValueTypes} = MyMutableStruct" };
                yield return new[] { $"dotnet_code_quality.{EditorConfigOptionNames.AdditionalMutableValueTypes} = T:MyMutableStruct" };
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

        [Theory]
        [MemberData(nameof(EnumeratorTypeNames))]
        public Task EnumeratorTypes_ByValue_Diagnostic_CS(string enumeratorTypeName)
        {
            string source = $@"
public class MyList
{{
    public struct {enumeratorTypeName} {{ }}
}}

public class Testopolis
{{
    public void ByValue({{|#0:MyList.{enumeratorTypeName} x|}}) {{ }}
}}";
            string fixedSource = $@"
public class MyList
{{
    public struct {enumeratorTypeName} {{ }}
}}

public class Testopolis
{{
    public void ByValue(ref MyList.{enumeratorTypeName} x) {{ }}
}}";
            var diagnostics = VerifyCS.Diagnostic(Rule).WithLocation(0).WithArguments($"MyList.{enumeratorTypeName}");

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(EnumeratorTypeNames))]
        public Task EnumeratorTypes_ByValue_Diagnostic_VB(string enumeratorTypeName)
        {
            string source = $@"
Public Class MyList
    Public Structure {enumeratorTypeName}
    End Structure
End Class
Public Class Testopolis
    Public Sub ByValue({{|#0:x As MyList.{enumeratorTypeName}|}})
    End Sub
End Class";
            string fixedSource = $@"
Public Class MyList
    Public Structure {enumeratorTypeName}
    End Structure
End Class
Public Class Testopolis
    Public Sub ByValue(ByRef x As MyList.{enumeratorTypeName})
    End Sub
End Class";
            var diagnostics = VerifyVB.Diagnostic(Rule).WithLocation(0).WithArguments($"MyList.{enumeratorTypeName}");

            return VerifyVB.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Fact]
        public Task EnumeratorTypes_NonNested_NoDiagnostic_CS()
        {
            string source = @"
public struct Enumerator { }

public class Testopolis
{
    public void ByValue(Enumerator x) { }
}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public Task EnumeratorTypes_NonNested_NoDiagnostic_VB()
        {
            string source = $@"
Public Structure Enumerator
End Structure

Public Class Testopolis
    Public Sub ByValue(x As Enumerator)
    End Sub
End Class";

            return VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public Task EnumeratorTypes_Class_NoDiagnostic_CS()
        {
            string source = @"
public class MyList
{
    public class Enumerator { }
}

public class Testopolis
{
    public void ByValue(MyList.Enumerator x) { }
}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public Task EnumeratorTypes_Class_NoDiagnostic_VB()
        {
            string source = @"
Public Class MyList
    Public Class Enumerator
    End Class
End Class

Public Class Testopolis
    Public Sub ByValue(x As MyList.Enumerator)
    End Sub
End Class";

            return VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public Task EnumeratorTypes_Enum_NoDiagnostic_CS()
        {
            string source = @"
public class MyList
{
    public enum Enumerator { None }
}

public class Testopolis
{
    public void ByValue(MyList.Enumerator x) { }
}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public Task EnumeratorTypes_Enum_NoDiagnostic_VB()
        {
            string source = @"
Public Class MyList
    Public Enum Enumerator
        None
    End Enum
End Class

Public Class Testopolis
    Public Sub ByValue(x As MyList.Enumerator)
    End Sub
End Class";

            return VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(EditorConfigText))]
        public Task EditorConfigTypes_Diagnostic_CS(string editorConfigText)
        {
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
public struct MyMutableStruct {{ }}
public class Testopolis
{{
    public void ByValue({{|#0:MyMutableStruct x|}}) {{ }}
}}"
                    },
                    AnalyzerConfigFiles =
                    {
                        ("/.editorconfig", $@"root = true
[*]
{editorConfigText}
")
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(Rule).WithLocation(0).WithArguments("MyMutableStruct")
                    }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(EditorConfigText))]
        public Task EditorConfigTypes_Diagnostic_VB(string editorConfigText)
        {
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
Public Structure MyMutableStruct
End Structure

Public Class Testopolis
    Public Sub ByValue({{|#0:x As MyMutableStruct|}})
    End Sub
End Class"
                    },
                    AnalyzerConfigFiles =
                    {
                        ("/.editorconfig", $@"root = true
[*]
{editorConfigText}
")
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic(Rule).WithLocation(0).WithArguments("MyMutableStruct")
                    }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [InlineData("fieldArgument")]
        [InlineData("localArgument")]
        public Task LValueArguments_AreFixed_CS(string argument)
        {
            string source = $@"
using System.Threading;

public class Testopolis
{{
    private SpinLock fieldArgument;
    
    public void ByValue({{|#0:SpinLock x|}}) {{ }}

    public void Consumer()
    {{
        SpinLock localArgument = new SpinLock();
        ByValue({argument});
    }}
}}";
            string fixedSource = $@"
using System.Threading;

public class Testopolis
{{
    private SpinLock fieldArgument;
    
    public void ByValue(ref SpinLock x) {{ }}

    public void Consumer()
    {{
        SpinLock localArgument = new SpinLock();
        ByValue(ref {argument});
    }}
}}";
            var diagnostics = VerifyCS.Diagnostic(Rule).WithArguments(WellKnownTypeNames.SystemThreadingSpinLock).WithLocation(0);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [InlineData("Factory()")]
        [InlineData("Property")]
        [InlineData("readOnlyField")]
        public Task RValueArguments_AreNotFixed_CS(string argument)
        {
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
using System.Threading;

public class Testopolis
{{
    private SpinLock Factory() => new SpinLock();
    private SpinLock Property {{ get; set; }}
    private readonly SpinLock readOnlyField;

    public void ByValue({{|#0:SpinLock x|}}) {{ }}

    public void Consumer()
    {{
        ByValue({argument});
    }}
}}"
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(Rule).WithArguments(WellKnownTypeNames.SystemThreadingSpinLock).WithLocation(0)
                    }
                },
                FixedState =
                {
                    Sources =
                    {
$@"
using System.Threading;

public class Testopolis
{{
    private SpinLock Factory() => new SpinLock();
    private SpinLock Property {{ get; set; }}
    private readonly SpinLock readOnlyField;

    public void ByValue(ref SpinLock x) {{ }}

    public void Consumer()
    {{
        ByValue({{|#0:{argument}|}});
    }}
}}"
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("CS1620").WithLocation(0)
                    }
                }
            };

            return test.RunAsync();
        }

        private static DiagnosticDescriptor Rule => DoNotPassMutableValueTypesByValueAnalyzer.Rule;
    }
}
