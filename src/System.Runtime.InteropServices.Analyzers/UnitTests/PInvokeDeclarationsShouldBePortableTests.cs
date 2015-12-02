// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace System.Runtime.InteropServices.Analyzers.UnitTests
{
    public class PInvokeDeclarationsShouldBePortableTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicPInvokeDeclarationsShouldBePortableAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpPInvokeDeclarationsShouldBePortableAnalyzer();
        }
    }
}