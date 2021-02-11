// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferDictionaryTryGetValueAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferDictionaryTryGetValueFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class PreferDictionaryTryGetValueMethodsTests
    {
        private const string CSharpTemplate = @"
using System;
using System.Collections;
using System.Collections.Concurrent;
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

        private class MyDictionary<TKey, TValue> {{
            public bool ContainsKey(TKey key) {{
                return true;
            }}

            public TValue this[TKey key] {{ get => default; set {{}} }}
        }} 
    }}
}}";

        private const string DictionaryContainsKeyPrintValue = @"
            string key = ""key"";
            Dictionary<string, int> data = new Dictionary<string, int>();
            if ([|data.ContainsKey(key)|])
            {
                Console.WriteLine(data[key]);
            }

            return 0;";

        private const string DictionaryContainsKeyPrintValueFixed = @"
            string key = ""key"";
            Dictionary<string, int> data = new Dictionary<string, int>();
            if (data.TryGetValue(key, out var value))
            {
                Console.WriteLine(value);
            }

            return 0;";

        private const string DictionaryContainsKeyReturnValue = @"
            string key = ""key"";
            ConcurrentDictionary<string, int> data = new ConcurrentDictionary<string, int>();
            if ([|data.ContainsKey(key)|])
            {
                return data[key];
            }

            return 0;";

        private const string DictionaryContainsKeyReturnValueFixed = @"
            string key = ""key"";
            ConcurrentDictionary<string, int> data = new ConcurrentDictionary<string, int>();
            if (data.TryGetValue(key, out var value))
            {
                return value;
            }

            return 0;";

        private const string DictionaryContainsKeyMultipleStatementsInIf = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if ([|data.ContainsKey(key)|])
            {
                Console.WriteLine(2);
                var x = 2;
                Console.WriteLine(data[key]);
                
                return x;
            }

            return 0;";

        private const string DictionaryContainsKeyMultipleStatementsInIfFixed = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (data.TryGetValue(key, out var value))
            {
                Console.WriteLine(2);
                var x = 2;
                Console.WriteLine(value);
                
                return x;
            }

            return 0;";

        private const string DictionaryContainsKeyMultipleConditions = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (key == ""key"" && [|data.ContainsKey(key)|])
            {
                Console.WriteLine(2);
                var x = 2;
                Console.WriteLine(data[key]);
                
                return x;
            }

            return 0;";

        private const string DictionaryContainsKeyMultipleConditionsFixed = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (key == ""key"" && data.TryGetValue(key, out var value))
            {
                Console.WriteLine(2);
                var x = 2;
                Console.WriteLine(value);
                
                return x;
            }

            return 0;";

        private const string DictionaryContainsKeyNestedDictionaryAccess = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (key == ""key"" && [|data.ContainsKey(key)|])
            {
                Console.WriteLine(2);
                var x = 2;
                Console.WriteLine(Wrapper(data[key]));
                
                return x;
            }

            int Wrapper(int i) {
                return i;
            }

            return 0;";

        private const string DictionaryContainsKeyNestedDictionaryAccessFixed = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (key == ""key"" && data.TryGetValue(key, out var value))
            {
                Console.WriteLine(2);
                var x = 2;
                Console.WriteLine(Wrapper(value));
                
                return x;
            }

            int Wrapper(int i) {
                return i;
            }

            return 0;";

        private const string DictionaryContainsKeyTernary = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();

            return [|data.ContainsKey(key)|] ? data[key] : 2;";

        private const string DictionaryContainsKeyTernaryFixed = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();

            return data.TryGetValue(key, out var value) ? value : 2;";

        #region NoDiagnostic

        private const string DictionaryContainsKeyModifyDictionary = @"
            string key = ""key"";
            IDictionary<string, int> data = new Dictionary<string, int>();
            if (data.ContainsKey(key))
            {
                Console.WriteLine(2);
                data[key] = 2;
                Console.WriteLine(data[key]);
                
                return 2;
            }

            return 0;";

        private const string DictionaryContainsKeyNonIDictionary = @"
            string key = ""key"";
            MyDictionary<string, int> data = new MyDictionary<string, int>();
            if (data.ContainsKey(key))
            {
                Console.WriteLine(2);
                data[key] = 2;
                Console.WriteLine(data[key]);
                
                return 2;
            }

            return 0;";

        #endregion

        [Theory]
        [InlineData(DictionaryContainsKeyPrintValue, DictionaryContainsKeyPrintValueFixed)]
        [InlineData(DictionaryContainsKeyReturnValue, DictionaryContainsKeyReturnValueFixed)]
        [InlineData(DictionaryContainsKeyMultipleStatementsInIf, DictionaryContainsKeyMultipleStatementsInIfFixed)]
        [InlineData(DictionaryContainsKeyMultipleConditions, DictionaryContainsKeyMultipleConditionsFixed)]
        [InlineData(DictionaryContainsKeyNestedDictionaryAccess, DictionaryContainsKeyNestedDictionaryAccessFixed)]
        [InlineData(DictionaryContainsKeyTernary, DictionaryContainsKeyTernaryFixed)]
        public Task ShouldReportDiagnostic(string codeSnippet, string fixedCodeSnippet)
        {
            string testCode = CreateCSharpCode(codeSnippet);
            string fixedCode = CreateCSharpCode(fixedCodeSnippet);
            var diagnostic = VerifyCS.Diagnostic(PreferDictionaryTryGetValueAnalyzer.ContainsKeyRule);

            return new VerifyCS.Test
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                ExpectedDiagnostics = {diagnostic}
            }.RunAsync();
        }

        [Theory]
        [InlineData(DictionaryContainsKeyModifyDictionary)]
        [InlineData(DictionaryContainsKeyNonIDictionary)]
        public Task ShouldNotReportDiagnostic(string codeSnippet)
        {
            string testCode = CreateCSharpCode(codeSnippet);

            return new VerifyCS.Test
            {
                TestCode = testCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
            }.RunAsync();
        }

        private static string CreateCSharpCode(string content)
        {
            return string.Format(CSharpTemplate, content);
        }
    }
}