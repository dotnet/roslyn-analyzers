// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AssigningSymbolAndItsMemberInSameStatement,
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AssigningSymbolAndItsMemberInSameStatementFixer>;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.QualityGuidelines
{
    public class AssigningSymbolAndItsMemberInSameStatementTests
    {
        [Theory]
        [InlineData(0, "a.Field = b;")]
        [InlineData(1, "a.Field = a;")]
        public async Task CSharpReassignLocalVariableAndReferToItsField(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Field;
}

public class Test
{
    public void Method()
    {
        C a = new C(), b = new C();
        [|a.Field|] = a = b;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Field;
}}

public class Test
{{
    public void Method()
    {{
        C a = new C(), b = new C();
        a = b;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "a.Property = b;")]
        [InlineData(1, "a.Property = c;")]

        public async Task CSharpReassignLocalVariableAndReferToItsProperty(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method()
    {
        C a = new C(), b = new C(), c;
        [|a.Property|] = c = a = b;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    public void Method()
    {{
        C a = new C(), b = new C(), c;
        a = b;
        c = a;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "a.Property.Property = b;")]
        [InlineData(1, "a.Property.Property = a.Property;")]
        public async Task CSharpReassignLocalVariablesPropertyAndReferToItsProperty(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method()
    {
        C a = new C(), b = new C();
        [|a.Property.Property|] = a.Property = b;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    public void Method()
    {{
        C a = new C(), b = new C();
        a.Property = b;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "a.Property.Property = b;")]
        [InlineData(1, "a.Property.Property = a.Property;")]
        public async Task CSharpReassignLocalVariableAndItsPropertyAndReferToItsProperty(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method()
    {
        C a = new C(), b = new C();
        [|a.Property.Property|] = [|a.Property|] = a = b;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    public void Method()
    {{
        C a = new C(), b = new C();
        a = b;
        a.Property = a;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "x.Field = y;")]
        [InlineData(1, "x.Field = x;")]
        public async Task CSharpReferToFieldOfReferenceTypeLocalVariableAfterItsReassignment(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Field;
}

public class Test
{
    static C x, y;

    public void Method()
    {
        [|x.Field|] = x = y;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Field;
}}

public class Test
{{
    static C x, y;

    public void Method()
    {{
        x = y;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "x.Property.Property = y;")]
        [InlineData(1, "x.Property.Property = x.Property;")]
        public async Task CSharpReassignGlobalVariableAndReferToItsField(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    static C x, y;

    public void Method()
    {
        [|x.Property.Property|] = x.Property = y;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    static C x, y;

    public void Method()
    {{
        x.Property = y;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "x.Property.Property = y;")]
        [InlineData(1, "x.Property.Property = x.Property;")]
        public async Task CSharpReassignGlobalVariableAndItsPropertyAndReferToItsProperty(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    static C x, y;

    public void Method()
    {
        [|x.Property.Property|] = [|x.Property|] = x = y;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    static C x, y;

    public void Method()
    {{
        x = y;
        x.Property = x;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Theory]
        [InlineData(0, "x.Property.Property = y;")]
        [InlineData(1, "x.Property.Property = x.Property;")]
        public async Task CSharpReassignGlobalPropertyAndItsPropertyAndReferToItsProperty(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    static C x { get; set; } 
    static C y { get; set; }

    public void Method()
    {
        [|x.Property.Property|] = [|x.Property|] = x = y;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    static C x {{ get; set; }}
    static C y {{ get; set; }}

    public void Method()
    {{
        x = y;
        x.Property = x;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Fact]
        public async Task CSharpReassignSecondLocalVariableAndReferToItsPropertyOfFirstVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method()
    {
        C a = new C(), b;
        a.Property = b = a;
    }
}
");
        }

        [Fact]
        public async Task CSharpReassignPropertyOfFirstLocalVariableWithSecondAndReferToPropertyOfSecondVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method()
    {
        C a = new C(), b = new C(), c;
        b.Property.Property = a.Property = b;
    }
}
");
        }

        [Fact]
        public async Task CSharpReassignPropertyOfFirstLocalVariableWithThirdAndReferToPropertyOfSecondVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method()
    {
        C a = new C(), b = new C(), c = new C();
        b.Property.Property = a.Property = c;
    }
}
");
        }

        [Theory]
        [InlineData(0, "b.Property = a;")]
        [InlineData(1, "b.Property = b;")]
        public async Task CSharpReassignMethodParameterAndReferToItsProperty(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Property { get; set; }
}

public class Test
{
    public void Method(C b)
    {
        C a = new C();
        [|b.Property|] = b = a;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Property {{ get; set; }}
}}

public class Test
{{
    public void Method(C b)
    {{
        C a = new C();
        b = a;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }

        [Fact]
        public async Task CSharpReassignLocalValueTypeVariableAndReferToItsField()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct S
{
    public S {|CS0523:Field|};
}

public class Test
{
    public void Method()
    {
        S a, b = new S();
        a.Field = a = b;
    }
}
");
        }

        [Fact]
        public async Task CSharpReassignLocalValueTypeVariableAndReferToItsProperty()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct S
{
    public S Property { get => default; set { } }
}

public class Test
{
    public void Method()
    {
        S a, b = new S();
        a.Property = a = b;
    }
}
");
        }

        [Fact]
        public async Task CSharpAssignmentInCodeWithOperationNone()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Test
{
    public System.IntPtr PtrField;
    public unsafe void Method(Test a, Test *b)
    {
        b->PtrField = a.PtrField;
    }
}
");
        }

        [Fact]
        [WorkItem(2889, "https://github.com/dotnet/roslyn-analyzers/issues/2889")]
        public async Task CSharpAssignmentLocalReferenceOperation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public static class Class1
{
    public static void SomeMethod()
    {
        var u = new System.UriBuilder();
        u.Host = u.Path = string.Empty;
    }
}
");
        }
    }
}
