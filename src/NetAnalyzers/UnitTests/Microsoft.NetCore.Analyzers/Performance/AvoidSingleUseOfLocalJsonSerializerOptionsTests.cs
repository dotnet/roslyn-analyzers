// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.AvoidSingleUseOfLocalJsonSerializerOptions,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.AvoidSingleUseOfLocalJsonSerializerOptions,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class AvoidSingleUseOfLocalJsonSerializerOptionsTests
    {
        #region Diagnostic Tests

        [Fact]
        public Task CS_UseNewOptionsAsArgument()
        {
            string source = @"
using System;
using System.Text.Json;

internal class Program
{
    static void Main(string[] args)
    {
        string json = JsonSerializer.Serialize(args, new JsonSerializerOptions { AllowTrailingCommas = true });
        Console.WriteLine(json);
    }
}
";
            return VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule)
                .WithSpan(9, 54, 9, 110));
        }

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument()
        {
            string source = @"
using System;
using System.Text.Json;

internal class Program
{
    static void Main(string[] args)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.AllowTrailingCommas = true;        

        string json = JsonSerializer.Serialize(args, options);
        Console.WriteLine(json);
    }
}
";
            return VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule)
                .WithSpan(9, 41, 9, 68));
        }

        [Fact]
        public Task VB_UseNewOptionsAsArgument()
        {
            string source = @"
Imports System
Imports System.Text.Json

Module Program
    Sub Main(args As String())
        Dim json = JsonSerializer.Serialize(args, New JsonSerializerOptions With {.AllowTrailingCommas = True})
        Console.WriteLine(json)
    End Sub
End Module
";
            return VerifyVB.VerifyAnalyzerAsync(source,
                VerifyVB.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule)
                .WithSpan(7, 51, 7, 111));
        }

        [Fact]
        public Task VB_UseNewLocalOptionsAsArgument()
        {
            string source = @"
Imports System
Imports System.Text.Json

Module Program
    Sub Main(args As String())
        Dim options = New JsonSerializerOptions()
        options.AllowTrailingCommas = True

        Dim json = JsonSerializer.Serialize(args, options)
        Console.WriteLine(json)
    End Sub
End Module
";
            return VerifyVB.VerifyAnalyzerAsync(source,
                VerifyVB.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule)
                .WithSpan(7, 23, 7, 50));
        }

        [Theory]
        [InlineData("new JsonSerializerOptions()", 0)]
        [InlineData("new JsonSerializerOptions{}", 0)]
        [InlineData("(new JsonSerializerOptions())", 1)]
        [InlineData("((new JsonSerializerOptions()))", 2)]
        [InlineData("1 == 1 ? new JsonSerializerOptions() : null", 9)]
        [InlineData("1 == 1 ? null : 2 == 2 ? null : new JsonSerializerOptions()", 32)]
        public Task CS_UseNewOptionsAsArgument_Variants(string expression, int startIndex)
        {
            string source = $@"
using System.Text.Json;

class Program
{{
    static string Serialize<T>(T value)
    {{
        return JsonSerializer.Serialize(value, {expression});
    }}
}}
";
            const int startLine = 8, endLine = 8;
            int startColumn = 48 + startIndex;
            int endColumn = startColumn + "new JsonSerializerOptions()".Length;

            return VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule)
                .WithSpan(startLine, startColumn, endLine, endColumn));
        }

        [Theory]
        [InlineData("new JsonSerializerOptions()", 0)]
        [InlineData("new JsonSerializerOptions{}", 0)]
        [InlineData("(new JsonSerializerOptions())", 1)]
        [InlineData("((new JsonSerializerOptions()))", 2)]
        [InlineData("1 == 1 ? new JsonSerializerOptions() : null", 9)]
        [InlineData("1 == 1 ? null : 2 == 2 ? null : new JsonSerializerOptions()", 32)]
        public Task CS_UseNewLocalOptionsAsArgument_Variants(string expression, int startIndex)
        {
            string source = $@"
using System.Text.Json;

class Program
{{
    static string Serialize<T>(T value)
    {{
        var options = {expression};
        return JsonSerializer.Serialize(value, options);
    }}
}}
";
            const int startLine = 8, endLine = 8;
            int startColumn = 23 + startIndex;
            int endColumn = startColumn + "new JsonSerializerOptions()".Length;

            return VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule)
                .WithSpan(startLine, startColumn, endLine, endColumn));
        }

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_Assignment()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        JsonSerializerOptions opt;
        opt = new JsonSerializerOptions();

        return JsonSerializer.Serialize(value, opt);
    }
}
", VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule).WithSpan(9, 15, 9, 42));

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_SecondLocalReference()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        JsonSerializerOptions opt = new JsonSerializerOptions();
        _ = opt;

        return JsonSerializer.Serialize(value, opt);
    }
}
", VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule).WithSpan(8, 37, 8, 64));

        [Fact] // this could be better handled with data flow analysis.
        public Task CS_UseNewLocalOptionsAsArgument_OverwriteLocal()
    => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static JsonSerializerOptions s_options;

    static string Serialize<T>(T value)
    {
        JsonSerializerOptions opt = new JsonSerializerOptions();
        opt = s_options;

        return JsonSerializer.Serialize(value, opt);
    }
}
", VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule).WithSpan(10, 37, 10, 64));

        [Theory]
        [InlineData("opt1")]
        [InlineData("opt2")]
        public Task CS_UseNewLocalOptionsAsArgument_MultiAssignment(string expression)
            => VerifyCS.VerifyAnalyzerAsync($@"
using System.Text.Json;

class Program
{{
    static string Serialize<T>(T value)
    {{
        JsonSerializerOptions opt1, opt2;
        opt1 = opt2 = new JsonSerializerOptions();

        return JsonSerializer.Serialize(value, {expression});
    }}
}}
", VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule).WithSpan(9, 23, 9, 50));

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_Delegate()
    => VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Text.Json;

class Program
{
    static Action Serialize<T>(T value)
    {
        Action lambda = () =>
        {
            JsonSerializerOptions opt = new JsonSerializerOptions();
            JsonSerializer.Serialize(value, opt);
        };
        return lambda;
    }
}
", VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule).WithSpan(11, 41, 11, 68));

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_LocalFunction()
=> VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        return LocalFunc();

        string LocalFunc()
        {
            JsonSerializerOptions opt = new JsonSerializerOptions();
            return JsonSerializer.Serialize(value, opt);
        }
    }
}
", VerifyCS.Diagnostic(AvoidSingleUseOfLocalJsonSerializerOptions.s_Rule).WithSpan(12, 41, 12, 68));

        #endregion

        #region No Diagnostic Tests
        [Fact]
        public Task CS_UseNewOptionsAsArgument_NonSerializerMethod_NoWarn()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        return MyCustomSerializeMethod(value, new JsonSerializerOptions());
    }

    static string MyCustomSerializeMethod<T>(T value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(value, options);
}
");

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_NonSerializerMethod_NoWarn()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        var options = new JsonSerializerOptions();
        return MyCustomSerializeMethod(value, options);
    }

    static string MyCustomSerializeMethod<T>(T value, JsonSerializerOptions options) 
        => JsonSerializer.Serialize(value, options);
}
");

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_EscapeCurrentScope_NonSerializerMethod_NoWarn()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        var options = new JsonSerializerOptions();
        string json1 = MyCustomSerializeMethod(value, options);
        string json2 = JsonSerializer.Serialize(value, options);
        return json1 + json2;
    }

    static string MyCustomSerializeMethod<T>(T value, JsonSerializerOptions options) 
        => JsonSerializer.Serialize(value, options);
}
");

        [Theory]
        [MemberData(nameof(CS_UseNewLocalOptionsAsArgument_FieldAssignment_NoWarn_TheoryData))]
        public Task CS_UseNewLocalOptionsAsArgument_FieldAssignment_NoWarn(string snippet)
        {
            var test = new VerifyCS.Test();
            test.LanguageVersion = LanguageVersion.CSharp8; // needed for coalescing assignment.
            test.TestCode = $@"
using System;
using System.Text.Json;

class Program
{{
    static JsonSerializerOptions s_options;

    static string Serialize<T>(T value)
    {{
        {snippet}
        return JsonSerializer.Serialize(value, opt);
    }}
}}
";

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(CS_UseNewLocalOptionsAsArgument_PropertyAssignment_NoWarn_TheoryData))]
        public Task CS_UseNewLocalOptionsAsArgument_PropertyAssignment_NoWarn(string snippet)
        {
            var test = new VerifyCS.Test();
            test.LanguageVersion = LanguageVersion.CSharp8; // needed for coalescing assignment.
            test.TestCode = $@"
using System;
using System.Text.Json;

class Program
{{
    private JsonSerializerOptions Options {{ get; set; }}

    string Serialize<T>(T value)
    {{
        {snippet}
        return JsonSerializer.Serialize(value, opt);
    }}
}}
";
            return test.RunAsync();
        }

        public static IEnumerable<object[]> CS_UseNewLocalOptionsAsArgument_FieldAssignment_NoWarn_TheoryData()
        {
            return CS_UseNewLocalOptionsAsArgument_Assignment_TheoryData(useField: true).Select(e => new object[] { e });
        }

        public static IEnumerable<object[]> CS_UseNewLocalOptionsAsArgument_PropertyAssignment_NoWarn_TheoryData()
        {
            return CS_UseNewLocalOptionsAsArgument_Assignment_TheoryData(useField: false).Select(e => new object[] { e });
        }

        private static List<string> CS_UseNewLocalOptionsAsArgument_Assignment_TheoryData(bool useField)
        {
            string target = useField ? "s_options" : "Options";

            return new List<string>()
            {
                $@"JsonSerializerOptions opt = new JsonSerializerOptions();
                    {target} = opt;",

                $@"JsonSerializerOptions opt;
                    {target} = opt = new JsonSerializerOptions();",

                $@"JsonSerializerOptions opt = {target} = new JsonSerializerOptions();",

                $@"JsonSerializerOptions opt = {target} ??= new JsonSerializerOptions();",

                $@"JsonSerializerOptions opt = new JsonSerializerOptions();
                    {target} ??= opt;",

                $@"JsonSerializerOptions opt = new JsonSerializerOptions();
                    ({target}, _) = (opt, 42);",

                $@"JsonSerializerOptions opt = new JsonSerializerOptions();
                    (({target}, _), _) = ((opt, 42), 42);"
            };
        }

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_NotSingleUse_NoWarn()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string SerializeTwice<T>(T value)
    {
        JsonSerializerOptions opt = new JsonSerializerOptions();

        string str1 = JsonSerializer.Serialize(value, opt);
        string str2 = JsonSerializer.Serialize(value, opt);

        return str1 + str2;
    }
}
");

        [Theory]
        [InlineData("opt1", "opt2")]
        [InlineData("opt1", "opt3")]
        [InlineData("opt2", "opt3")]
        public Task CS_UseNewLocalOptionsAsArgument_MultiAssignment_NotSingleUse_NoWarn(string expression1, string expression2)
            => VerifyCS.VerifyAnalyzerAsync($@"
using System.Text.Json;

class Program
{{
    static string Serialize<T>(T value)
    {{
        JsonSerializerOptions opt1, opt2, opt3;
        opt1 = opt2 = opt3 = new JsonSerializerOptions();

        string json1 = JsonSerializer.Serialize(value, {expression1});
        string json2 = JsonSerializer.Serialize(value, {expression2});
        
        return json1 + json2;
    }}
}}
");

        [Theory]
        [InlineData("opt1")]
        [InlineData("opt2")]
        [InlineData("opt3")]
        public Task CS_UseNewLocalOptionsAsArgument_MultiAssignment_EscapeCurrentScope_FieldAssignment_NoWarn(string expression)
            => VerifyCS.VerifyAnalyzerAsync($@"
using System.Text.Json;

class Program
{{
    static JsonSerializerOptions s_options;    

    static string Serialize<T>(T value)
    {{
        JsonSerializerOptions opt1, opt2, opt3;
        opt1 = opt2 = opt3 = new JsonSerializerOptions();

        s_options = {expression};

        return JsonSerializer.Serialize(value, opt1);   
    }}
}}
");

        [Theory]
        [InlineData("opt1 = opt2 = s_options")]
        [InlineData("opt1 = s_options = opt2")]
        [InlineData("s_options = opt1 = opt2")]
        public Task CS_UseNewLocalOptionsAsArgument_MultiAssignment_EscapeCurrentScope_FieldInMultiAssignment_NoWarn(string expression)
            => VerifyCS.VerifyAnalyzerAsync($@"
using System.Text.Json;

class Program
{{
    static JsonSerializerOptions s_options;    

    static string Serialize<T>(T value)
    {{
        JsonSerializerOptions opt1, opt2;
        {expression} = new JsonSerializerOptions();

        return JsonSerializer.Serialize(value, opt1);   
    }}
}}
");

        [Theory]
        [InlineData("s_options = opt1 = opt2")]
        [InlineData("opt1 = s_options = opt2")]
        public Task CSharpUseNewOptionsAsLocalThenAsArgument_AssignmentOnNextStatement_Multiple_WithEscapeScopeOnAssignment_NoWarn(string expression)
            => VerifyCS.VerifyAnalyzerAsync($@"
using System.Text.Json;

class Program
{{
    static JsonSerializerOptions s_options;

    static string Serialize<T>(T value)
    {{
        JsonSerializerOptions opt1, opt2;
        opt1 = opt2 = new JsonSerializerOptions();

        {expression};

        return JsonSerializer.Serialize(value, opt1);
    }}
}}
");

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_EscapeCurrentScope_ClosureDelegate_NoWarn()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Text.Json;

class Program
{
    static Action Serialize<T>(T value)
    {
        JsonSerializerOptions opt = new JsonSerializerOptions();
        Action lambda = () =>
        {
            JsonSerializer.Serialize(value, opt);
        };
        return lambda;
    }
}
");

        [Fact]
        public Task CS_UseNewLocalOptionsAsArgument_EscapeCurrentScope_ClosureLocalFunction_NoWarn()
            => VerifyCS.VerifyAnalyzerAsync(@"
using System.Text.Json;

class Program
{
    static string Serialize<T>(T value)
    {
        JsonSerializerOptions opt = new JsonSerializerOptions();
        return LocalFunc();

        string LocalFunc()
        {
            return JsonSerializer.Serialize(value, opt);
        }
    }
}
");
        #endregion
    }
}
