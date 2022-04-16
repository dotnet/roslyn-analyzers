// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpSpecifyCultureForToLowerAndToUpperAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpSpecifyCultureForToLowerAndToUpperFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicSpecifyCultureForToLowerAndToUpperAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicSpecifyCultureForToLowerAndToUpperFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class SpecifyCultureForToLowerAndToUpperTests
    {
        [Fact]
        public async Task CA1311_FixToLowerCSharpAsync_SpecifyCurrentCulture()
        {
            const string source = @"
using System.Globalization;

class C
{
    void M()
    {
        var a = ""test"";
        a.[|ToLower|]();
        a?.[|ToLower|]();
    }
}
";

            const string fixedSource = @"
using System.Globalization;

class C
{
    void M()
    {
        var a = ""test"";
        a.ToLower(CultureInfo.CurrentCulture);
        a?.ToLower(CultureInfo.CurrentCulture);
    }
}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.SpecifyCurrentCulture),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToLowerCSharpAsync_UseInvariantVersion()
        {
            const string source = @"
class C
{
    void M()
    {
        var a = ""test"";
        a.[|ToLower|]();
        a?.[|ToLower|]();
    }
}
";

            const string fixedSource = @"
class C
{
    void M()
    {
        var a = ""test"";
        a.ToLowerInvariant();
        a?.ToLowerInvariant();
    }
}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.UseInvariantVersion),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToLowerBasicAsync_SpecifyCurrentCulture()
        {
            var source = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.[|ToLower|]()
        a?.[|ToLower|]()
    End Sub
End Class
";

            var fixedSource = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.ToLower(CultureInfo.CurrentCulture)
        a?.ToLower(CultureInfo.CurrentCulture)
    End Sub
End Class
";
            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.SpecifyCurrentCulture),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToLowerBasicAsync_SpecifyCurrentCulture_MemberAccessSyntax()
        {
            var source = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.[|ToLower|]
        a?.[|ToLower|]
    End Sub
End Class
";

            var fixedSource = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.ToLower(CultureInfo.CurrentCulture)
        a?.ToLower(CultureInfo.CurrentCulture)
    End Sub
End Class
";
            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.SpecifyCurrentCulture),
                CodeActionValidationMode = CodeActionValidationMode.None,
            }.RunAsync();
        }


        [Fact]
        public async Task CA1311_FixToLowerBasicAsync_UseInvariantVersion()
        {
            const string source = @"
Class C
    Sub M()
        Dim a = ""test""
        a.[|ToLower|]()
        a?.[|ToLower|]()
    End Sub
End Class
";

            const string fixedSource = @"
Class C
    Sub M()
        Dim a = ""test""
        a.ToLowerInvariant()
        a?.ToLowerInvariant()
    End Sub
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.UseInvariantVersion),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToLowerBasicAsync_UseInvariantVersion_MemberAccessSyntax()
        {
            const string source = @"
Class C
    Sub M()
        Dim a = ""test""
        a.[|ToLower|]
        a?.[|ToLower|]
    End Sub
End Class
";

            const string fixedSource = @"
Class C
    Sub M()
        Dim a = ""test""
        a.ToLowerInvariant
        a?.ToLowerInvariant
    End Sub
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.UseInvariantVersion),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToUpperCSharpAsync_SpecifyCurrentCulture()
        {
            const string source = @"
using System.Globalization;

class C
{
    void M()
    {
        var a = ""test"";
        a.[|ToUpper|]();
        a?.[|ToUpper|]();
    }
}
";

            const string fixedSource = @"
using System.Globalization;

class C
{
    void M()
    {
        var a = ""test"";
        a.ToUpper(CultureInfo.CurrentCulture);
        a?.ToUpper(CultureInfo.CurrentCulture);
    }
}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.SpecifyCurrentCulture),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToUpperCSharpAsync_UseInvariantVersion()
        {
            const string source = @"
class C
{
    void M()
    {
        var a = ""test"";
        a.[|ToUpper|]();
        a?.[|ToUpper|]();
    }
}
";

            const string fixedSource = @"
class C
{
    void M()
    {
        var a = ""test"";
        a.ToUpperInvariant();
        a?.ToUpperInvariant();
    }
}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.UseInvariantVersion),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToUpperBasicAsync_SpecifyCurrentCulture()
        {
            var source = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.[|ToUpper|]()
        a?.[|ToUpper|]()
    End Sub
End Class
";

            var fixedSource = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.ToUpper(CultureInfo.CurrentCulture)
        a?.ToUpper(CultureInfo.CurrentCulture)
    End Sub
End Class
";
            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.SpecifyCurrentCulture),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToUpperBasicAsync_SpecifyCurrentCulture_MemberAccessSyntax()
        {
            var source = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.[|ToUpper|]
        a?.[|ToUpper|]
    End Sub
End Class
";

            var fixedSource = @"
Imports System.Globalization

Class C
    Sub M()
        Dim a = ""test""
        a.ToUpper(CultureInfo.CurrentCulture)
        a?.ToUpper(CultureInfo.CurrentCulture)
    End Sub
End Class
";
            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.SpecifyCurrentCulture),
                CodeActionValidationMode = CodeActionValidationMode.None,
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToUpperBasicAsync_UseInvariantVersion()
        {
            const string source = @"
Class C
    Sub M()
        Dim a = ""test""
        a.[|ToUpper|]()
        a?.[|ToUpper|]()
    End Sub
End Class
";

            const string fixedSource = @"
Class C
    Sub M()
        Dim a = ""test""
        a.ToUpperInvariant()
        a?.ToUpperInvariant()
    End Sub
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.UseInvariantVersion),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_FixToUpperBasicAsync_UseInvariantVersion_MemberAccessSyntax()
        {
            const string source = @"
Class C
    Sub M()
        Dim a = ""test""
        a.[|ToUpper|]()
        a?.[|ToUpper|]()
    End Sub
End Class
";

            const string fixedSource = @"
Class C
    Sub M()
        Dim a = ""test""
        a.ToUpperInvariant()
        a?.ToUpperInvariant()
    End Sub
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = nameof(MicrosoftNetCoreAnalyzersResources.UseInvariantVersion),
            }.RunAsync();
        }

        [Fact]
        public async Task CA1311_ToLower_WithExplicitCultureTest_CSharp()
        {

            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Globalization;

class C
{
    void Method()
    {
        string a = ""test"";
        CultureInfo culture = CultureInfo.CreateSpecificCulture(""ka-GE"");
        a.ToLower(culture);
        a?.ToLower(culture);
    }
}
");
        }

        [Fact]
        public async Task CA1311_ToLower_WithExplicitCultureTest_Basic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Globalization

Class C
    Sub Method()
        Dim a As String = ""test""
        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture(""ka-GE"")
        a.ToLower(culture)
        a?.ToLower(culture)
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1311_ToUpper_WithExplicitCultureTest_CSharp()
        {

            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Globalization;

class C
{
    void Method()
    {
        string a = ""test"";
        CultureInfo culture = CultureInfo.CreateSpecificCulture(""ka-GE"");
        a.ToUpper(culture);
        a?.ToUpper(culture);
    }
}
");
        }

        [Fact]
        public async Task CA1311_ToUpper_WithExplicitCultureTest_Basic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Globalization

Class C
    Sub Method()
        Dim a As String = ""test""
        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture(""ka-GE"")
        a.ToUpper(culture)
        a?.ToUpper(culture)
    End Sub
End Class
");
        }
    }
}
