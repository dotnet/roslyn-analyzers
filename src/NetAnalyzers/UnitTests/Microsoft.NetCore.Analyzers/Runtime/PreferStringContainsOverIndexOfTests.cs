// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferStringContainsOverIndexOfTests
    {
        [Theory]
        [InlineData("This", "This", false)]
        [InlineData("a", "a", true)]
        public async Task TestSingleStringAndChar(string input, string fix, bool isCharTest)
        {
            string quotes = isCharTest ? "'" : "\"";
            string csInput = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            int index = str.IndexOf(" + quotes + input + quotes + @");
            if ([|index == -1|])
            {

            }
        } 
    } 
}";
            string csFixOrdinal = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            if (!str.Contains(" + quotes + fix + quotes + @", System.StringComparison.Ordinal))
            {

            }
        } 
    } 
}";
            string csFixCulture = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            if (!str.Contains(" + quotes + fix + quotes + @", System.StringComparison.CurrentCulture))
            {

            }
        } 
    } 
}";

            var testOrdinal = new VerifyCS.Test
            {
                TestState = { Sources = { csInput } },
                FixedState = { Sources = { csFixOrdinal } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = "PreferStringContainsOrdinalOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
            };
            await testOrdinal.RunAsync();
            var testCulture = new VerifyCS.Test
            {
                TestState = { Sources = { csInput } },
                FixedState = { Sources = { csFixCulture } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = "PreferStringContainsCurrentCultureOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
            };
            await testCulture.RunAsync();

            quotes = "\"";
            string vbCharLiteral = isCharTest ? "c" : "";
            string vbInput = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""
            Dim index As Integer = Str.IndexOf(" + quotes + input + quotes + vbCharLiteral + @")
            If [|index = -1|] Then

            End If
        End Sub
    End Class
End Class
";

            string vbFixOrdinal = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""

            If Not Str.Contains(" + quotes + fix + quotes + vbCharLiteral + @", System.StringComparison.Ordinal) Then

            End If
        End Sub
    End Class
End Class
";
            string vbFixCulture = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""

            If Not Str.Contains(" + quotes + fix + quotes + vbCharLiteral + @", System.StringComparison.CurrentCulture) Then

            End If
        End Sub
    End Class
End Class
";
            var testOrdinal_vb = new VerifyVB.Test
            {
                TestState = { Sources = { vbInput } },
                FixedState = { Sources = { vbFixOrdinal } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = "PreferStringContainsOrdinalOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
            };
            await testOrdinal_vb.RunAsync();
            var testCulture_vb = new VerifyVB.Test
            {
                TestState = { Sources = { vbInput } },
                FixedState = { Sources = { vbFixCulture } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = "PreferStringContainsCurrentCultureOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
            };
            await testCulture_vb.RunAsync();

        }

        [Theory]
        [InlineData("This", "This", false)]
        [InlineData("a", "a", true)]
        public async Task TestStringAndCharWithComparison(string input, string fix, bool isCharTest)
        {
            string quotes = isCharTest ? "'" : "\"";
            string csInput = @" 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            int index = str.IndexOf(" + quotes + input + quotes + @", System.StringComparison.InvariantCulture);
            if ([|index == -1|])
            {

            }
        } 
    } 
}";
            string csFix = @" 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            if (!str.Contains(" + quotes + fix + quotes + @", System.StringComparison.InvariantCulture))
            {

            }
        } 
    } 
}";

            var test = new VerifyCS.Test
            {
                TestCode = csInput,
                FixedCode = csFix,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
            };
            await test.RunAsync();

            quotes = "\"";
            string vbCharLiteral = isCharTest ? "c" : "";
            string vbInput = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""
            Dim index As Integer = Str.IndexOf(" + quotes + input + quotes + vbCharLiteral + @", System.StringComparison.InvariantCulture)
            If [|index = -1|] Then

            End If
        End Sub
    End Class
