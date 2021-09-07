// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using System.Threading;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDoNotPassMutableValueTypesByValueAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDoNotPassMutableValueTypesByValueFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicDoNotPassMutableValueTypesByValueAnalyzer,
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
                yield return new[] { $"dotnet_code_quality.{DoNotPassMutableValueTypesByValueAnalyzer.ParametersRuleId}.{EditorConfigOptionNames.AdditionalMutableValueTypes} = MyMutableStruct" };
                yield return new[] { $"dotnet_code_quality.{EditorConfigOptionNames.AdditionalMutableValueTypes} = T:MyMutableStruct" };
            }
        }

        [Theory]
        [MemberData(nameof(CS_KnownProblematicTypeNames))]
        public Task KnownTypes_ByValueParameter_Diagnostic_CS(string knownTypeName)
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
            var diagnostics = VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments(knownTypeName);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(VB_KnownProblematicTypeNames))]
        public Task KnownTypes_ByValueParameter_Diagnostic_VB(string knownTypeName)
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
            var diagnostics = VerifyVB.Diagnostic(ParametersRule).WithLocation(0).WithArguments(knownTypeName);

            return VerifyVB.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(CS_KnownProblematicTypeNames))]
        public Task KnownTypes_ByReferenceReadOnlyParameter_Diagnostic_CS(string knownTypeName)
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
            var diagnostics = VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments(knownTypeName);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(CS_KnownProblematicTypeNames))]
        public Task KnownTypes_ByReferenceParameter_NoDiagnostic_CS(string knownTypeName)
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
        public Task KnownTypes_ByReferenceParameter_NoDiagnostic_VB(string knownTypeName)
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
        public Task EnumeratorTypes_ByValueParameter_Diagnostic_CS(string enumeratorTypeName)
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
            var diagnostics = VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments($"MyList.{enumeratorTypeName}");

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [MemberData(nameof(EnumeratorTypeNames))]
        public Task EnumeratorTypes_ByValueParameter_Diagnostic_VB(string enumeratorTypeName)
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
            var diagnostics = VerifyVB.Diagnostic(ParametersRule).WithLocation(0).WithArguments($"MyList.{enumeratorTypeName}");

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

        [Fact]
        public Task EnumeratorTypes_GetEnumeratorReturn_NoDiagnostic_CS()
        {
            string source = @"
public class MyList
{
    public struct Enumerator { }
    public Enumerator GetEnumerator() => new Enumerator();
}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public Task EnumeratorTypes_GetEnumeratorReturn_NoDiagnostic_VB()
        {
            string source = @"
Public Class MyList
    Public Structure Enumerator
    End Structure
    Public Function GetEnumerator() As Enumerator
    End Function
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
                        VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments("MyMutableStruct")
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
                        VerifyVB.Diagnostic(ParametersRule).WithLocation(0).WithArguments("MyMutableStruct")
                    }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [InlineData("local")]
        [InlineData("parameter")]
        [InlineData("Widget.Field")]
        [InlineData("Widget.RefProperty")]
        [InlineData("Widget.RefMethod()")]
        [InlineData("Widget.RefDelegateField()")]
        public Task LValueArguments_AreFixed_CS(string argument)
        {
            string source = $@"
using System.Threading;

public delegate ref SpinLock RefDelegate();
public class Widget
{{
    public static Widget Value;
    
    public static SpinLock Field;
    public static ref SpinLock RefProperty => ref Field;
    public static ref SpinLock RefMethod() => ref Field;
    public static RefDelegate RefDelegateField;
}}

public class Testopolis
{{
    public void ByValue({{|#0:SpinLock x|}}) {{ }}
    public void Consume(ref SpinLock parameter)
    {{
        var local = new SpinLock();
        ByValue({argument});
    }}
}}";
            string fixedSource = $@"
using System.Threading;

public delegate ref SpinLock RefDelegate();
public class Widget
{{
    public static Widget Value;
    
    public static SpinLock Field;
    public static ref SpinLock RefProperty => ref Field;
    public static ref SpinLock RefMethod() => ref Field;
    public static RefDelegate RefDelegateField;
}}

public class Testopolis
{{
    public void ByValue(ref SpinLock x) {{ }}
    public void Consume(ref SpinLock parameter)
    {{
        var local = new SpinLock();
        ByValue(ref {argument});
    }}
}}";
            var diagnostic = VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments(WellKnownTypeNames.SystemThreadingSpinLock);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostic, fixedSource);
        }

        [Fact]
        public Task LValueArguments_SeparateDocument_AreFixed_CS()
        {
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
using System.Threading;

public static class Provider
{
    public static void ByValue({|#0:SpinLock x|}) { }
}",
                        @"
using System.Threading;

public class Testopolis
{
    public void Consume()
    {
        SpinLock x = new SpinLock();
        Provider.ByValue(x);
    }
}"
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments(WellKnownTypeNames.SystemThreadingSpinLock)
                    }
                },

                FixedState =
                {
                    Sources =
                    {
                        @"
using System.Threading;

public static class Provider
{
    public static void ByValue(ref SpinLock x) { }
}",
                        @"
using System.Threading;

public class Testopolis
{
    public void Consume()
    {
        SpinLock x = new SpinLock();
        Provider.ByValue(ref x);
    }
}"
                    }
                }
            };

            return test.RunAsync();
        }

        [Fact]
        public Task LValueArguments_SeparateProjects_AreFixed_CS()
        {
            ProjectState consumer1 = new("Consumer1", LanguageNames.CSharp, "/1/c", "cs")
            {
                Sources =
                {
                    @"
using System.Threading;
public class Consumer
{
    public void Consume()
    {
        var x = new SpinLock();
        Provider.ByValue(x);
    }
}"
                },
                AdditionalProjectReferences =
                {
                    "TestProject"
                }
            };

            ProjectState fixedConsumer1 = new("Consumer1", LanguageNames.CSharp, "/1/c", "cs")
            {
                Sources =
                {
                    @"
using System.Threading;
public class Consumer
{
    public void Consume()
    {
        var x = new SpinLock();
        Provider.ByValue(ref x);
    }
}"
                },
                AdditionalProjectReferences =
                {
                    "TestProject"
                }
            };

            ProjectState consumer2 = new("Consumer2", LanguageNames.CSharp, "/2/c", "cs")
            {
                Sources =
                {
                    @"
using System.Threading;
public class Consumer
{
    public void Consume()
    {
        var x = new SpinLock();
        Provider.ByValue(x);
        Provider.ByValue(x);
    }
}"
                },
                AdditionalProjectReferences =
                {
                    "TestProject"
                }
            };

            ProjectState fixedConsumer2 = new("Consumer2", LanguageNames.CSharp, "/2/c", "cs")
            {
                Sources =
                {
                    @"
using System.Threading;
public class Consumer
{
    public void Consume()
    {
        var x = new SpinLock();
        Provider.ByValue(ref x);
        Provider.ByValue(ref x);
    }
}"
                },
                AdditionalProjectReferences =
                {
                    "TestProject"
                }
            };

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
using System.Threading;
public static class Provider
{
    public static void ByValue({|#0:SpinLock x|}) { }
    public static void Consume()
    {
        var y = new SpinLock();
        ByValue(y);
    }
}"
                    },
                    AdditionalProjects =
                    {
                        { "Consumer1", consumer1 },
                        { "Consumer2", consumer2 }
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(ParametersRule).WithLocation(0).WithArguments(WellKnownTypeNames.SystemThreadingSpinLock)
                    }
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
using System.Threading;
public static class Provider
{
    public static void ByValue(ref SpinLock x) { }
    public static void Consume()
    {
        var y = new SpinLock();
        ByValue(ref y);
    }
}"
                    },
                    AdditionalProjects =
                    {
                        { "Consumer1", fixedConsumer1 },
                        { "Consumer2", fixedConsumer2 }
                    }
                }
            };

            return test.RunAsync();
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
#pragma warning disable {DoNotPassMutableValueTypesByValueAnalyzer.ReturnValuesRuleId}
    private SpinLock Factory() => new SpinLock();
    private SpinLock Property {{ get; set; }}
#pragma warning restore {DoNotPassMutableValueTypesByValueAnalyzer.ReturnValuesRuleId}
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
                        VerifyCS.Diagnostic(ParametersRule).WithArguments(WellKnownTypeNames.SystemThreadingSpinLock).WithLocation(0)
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
#pragma warning disable {DoNotPassMutableValueTypesByValueAnalyzer.ReturnValuesRuleId}
    private SpinLock Factory() => new SpinLock();
    private SpinLock Property {{ get; set; }}
#pragma warning restore {DoNotPassMutableValueTypesByValueAnalyzer.ReturnValuesRuleId}
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

        [Theory]
        [InlineData("System.Threading.SpinLock")]
        [InlineData("UserDefined")]
        [InlineData("MyList.MyEnumerator")]
        public Task ReturnTypes_Diagnostic_CS(string returnType)
        {
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
public struct UserDefined {{ }}
public class MyList
{{
    public struct MyEnumerator {{ }}
}}

public class Testopolis
{{
    private {returnType} _field;
    public {{|#0:{returnType}|}} Property {{ get; set; }}
    public {{|#1:{returnType}|}} Method(int x, int y)
    {{
        return _field;
    }}
}}"
                    },
                    AnalyzerConfigFiles =
                    {
                        ("/.editorconfig", $@"root = true
[*]
dotnet_code_quality.{EditorConfigOptionNames.AdditionalMutableValueTypes} = UserDefined
")
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(ReturnValuesRule).WithLocation(0).WithArguments(returnType),
                        VerifyCS.Diagnostic(ReturnValuesRule).WithLocation(1).WithArguments(returnType)
                    }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [InlineData("System.Threading.SpinLock")]
        [InlineData("UserDefined")]
        [InlineData("MyList.MyEnumerator")]
        public Task ReturnTypes_Diagnostic_VB(string returnType)
        {
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
Public Structure UserDefined
End Structure
Public Class MyList
    Public Structure MyEnumerator
    End Structure
End Class

Public Class Testopolis
    Private _field As {returnType}
    Public Property {{|#0:MyProperty|}} As {returnType}
    Public Function {{|#1:MyMethod|}}(x As Integer, y As Integer) As {returnType}
        Return _field
    End Function
End Class"
                    },
                    AnalyzerConfigFiles =
                    {
                        ("/.editorconfig", $@"root = true
[*]
dotnet_code_quality.{EditorConfigOptionNames.AdditionalMutableValueTypes} = UserDefined
")
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic(ReturnValuesRule).WithLocation(0).WithArguments(returnType),
                        VerifyVB.Diagnostic(ReturnValuesRule).WithLocation(1).WithArguments(returnType)
                    }
                }
            };

            return test.RunAsync();
        }

        private static DiagnosticDescriptor ParametersRule => DoNotPassMutableValueTypesByValueAnalyzer.ParametersRule;
        private static DiagnosticDescriptor ReturnValuesRule => DoNotPassMutableValueTypesByValueAnalyzer.ReturnValuesRule;
    }
}
