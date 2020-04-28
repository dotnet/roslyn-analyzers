// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.DefineResourceEntryCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.DefineResourceEntryCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class DefineResourceEntryCorrectlyTests
    {
        [Fact]
        public async Task DescriptionResourceEntryValueEndsWithPeriod_NoDiagnostic()
        {
            const string resxContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""MyAnalyzerDescription"" xml:space=""preserve"">
    <value>Do something.</value>
  </data>
</root>";

            await new VerifyCS.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                }
            }.RunAsync();

            await new VerifyVB.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task DescriptionResourceEntryValueDoesNotEndWithPeriod_Diagnostic()
        {
            const string resxContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""MyAnalyzerDescription"" xml:space=""preserve"">
    <value>Do something</value>
  </data>
</root>";

            await new VerifyCS.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic().WithLocation("Strings.resx", 4, 1),
                    },
                }
            }.RunAsync();

            await new VerifyVB.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic().WithLocation("Strings.resx", 4, 1),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task NotDescriptionResourceEntryValueDoesNotEndWithPeriod_NoDiagnostic()
        {
            const string resxContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""MyAnalyzerTitle"" xml:space=""preserve"">
    <value>Do something</value>
  </data>
</root>";

            await new VerifyCS.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                }
            }.RunAsync();

            await new VerifyVB.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task NotDescriptionResourceEntryValueEndsWithPeriod_Diagnostic()
        {
            const string resxContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""MyAnalyzerTitle"" xml:space=""preserve"">
    <value>Do something.</value>
  </data>
</root>";

            await new VerifyCS.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic().WithLocation("Strings.resx", 4, 1),
                    },
                }
            }.RunAsync();

            await new VerifyVB.Test
            {
                TestState =
                {
                    AdditionalFiles = { ("Strings.resx", resxContent), },
                    Sources = { "", },
                     ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic().WithLocation("Strings.resx", 4, 1),
                    },
                }
            }.RunAsync();
        }
    }
}
