// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferDictionaryTryGetValueAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpPreferDictionaryTryGetValueFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class PreferDictionaryTryGetValueMethodsTests
    {
        private const string CSharpTemplate = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{{
    public class TestClass
    {{
        public int TestMethod()
        {{
            {0}
        }}
    }}
}}";

        private const string DictionaryTryGetValue = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            {|#0:if (data.ContainsKey(key))
            {
                Console.WriteLine(data[key]);
            }|}

            return 0;
";
        
        private const string DictionaryTryGetValueFixed = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (data.TryGetValue(key, out var value))
            {
                Console.WriteLine(value);
            }

            return 0;
";

        private const string DictionaryTryGetValue2 = @"
string key = ""key"";
IDictionary<string, int> data = new Dictionary<string, int>();
if (data.ContainsKey(key))
{
    return data[key];
}

return 0;
";
        
        private const string DictionaryTryGetValueFixed2 = @"
string key = ""key"";
IDictionary<string, int> data = new Dictionary<string, int>();
if (data.TryGetValue(key, out var value))
{
    return value;
}

return 0;
";
        [Fact]
        public Task IDictionary_Keys_Contains_ReportsDiagnostic_CS()
        {
            string testCode = CreateCSharpCode(DictionaryTryGetValue);
            string fixedCode = CreateCSharpCode(DictionaryTryGetValueFixed);
            var diagnostic = VerifyCS.Diagnostic(PreferDictionaryTryGetValueAnalyzer.ContainsKeyRule)
                .WithLocation(0)
                .WithArguments("IDictionary");

            return new VerifyCS.Test
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                ExpectedDiagnostics = { diagnostic }
            }.RunAsync();
        }
        
        private string CreateCSharpCode(string content)
        {
            return string.Format(CSharpTemplate, content);
        }
    }
}