End Class
";

            string vbFixOrdinal = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""

            If Not Str.Contains(" + quotes + fix + quotes + vbCharLiteral + @", System.StringComparison.InvariantCulture) Then

            End If
        End Sub
    End Class
End Class
";
            var testOrdinal_vb = new VerifyVB.Test
            {
                TestState = { Sources = { vbInput } },
                FixedState = { Sources = { vbFixOrdinal } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = "PreferStringContainsOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
            };
            await testOrdinal_vb.RunAsync();
        }

        [Theory]
        [InlineData("This", "This", false)]
        [InlineData("a", "a", true)]
        public async Task TestLeftOperandInvocation(string input, string fix, bool isCharTest)
        {
            string quotes = isCharTest ? "'" : "\"";
            string csInput = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            if ([|str.IndexOf(" + quotes + input + quotes + @") == -1|])
            {

            }
        } 
    } 
}";
            string csFixOrdinal = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            if (!str.Contains(" + quotes + fix + quotes + @", System.StringComparison.Ordinal))
            {

            }
        } 
    } 
}";
            string csFixCulture = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            if (!str.Contains(" + quotes + fix + quotes + @", System.StringComparison.CurrentCulture))
            {

            }
        } 
    } 
}";
            var testOrdinal = new VerifyCS.Test
            {
                TestState = { Sources = { csInput } },
                FixedState = { Sources = { csFixOrdinal } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = "PreferStringContainsOrdinalOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
            };
            await testOrdinal.RunAsync();
            var testCulture = new VerifyCS.Test
            {
                TestState = { Sources = { csInput } },
                FixedState = { Sources = { csFixCulture } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = "PreferStringContainsCurrentCultureOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
            };
            await testCulture.RunAsync();

            quotes = "\"";
            string vbCharLiteral = isCharTest ? "c" : "";
            string vbInput = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""
            If [|Str.IndexOf(" + quotes + input + quotes + vbCharLiteral + @") = -1|] Then

            End If
        End Sub
    End Class
End Class
";

            string vbFixOrdinal = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""
            If Not Str.Contains(" + quotes + fix + quotes + vbCharLiteral + @", System.StringComparison.Ordinal) Then

            End If
        End Sub
    End Class
End Class
";
            string vbFixCulture = @"
Public Class StringOf
    Class TestClass
        Public Sub Main()
            Dim Str As String = ""This is a statement""
            If Not Str.Contains(" + quotes + fix + quotes + vbCharLiteral + @", System.StringComparison.CurrentCulture) Then

            End If
        End Sub
    End Class
End Class
";
            var testOrdinal_vb = new VerifyVB.Test
            {
                TestState = { Sources = { vbInput } },
                FixedState = { Sources = { vbFixOrdinal } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = "PreferStringContainsOrdinalOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
            };
            await testOrdinal_vb.RunAsync();
            var testCulture_vb = new VerifyVB.Test
            {
                TestState = { Sources = { vbInput } },
                FixedState = { Sources = { vbFixCulture } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = "PreferStringContainsCurrentCultureOverIndexOfFixer",
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
            };
            await testCulture_vb.RunAsync();
        }

        [Theory]
        [InlineData("This", false, ", 1")]
        [InlineData("a", true, ", 1")]
        [InlineData("This", false, ", 1", ", 2")]
        [InlineData("a", true, ", 1", ", 2")]
        [InlineData("This", false, ", 1", ", System.StringComparison.OrdinalIgnoreCase")]
        [InlineData("This", false, ", 1", ", 2", ", System.StringComparison.OrdinalIgnoreCase")]
        public async Task TestTooManyArgumentsToIndexOf(string input, bool isCharTest, params string[] inputArguments)
        {
            string quotes = isCharTest ? "'" : "\"";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < inputArguments.Length; i++)
            {
                sb.Append(inputArguments[i]);
            }

            string csInput = @"
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            int index = str.IndexOf(" + quotes + input + quotes + sb.ToString() + @");
            if (index == -1)
            {

            }
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);
        }
    }
}