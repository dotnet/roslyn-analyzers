// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseXslTransformTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestConstructXslTransformDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

class TestClass
{
    public void TestMethod()
    {
        new XslTransform();
    }
}",
            GetCSharpResultAt(9, 9, DoNotUseXslTransform.Rule));
        }

        [Fact]
        public void TestConstructNormalClassNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

class TestClass
{
    public void TestMethod()
    {
        new TestClass();
    }
}");
        }

        [Fact]
        public void TestInvokeMethodOfXslTransformNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

class TestClass
{
    public void TestMethod(XslTransform xslTransform)
    {
        xslTransform.Load(""url"");
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseXslTransform();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseXslTransform();
        }
    }
}
