// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.UseSharedAccessProtocolHttpsOnly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseSharedAccessProtocolHttpsOnlyTests
    {
        private const string MicrosoftWindowsAzureStorageCSharpSourceCode = @"
using System;

namespace Microsoft.WindowsAzure.Storage
{
    public class CloudStorageAccount
    {
        public string GetSharedAccessSignature (SharedAccessAccountPolicy policy)
        {
            return """";
        }
    }

    public sealed class SharedAccessAccountPolicy
    {
    }

    public class IPAddressOrRange
    {
    }

    public enum SharedAccessProtocol
    {
        HttpsOnly = 1,
        HttpsOrHttp = 2
    }

    namespace File
    {
        public class CloudFile
        {
            public string GetSharedAccessSignature (SharedAccessFilePolicy policy, string groupPolicyIdentifier)
            {
                return """";
            }

            public string GetSharedAccessSignature (SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, Nullable<SharedAccessProtocol> protocols, IPAddressOrRange ipAddressOrRange)
            {
                return """";
            }

            // This stub API is not a real method of CloudFile.
            // It is written for testing the case when the signature of method didn't match.
            public string GetSharedAccessSignature (SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, int protocols, IPAddressOrRange ipAddressOrRange)
            {
                return """";
            }
        }

        public sealed class SharedAccessFilePolicy
        {
        }

        public sealed class SharedAccessFileHeaders
        {
        }
    }
}";

        protected async Task VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, MicrosoftWindowsAzureStorageCSharpSourceCode  }
                }
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        protected async Task VerifyCSharpWithDependencies(string source, string editorConfigText, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, MicrosoftWindowsAzureStorageCSharpSourceCode  },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        [Fact]
        public async Task TestGetSharedAccessSignatureNotFromCloudStorageAccountWithProtocolsParameterDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, IPAddressOrRange ipAddressOrRange)
    {
        var cloudFile = new CloudFile();
        var protocols = SharedAccessProtocol.HttpsOrHttp;
        cloudFile.GetSharedAccessSignature(policy, headers, groupPolicyIdentifier, protocols, ipAddressOrRange); 
    }
}",
            GetCSharpResultAt(12, 9));
        }

        [Fact]
        public async Task TestGetSharedAccessSignatureNotFromCloudStorageAccountWithoutProtocolsParameterNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy, string groupPolicyIdentifier)
    {
        var cloudFile = new CloudFile();
        cloudFile.GetSharedAccessSignature(policy, groupPolicyIdentifier);
    }
}");
        }

        [Fact]
        public async Task TestGetSharedAccessSignatureNotFromCloudStorageAccountWithProtocolsParameterNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, IPAddressOrRange ipAddressOrRange)
    {
        var cloudFile = new CloudFile();
        var protocols = SharedAccessProtocol.HttpsOnly;
        cloudFile.GetSharedAccessSignature(policy, headers, groupPolicyIdentifier, protocols, ipAddressOrRange); 
    }
}");
        }

        [Fact]
        public async Task TestGetSharedAccessSignatureNotFromCloudStorageAccountWithProtocolsParameterOfTypeIntNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, IPAddressOrRange ipAddressOrRange)
    {
        var cloudFile = new CloudFile();
        cloudFile.GetSharedAccessSignature(policy, headers, groupPolicyIdentifier, 1, ipAddressOrRange); 
    }
}");
        }

        [Fact]
        public async Task TestGetSharedAccessSignatureOfANormalTypeNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;

class TestClass
{
    public string GetSharedAccessSignature (SharedAccessAccountPolicy policy)
    {
        return """";
    }

    public void TestMethod(SharedAccessAccountPolicy policy)
    {
        GetSharedAccessSignature(policy);
    }
}");
        }

        [Fact]
        public async Task TestWithoutMicrosoftWindowsAzureNamespaceNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class TestClass
{
    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestMicrosoftWindowsAzureNamespaceNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.WindowsAzure;

namespace Microsoft.WindowsAzure
{
    class A
    {
    }
}

class TestClass
{
    public void TestMethod()
    {
        var a = new A();
    }
}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.excluded_symbol_names = TestMethod")]
        [InlineData("dotnet_code_quality." + UseSharedAccessProtocolHttpsOnly.DiagnosticId + ".excluded_symbol_names = TestMethod")]
        [InlineData("dotnet_code_quality.dataflow.excluded_symbol_names = TestMethod")]
        public async Task EditorConfigConfiguration_ExcludedSymbolNamesOption(string editorConfigText)
        {
            var expected = Array.Empty<DiagnosticResult>();
            if (editorConfigText.Length == 0)
            {
                expected = new DiagnosticResult[]
                {
                    GetCSharpResultAt(12, 9)
                };
            }

            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, IPAddressOrRange ipAddressOrRange)
    {
        var cloudFile = new CloudFile();
        var protocols = SharedAccessProtocol.HttpsOrHttp;
        cloudFile.GetSharedAccessSignature(policy, headers, groupPolicyIdentifier, protocols, ipAddressOrRange); 
    }
}", editorConfigText, expected);
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column)
           => VerifyCS.Diagnostic()
               .WithLocation(line, column);
    }
}
