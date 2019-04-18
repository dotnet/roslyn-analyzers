// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseSecureSharedAccesSignatureTests : DiagnosticAnalyzerTestBase
    {
        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string microsoftWindowsAzureStorageCSharpSourceCode = @"
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

    namespace Blob
    {
        public class CloudBlob
        {
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
            this.VerifyCSharp(
                new[] { source, microsoftWindowsAzureStorageCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void TestGetSharedAccessSignatureOfCloudStorageAccountDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;

class TestClass
{
    public void TestMethod(SharedAccessAccountPolicy policy)
    {
        var cloudStorageAccount = new CloudStorageAccount();
        cloudStorageAccount.GetSharedAccessSignature(policy);
    }
}",
            GetCSharpResultAt(10, 9, UseSecureSharedAccesSignature.DoNotUseAccountSASRule));
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
            GetCSharpResultAt(12, 9, UseSecureSharedAccesSignature.UseSharedAccessProtocolHttpsOnlyRule));
        }

        [Fact]
        public void TestGroupPolicyIdentifierOfBlobNamespaceIsNullDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(12, 9, UseSecureSharedAccesSignature.UseContainerLevelAccessPolicyRule));
        }

        [Fact]
        public void TestAccessPolicyIdentifierOfTableNamespaceIsNullDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(11, 9, UseSecureSharedAccesSignature.UseContainerLevelAccessPolicyRule));
        }

        [Fact]
        public void TestGroupPolicyIdentifierOfFileNamespaceIsNullDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(11, 9, UseSecureSharedAccesSignature.UseContainerLevelAccessPolicyRule));
        }

        [Fact]
        public void TestAccessPolicyIdentifierOfQueueNamespaceIsNullDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            GetCSharpResultAt(12, 9, UseSecureSharedAccesSignature.UseContainerLevelAccessPolicyRule));
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
        public void TestGroupPolicyIdentifierOfBlobNamespaceIsNotNullNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestGroupPolicyIdentifierOfFileNamespaceIsNotNullNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestAccessPolicyIdentifierOfQueueNamespaceIsNotNullNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void TestAccessPolicyIdentifierOfTableNamespaceIsNotNullNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseSecureSharedAccesSignature();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseSecureSharedAccesSignature();
        }
    }
}
