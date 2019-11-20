// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.UseContainerLevelAccessPolicy,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseContainerLevelAccessPolicyTests
    {
        private const string MicrosoftWindowsAzureStorageCSharpSourceCode = @"
using System;

namespace Microsoft.WindowsAzure.Storage
{
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

    namespace Blob
    {
        public class CloudBlob
        {
            public string GetSharedAccessSignature (SharedAccessBlobPolicy policy)
            {
                return """";
            }

            public string GetSharedAccessSignature (SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, string groupPolicyIdentifier, Nullable<SharedAccessProtocol> protocols, IPAddressOrRange ipAddressOrRange)
            {
                return """";
            }
        }

        public sealed class SharedAccessBlobPolicy
        {
        }

        public sealed class SharedAccessBlobHeaders
        {
        }

        public class CloudAppendBlob : CloudBlob
        {
        }
    }
    namespace File
    {
        public class CloudFile
        {
            public string GetSharedAccessSignature (SharedAccessFilePolicy policy)
            {
                return """";
            }

            public string GetSharedAccessSignature (SharedAccessFilePolicy policy, string groupPolicyIdentifier)
            {
                return """";
            }

            public string GetSharedAccessSignature (SharedAccessFilePolicy policy, SharedAccessFileHeaders headers, string groupPolicyIdentifier, Nullable<SharedAccessProtocol> protocols, IPAddressOrRange ipAddressOrRange)
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

    namespace Queue
    {
        public class CloudQueue
        {
            public string GetSharedAccessSignature (SharedAccessQueuePolicy policy)
            {
                return """";
            }

            public string GetSharedAccessSignature (SharedAccessQueuePolicy policy, string accessPolicyIdentifier)
            {
                return """";
            }
        }

        public sealed class SharedAccessQueuePolicy
        {
        }
    }
    namespace Table
    {
        public class CloudTable
        {
            public string GetSharedAccessSignature (SharedAccessTablePolicy policy)
            {
                return """";
            }

            public string GetSharedAccessSignature (SharedAccessTablePolicy policy, string accessPolicyIdentifier, string startPartitionKey, string startRowKey, string endPartitionKey, string endRowKey)
            {
                return """";
            }
        }

        public sealed class SharedAccessTablePolicy
        {
        }
    }
}";

        private async Task VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, MicrosoftWindowsAzureStorageCSharpSourceCode  }
                },
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        private async Task VerifyCSharpWithDependencies(string source, string editorConfigText, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, MicrosoftWindowsAzureStorageCSharpSourceCode  },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        [Fact]
        public async Task TestGroupPolicyIdentifierOfBlobNamespaceIsNullDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

class TestClass
{
    public void TestMethod(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, Nullable<SharedAccessProtocol> protocols, IPAddressOrRange ipAddressOrRange)
    {
        var cloudAppendBlob = new CloudAppendBlob();
        string groupPolicyIdentifier = null;
        cloudAppendBlob.GetSharedAccessSignature(policy, headers, groupPolicyIdentifier, protocols, ipAddressOrRange);
    }
}",
            GetCSharpResultAt(12, 9));
        }

        [Fact]
        public async Task TestAccessPolicyIdentifierOfTableNamespaceIsNullDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Table;

class TestClass
{
    public void TestMethod(SharedAccessTablePolicy policy, string startPartitionKey, string startRowKey, string endPartitionKey, string endRowKey)
    {
        var cloudTable = new CloudTable();
        string accessPolicyIdentifier = null;
        cloudTable.GetSharedAccessSignature(policy, accessPolicyIdentifier, startPartitionKey, startRowKey, endPartitionKey, endRowKey);
    }
}",
            GetCSharpResultAt(11, 9));
        }

        [Fact]
        public async Task TestGroupPolicyIdentifierOfFileNamespaceIsNullDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy)
    {
        var cloudFile = new CloudFile();
        string groupPolicyIdentifier = null;
        cloudFile.GetSharedAccessSignature(policy, groupPolicyIdentifier);
    }
}",
            GetCSharpResultAt(11, 9));
        }

        [Fact]
        public async Task TestAccessPolicyIdentifierOfQueueNamespaceIsNullDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Queue;

class TestClass
{
    public int a; 
    public void TestMethod(SharedAccessQueuePolicy policy)
    {
        var cloudQueue = new CloudQueue();
        string accessPolicyIdentifier = null;
        cloudQueue.GetSharedAccessSignature(policy, accessPolicyIdentifier);
    }
}",
            GetCSharpResultAt(12, 9));
        }

        [Fact]
        public async Task TestWithoutGroupPolicyIdentifierParameterOfBlobNamespaceDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

