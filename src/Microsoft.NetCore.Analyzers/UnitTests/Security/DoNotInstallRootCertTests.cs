// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotInstallRootCertTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestConstructorWithStoreNameParameterDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = StoreName.Root; 
        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(10, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStoreNameParameterMaybeChangedDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = StoreName.Root; 
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            storeName = StoreName.My;
        }

        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(18, 9, DoNotInstallRootCert.MaybeInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStoreNameParameterUnassignedMaybeChangedWithRootDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod(StoreName storeName)
    {
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            storeName = StoreName.Root;
        }

        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(17, 9, DoNotInstallRootCert.MaybeInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStoreNameAndStoreLocationParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = StoreName.Root; 
        var x509Store = new X509Store(storeName, StoreLocation.CurrentUser);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(10, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStringParameterDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = ""Root""; 
        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(10, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestStringCaseSensitiveDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = ""rooT""; 
        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(10, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStringAndStoreLocationParametersDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = ""Root""; 
        var x509Store = new X509Store(storeName, StoreLocation.CurrentUser);
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(10, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStoreNameParameterWithoutTemporaryObjectDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        new X509Store(StoreName.Root).Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(8, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestConstructorWithStringParameterWithoutTemporaryObjectDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        new X509Store(""Root"").Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(8, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestPassX509StoreAsParameterInterproceduralDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = StoreName.Root; 
        var x509Store = new X509Store(storeName);
        TestMethod2(x509Store); 
    }

    public void TestMethod2(X509Store x509Store)
    {
        x509Store.Add(new X509Certificate2());
    }
}",
            GetCSharpResultAt(15, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestGetX509StoreFromLocalFunctionDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        GetX509Store().Add(new X509Certificate2());

        X509Store GetX509Store() => new X509Store(StoreName.Root);
    }
}",
            GetCSharpResultAt(8, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestReturnX509StoreInterproceduralDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        GetX509Store().Add(new X509Certificate2());
    }

    public X509Store GetX509Store()
    {
        return new X509Store(StoreName.Root);
    }
}",
            GetCSharpResultAt(8, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule));
        }

        [Fact]
        public void TestNotCallAddMethodNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var x509Store = new X509Store(""Root"");
    }
}");
        }

        [Fact]
        public void TestInstallCertToOtherStoreNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var x509Store = new X509Store(""My"");
        x509Store.Add(new X509Certificate2());
    }
}");
        }

        [Fact]
        public void TestInstallCertToNullStoreNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var x509Store = new X509Store(null);
        x509Store.Add(new X509Certificate2());
    }
}");
        }

        [Fact]
        public void TestCreateAStoreWithoutSettingStoreNameNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var x509Store = new X509Store();
        x509Store.Add(new X509Certificate2());
    }
}");
        }

        [Fact]
        public void TestConstructorWithStoreNameParameterUnassignedNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod(StoreName storeName)
    {
        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}");
        }

        [Fact]
        public void TestConstructorWithStoreNameParameterUnassignedMaybeChangedWithMyNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod(StoreName storeName)
    {
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            storeName = StoreName.My;
        }

        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}");
        }

        [Fact]
        public void TestPassX509StoreAsParameterInterproceduralNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = StoreName.My; 
        var x509Store = new X509Store(storeName);
        TestMethod2(x509Store); 
    }

    public void TestMethod2(X509Store x509Store)
    {
        x509Store.Add(new X509Certificate2());
    }
}");
        }

        [Fact]
        public void TestLambdaNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        GetX509Store().Add(new X509Certificate2());

        X509Store GetX509Store() => new X509Store(StoreName.My);
    }
}");
        }

        [Fact]
        public void TestReturnX509StoreInterproceduralNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        GetX509Store().Add(new X509Certificate2());
    }

    public X509Store GetX509Store()
    {
        return new X509Store(StoreName.My);
    }
}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.excluded_symbol_names = TestMethod")]
        [InlineData(@"dotnet_code_quality.CA5380.excluded_symbol_names = TestMethod
                      dotnet_code_quality.CA5381.excluded_symbol_names = TestMethod")]
        [InlineData("dotnet_code_quality.dataflow.excluded_symbol_names = TestMethod")]
        public void EditorConfigConfiguration_ExcludedSymbolNamesOption(string editorConfigText)
        {
            var expected = Array.Empty<DiagnosticResult>();
            if (editorConfigText.Length == 0)
            {
                expected = new DiagnosticResult[]
                {
                    GetCSharpResultAt(10, 9, DoNotInstallRootCert.DefinitelyInstallRootCertRule)
                };
            }

            VerifyCSharp(@"
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        var storeName = StoreName.Root; 
        var x509Store = new X509Store(storeName);
        x509Store.Add(new X509Certificate2());
    }
}", GetEditorConfigAdditionalFile(editorConfigText), expected);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotInstallRootCert();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotInstallRootCert();
        }
    }
}
