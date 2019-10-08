// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseSharedAccessProtocolHttpsOnlyTests : DiagnosticAnalyzerTestBase
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

        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            this.VerifyCSharp(
                new[] { source, MicrosoftWindowsAzureStorageCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        protected void VerifyCSharpWithDependencies(string source, FileAndSource additionalFile, params DiagnosticResult[] expected)
        {
            this.VerifyCSharp(
                new[] { source, MicrosoftWindowsAzureStorageCSharpSourceCode },
                additionalFile,
                ReferenceFlags.None,
                expected);
        }

        [Fact]
        public void TestGetSharedAccessSignatureNotFromCloudStorageAccountWithProtocolsParameterDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(12, 9, UseSharedAccessProtocolHttpsOnly.Rule));
        }

        [Fact]
        public void TestGetSharedAccessSignatureNotFromCloudStorageAccountWithoutProtocolsParameterNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestGetSharedAccessSignatureNotFromCloudStorageAccountWithProtocolsParameterNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestGetSharedAccessSignatureNotFromCloudStorageAccountWithProtocolsParameterOfTypeIntNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestGetSharedAccessSignatureOfANormalTypeNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestWithoutMicrosoftWindowsAzureNamespaceNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestMicrosoftWindowsAzureNamespaceNoDiagnostic()
        {
            VerifyCSharp(@"
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
        public void EditorConfigConfiguration_ExcludedSymbolNamesOption(string editorConfigText)
        {
            var expected = Array.Empty<DiagnosticResult>();
            if (editorConfigText.Length == 0)
            {
                expected = new DiagnosticResult[]
                {
                    GetCSharpResultAt(12, 9, UseSharedAccessProtocolHttpsOnly.Rule)
                };
            }

            VerifyCSharpWithDependencies(@"
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
}", GetEditorConfigAdditionalFile(editorConfigText), expected);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseSharedAccessProtocolHttpsOnly();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseSharedAccessProtocolHttpsOnly();
        }
    }
}
