// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseAccountSASTests : DiagnosticAnalyzerTestBase
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

        public void NormalMethod()
        {
        }
    }

    public sealed class SharedAccessAccountPolicy
    {
    }
}

namespace NormalNamespace
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
            GetCSharpResultAt(10, 9, DoNotUseAccountSAS.Rule));
        }

        [Fact]
        public void TestNormalMethodOfCloudStorageAccountNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.WindowsAzure.Storage;

class TestClass
{
    public void TestMethod()
    {
        var cloudStorageAccount = new CloudStorageAccount();
        cloudStorageAccount.NormalMethod();
    }
}");
        }

        [Fact]
        public void TestGetSharedAccessSignatureOfCloudStorageAccountOfNormalNamespaceNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using NormalNamespace;

class TestClass
{
    public void TestMethod(SharedAccessAccountPolicy policy)
    {
        var cloudStorageAccount = new CloudStorageAccount();
        cloudStorageAccount.GetSharedAccessSignature(policy);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseAccountSAS();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseAccountSAS();
        }
    }
}