class TestClass
{
    public void TestMethod(SharedAccessBlobPolicy policy)
    {
        var cloudAppendBlob = new CloudAppendBlob();
        cloudAppendBlob.GetSharedAccessSignature(policy);
    }
}",
            GetCSharpResultAt(11, 9));
        }

        [Fact]
        public async Task TestWithoutAccessPolicyIdentifierParameterOfTableNamespaceDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Table;

class TestClass
{
    public void TestMethod(SharedAccessTablePolicy policy)
    {
        var cloudTable = new CloudTable();
        cloudTable.GetSharedAccessSignature(policy);
    }
}",
            GetCSharpResultAt(10, 9));
        }

        [Fact]
        public async Task TestWithoutGroupPolicyIdentifierParameterOfFileNamespaceDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy)
    {
        var cloudFile = new CloudFile();
        cloudFile.GetSharedAccessSignature(policy);
    }
}",
            GetCSharpResultAt(10, 9));
        }

        [Fact]
        public async Task TestWithoutAccessPolicyIdentifierParameterOfQueueNamespaceDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Queue;

class TestClass
{
    public int a; 
    public void TestMethod(SharedAccessQueuePolicy policy)
    {
        var cloudQueue = new CloudQueue();
        cloudQueue.GetSharedAccessSignature(policy);
    }
}",
            GetCSharpResultAt(11, 9));
        }

        [Fact]
        public async Task TestGroupPolicyIdentifierOfBlobNamespaceIsNotNullNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

class TestClass
{
    public void TestMethod(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, Nullable<SharedAccessProtocol> protocols, IPAddressOrRange ipAddressOrRange)
    {
        var cloudAppendBlob = new CloudAppendBlob();
        string groupPolicyIdentifier = ""123"";
        cloudAppendBlob.GetSharedAccessSignature(policy, headers, groupPolicyIdentifier, protocols, ipAddressOrRange);
    }
}");
        }

        [Fact]
        public async Task TestGroupPolicyIdentifierOfFileNamespaceIsNotNullNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.File;

class TestClass
{
    public void TestMethod(SharedAccessFilePolicy policy)
    {
        var cloudFile = new CloudFile();
        string groupPolicyIdentifier = ""123"";
        cloudFile.GetSharedAccessSignature(policy, groupPolicyIdentifier);
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
        public async Task TestAccessPolicyIdentifierOfQueueNamespaceIsNotNullNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Queue;

class TestClass
{
    public void TestMethod(SharedAccessQueuePolicy policy)
    {
        var cloudQueue = new CloudQueue();
        string groupPolicyIdentifier = ""123"";
        cloudQueue.GetSharedAccessSignature(policy, groupPolicyIdentifier);
    }
}");
        }

        [Fact]
        public async Task TestAccessPolicyIdentifierOfTableNamespaceIsNotNullNoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Table;

class TestClass
{
    public void TestMethod(SharedAccessTablePolicy policy, string startPartitionKey, string startRowKey, string endPartitionKey, string endRowKey)
    {
        var cloudTable = new CloudTable();
        string accessPolicyIdentifier = ""123"";
        cloudTable.GetSharedAccessSignature(policy, accessPolicyIdentifier, startPartitionKey, startRowKey, endPartitionKey, endRowKey);
    }
}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.excluded_symbol_names = TestMethod")]
        [InlineData("dotnet_code_quality." + UseContainerLevelAccessPolicy.DiagnosticId + ".excluded_symbol_names = TestMethod")]
        [InlineData("dotnet_code_quality.dataflow.excluded_symbol_names = TestMethod")]
        public async Task EditorConfigConfiguration_ExcludedSymbolNamesOption(string editorConfigText)
        {
            var expected = Array.Empty<DiagnosticResult>();
            if (editorConfigText.Length == 0)
            {
                expected = new DiagnosticResult[]
                {
                    GetCSharpResultAt(11, 9)
                };
            }

            await VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage.Table;

class TestClass
{
    public void TestMethod(SharedAccessTablePolicy policy, string startPartitionKey, string startRowKey, string endPartitionKey, string endRowKey)
    {
        var cloudTable = new CloudTable();
        string accessPolicyIdentifier = null;
        cloudTable.GetSharedAccessSignature(policy, accessPolicyIdentifier, startPartitionKey, startRowKey, endPartitionKey, endRowKey);
    }
}", editorConfigText, expected);
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
           => VerifyCS.Diagnostic()
               .WithLocation(line, column);
    }
}
